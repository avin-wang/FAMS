using FAMS.Services;
using FAMS.ViewModels.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAMS.Models.Documents
{
    class DocModel
    {
        private CFamsFileHelper _cfgHelper = new CFamsFileHelper(); // config file access
        private CFamsFileHelper _docHelper = new CFamsFileHelper(); // doc file access
        private CLogWriter _logWriter = CLogWriter.GetInstance();

        private string _docDir = string.Empty;
        private string _attachDir = string.Empty;

        public DocModel()
        {
            // Initialize config file helper
            _cfgHelper.Init(Directory.GetCurrentDirectory() + "\\config\\system.fams");

            // Get log file info
            string logDir = Directory.GetCurrentDirectory() + _cfgHelper.GetData("config", "log_dir");
            string logName = _cfgHelper.GetData("config", "log_name");

            // Initialize log writer
            if (!System.IO.Directory.Exists(logDir))
            {
                System.IO.Directory.CreateDirectory(logDir);
            }
            _logWriter.Init(logDir + "\\", logName);
            _logWriter.SetCurrLevel(5);

            // get dir
            _docDir = Directory.GetCurrentDirectory() + _cfgHelper.GetData("config", "doc_dir");
            _attachDir = Directory.GetCurrentDirectory() + _cfgHelper.GetData("config", "doc_attach_dir");

            if (!Directory.Exists(_docDir))
            {
                Directory.CreateDirectory(_docDir);
            }

            if (!Directory.Exists(_attachDir))
            {
                Directory.CreateDirectory(_attachDir);
            }
        }

        #region model functions
        /// <summary>
        /// Load doc data
        /// </summary>
        /// <returns></returns>
        public List<DocViewModel> LoadDoc()
        {
            // get doc data
            List<DocViewModel> docList = new List<DocViewModel>();
            DirectoryInfo di = new DirectoryInfo(_docDir);

            try
            {
                foreach (FileInfo file in di.GetFiles("*.fams"))
                {
                    _docHelper.Init(file.FullName);

                    DocViewModel doc = new DocViewModel();
                    doc.DocTitle = _docHelper.GetData("header", "title");
                    doc.OriginalTitle = _docHelper.GetData("header", "org_title");
                    doc.Source = _docHelper.GetData("header", "source");
                    doc.Category = _docHelper.GetData("header", "category");
                    doc.Author = _docHelper.GetData("header", "author");
                    doc.CreateTime = _docHelper.GetData("header", "create_time");
                    doc.LastRevised = _docHelper.GetData("header", "last_revised");
                    doc.FormerTitle = _docHelper.GetData("header", "former_title");

                    foreach (string name in _docHelper.GetData("header", "attach_name").Split('|'))
                    {
                        if (name != "")
                        {
                            doc.AttachFileNames.Add(name);
                        }
                    }

                    foreach (string path in _docHelper.GetData("header", "attach_src_path").Split('|'))
                    {
                        if (path != "")
                        {
                            doc.AttachSourcePaths.Add(path);
                        }
                    }

                    doc.DocText = _docHelper.GetData("content", "text");

                    docList.Add(doc);
                }
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("DocModel::LoadDoc >> exception occurs when read doc file: " + ex.Message);
            }

            return docList;
        }

        /// <summary>
        /// Create a new doc
        /// </summary>
        /// <param name="doc">doc viewmodel instance</param>
        /// <returns></returns>
        public int CreateDoc(DocViewModel doc)
        {
            _docHelper.Init(_docDir + "\\" + doc.DocTitle + ".fams");

            // write text data
            try
            {
                _docHelper.WriteData("header", "title", doc.DocTitle);
                _docHelper.WriteData("header", "org_title", doc.OriginalTitle);
                _docHelper.WriteData("header", "source", doc.Source);
                _docHelper.WriteData("header", "category", doc.Category);
                _docHelper.WriteData("header", "author", doc.Author);
                _docHelper.WriteData("header", "create_time", doc.CreateTime);
                _docHelper.WriteData("header", "last_revised", doc.LastRevised);
                _docHelper.WriteData("header", "former_title", doc.FormerTitle);

                string names = string.Empty;
                foreach (string name in doc.AttachFileNames)
                {
                    names += name + "|";
                }
                names = names == string.Empty ? "" : (names.Contains("|") ? names.Remove(names.LastIndexOf('|')) : names);
                _docHelper.WriteData("header", "attach_name", names);

                string paths = string.Empty;
                foreach (string path in doc.AttachSourcePaths)
                {
                    paths += path + "|";
                }
                paths = paths == string.Empty ? "" : (paths.Contains("|") ? paths.Remove(paths.LastIndexOf('|')) : paths);
                _docHelper.WriteData("header", "attach_src_path", paths);

                _docHelper.WriteData("content", "text", doc.DocText);
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("DocModel::CreateDoc >> exception occurs when write doc date: " + ex.Message);
                return -1;
            }

            // save attached files
            if (doc.AttachSourcePaths.Count > 0)
            {
                if (!Directory.Exists(_attachDir + "\\" + doc.DocTitle))
                {
                    Directory.CreateDirectory(_attachDir + "\\" + doc.DocTitle);
                }

                try
                {
                    for (int i = 0; i < doc.AttachSourcePaths.Count; i++)
                    {
                        File.Copy(doc.AttachSourcePaths[i], _attachDir + "\\" + doc.DocTitle + "\\" + Path.GetFileName(doc.AttachSourcePaths[i]));
                    }
                }
                catch (Exception ex)
                {
                    _logWriter.WriteErrorLog("DocModel::CreateDoc >> exception occurs when save attachments: " + ex.Message);
                    return -1;
                }
            }

            return 0;
        }

        /// <summary>
        /// Delete a doc
        /// </summary>
        /// <param name="doc">doc viewmodel instance</param>
        /// <returns></returns>
        public int DeleteDoc(DocViewModel doc)
        {
            try
            {
                // delete text file
                if (File.Exists(_docDir + "\\" + doc.DocTitle + ".fams"))
                {
                    File.Delete(_docDir + "\\" + doc.DocTitle + ".fams");
                }

                // delete attached files
                if (Directory.Exists(_attachDir + "\\" + doc.DocTitle))
                {
                    Directory.Delete(_attachDir + "\\" + doc.DocTitle, true);
                }
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("DocModel::DeleteDoc >> exception occurs when delete doc file: " + ex.Message);
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Update a doc
        /// </summary>
        /// <param name="doc">doc viewmodel instance</param>
        /// <returns></returns>
        public int UpdateDoc(DocViewModel doc)
        {
            if (doc.DocTitle != doc.FormerTitle)
            {
                _docHelper.Init(_docDir + "\\" + doc.FormerTitle + ".fams");
            }
            else
            {
                _docHelper.Init(_docDir + "\\" + doc.DocTitle + ".fams");
            }

            // Update text data
            try
            {
                _docHelper.WriteData("header", "title", doc.DocTitle);
                _docHelper.WriteData("header", "org_title", doc.OriginalTitle);
                _docHelper.WriteData("header", "source", doc.Source);
                _docHelper.WriteData("header", "category", doc.Category);
                _docHelper.WriteData("header", "author", doc.Author);
                _docHelper.WriteData("header", "create_time", doc.CreateTime);
                _docHelper.WriteData("header", "last_revised", doc.LastRevised);
                _docHelper.WriteData("header", "former_title", doc.DocTitle); // save the former title as current title

                string names = string.Empty;
                foreach (string name in doc.AttachFileNames)
                {
                    names += name + "|";
                }
                names = names == string.Empty ? "" : (names.Contains("|") ? names.Remove(names.LastIndexOf('|')) : names);
                _docHelper.WriteData("header", "attach_name", names);

                string paths = string.Empty;
                foreach (string path in doc.AttachSourcePaths)
                {
                    paths += path + "|";
                }
                paths = paths == string.Empty ? "" : (paths.Contains("|") ? paths.Remove(paths.LastIndexOf('|')) : paths);
                _docHelper.WriteData("header", "attach_src_path", paths);

                _docHelper.WriteData("content", "text", doc.DocText);
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("DocModel::UpdateDoc >> exception occurs when write doc data: " + ex.Message);
                return -1;
            }

            string title = string.Empty;
            if (doc.DocTitle != doc.FormerTitle)
            {
                title = doc.FormerTitle;
            }
            else
            {
                title = doc.DocTitle;
            }

            // Update attached files
            if (Directory.Exists(_attachDir + "\\" + title)) // if attached file folder exists, there must be attached files
            {
                if (doc.AttachFileNames.Count > 0) // update attached files
                {
                    string[] filePaths = Directory.GetFiles(_attachDir + "\\" + title, "*.*");
                    List<string> fileNames = new List<string>();

                    // delete attached files
                    foreach (string filePath in filePaths)
                    {
                        string name = Path.GetFileName(filePath);
                        if (!doc.AttachFileNames.Contains(name))
                        {
                            File.Delete(filePath);
                        }
                        else
                        {
                            fileNames.Add(name);
                        }
                    }

                    // add attached files
                    foreach (string name in doc.AttachFileNames)
                    {
                        if (!fileNames.Contains(name)) // if not exist, then add it
                        {
                            foreach (string path in doc.AttachSourcePaths)
                            {
                                if (path.Contains(name))
                                {
                                    File.Copy(path, _attachDir + "\\" + title + "\\" + name);
                                    break;
                                }
                            }
                        }
                    }
                }
                else // clear attached files
                {
                    try
                    {
                        Directory.Delete(_attachDir + "\\" + title, true);
                    }
                    catch (Exception ex)
                    {
                        _logWriter.WriteErrorLog("DocModel::UpdateDoc >> exception occurs when delete attachments: " + ex.Message);
                        return -1;
                    }
                }
            }
            else
            {
                if (doc.AttachFileNames.Count > 0) // add attached files
                {
                   Directory.CreateDirectory(_attachDir + "\\" + title);

                    try
                    {
                        for (int i = 0; i < doc.AttachSourcePaths.Count; i++)
                        {
                            File.Copy(doc.AttachSourcePaths[i], _attachDir + "\\" + title + "\\" + Path.GetFileName(doc.AttachSourcePaths[i]));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logWriter.WriteErrorLog("DocModel::UpdateDoc >> exception occurs when copy attachments: " + ex.Message);
                        return -1;
                    }
                }
            }
            
            // rename doc file & foler name (if the doc title is modified)
            try
            {
                if (doc.DocTitle != doc.FormerTitle)
                {
                    // rename file
                    File.Copy(_docDir + "\\" + doc.FormerTitle + ".fams", _docDir + "\\" + doc.DocTitle + ".fams");
                    File.Delete(_docDir + "\\" + doc.FormerTitle + ".fams");

                    // rename folder
                    if (Directory.Exists(_attachDir + "\\" + doc.FormerTitle))
                    {
                        RenameFolder(_attachDir + "\\" + doc.FormerTitle, _attachDir + "\\" + doc.DocTitle);
                    }
                }
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("DocModel::UpdateDoc >> exception occurs when rename doc file & folder: " + ex.Message);
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Export doc to a txt file
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public int Export2Txt(DocViewModel doc)
        {
            // Get output path name
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.ShowNewFolderButton = true;
            dlg.Description = "Export to:";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string txtFileName = dlg.SelectedPath + "\\" + (doc.DocTitle.StartsWith("*") ? doc.DocTitle.Remove(0, 1) : doc.DocTitle) + ".txt";
                if (File.Exists(txtFileName))
                {
                    System.Windows.MessageBoxResult option = System.Windows.MessageBox.Show("File already exists! Replace it?", "Question",
                    System.Windows.MessageBoxButton.YesNoCancel, System.Windows.MessageBoxImage.Question, System.Windows.MessageBoxResult.Cancel);

                    if (option == System.Windows.MessageBoxResult.Cancel)
                    {
                        return 0;
                    }
                    else if (option == System.Windows.MessageBoxResult.No)
                    {
                        int num = 2;
                        txtFileName = txtFileName.Substring(0, txtFileName.Length - 4) + " (" + num.ToString() + ").txt";
                        while (File.Exists(txtFileName))
                        {
                            ++num;
                            txtFileName = num < 10 ? txtFileName.Substring(0, txtFileName.Length - 6) : txtFileName.Substring(0, txtFileName.Length - 7);
                            txtFileName += num.ToString() + ").txt";
                        }
                    }
                }

                // Export data
                if (WriteTxt(txtFileName, FileMode.Create, doc.DocText) < 0)
                {
                    return -1;
                }
            }

            return 0;
        }
        #endregion model functions

        #region base functions
        /// <summary>
        /// Rename folder.
        /// </summary>
        /// <param name="orgDirName">original directory path</param>
        /// <param name="newDirName">new  directory path</param>
        /// <history time="2018/02/26">create this method</history>
        private void RenameFolder(string orgDirName, string newDirName)
        {
            if (!Directory.Exists(newDirName))
            {
                Directory.CreateDirectory(newDirName);
            }

            // Move files to the new folder.
            DirectoryInfo di = new DirectoryInfo(orgDirName);
            foreach (FileInfo file in di.GetFiles("*.*")) // *.*: files of all pattern, *.txt: file of pattern of .txt.
            {
                file.MoveTo(newDirName + "\\" + file.Name);
            }
            Directory.Delete(orgDirName); // Delete the old folder.
        }

        /// <summary>
        /// Write text
        /// </summary>
        /// <param name="txtFileName">full path name of the specified text file</param>
        /// <param name="mode">open mode</param>
        /// <param name="txtString">content string to be written</param>
        /// <returns>0-succeeded, others-failed</returns>
        public int WriteTxt(string txtFileName, FileMode mode, string txtString)
        {
            FileStream fs = new FileStream(txtFileName, mode);
            StreamWriter sw = new StreamWriter(fs);
            try
            {
                sw.Write(txtString);
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("AccountsModel::WriteTxt >> " + ex.Message);
                sw.Flush();
                sw.Close();
                fs.Close();
                return -1;
            }
            sw.Flush(); // Clear all buffers for the current writer.
            sw.Close(); // Close the current StreamWriter object.
            fs.Close(); // Close the current stream and release any resources.

            return 0;
        }
        #endregion base functions
    }
}
