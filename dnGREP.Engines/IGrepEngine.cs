using System;
using System.Collections.Generic;
using System.Text;
using dnGREP.Common;

namespace dnGREP.Engines
{
    public interface IGrepEngine
    {
        bool Initialize(GrepEngineInitParams param, FileFilter filter);

        /// <summary>
        /// Return true if engine supports search only. Return false is engine supports replace as well.
        /// </summary>
        bool IsSearchOnly { get; }

        FileFilter FileFilter { get; }

        /// <summary>
        /// Searches folder for files whose content matches regex
        /// </summary>
        /// <param name="file">File to search in</param>
        /// <param name="searchPattern"></param>
        /// <param name="searchType"></param>
        /// <param name="isCaseSensitive"></param>
        /// <param name="isMultiline"></param>
        /// <param name="encoding"></param>
        /// <returns>List of results. If nothing is found returns empty list</returns>
        List<GrepSearchResult> Search(string file, string searchPattern, SearchType searchType, GrepSearchOption searchOptions, Encoding encoding);

        bool Replace(string sourceFile, string destinationFile, string searchPattern, string replacePattern, SearchType searchType, GrepSearchOption searchOptions, Encoding encoding);

        /// <summary>
        /// Method gets called when search/replace process is complete
        /// </summary>
        void Unload();

        /// <summary>
        /// Return version of the framework (dgGREP.Engines.dll) that the plugin was compiled against
        /// </summary>
        Version FrameworkVersion { get; }

        /// <summary>
        /// Can be used to provide custom file opening functionality
        /// </summary>
        /// <param name="args"></param>
        void OpenFile(OpenFileArgs args);
    }
}
