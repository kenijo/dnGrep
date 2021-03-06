﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using dnGREP.Common;
using dnGREP.Engines;

namespace dnGREP.WPF
{
    public class TestPatternViewModel : BaseMainViewModel
    {
        private string sampleText;
        public string SampleText
        {
            get { return sampleText; }
            set
            {
                if (value == sampleText)
                    return;

                sampleText = value;
                base.OnPropertyChanged(() => SampleText);
            }
        }

        private InlineCollection testOutput;
        public InlineCollection TestOutput
        {
            get { return testOutput; }
            set
            {
                if (value == testOutput)
                    return;

                testOutput = value;
                base.OnPropertyChanged(() => TestOutput);
            }
        }

        private string testOutputText;
        public string TestOutputText
        {
            get { return testOutputText; }
            set
            {
                if (value == testOutputText)
                    return;

                testOutputText = value;

                base.OnPropertyChanged(() => TestOutputText);
            }
        }

        RelayCommand _searchCommand;
        /// <summary>
        /// Returns a command that starts a search.
        /// </summary>
        public ICommand SearchCommand
        {
            get
            {
                if (_searchCommand == null)
                {
                    _searchCommand = new RelayCommand(
                        param => this.Search(),
                        param => this.CanSearch
                        );
                }
                return _searchCommand;
            }
        }

        RelayCommand _replaceCommand;
        /// <summary>
        /// Returns a command that starts a search in results.
        /// </summary>
        public ICommand ReplaceCommand
        {
            get
            {
                if (_replaceCommand == null)
                {
                    _replaceCommand = new RelayCommand(
                        param => this.Replace(),
                        param => this.CanReplace
                        );
                }
                return _replaceCommand;
            }
        }

        public override void UpdateState(string name)
        {
            base.UpdateState(name);
            CanReplace = true;
        }

        private void Search()
        {
            if (SampleText == null)
                SampleText = string.Empty;
            GrepEnginePlainText engine = new GrepEnginePlainText();
            engine.Initialize(new GrepEngineInitParams(
                GrepSettings.Instance.Get<bool>(GrepSettings.Key.ShowLinesInContext),
                GrepSettings.Instance.Get<int>(GrepSettings.Key.ContextLinesBefore),
                GrepSettings.Instance.Get<int>(GrepSettings.Key.ContextLinesAfter),
                GrepSettings.Instance.Get<double>(GrepSettings.Key.FuzzyMatchThreshold),
                GrepSettings.Instance.Get<bool>(GrepSettings.Key.ShowVerboseMatchCount)),
                new FileFilter());
            List<GrepSearchResult> results = new List<GrepSearchResult>();
            GrepSearchOption searchOptions = GrepSearchOption.None;
            if (Multiline)
                searchOptions |= GrepSearchOption.Multiline;
            if (CaseSensitive)
                searchOptions |= GrepSearchOption.CaseSensitive;
            if (Singleline)
                searchOptions |= GrepSearchOption.SingleLine;
            if (WholeWord)
                searchOptions |= GrepSearchOption.WholeWord;
            using (Stream inputStream = new MemoryStream(Encoding.Default.GetBytes(SampleText)))
            {
                try
                {
                    results = engine.Search(inputStream, "test.txt", SearchFor, TypeOfSearch,
                        searchOptions, Encoding.Default);
                    if (results != null)
                    {
                        using (StringReader reader = new StringReader(SampleText))
                        {
                            foreach (var result in results)
                            {
                                if (!result.HasSearchResults)
                                    result.SearchResults = Utils.GetLinesEx(reader, result.Matches, 0, 0);
                            }
                        }
                    }
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show("Incorrect pattern: " + ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            SearchResults.Clear();
            SearchResults.AddRange(results);
            Paragraph paragraph = new Paragraph();
            if (SearchResults.Count == 1)
            {
                SearchResults[0].FormattedLines.Load(false);
                foreach (FormattedGrepLine line in SearchResults[0].FormattedLines)
                {
                    // Copy children Inline to a temporary array.
                    paragraph.Inlines.AddRange(line.FormattedText.ToList());

                    paragraph.Inlines.Add(new LineBreak());
                    paragraph.Inlines.Add(new Run("================================="));
                    paragraph.Inlines.Add(new LineBreak());
                }
            }
            else
            {
                paragraph.Inlines.Add(new Run("No matches found"));
            }
            TestOutput = paragraph.Inlines;
        }

        private void Replace()
        {
            GrepEnginePlainText engine = new GrepEnginePlainText();
            engine.Initialize(new GrepEngineInitParams(
                GrepSettings.Instance.Get<bool>(GrepSettings.Key.ShowLinesInContext),
                GrepSettings.Instance.Get<int>(GrepSettings.Key.ContextLinesBefore),
                GrepSettings.Instance.Get<int>(GrepSettings.Key.ContextLinesAfter),
                GrepSettings.Instance.Get<double>(GrepSettings.Key.FuzzyMatchThreshold),
                GrepSettings.Instance.Get<bool>(GrepSettings.Key.ShowVerboseMatchCount)),
                new FileFilter());
            List<GrepSearchResult> results = new List<GrepSearchResult>();

            GrepSearchOption searchOptions = GrepSearchOption.None;
            if (Multiline)
                searchOptions |= GrepSearchOption.Multiline;
            if (CaseSensitive)
                searchOptions |= GrepSearchOption.CaseSensitive;
            if (Singleline)
                searchOptions |= GrepSearchOption.SingleLine;
            if (WholeWord)
                searchOptions |= GrepSearchOption.WholeWord;

            string replacedString = "";
            using (Stream inputStream = new MemoryStream(Encoding.Default.GetBytes(SampleText)))
            using (Stream writeStream = new MemoryStream())
            {
                engine.Replace(inputStream, writeStream, SearchFor, ReplaceWith, TypeOfSearch,
                    searchOptions, Encoding.Default);
                writeStream.Position = 0;
                StreamReader reader = new StreamReader(writeStream);
                replacedString = reader.ReadToEnd();
            }
            SearchResults.Clear();
            SearchResults.AddRange(results);
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run(replacedString));
            TestOutput = paragraph.Inlines;
            TestOutputText = replacedString;
        }
    }
}
