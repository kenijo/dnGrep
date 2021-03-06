using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using dnGREP.Common;
using NLog;

namespace dnGREP.Engines
{
    public class GrepEnginePlainText : GrepEngineBase, IGrepEngine
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public GrepEnginePlainText() : base() { }

        public bool IsSearchOnly
        {
            get { return false; }
        }

        public List<GrepSearchResult> Search(string file, string searchPattern, SearchType searchType, GrepSearchOption searchOptions, Encoding encoding)
        {
            using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan))
            {
                return Search(fileStream, file, searchPattern, searchType, searchOptions, encoding);
            }
        }

        public List<GrepSearchResult> Search(Stream input, string fileName, string searchPattern, SearchType searchType, GrepSearchOption searchOptions, Encoding encoding)
        {
            SearchDelegates.DoSearch searchMethod = doTextSearchCaseSensitive;
            switch (searchType)
            {
                case SearchType.PlainText:
                    if ((searchOptions & GrepSearchOption.CaseSensitive) == GrepSearchOption.CaseSensitive)
                    {
                        searchMethod = doTextSearchCaseSensitive;
                    }
                    else
                    {
                        searchMethod = doTextSearchCaseInsensitive;
                    }
                    break;
                case SearchType.Regex:
                    searchMethod = doRegexSearch;
                    break;
                case SearchType.XPath:
                    searchMethod = doXPathSearch;
                    break;
                case SearchType.Soundex:
                    searchMethod = doFuzzySearchMultiline;
                    break;
            }

            if ((searchOptions & GrepSearchOption.Multiline) == GrepSearchOption.Multiline || searchType == SearchType.XPath)
                return searchMultiline(input, fileName, searchPattern, searchOptions, searchMethod, encoding);
            else
                return search(input, fileName, searchPattern, searchOptions, searchMethod, encoding);
        }

        public bool Replace(string sourceFile, string destinationFile, string searchPattern, string replacePattern, SearchType searchType, GrepSearchOption searchOptions, Encoding encoding)
        {
            using (FileStream readStream = File.Open(sourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (FileStream writeStream = File.OpenWrite(destinationFile))
            {
                return Replace(readStream, writeStream, searchPattern, replacePattern, searchType, searchOptions, encoding);
            }
        }

        public bool Replace(Stream readStream, Stream writeStream, string searchPattern, string replacePattern, SearchType searchType, GrepSearchOption searchOptions, Encoding encoding)
        {
            SearchDelegates.DoReplace replaceMethod = doTextReplaceCaseSensitive;
            switch (searchType)
            {
                case SearchType.PlainText:
                    if ((searchOptions & GrepSearchOption.CaseSensitive) == GrepSearchOption.CaseSensitive)
                    {
                        replaceMethod = doTextReplaceCaseSensitive;
                    }
                    else
                    {
                        replaceMethod = doTextReplaceCaseInsensitive;
                    }
                    break;
                case SearchType.Regex:
                    replaceMethod = doRegexReplace;
                    break;
                case SearchType.XPath:
                    replaceMethod = doXPathReplace;
                    break;
                case SearchType.Soundex:
                    replaceMethod = doFuzzyReplace;
                    break;
            }

            if ((searchOptions & GrepSearchOption.Multiline) == GrepSearchOption.Multiline)
                return replaceMultiline(readStream, writeStream, searchPattern, replacePattern, searchOptions, replaceMethod, encoding);
            else
                return replace(readStream, writeStream, searchPattern, replacePattern, searchOptions, replaceMethod, encoding);
        }

        public void Unload()
        {
            // Do nothing
        }

        public Version FrameworkVersion
        {
            get
            {
                return Assembly.GetAssembly(typeof(IGrepEngine)).GetName().Version;
            }
        }

        #region Actual Implementation

        private List<GrepSearchResult> search(Stream input, string fileName, string searchPattern, GrepSearchOption searchOptions, SearchDelegates.DoSearch searchMethod, Encoding encoding)
        {
            List<GrepSearchResult> searchResults = new List<GrepSearchResult>();

            using (StreamReader baseReader = new StreamReader(input, encoding))
            {
                using (EolReader readStream = new EolReader(baseReader))
                {
                    string line = null;
                    int counter = 1;
                    int charCounter = 0;
                    List<GrepSearchResult.GrepMatch> matches = new List<GrepSearchResult.GrepMatch>();
                    while (!readStream.EndOfStream)
                    {
                        line = readStream.ReadLine();

                        if (Utils.CancelSearch)
                        {
                            return searchResults;
                        }
                        List<GrepSearchResult.GrepMatch> results = searchMethod(counter, line, searchPattern, searchOptions, false);
                        if (results.Count > 0)
                        {
                            foreach (GrepSearchResult.GrepMatch m in results)
                            {
                                matches.Add(new GrepSearchResult.GrepMatch(counter, m.StartLocation + charCounter, (int)m.Length));
                            }
                        }
                        charCounter += line.Length;
                        counter++;
                    }
                    if (matches.Count > 0)
                    {
                        searchResults.Add(new GrepSearchResult(fileName, searchPattern, matches, encoding));
                    }
                }
            }
            return searchResults;
        }

        private List<GrepSearchResult> searchMultiline(Stream input, string fileName, string searchPattern, GrepSearchOption searchOptions, SearchDelegates.DoSearch searchMethod, Encoding encoding)
        {
            List<GrepSearchResult> searchResults = new List<GrepSearchResult>();

            using (StreamReader readStream = new StreamReader(input, encoding))
            {
                string fileBody = readStream.ReadToEnd();
                var lines = searchMethod(-1, fileBody, searchPattern, searchOptions, true);
                //Utils.CleanResults(ref lines);
                if (lines.Count > 0)
                {
                    searchResults.Add(new GrepSearchResult(fileName, searchPattern, lines, encoding));
                }
            }

            return searchResults;
        }

        private bool replace(Stream inputStream, Stream outputStream, string searchPattern, string replacePattern, GrepSearchOption searchOptions, SearchDelegates.DoReplace replaceMethod, Encoding encoding)
        {
            using (StreamReader readStream = new StreamReader(inputStream, encoding))
            {
                StreamWriter writeStream = new StreamWriter(outputStream, encoding);

                string line = null;
                int counter = 1;

                // use first line to determine eol character(s);
                using (EolReader eolReader = new EolReader(readStream))
                {
                    line = eolReader.ReadLine();
                    if (line != null)
                    {
                        if (line.EndsWith("\r\n"))
                        {
                            writeStream.NewLine = "\r\n";
                        }
                        else if (line.EndsWith("\n"))
                        {
                            writeStream.NewLine = "\n";
                        }
                        else if (line.EndsWith("\r"))
                        {
                            writeStream.NewLine = "\r";
                        }
                    }
                }

                readStream.BaseStream.Seek(0, SeekOrigin.Begin);
                while (!readStream.EndOfStream)
                {
                    line = readStream.ReadLine();
                    line = replaceMethod(line, searchPattern, replacePattern, searchOptions);
                    writeStream.WriteLine(line);
                    counter++;
                }

                writeStream.Flush();
            }

            return true;
        }

        private bool replaceMultiline(Stream inputStream, Stream outputStream, string searchPattern, string replacePattern, GrepSearchOption searchOptions, SearchDelegates.DoReplace replaceMethod, Encoding encoding)
        {
            using (StreamReader readStream = new StreamReader(inputStream, encoding))
            {
                StreamWriter writeStream = new StreamWriter(outputStream, encoding);

                string fileBody = readStream.ReadToEnd();

                fileBody = replaceMethod(fileBody, searchPattern, replacePattern, searchOptions);
                writeStream.Write(fileBody);

                writeStream.Flush();
            }

            return true;
        }

        #endregion
    }
}
