﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using dnGREP.Common;
using dnGREP.Engines;
using NLog;
using System.IO;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace dnGREP.WPF
{
    /// <summary>
	/// Interaction logic for MainForm.xaml
    /// </summary>
    public partial class MainForm : Window
    {
		private List<KeyValuePair<string, int>> encodings = new List<KeyValuePair<string, int>>();
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private DateTime timer = DateTime.Now;
		private PublishedVersionExtractor ve = new PublishedVersionExtractor();
		private FileFolderDialog fileFolderDialog = new FileFolderDialog();
		private BackgroundWorker workerSearchReplace = new BackgroundWorker();
		private MainFormState inputData = new MainFormState();
		private BookmarksForm bookmarkForm = new BookmarksForm();
        private System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
        private System.Windows.Forms.HelpProvider helpProvider = new System.Windows.Forms.HelpProvider();
		public GrepSettings settings
		{
			get { return GrepSettings.Instance; }
		}
        
		#region Check version
		private void checkVersion()
		{
			try
			{
				if (settings.Get<bool>(GrepSettings.Key.EnableUpdateChecking))
				{
					DateTime lastCheck = settings.Get<DateTime>(GrepSettings.Key.LastCheckedVersion);
					TimeSpan duration = DateTime.Now.Subtract(lastCheck);
					if (duration.TotalDays >= settings.Get<int>(GrepSettings.Key.UpdateCheckInterval));
					{
						ve.StartWebRequest();
						settings.Set<DateTime>(GrepSettings.Key.LastCheckedVersion, DateTime.Now);
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogException(LogLevel.Error, ex.Message, ex);
			}
		}

		void ve_RetrievedVersion(object sender, PublishedVersionExtractor.PackageVersion version)
		{
			try
			{
				if (version.Version != null)
				{
					string currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
					if (PublishedVersionExtractor.IsUpdateNeeded(currentVersion, version.Version))
					{
						if (MessageBox.Show("New version of dnGREP (" + version.Version + ") is available for download.\nWould you like to download it now?", "New version", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
						{
							System.Diagnostics.Process.Start("http://code.google.com/p/dngrep/");
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogException(LogLevel.Error, ex.Message, ex);
			}
		}
		#endregion

		private void winFormControlsInit()
		{
			this.workerSearchReplace.WorkerReportsProgress = true;
			this.workerSearchReplace.WorkerSupportsCancellation = true;
			this.workerSearchReplace.DoWork += new System.ComponentModel.DoWorkEventHandler(this.doSearchReplace);
			this.workerSearchReplace.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.searchComplete);
			this.workerSearchReplace.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.searchProgressChanged);
            this.saveFileDialog.Filter = "CSV file|*.csv";
            DiginesisHelpProvider.HelpNamespace = "Doc\\dnGREP.chm";
            DiginesisHelpProvider.ShowHelp = true;
        }

		private void populateEncodings()
		{
			KeyValuePair<string, int> defaultValue = new KeyValuePair<string, int>("Auto detection (default)", -1);

			foreach (EncodingInfo ei in Encoding.GetEncodings())
			{
				Encoding e = ei.GetEncoding();
				encodings.Add(new KeyValuePair<string, int>(e.EncodingName, e.CodePage));
			}

			encodings.Sort(new KeyValueComparer());

			encodings.Insert(0, defaultValue);

			cbEncoding.ItemsSource = encodings;
			cbEncoding.DisplayMemberPath = "Key";
			cbEncoding.SelectedValuePath = "Value";
			cbEncoding.SelectedIndex = 0;
		}

		public MainForm()
		{
			InitializeComponent();
			this.DataContext = inputData;
			tvSearchResult.ItemsSource = inputData.SearchResults;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			winFormControlsInit();
			populateEncodings();
			ve.RetrievedVersion += new PublishedVersionExtractor.VersionExtractorHandler(ve_RetrievedVersion);
			bookmarkForm.PropertyChanged += new PropertyChangedEventHandler(bookmarkForm_PropertyChanged);
			checkVersion();
			inputData.UpdateState("");
		}

		void bookmarkForm_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "FilePattern")
				inputData.FilePattern = bookmarkForm.FilePattern;
			else if (e.PropertyName == "SearchFor")
				inputData.SearchFor = bookmarkForm.SearchFor;
			else if (e.PropertyName == "ReplaceWith")
				inputData.ReplaceWith = bookmarkForm.ReplaceWith;
		}

		private void btnBrowse_Click(object sender, RoutedEventArgs e)
		{
			fileFolderDialog.Dialog.Multiselect = true;
			fileFolderDialog.SelectedPath = Utils.GetBaseFolder(tbFolderName.Text);
			if (tbFolderName.Text == "")
			{
				string clipboard = Clipboard.GetText();
				try
				{
					if (System.IO.Path.IsPathRooted(clipboard))
						fileFolderDialog.SelectedPath = clipboard;
				}
				catch (Exception ex)
				{
					// Ignore
				}
			}
			if (fileFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				if (fileFolderDialog.SelectedPaths != null)
					inputData.FileOrFolderPath = fileFolderDialog.SelectedPaths;
				else
					inputData.FileOrFolderPath = fileFolderDialog.SelectedPath;
			}
		}

		private void btnSearch_Click(object sender, RoutedEventArgs e)
		{
			if (inputData.CurrentGrepOperation == GrepOperation.None && !workerSearchReplace.IsBusy)
			{
				inputData.CurrentGrepOperation = GrepOperation.Search;
				lblStatus.Text = "Searching...";
				barProgressBar.Value = 0;
				inputData.SearchResults.Clear();
                Dictionary<string, object> workerParames = new Dictionary<string, object>();
                workerParames["State"] = inputData;
				if (!settings.Get<bool>(GrepSettings.Key.PreviewResults))
					tvSearchResult.ItemsSource = null;
                workerSearchReplace.RunWorkerAsync(workerParames);
			}
		}

		private void btnSearchInResults_Click(object sender, RoutedEventArgs e)
		{
			if (inputData.CurrentGrepOperation == GrepOperation.None && !workerSearchReplace.IsBusy)
			{
				inputData.CurrentGrepOperation = GrepOperation.SearchInResults;
				lblStatus.Text = "Searching...";
				barProgressBar.Value = 0;
                List<string> foundFiles = new List<string>();
                foreach (FormattedGrepResult n in inputData.SearchResults) foundFiles.Add(n.GrepResult.FileNameReal);
                Dictionary<string, object> workerParames = new Dictionary<string, object>();
                workerParames["State"] = inputData;
                workerParames["Files"] = foundFiles;
				inputData.SearchResults.Clear();
				if (!settings.Get<bool>(GrepSettings.Key.PreviewResults))
					tvSearchResult.ItemsSource = null;
				workerSearchReplace.RunWorkerAsync(workerParames);
			}
		}

		private void btnReplace_Click(object sender, RoutedEventArgs e)
		{
			if (inputData.CurrentGrepOperation == GrepOperation.None && !workerSearchReplace.IsBusy)
			{
				if (string.IsNullOrEmpty(inputData.ReplaceWith))
				{
					if (MessageBox.Show("Are you sure you want to replace search pattern with empty string?", "Replace", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
						return;
				}
				List<string> roFiles = Utils.GetReadOnlyFiles(inputData.SearchResults.GetList());
				if (roFiles.Count > 0)
				{
					StringBuilder sb = new StringBuilder("Some of the files can not be modified. If you continue, these files will be skipped.\nWould you like to continue?\n\n");
					foreach (string fileName in roFiles)
					{
						sb.AppendLine(" - " + new FileInfo(fileName).Name);
					}
					if (MessageBox.Show(sb.ToString(), "Replace", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
						return;
				}
				lblStatus.Text = "Replacing...";
				inputData.CurrentGrepOperation = GrepOperation.Replace;
				inputData.CanUndo = false;
				inputData.UndoFolder = Utils.GetBaseFolder(tbFolderName.Text);
				barProgressBar.Value = 0;
                List<string> foundFiles = new List<string>();
                foreach (FormattedGrepResult n in inputData.SearchResults)
                {
                    if (!n.GrepResult.ReadOnly)
                        foundFiles.Add(n.GrepResult.FileNameReal);
                }
                Dictionary<string, object> workerParames = new Dictionary<string, object>();
                workerParames["State"] = inputData;
                workerParames["Files"] = foundFiles;
                inputData.SearchResults.Clear();
                workerSearchReplace.RunWorkerAsync(workerParames);
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			if (inputData.CurrentGrepOperation != GrepOperation.None)
			{
				GrepCore.CancelProcess = true;
			}
		}

		private void doSearchReplace(object sender, DoWorkEventArgs e)
		{
			try
			{
				if (!workerSearchReplace.CancellationPending)
				{
					timer = DateTime.Now;
                    Dictionary<string, object> workerParams = (Dictionary<string, object>)e.Argument;
                    MainFormState param = (MainFormState)workerParams["State"];
					if (param.CurrentGrepOperation == GrepOperation.Search || param.CurrentGrepOperation == GrepOperation.SearchInResults)
					{
						int sizeFrom = 0;
						int sizeTo = 0;
						if (param.UseFileSizeFilter == FileSizeFilter.Yes)
						{
							sizeFrom = param.SizeFrom;
							sizeTo = param.SizeTo;
						}

						string filePatternInclude = "*.*";
						if (param.TypeOfFileSearch == FileSearchType.Regex)
							filePatternInclude = ".*";

						if (!string.IsNullOrEmpty(param.FilePattern))
							filePatternInclude = param.FilePattern;

						if (param.TypeOfFileSearch == FileSearchType.Asterisk)
							filePatternInclude = filePatternInclude.Replace("\\", "");

						string filePatternExclude = "";
						if (!string.IsNullOrEmpty(param.FilePatternIgnore))
							filePatternExclude = param.FilePatternIgnore;

						if (param.TypeOfFileSearch == FileSearchType.Asterisk)
							filePatternExclude = filePatternExclude.Replace("\\", "");

						string[] files;

						if (param.CurrentGrepOperation == GrepOperation.SearchInResults)
						{
                            files = ((List<string>)workerParams["Files"]).ToArray();
						}
						else
						{
							files = Utils.GetFileList(inputData.FileOrFolderPath, filePatternInclude, filePatternExclude, param.TypeOfFileSearch == FileSearchType.Regex, param.IncludeSubfolder,
								param.IncludeHidden, param.IncludeBinary, sizeFrom, sizeTo);
						}
						GrepCore grep = new GrepCore();
						grep.SearchParams.FuzzyMatchThreshold = settings.Get<double>(GrepSettings.Key.FuzzyMatchThreshold);
						grep.SearchParams.ShowLinesInContext = settings.Get<bool>(GrepSettings.Key.ShowLinesInContext);
						grep.SearchParams.LinesBefore = settings.Get<int>(GrepSettings.Key.ContextLinesBefore);
						grep.SearchParams.LinesAfter = settings.Get<int>(GrepSettings.Key.ContextLinesAfter);
						grep.PreviewFilesDuringSearch = settings.Get<bool>(GrepSettings.Key.PreviewResults);

                        GrepSearchOption searchOptions = GrepSearchOption.None;
                        if (inputData.Multiline)
                            searchOptions |= GrepSearchOption.Multiline;
                        if (inputData.CaseSensitive)
                            searchOptions |= GrepSearchOption.CaseSensitive;
                        if (inputData.Singleline)
                            searchOptions |= GrepSearchOption.SingleLine;
						if (inputData.WholeWord)
							searchOptions |= GrepSearchOption.WholeWord;

						grep.ProcessedFile += new GrepCore.SearchProgressHandler(grep_ProcessedFile);
						List<GrepSearchResult> results = null;
						e.Result = grep.Search(files, param.TypeOfSearch, param.SearchFor, searchOptions, param.CodePage);

						grep.ProcessedFile -= new GrepCore.SearchProgressHandler(grep_ProcessedFile);
					}
					else
					{
						GrepCore grep = new GrepCore();
						grep.SearchParams.FuzzyMatchThreshold = settings.Get<double>(GrepSettings.Key.FuzzyMatchThreshold);
						grep.SearchParams.ShowLinesInContext = settings.Get<bool>(GrepSettings.Key.ShowLinesInContext);
						grep.SearchParams.LinesBefore = settings.Get<int>(GrepSettings.Key.ContextLinesBefore);
						grep.SearchParams.LinesAfter = settings.Get<int>(GrepSettings.Key.ContextLinesAfter);
						grep.PreviewFilesDuringSearch = settings.Get<bool>(GrepSettings.Key.PreviewResults);

                        GrepSearchOption searchOptions = GrepSearchOption.None;
                        if (inputData.Multiline)
                            searchOptions |= GrepSearchOption.Multiline;
                        if (inputData.CaseSensitive)
                            searchOptions |= GrepSearchOption.CaseSensitive;
                        if (inputData.Singleline)
                            searchOptions |= GrepSearchOption.SingleLine;
						if (inputData.WholeWord)
							searchOptions |= GrepSearchOption.WholeWord;

						grep.ProcessedFile += new GrepCore.SearchProgressHandler(grep_ProcessedFile);
                        string[] files = ((List<string>)workerParams["Files"]).ToArray();
                        e.Result = grep.Replace(files, param.TypeOfSearch, Utils.GetBaseFolder(param.FileOrFolderPath), param.SearchFor, param.ReplaceWith, searchOptions, param.CodePage);

						grep.ProcessedFile -= new GrepCore.SearchProgressHandler(grep_ProcessedFile);
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogException(LogLevel.Error, ex.Message, ex);
				MainFormState param = (MainFormState)e.Argument;
				if (param.CurrentGrepOperation == GrepOperation.Search || param.CurrentGrepOperation == GrepOperation.SearchInResults)
					MessageBox.Show("Search failed! See error log.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				else
					MessageBox.Show("Replace failed! See error log.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		void grep_ProcessedFile(object sender, GrepCore.ProgressStatus progress)
		{
			workerSearchReplace.ReportProgress((int)(progress.ProcessedFiles * 100 / progress.TotalFiles), progress);
		}

		private void searchProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			try
			{
				if (!GrepCore.CancelProcess)
				{
					GrepCore.ProgressStatus progress = (GrepCore.ProgressStatus)e.UserState;
					barProgressBar.Value = e.ProgressPercentage;
					lblStatus.Text = "(" + progress.ProcessedFiles + " of " + progress.TotalFiles + ")";
					if (progress.SearchResults != null)
					{
						inputData.SearchResults.AddRange(progress.SearchResults);
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogException(LogLevel.Error, ex.Message, ex);
				MessageBox.Show("Search or replace failed! See error log.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void searchComplete(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{
				if (inputData.CurrentGrepOperation == GrepOperation.Search || inputData.CurrentGrepOperation == GrepOperation.SearchInResults)
				{
					List<GrepSearchResult> results = new List<GrepSearchResult>();
					if (e.Result == null)
					{
						lblStatus.Text = "Search Failed";
					}
					else if (!e.Cancelled)
					{
						TimeSpan duration = DateTime.Now.Subtract(timer);
						results = (List<GrepSearchResult>)e.Result;
						lblStatus.Text = "Search Complete - " + results.Count + " files found in " + duration.TotalMilliseconds + "ms.";
					}
					else
					{
						lblStatus.Text = "Search Canceled";
					}
					barProgressBar.Value = 0;
					inputData.CurrentGrepOperation = GrepOperation.None;
					if (inputData.SearchResults.Count > 0)
						inputData.FilesFound = true;
					if (!settings.Get<bool>(GrepSettings.Key.PreviewResults))
					{
						inputData.SearchResults.AddRange(results);
						tvSearchResult.ItemsSource = inputData.SearchResults;
					}
				}
				else if (inputData.CurrentGrepOperation == GrepOperation.Replace)
				{
					if (!e.Cancelled)
					{
						if (e.Result == null || ((int)e.Result) == -1)
						{
							lblStatus.Text = "Replace Failed.";
							MessageBox.Show("Replace failed! See error log.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						}
						else
						{
							lblStatus.Text = "Replace Complete - " + (int)e.Result + " files replaced.";
							inputData.CanUndo = true;
						}
					}
					else
					{
						lblStatus.Text = "Replace Canceled";
					}
					barProgressBar.Value = 0;
					inputData.CurrentGrepOperation = GrepOperation.None;
					inputData.SearchResults.Clear();
				}

				string outdatedEngines = dnGREP.Engines.GrepEngineFactory.GetListOfFailedEngines();
				if (!string.IsNullOrEmpty(outdatedEngines))
				{
					MessageBox.Show("The following plugins failed to load:\n\n" + outdatedEngines + "\n\nDefault engine was used instead.", "Plugin Errors", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
			catch (Exception ex)
			{
				logger.LogException(LogLevel.Error, ex.Message, ex);
				MessageBox.Show("Search or replace failed! See error log.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			inputData.UpdateState("");
		}

		private void MainForm_Closing(object sender, CancelEventArgs e)
		{
			GrepCore.CancelProcess = true;
			if (workerSearchReplace.IsBusy)
				workerSearchReplace.CancelAsync();
			settings.Save();
		}

		private void formKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
				Close();
		}


		private void treeKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
			{
				if (tvSearchResult.SelectedItem is FormattedGrepLine)
				{
					FormattedGrepLine selectedNode = (FormattedGrepLine)tvSearchResult.SelectedItem;
					Clipboard.SetText(selectedNode.GrepLine.LineText);
				}
				else if (tvSearchResult.SelectedItem is FormattedGrepResult)
				{
					FormattedGrepResult result = (FormattedGrepResult)tvSearchResult.SelectedItem;
					Clipboard.SetText(result.GrepResult.FileNameDisplayed);
				}
			}
		}	

		private void undoToolStripMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (inputData.CanUndo)
			{
				MessageBoxResult response = MessageBox.Show("Undo will revert modified file(s) back to their original state. Any changes made to the file(s) after the replace will be overwritten. Are you sure you want to procede?", 
					"Undo", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);
				if (response == MessageBoxResult.Yes)
				{
					GrepCore core = new GrepCore();
					bool result = core.Undo(inputData.UndoFolder);
					if (result)
					{
						MessageBox.Show("Files have been successfully reverted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
						Utils.DeleteTempFolder();
					}
					else
					{
						MessageBox.Show("There was an error reverting files. Please examine the error log.", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
					}
					inputData.CanUndo = false;
				}
			}
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			//TODO
			//AboutForm about = new AboutForm();
			//about.ShowDialog();
		}

		private void helpToolStripMenuItem_Click(object sender, RoutedEventArgs e)
		{
            ApplicationCommands.Help.Execute(null, helpToolStripMenuItem);
		}

        private void aboutToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }

		private void cbCaseSensitive_CheckedChanged(object sender, RoutedEventArgs e)
		{
			inputData.FilesFound = false;
		}

        private void btnOptions_Click(object sender, RoutedEventArgs e)
        {
            OptionsForm optionsForm = new OptionsForm();
            optionsForm.ShowDialog();
            inputData.LoadAppSettings();
        }

		private void btnTest_Click(object sender, RoutedEventArgs e)
		{
            try
            {
                TestPattern testForm = new TestPattern();
                testForm.ShowDialog();
                inputData.LoadAppSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error running regex test. Please examine the error log.", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
            }
		}

		private void btnBookmarkOpen_Click(object sender, RoutedEventArgs e)
		{
			bookmarkForm.Show();
		}

		private void btnBookmark_Click(object sender, RoutedEventArgs e)
		{
			BookmarkDetails bookmarkEditForm = new BookmarkDetails(CreateOrEdit.Create);
			Bookmark newBookmark = new Bookmark(tbSearchFor.Text, tbReplaceWith.Text, tbFilePattern.Text, "");
			bookmarkEditForm.Bookmark = newBookmark;
			if (bookmarkEditForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
			    if (!BookmarkLibrary.Instance.Bookmarks.Contains(newBookmark))
			    {
			        BookmarkLibrary.Instance.Bookmarks.Add(newBookmark);
			        BookmarkLibrary.Save();
			    }
			}
		}

		#region Advance actions
		private void copyFilesToolStripMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (inputData.FilesFound)
			{
			    if (fileFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			    {
			        try
			        {
                        if (!Utils.CanCopyFiles(inputData.SearchResults.GetList(), Utils.GetBaseFolder(fileFolderDialog.SelectedPath)))
			            {
			                MessageBox.Show("Attention, some of the files are located in the selected directory.\nPlease select another directory and try again.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
			                return;
			            }

                        Utils.CopyFiles(inputData.SearchResults.GetList(), Utils.GetBaseFolder(tbFolderName.Text), Utils.GetBaseFolder(fileFolderDialog.SelectedPath), true);
                        MessageBox.Show("Files have been successfully copied.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
			        }
			        catch (Exception ex)
			        {
                        MessageBox.Show("There was an error copying files. Please examine the error log.", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
			        }
			        inputData.CanUndo = false;
			    }
			}
		}

		private void copyToClipboardToolStripMenuItem_Click(object sender, RoutedEventArgs e)
		{
			StringBuilder sb = new StringBuilder();
			foreach (GrepSearchResult result in inputData.SearchResults.GetList())
			{
				sb.AppendLine(result.FileNameReal);
			}
			Clipboard.SetText(sb.ToString());
		}

		private void moveFilesToolStripMenuItem_Click(object sender, RoutedEventArgs e)
		{
            if (inputData.FilesFound)
            {
                if (fileFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        if (!Utils.CanCopyFiles(inputData.SearchResults.GetList(), Utils.GetBaseFolder(fileFolderDialog.SelectedPath)))
                        {
                            MessageBox.Show("Attention, some of the files are located in the selected directory.\nPlease select another directory and try again.", 
                                "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        Utils.CopyFiles(inputData.SearchResults.GetList(), Utils.GetBaseFolder(tbFolderName.Text), inputData.FilePattern, true);
                        Utils.DeleteFiles(inputData.SearchResults.GetList());
                        MessageBox.Show("Files have been successfully moved.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("There was an error moving files. Please examine the error log.", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    inputData.CanUndo = false;
                    inputData.SearchResults.Clear();
                    inputData.FilesFound = false;
                }
            }
		}

		private void deleteFilesToolStripMenuItem_Click(object sender, RoutedEventArgs e)
		{
            if (inputData.FilesFound)
            {
                try
                {
                    if (MessageBox.Show("Attention, you are about to delete files found during search.\nAre you sure you want to procede?", "Attention", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) !=  MessageBoxResult.Yes)
                    {
                        return;
                    }

                    Utils.DeleteFiles(inputData.SearchResults.GetList());
                    MessageBox.Show("Files have been successfully deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There was an error deleting files. Please examine the error log.", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                inputData.CanUndo = false;
                inputData.SearchResults.Clear();
                inputData.FilesFound = false;
            }
		}

		private void saveAsCSVToolStripMenuItem_Click(object sender, RoutedEventArgs e)
		{
            if (inputData.FilesFound)
            {
                saveFileDialog.InitialDirectory = Utils.GetBaseFolder(inputData.FileOrFolderPath);
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        Utils.SaveResultsAsCSV(inputData.SearchResults.GetList(), saveFileDialog.FileName);
                        MessageBox.Show("CSV file has been successfully created.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("There was an error creating a CSV file. Please examine the error log.", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
		}

		private void btnOtherActions_Click(object sender, RoutedEventArgs e)
		{
            btnOtherActions.ContextMenu.IsOpen = true;
		}
		#endregion

        private void tvSearchResult_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                item.Focus();
                e.Handled = true;
            }
        }

        private void tvSearchResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (tvSearchResult.SelectedItem is FormattedGrepLine && 
				e.OriginalSource is TextBlock || e.OriginalSource is Run)
            {
                btnOpenFile_Click(sender, new RoutedEventArgs(e.RoutedEvent));
            }
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (tvSearchResult.SelectedItem is FormattedGrepLine)
            {
                FormattedGrepLine selectedNode = (FormattedGrepLine)tvSearchResult.SelectedItem;
                // Line was selected
                int lineNumber = selectedNode.GrepLine.LineNumber;

                FormattedGrepResult result = selectedNode.Parent;
                OpenFileArgs fileArg = new OpenFileArgs(result.GrepResult, lineNumber, settings.Get<bool>(GrepSettings.Key.UseCustomEditor), settings.Get<string>(GrepSettings.Key.CustomEditor), settings.Get<string>(GrepSettings.Key.CustomEditorArgs));
                dnGREP.Engines.GrepEngineFactory.GetSearchEngine(result.GrepResult.FileNameReal, new GrepEngineInitParams(false, 0, 0, 0.5)).OpenFile(fileArg);
                if (fileArg.UseBaseEngine)
					Utils.OpenFile(new OpenFileArgs(result.GrepResult, lineNumber, settings.Get<bool>(GrepSettings.Key.UseCustomEditor), settings.Get<string>(GrepSettings.Key.CustomEditor), settings.Get<string>(GrepSettings.Key.CustomEditorArgs)));
            }
            else if (tvSearchResult.SelectedItem is FormattedGrepResult)
            {
                FormattedGrepResult result = (FormattedGrepResult)tvSearchResult.SelectedItem;
                // Line was selected
                int lineNumber = 0;
				OpenFileArgs fileArg = new OpenFileArgs(result.GrepResult, lineNumber, settings.Get<bool>(GrepSettings.Key.UseCustomEditor), settings.Get<string>(GrepSettings.Key.CustomEditor), settings.Get<string>(GrepSettings.Key.CustomEditorArgs));
				dnGREP.Engines.GrepEngineFactory.GetSearchEngine(result.GrepResult.FileNameReal, new GrepEngineInitParams(false, 0, 0, 0.5)).OpenFile(fileArg);
                if (fileArg.UseBaseEngine)
					Utils.OpenFile(new OpenFileArgs(result.GrepResult, lineNumber, settings.Get<bool>(GrepSettings.Key.UseCustomEditor), settings.Get<string>(GrepSettings.Key.CustomEditor), settings.Get<string>(GrepSettings.Key.CustomEditorArgs)));
            }
        }

        private void btnOpenContainingFolder_Click(object sender, RoutedEventArgs e)
        {
            if (tvSearchResult.SelectedItem is FormattedGrepLine)
            {
                FormattedGrepLine selectedNode = (FormattedGrepLine)tvSearchResult.SelectedItem;
                Utils.OpenContainingFolder(selectedNode.Parent.GrepResult.FileNameReal, selectedNode.GrepLine.LineNumber);
            }
            else if (tvSearchResult.SelectedItem is FormattedGrepResult)
            {
                FormattedGrepResult selectedNode = (FormattedGrepResult)tvSearchResult.SelectedItem;
                Utils.OpenContainingFolder(selectedNode.GrepResult.FileNameReal, -1);
            }
        }

        private void btnExpandAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (FormattedGrepResult result in tvSearchResult.Items)
            {
                result.IsExpanded = true;
            }
        }

        private void btnCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (FormattedGrepResult result in tvSearchResult.Items)
            {
                result.IsExpanded = false;
            }
        }

        private void btnExclude_Click(object sender, RoutedEventArgs e)
        {
            if (tvSearchResult.SelectedItem is FormattedGrepLine)
            {
                FormattedGrepLine selectedNode = (FormattedGrepLine)tvSearchResult.SelectedItem;
                inputData.SearchResults.Remove(selectedNode.Parent);
            }
            else if (tvSearchResult.SelectedItem is FormattedGrepResult)
            {
                FormattedGrepResult selectedNode = (FormattedGrepResult)tvSearchResult.SelectedItem;
                inputData.SearchResults.Remove(selectedNode);
            }
        }

        public void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }

        private void OutputFolderDrop(object sender, DragEventArgs e)
        {
            if (e.Data is System.Windows.DataObject &&
            ((System.Windows.DataObject)e.Data).ContainsFileDropList())
            {
                inputData.FileOrFolderPath = "";
                StringCollection fileNames = ((System.Windows.DataObject)e.Data).GetFileDropList();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < fileNames.Count; i++ )
                {
                    sb.Append(fileNames[i]);
                    if (i < (fileNames.Count - 1))
                        sb.Append(";");
                }
                inputData.FileOrFolderPath = sb.ToString();
            } 
        }

		private void TextBoxFocus(object sender, RoutedEventArgs e)
		{
			if (e.Source is TextBox)
			{
				((TextBox)e.Source).SelectAll();
			}
		}
	
	}
}
