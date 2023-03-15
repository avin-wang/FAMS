using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using SysWinForm = System.Windows.Forms;
using FAMS.Commons.BaseClasses;
using FAMS.Services;
using FAMS.ViewModels.Accounts;

namespace FAMS.Views.Accounts
{
    /// <summary>
    /// Interaction logic for EditWin.xaml
    /// </summary>
    public partial class EditWin : PopupWindowBase
    {
        private CFamsFileHelper _ffHelper = new CFamsFileHelper();
        private CLogWriter _logWriter = CLogWriter.GetInstance();

        private AccountViewModel _dataContext = null;

        private string _orgAccountName = null;
        private string _orgAttachmentFlag = null;
        private string _orgAttachedFileNames = null;
        private string _attachDir = string.Empty; // account attached file directory

        public EditWin()
        {
            InitializeComponent();
            InitializeContext();
        }


        /// <summary>
        /// Global initialization
        /// </summary>
        private void InitializeContext()
        {
            // Initialize *.fams file helper (fix system config file path)
            _ffHelper.Init(Directory.GetCurrentDirectory() + "\\config\\system.fams");

            // Get log file info
            string logDir = Directory.GetCurrentDirectory() + _ffHelper.GetData("config", "log_dir");
            string logName = _ffHelper.GetData("config", "log_name");

            // Initialize log writer
            if (!System.IO.Directory.Exists(logDir))
            {
                System.IO.Directory.CreateDirectory(logDir);
            }
            _logWriter.Init(logDir + "\\", logName);
            _logWriter.SetCurrLevel(5);

            // Get attached file directory
            _attachDir = Directory.GetCurrentDirectory() + _ffHelper.GetData("config", "acc_attach_dir");

            // Event handler registration.
            this.Loaded += EditWin_Loaded;
            this.KeyDown += EditWin_KeyDown;
        }

        /// <summary>
        /// Initialize data context.
        /// </summary>
        private void EditWin_Loaded(object sender, RoutedEventArgs e)
        {
            _dataContext = this.DataContext as AccountViewModel;
            _orgAccountName = _dataContext.AccountName;
            _orgAttachmentFlag = _dataContext.AttachmentFlag;
            _orgAttachedFileNames = _dataContext.AttachedFileNames;
            LoadAttachedFileList(); // Load attached files list
        }

        /// <summary>
        /// Load attached file list
        /// </summary>
        /// <history time="2018/02/27">create this method</history>
        private void LoadAttachedFileList()
        {
            List<string> attachedFileNames = GetAttachedFileNames();
            if (attachedFileNames == null)
            {
                this.attachedFileList.ItemsSource = null;
                return;
            }

            List<ListBoxItem> itemList = new List<ListBoxItem>();
            foreach (string fileName in attachedFileNames)
            {
                // Get icon
                Image icon = new Image();
                icon.Width = 30;
                icon.Height = 30;
                try
                {
                    icon.Source = GetImageSource(System.Drawing.Icon.ExtractAssociatedIcon(fileName).ToBitmap());
                }
                catch (Exception ex)
                {
                    _logWriter.WriteErrorLog("EditWin::LoadAttachedFileList >> " + ex.Message);
                    return;
                }

                // Get file name
                TextBlock name = new TextBlock();
                name.FontSize = 11;
                name.HorizontalAlignment = HorizontalAlignment.Center;
                string safeFileName = Path.GetFileName(fileName);
                if (safeFileName.LastIndexOf('.') > 6)
                {
                    safeFileName = safeFileName.Substring(0, 6) + "...";
                }
                name.Text = safeFileName;

                StackPanel panel = new StackPanel();
                panel.Children.Add(icon);
                panel.Children.Add(name);

                ListBoxItem item = new ListBoxItem();
                item.BorderThickness = new Thickness(0);
                item.ToolTip = new ToolTip()
                {
                    BorderThickness = new Thickness(0),
                    Content = Path.GetFileName(fileName)
                };
                item.Content = panel;
                itemList.Add(item);
            }
            this.attachedFileList.ItemsSource = itemList;
        }

        /// <summary>
        /// Get attached file names
        /// </summary>
        /// <history time="2018/02/27">create this method</history>
        /// <returns>attached file names list</returns>
        private List<string> GetAttachedFileNames()
        {
            List<string> fileNameList = new List<string>();

            // No attached files
            if (_dataContext.AttachedFileNames == null || _dataContext.AttachedFileNames == "")
            {
                return null;
            }

            // There exist attached files
            char[] separators = { '|', '@' };
            string[] fileNames = _dataContext.AttachedFileNames.Split(separators);
            foreach (string fileName in fileNames)
            {
                if (fileName != "")
                {
                    string filePath = _attachDir + "\\" + _orgAccountName + "\\" + fileName;
                    fileNameList.Add(filePath);
                }
            }

            return fileNameList;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        private BitmapSource GetImageSource(System.Drawing.Bitmap bitmap)
        {
            IntPtr ptr = bitmap.GetHbitmap();
            BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(ptr); // Never forget to release resources!

            return bs;
        }

        /// <summary>
        /// Click button of Save
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Regularize attached file name string with separator '|'
            if (_dataContext.AttachedFileNames == null || _dataContext.AttachedFileNames == "")
            {
                _dataContext.AttachmentFlag = "无";
            }
            else if (_dataContext.AttachedFileNames.Contains("@"))
            {
                char[] separators = { '|', '@' };
                string[] fileNames = _dataContext.AttachedFileNames.Split(separators);
                _dataContext.AttachedFileNames = "";
                foreach (string fileName in fileNames)
                {
                    if (fileName != "")
                    {
                        _dataContext.AttachedFileNames += "|" + fileName;
                    }
                }
                _dataContext.AttachedFileNames = _dataContext.AttachedFileNames.Substring(1, _dataContext.AttachedFileNames.Length - 1);
            }

            // Delete the temporary folder "deleted_files" if it exists
            if (Directory.Exists(_attachDir + "\\" + _orgAccountName + "\\deleted_files"))
            {
                Directory.Delete(_attachDir + "\\" + _orgAccountName + "\\deleted_files", true);
            }

            // If the account name was modified
            if (_dataContext.AccountName != _orgAccountName)
            {
                // Save the original account name to the former account names list
                if (_dataContext.FormerAccountNames != null && _dataContext.FormerAccountNames != "")
                {
                    _dataContext.FormerAccountNames += "|" + _orgAccountName;
                }
                else
                {
                    _dataContext.FormerAccountNames = _orgAccountName;
                }

                // Rename the attached file folder (only when the folder exists)
                if (Directory.Exists(_attachDir + "\\" + _orgAccountName))
                {
                    RenameFolder(_attachDir + "\\" + _orgAccountName, _attachDir + "\\" + _dataContext.AccountName);
                }
            }

            // Delete the corresponding attached file folder if no files attached
            if (_orgAttachmentFlag == "有" && _dataContext.AttachmentFlag == "无")
            {
                if (Directory.Exists(_attachDir + "\\" + _dataContext.AccountName))
                {
                    Directory.Delete(_attachDir + "\\" + _dataContext.AccountName, true);
                }
            }

            _dataContext.LastRevised = DateTime.Now.ToShortDateString(); // update revision date

            this.DialogResult = true;
        }

        /// <summary>
        /// Rename folder
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

            // Move files to the new folder
            DirectoryInfo di = new DirectoryInfo(orgDirName);
            foreach (FileInfo file in di.GetFiles("*.*")) // *.*: files of all pattern, *.txt: file of pattern of .txt
            {
                file.MoveTo(newDirName + "\\" + file.Name);
            }
            Directory.Delete(orgDirName, true); // Delete the old folder
        }

        /// <summary>
        /// Click button of Cancel
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Delete newly-attached files
            if (_orgAttachedFileNames == null || _orgAttachedFileNames == "") // Delete all
            {
                if (_dataContext.AttachedFileNames != null)
                {
                    string[] fileNames = _dataContext.AttachedFileNames.Split('@');
                    foreach (string fileName in fileNames)
                    {
                        if (File.Exists(_attachDir + "\\" + _orgAccountName + "\\" + fileName))
                        {
                            File.Delete(_attachDir + "\\" + _orgAccountName + "\\" + fileName);
                        }
                    }
                }
                
            }
            else // Delete parts
            {
                string[] fileNames = _dataContext.AttachedFileNames.Split('@');
                for (int i = 1; i < fileNames.Length; ++i)
                {
                    if (File.Exists(_attachDir + "\\" + _orgAccountName + "\\" + fileNames[i]))
                    {
                        File.Delete(_attachDir + "\\" + _orgAccountName + "\\" + fileNames[i]);
                    }
                }
            }

            // Get back original attachments
            if (_orgAttachedFileNames != null)
            {
                char[] separators = { '|', '@' };
                List<string> oldFileList = _orgAttachedFileNames.Split(separators[0]).ToList<string>();
                List<string> newFileList = _dataContext.AttachedFileNames.Split(separators).ToList<string>();
                foreach (string fileName in oldFileList)
                {
                    if (!newFileList.Contains(fileName))
                    {
                        File.Copy(_attachDir + "\\" + _orgAccountName + "\\deleted_files\\" + fileName,
                            _attachDir + "\\" + _orgAccountName + "\\" + fileName);
                    }
                }
                if (Directory.Exists(_attachDir + "\\" + _orgAccountName + "\\deleted_files"))
                {
                    Directory.Delete(_attachDir + "\\" + _orgAccountName + "\\deleted_files", true);
                }
            }
            _dataContext.AttachedFileNames = _orgAttachedFileNames;

            // Delete the attachment folder if no attachments exist originally
            if (_orgAttachmentFlag == "有" && _dataContext.AttachmentFlag == "无")
            {
                _dataContext.AttachmentFlag = "有";
            }
            else if (_orgAttachmentFlag == "无" && _dataContext.AttachmentFlag == "有")
            {
                if (Directory.Exists(_attachDir + "\\" + _orgAccountName))
                {
                    Directory.Delete(_attachDir + "\\" + _orgAccountName, true);
                }
                _dataContext.AttachmentFlag = "无";
            }

            // Recover account name if modified
            if (_dataContext.AccountName != _orgAccountName)
            {
                _dataContext.AccountName = _orgAccountName;
            }

            this.DialogResult = false;
        }

        /// <summary>
        /// Shortcuts
        /// </summary>
        private void EditWin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSave_Click(sender, e);
            }
            else if (e.Key == Key.Escape)
            {
                BtnCancel_Click(sender, e);
            }
        }

        /// <summary>
        /// Update property "AccountName" immediately once it was changed
        /// </summary>
        /// <history time="2018/04/13">create this method</history>
        private void TbxAccountName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_dataContext != null)
            {
                _dataContext.AccountName = (sender as TextBox).Text;
            }
        }

        /// <summary>
        /// View attached files in Explorer
        /// </summary>
        /// <history time="2018/02/27">create this method</history>
        private void btnViewAttachedFiles_Click(object sender, RoutedEventArgs e)
        {
            // Open export directory in Explorer
            string dir = _attachDir + "\\" + _orgAccountName;
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", dir);
            }
            catch (Exception ex)
            {
                _logWriter.WriteWarningLog("EditWin::btnViewAttachedFiles_Click >> Exception occurs when open the attached files folder: "
                    + ex.Message);
            }
        }

        /// <summary>
        /// Delete the selected attached files
        /// </summary>
        /// <history time="2018/02/27">create this method</history>
        private void btnDeleteAttachedFiles_Click(object sender, RoutedEventArgs e)
        {
            if (this.attachedFileList.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("已选择" + this.attachedFileList.SelectedItems.Count + "个文件，确定删除吗？", "",
                    MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                {
                    foreach (ListBoxItem item in this.attachedFileList.SelectedItems)
                    {
                        string fileName = (item.ToolTip as ToolTip).Content.ToString();
                        RemoveAttachedFile(fileName); // Remove attached file's name
                        // Remove corresponding local file to a temporary folder named "delete_files"
                        if (!Directory.Exists(_attachDir + "\\" + _orgAccountName + "\\deleted_files"))
                        {
                            Directory.CreateDirectory(_attachDir + "\\" + _orgAccountName + "\\deleted_files");
                        }
                        if (!File.Exists(_attachDir + "\\" + _orgAccountName + "\\deleted_files\\" + fileName))
                        {
                            File.Copy(_attachDir + "\\" + _orgAccountName + "\\" + fileName,
                                _attachDir + "\\" + _orgAccountName + "\\deleted_files\\" + fileName);
                            File.Delete(_attachDir + "\\" + _orgAccountName + "\\" + fileName);
                        }
                    }
                    LoadAttachedFileList(); // Reload attached file list
                }
            }
            else
            {
                MessageBox.Show("请选择至少一个文件！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Remove file name from the attached file names' list
        /// </summary>
        /// <param name="file">safe file name to be removed</param>
        /// <history time="2018/02/27">create this method</history>
        private void RemoveAttachedFile(string file)
        {
            // if only one file exists
            if (!_dataContext.AttachedFileNames.Contains("@")
                && !_dataContext.AttachedFileNames.Contains("|")
                && _dataContext.AttachedFileNames == file)
            {
                _dataContext.AttachedFileNames = "";
                return;
            }

            // if only new files exist
            if (_dataContext.AttachedFileNames.Contains("@") && !_dataContext.AttachedFileNames.Contains("|"))
            {
                List<string> fileList = _dataContext.AttachedFileNames.Split('@').ToList<string>();
                if (fileList.Contains(file))
                {
                    if (file == fileList[0])
                    {
                        fileList.Remove(file);
                        _dataContext.AttachedFileNames = "";
                        foreach (string item in fileList)
                        {
                            _dataContext.AttachedFileNames += "@" + item;
                        }
                    }
                    else
                    {
                        fileList.Remove(file);
                        _dataContext.AttachedFileNames = "";
                        foreach (string item in fileList)
                        {
                            _dataContext.AttachedFileNames += "@" + item;
                        }
                        _dataContext.AttachedFileNames = _dataContext.AttachedFileNames.Substring(1, _dataContext.AttachedFileNames.Length - 1);
                    }
                    return;
                }
            }

            // if only old files exist
            if (!_dataContext.AttachedFileNames.Contains("@") && _dataContext.AttachedFileNames.Contains("|"))
            {
                List<string> fileList = _dataContext.AttachedFileNames.Split('|').ToList<string>();
                if (fileList.Contains(file))
                {
                    fileList.Remove(file);
                    _dataContext.AttachedFileNames = "";
                    foreach (string item in fileList)
                    {
                        _dataContext.AttachedFileNames += item + "|";
                    }
                    _dataContext.AttachedFileNames = _dataContext.AttachedFileNames.Substring(0, _dataContext.AttachedFileNames.Length - 1);
                    return;
                }
            }

            /* If both new and old files exist, go downward...*/

            // Remove from new files
            List<string> newFileList = _dataContext.AttachedFileNames.Split('@').ToList<string>();
            if (newFileList.Contains(file))
            {
                newFileList.Remove(file);
                _dataContext.AttachedFileNames = "";
                foreach (string item in newFileList)
                {
                    _dataContext.AttachedFileNames += item + "@";
                }
                _dataContext.AttachedFileNames = _dataContext.AttachedFileNames.Substring(0, _dataContext.AttachedFileNames.Length - 1);
                return;
            }

            // Remove from old files
            List<string> oldFileList = newFileList[0].Split('|').ToList<string>();
            if (oldFileList.Contains(file))
            {
                oldFileList.Remove(file);
                _dataContext.AttachedFileNames = "";
                foreach (string item in oldFileList)
                {
                    _dataContext.AttachedFileNames += item + "|";
                }
                _dataContext.AttachedFileNames = _dataContext.AttachedFileNames.Substring(0, _dataContext.AttachedFileNames.Length - 1);
                for (int i = 1; i < newFileList.Count; ++i)
                {
                    _dataContext.AttachedFileNames += "@" + newFileList[i];
                }
            }
        }

        /// <summary>
        /// Add attached files
        /// </summary>
        private void btnAddAttachment_Click(object sender, RoutedEventArgs e)
        {
            SysWinForm.OpenFileDialog ofd = new SysWinForm.OpenFileDialog();
            ofd.Filter = "文件(*.png;*.bmp;*.jpg)|*.png;*.bmp;*.jpg|全部文件(*.*)|*.*";
            ofd.FilterIndex = 0;
            ofd.Multiselect = true;
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == SysWinForm.DialogResult.OK)
            {
                foreach (string fileName in ofd.FileNames)
                {
                    string safeFileName = Path.GetFileName(fileName);

                    if (!Directory.Exists(_attachDir + "\\" + _orgAccountName))
                    {
                        Directory.CreateDirectory(_attachDir + "\\" + _orgAccountName);
                    }
                    if (!File.Exists(_attachDir + "\\" + _orgAccountName + "\\" + safeFileName))
                    {
                        File.Copy(fileName, _attachDir + "\\" + _orgAccountName + "\\" + safeFileName);
                        _dataContext.AttachedFileNames += "@" + safeFileName;
                    }
                }
                _dataContext.AttachmentFlag = "有";
                LoadAttachedFileList(); // Reload attached file list
            }
        }

        /// <summary>
        /// Add key word button click event handler
        /// </summary>
        /// <history time="2020/03/04">create this method</history>
        private void BtnAddKeyWord_Click(object sender, RoutedEventArgs e)
        {
            if (this.tbxKeyWord.Text != "")
            {
                if (this.tbxKeyWordStr.Text == null || this.tbxKeyWordStr.Text == "")
                {
                    this.tbxKeyWordStr.Text = this.tbxKeyWord.Text;
                }
                else
                {
                    this.tbxKeyWordStr.Text += ";\r\n" + this.tbxKeyWord.Text;
                }

                this.tbxKeyWord.Text = "";
                this.tbxKeyWord.Focus();
            }
        }

        /// <summary>
        /// Delete key word button click event handler
        /// </summary>
        /// <history time="2020/03/04">create this method</history>
        private void BtnDeleteKeyWord_Click(object sender, RoutedEventArgs e)
        {
            if (this.tbxKeyWordStr.Text.Contains(";"))
            {
                this.tbxKeyWordStr.Text = this.tbxKeyWordStr.Text.Remove(this.tbxKeyWordStr.Text.LastIndexOf(';'));
            }
            else
            {
                this.tbxKeyWordStr.Text = "";
            }
        }
    }
}
