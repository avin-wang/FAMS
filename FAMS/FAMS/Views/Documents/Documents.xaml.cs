using FAMS.Models.Documents;
using FAMS.Services;
using FAMS.ViewModels.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SysWinForm = System.Windows.Forms;

namespace FAMS.Views.Documents
{
    /// <summary>
    /// Interaction logic for Documents.xaml
    /// </summary>
    public partial class Documents : Page
    {
        private CFamsFileHelper _ffHelper = new CFamsFileHelper(); // config file access
        private CLogWriter _logWriter = CLogWriter.GetInstance();

        // Update status info.
        public UpdateStatusInfoHandler _hUpdateStatusInfoHandler;
        public delegate void UpdateStatusInfoHandler(string infoStr);

        private string _attachDir = string.Empty;
        private List<string> _paths = new List<string>(); // new added attached file full paths
        private DocViewModel _backup = new DocViewModel(); // record original doc
        private int _index = 0; // record last selected doc's index in the doc list

        private DocModel _model = new DocModel();

        public Documents()
        {
            InitializeComponent();
            InitializeContext();
        }

        private void InitializeContext()
        {
            // Initialize config file helper
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

            // get dir
            _attachDir = Directory.GetCurrentDirectory() + _ffHelper.GetData("config", "doc_attach_dir");

            // Load data (data binding).
            this.lbxDocTitle.ItemsSource = _model.LoadDoc();

            // Update status info.
            if (_hUpdateStatusInfoHandler != null)
            {
                _hUpdateStatusInfoHandler("总共：" + (this.lbxDocTitle.ItemsSource as List<DocViewModel>).Count);
            }

            // Event handlers registration
            this.lbxDocTitle.SelectionChanged += LbxDocTitle_SelectionChanged;
            this.btnSearchDoc.Click += BtnSearchDoc_Click;
            this.tbxSearchKeyWord.KeyDown += TbxSearchKeyWord_KeyDown;
            this.tbxSearchKeyWord.TextChanged += TbxSearchKeyWord_TextChanged;
            this.tbxDocTitle.MouseDoubleClick += Tbx_MouseDoubleClick;
            this.tbxSource.MouseDoubleClick += Tbx_MouseDoubleClick;
            this.tbxOriginalTitle.MouseDoubleClick += Tbx_MouseDoubleClick;
            this.tbxCategory.MouseDoubleClick += Tbx_MouseDoubleClick;
            this.tbxAuthor.MouseDoubleClick += Tbx_MouseDoubleClick;
            this.tbxDocText.MouseDoubleClick += Tbx_MouseDoubleClick;
            this.tbxDocTitle.KeyDown += Tbx_KeyDown;
            this.tbxSource.KeyDown += Tbx_KeyDown;
            this.tbxOriginalTitle.KeyDown += Tbx_KeyDown;
            this.tbxCategory.KeyDown += Tbx_KeyDown;
            this.tbxAuthor.KeyDown += Tbx_KeyDown;
            this.tbxDocText.KeyDown += Tbx_KeyDown;
        }

        private void Tbx_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Keyboard.IsKeyDown(Key.S))
            {
                BtnSaveDoc_Click(sender, e); // save doc
            }
        }

        private void TbxSearchKeyWord_TextChanged(object sender, TextChangedEventArgs e)
        {
            // if the doc is editable state?
            if (!this.tbxDocTitle.IsReadOnly || !this.tbxSource.IsReadOnly || !this.tbxOriginalTitle.IsReadOnly
                || !this.tbxCategory.IsReadOnly || !this.tbxAuthor.IsReadOnly || !this.tbxDocText.IsReadOnly)
            {
                if (this.grdDocInfo.DataContext != null && (this.grdDocInfo.DataContext as DocViewModel).DocTitle.StartsWith("*"))
                {
                    MessageBox.Show("当前编辑的文档未保存，请先保存！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else
                {
                    // disable the editing state
                    this.tbxDocTitle.IsReadOnly = true;
                    this.tbxSource.IsReadOnly = true;
                    this.tbxOriginalTitle.IsReadOnly = true;
                    this.tbxCategory.IsReadOnly = true;
                    this.tbxAuthor.IsReadOnly = true;
                    this.tbxDocText.IsReadOnly = true;
                }
            }

            if (this.tbxSearchKeyWord.Text == null || this.tbxSearchKeyWord.Text == "")
            {
                Refresh();
                return;
            }
        }

        private void TbxSearchKeyWord_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                // if the doc is editable state?
                if (!this.tbxDocTitle.IsReadOnly || !this.tbxSource.IsReadOnly || !this.tbxOriginalTitle.IsReadOnly
                    || !this.tbxCategory.IsReadOnly || !this.tbxAuthor.IsReadOnly || !this.tbxDocText.IsReadOnly)
                {
                    if (this.grdDocInfo.DataContext != null && (this.grdDocInfo.DataContext as DocViewModel).DocTitle.StartsWith("*"))
                    {
                        MessageBox.Show("当前编辑的文档未保存，请先保存！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    else
                    {
                        // disable the editing state
                        this.tbxDocTitle.IsReadOnly = true;
                        this.tbxSource.IsReadOnly = true;
                        this.tbxOriginalTitle.IsReadOnly = true;
                        this.tbxCategory.IsReadOnly = true;
                        this.tbxAuthor.IsReadOnly = true;
                        this.tbxDocText.IsReadOnly = true;
                    }
                }

                string keyWord = this.tbxSearchKeyWord.Text.ToLower();
                if (keyWord == null || keyWord == "")
                {
                    Refresh();
                    return;
                }

                List<DocViewModel> docList = this.lbxDocTitle.ItemsSource as List<DocViewModel>;
                if (docList != null && docList.Count > 0)
                {
                    List<DocViewModel> resultList = new List<DocViewModel>();
                    foreach (DocViewModel doc in docList)
                    {
                        if (doc.DocTitle.ToLower().Contains(keyWord))
                        {
                            resultList.Add(doc);
                        }
                    }

                    // reset global variables
                    _paths.Clear();

                    // reset doc editing area data context
                    this.grdDocInfo.DataContext = null;
                    this.tbxDocText.DataContext = null;
                    if (this.lbxAttachFileList.ItemsSource != null)
                    {
                        (this.lbxAttachFileList.ItemsSource as List<ListBoxItem>).Clear();
                        this.lbxAttachFileList.ItemsSource = null;
                    }
                    this.tbxFileCount.Text = "";

                    // reset doc title list
                    if (this.lbxDocTitle.ItemsSource != null)
                    {
                        (this.lbxDocTitle.ItemsSource as List<DocViewModel>).Clear();
                        this.lbxDocTitle.ItemsSource = null;
                    }

                    this.lbxDocTitle.ItemsSource = resultList;

                    // Update status info
                    if (_hUpdateStatusInfoHandler != null)
                    {
                        _hUpdateStatusInfoHandler("总共：" + (this.lbxDocTitle.ItemsSource as List<DocViewModel>).Count);
                    }
                }
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.tbxSearchKeyWord.Text = "";
            }
        }

        private void BtnSearchDoc_Click(object sender, RoutedEventArgs e)
        {
            // if the doc is editable state?
            if (!this.tbxDocTitle.IsReadOnly || !this.tbxSource.IsReadOnly || !this.tbxOriginalTitle.IsReadOnly
                || !this.tbxCategory.IsReadOnly || !this.tbxAuthor.IsReadOnly || !this.tbxDocText.IsReadOnly)
            {
                if (this.grdDocInfo.DataContext != null && (this.grdDocInfo.DataContext as DocViewModel).DocTitle.StartsWith("*"))
                {
                    MessageBox.Show("当前编辑的文档未保存，请先保存！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else
                {
                    // disable the editing state
                    this.tbxDocTitle.IsReadOnly = true;
                    this.tbxSource.IsReadOnly = true;
                    this.tbxOriginalTitle.IsReadOnly = true;
                    this.tbxCategory.IsReadOnly = true;
                    this.tbxAuthor.IsReadOnly = true;
                    this.tbxDocText.IsReadOnly = true;
                }
            }

            string keyWord = this.tbxSearchKeyWord.Text.ToLower();

            if (keyWord == null || keyWord == "")
            {
                Refresh();
                return;
            }

            List<DocViewModel> docList = this.lbxDocTitle.ItemsSource as List<DocViewModel>;
            if (docList != null && docList.Count > 0)
            {
                List<DocViewModel> resultList = new List<DocViewModel>();
                foreach (DocViewModel doc in docList)
                {
                    if (doc.DocTitle.ToLower().Contains(keyWord))
                    {
                        resultList.Add(doc);
                    }
                }

                // reset global variables
                _paths.Clear();

                // reset doc editing area data context
                this.grdDocInfo.DataContext = null;
                this.tbxDocText.DataContext = null;
                if (this.lbxAttachFileList.ItemsSource != null)
                {
                    (this.lbxAttachFileList.ItemsSource as List<ListBoxItem>).Clear();
                    this.lbxAttachFileList.ItemsSource = null;
                }
                this.tbxFileCount.Text = "";

                // reset doc title list
                if (this.lbxDocTitle.ItemsSource != null)
                {
                    (this.lbxDocTitle.ItemsSource as List<DocViewModel>).Clear();
                    this.lbxDocTitle.ItemsSource = null;
                }

                this.lbxDocTitle.ItemsSource = resultList;

                // Update status info.
                if (_hUpdateStatusInfoHandler != null)
                {
                    _hUpdateStatusInfoHandler("总共：" + (this.lbxDocTitle.ItemsSource as List<DocViewModel>).Count);
                }
            }
        }

        private void LbxDocTitle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.lbxDocTitle.ItemsSource == null || this.lbxDocTitle.SelectedItems == null)
            {
                return;
            }

            // if the doc is editable state?
            if (!this.tbxDocTitle.IsReadOnly || !this.tbxSource.IsReadOnly || !this.tbxOriginalTitle.IsReadOnly
                || !this.tbxCategory.IsReadOnly || !this.tbxAuthor.IsReadOnly || !this.tbxDocText.IsReadOnly)
            {
                if (this.grdDocInfo.DataContext != null && (this.grdDocInfo.DataContext as DocViewModel).DocTitle.StartsWith("*"))
                {
                    MessageBoxResult res = MessageBox.Show("当前编辑的文档未保存，要保存吗？", "", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                    if (res == MessageBoxResult.Yes) // save currently edited doc
                    {
                        BtnSaveDoc_Click(sender, e);
                    }
                    else if (res == MessageBoxResult.No) // recover currently edited doc
                    {
                        List<DocViewModel> docList = this.lbxDocTitle.ItemsSource as List<DocViewModel>;

                        docList[_index].DocTitle = _backup.DocTitle;
                        docList[_index].OriginalTitle = _backup.OriginalTitle;
                        docList[_index].Source = _backup.Source;
                        docList[_index].Category = _backup.Category;
                        docList[_index].Author = _backup.Author;
                        docList[_index].CreateTime = _backup.CreateTime;
                        docList[_index].LastRevised = _backup.LastRevised;
                        docList[_index].FormerTitle = _backup.FormerTitle;

                        docList[_index].AttachFileNames.Clear();
                        foreach (string str in _backup.AttachFileNames)
                        {
                            docList[_index].AttachFileNames.Add(str);
                        }

                        docList[_index].AttachSourcePaths.Clear();
                        foreach (string str in _backup.AttachSourcePaths)
                        {
                            docList[_index].AttachSourcePaths.Add(str);
                        }

                        docList[_index].DocText = _backup.DocText;

                        // disable the editing state
                        this.tbxDocTitle.IsReadOnly = true;
                        this.tbxSource.IsReadOnly = true;
                        this.tbxOriginalTitle.IsReadOnly = true;
                        this.tbxCategory.IsReadOnly = true;
                        this.tbxAuthor.IsReadOnly = true;
                        this.tbxDocText.IsReadOnly = true;
                    }
                    else
                    {
                        return; // keep the doc unsaved
                    }
                }
                else
                {
                    // disable the editing state
                    this.tbxDocTitle.IsReadOnly = true;
                    this.tbxSource.IsReadOnly = true;
                    this.tbxOriginalTitle.IsReadOnly = true;
                    this.tbxCategory.IsReadOnly = true;
                    this.tbxAuthor.IsReadOnly = true;
                    this.tbxDocText.IsReadOnly = true;
                }
            }

            DocViewModel doc = (sender as ListBox).SelectedItems[0] as DocViewModel; // only show the first selected doc
            _index = (sender as ListBox).SelectedIndex;

            // record original doc data
            _backup.DocTitle = doc.DocTitle;
            _backup.OriginalTitle = doc.OriginalTitle;
            _backup.Source = doc.Source;
            _backup.Category = doc.Category;
            _backup.Author = doc.Author;
            _backup.CreateTime = doc.CreateTime;
            _backup.LastRevised = doc.LastRevised;
            _backup.FormerTitle = doc.FormerTitle;

            _backup.AttachFileNames.Clear();
            foreach (string str in doc.AttachFileNames)
            {
                _backup.AttachFileNames.Add(str);
            }

            _backup.AttachSourcePaths.Clear();
            foreach (string str in doc.AttachSourcePaths)
            {
                _backup.AttachSourcePaths.Add(str);
            }

            _backup.DocText = doc.DocText;

            // reset global variables
            _paths.Clear();

            // reset doc editing area data context
            this.grdDocInfo.DataContext = null;
            this.tbxDocText.DataContext = null;
            if (this.lbxAttachFileList.ItemsSource != null)
            {
                (this.lbxAttachFileList.ItemsSource as List<ListBoxItem>).Clear();
                this.lbxAttachFileList.ItemsSource = null;
            }
            this.tbxFileCount.Text = "";

            //ResetContext();
            this.grdDocInfo.DataContext = doc;
            this.tbxDocText.DataContext = doc;
            LoadAttachFileList(); // reload attached file list

            // Update status info.
            if (_hUpdateStatusInfoHandler != null)
            {
                _hUpdateStatusInfoHandler("总共：" + this.lbxDocTitle.Items.Count + "  选择：" + this.lbxDocTitle.SelectedItems.Count);
            }
        }

        /// <summary>
        /// Load attached file list
        /// </summary>
        /// <history time="2018/02/27">create this method</history>
        private void LoadAttachFileList()
        {
            DocViewModel doc = this.grdDocInfo.DataContext as DocViewModel;
            List<string> paths = new List<string>();

            string title = string.Empty;
            if (doc.DocTitle.StartsWith("*"))
            {
                title = doc.DocTitle.Substring(1, doc.DocTitle.Length - 1);
            }
            else
            {
                title = doc.DocTitle;
            }

            if (title != doc.FormerTitle)
            {
                title = doc.FormerTitle;
            }

            // get existing attached file paths
            foreach (string fileName in doc.AttachFileNames)
            {
                if (File.Exists(_attachDir + "\\" + title + "\\" + fileName))
                {
                    paths.Add(_attachDir + "\\" + title + "\\" + fileName);
                }
            }

            // get new added attached file paths
            foreach (string path in _paths)
            {
                paths.Add(path);
            }

            if (paths.Count == 0)
            {
                // clear
                if (this.lbxAttachFileList.ItemsSource != null)
                {
                    (this.lbxAttachFileList.ItemsSource as List<ListBoxItem>).Clear();
                    this.lbxAttachFileList.ItemsSource = null;
                }
                this.tbxFileCount.Text = "";
                return;
            }

            List<ListBoxItem> itemList = new List<ListBoxItem>();
            foreach (string fileName in paths)
            {
                // Get icon.
                Image icon = new Image();
                icon.Width = 30;
                icon.Height = 30;
                try
                {
                    icon.Source = GetImageSource(System.Drawing.Icon.ExtractAssociatedIcon(fileName).ToBitmap());
                }
                catch (Exception ex)
                {
                    _logWriter.WriteErrorLog("Documents::LoadAttachFileList >> " + ex.Message);
                    return;
                }

                // Get file name.
                TextBlock name = new TextBlock();
                name.FontSize = 11;
                name.HorizontalAlignment = HorizontalAlignment.Center;
                string safeFileName = System.IO.Path.GetFileName(fileName);
                name.Tag = safeFileName; // used to mark the file icon
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
                    Content = System.IO.Path.GetFileName(fileName)
                };
                item.Content = panel;
                itemList.Add(item);
            }

            // clear
            if (this.lbxAttachFileList.ItemsSource != null)
            {
                (this.lbxAttachFileList.ItemsSource as List<ListBoxItem>).Clear();
                this.lbxAttachFileList.ItemsSource = null;
            }

            this.lbxAttachFileList.ItemsSource = itemList;

            if (itemList.Count.ToString() != this.tbxFileCount.Text)
            {
                this.tbxFileCount.Text = itemList.Count.ToString();
            }
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

        private void BtnAddAttachFiles_Click(object sender, RoutedEventArgs e)
        {
            // if the doc is editable state?
            if (this.tbxDocTitle.IsReadOnly || this.tbxSource.IsReadOnly || this.tbxOriginalTitle.IsReadOnly
                || this.tbxCategory.IsReadOnly || this.tbxAuthor.IsReadOnly || this.tbxDocText.IsReadOnly)
            {
                return;
            }

            DocViewModel doc = (sender as Button).DataContext as DocViewModel;

            if (doc != null)
            {
                SysWinForm.OpenFileDialog ofd = new SysWinForm.OpenFileDialog();
                ofd.Filter = "文件(*.*)|*.*";
                ofd.FilterIndex = 0;
                ofd.Multiselect = true;
                ofd.RestoreDirectory = true;
                if (ofd.ShowDialog() == SysWinForm.DialogResult.OK)
                {
                    foreach (string fileName in ofd.FileNames)
                    {
                        if (!doc.AttachFileNames.Contains(System.IO.Path.GetFileName(fileName)))
                        {
                            doc.AttachFileNames.Add(System.IO.Path.GetFileName(fileName));
                        }

                        if (!doc.AttachSourcePaths.Contains(fileName))
                        {
                            doc.AttachSourcePaths.Add(fileName);
                            if (!_paths.Contains(fileName))
                            {
                                _paths.Add(fileName);
                            }
                        }
                    }

                    LoadAttachFileList(); // reload attached file list

                    // mark editing state
                    if (!doc.DocTitle.StartsWith("*"))
                    {
                        if (doc.AttachFileNames.Count != _backup.AttachFileNames.Count)
                        {
                            doc.DocTitle = "*" + doc.DocTitle;
                            return;
                        }

                        // I have your all, you have my all, then we are the same, no change is made
                        if (doc.AttachFileNames.Count > 0)
                        {
                            foreach (string fn in doc.AttachFileNames)
                            {
                                if (!_backup.AttachFileNames.Contains(fn))
                                {
                                    doc.DocTitle = "*" + doc.DocTitle;
                                    return;
                                }
                            }

                            foreach (string fn in _backup.AttachFileNames)
                            {
                                if (!doc.AttachFileNames.Contains(fn))
                                {
                                    doc.DocTitle = "*" + doc.DocTitle;
                                    return;
                                }
                            }
                        }

                        if (doc.AttachSourcePaths.Count != _backup.AttachSourcePaths.Count)
                        {
                            doc.DocTitle = "*" + doc.DocTitle;
                            return;
                        }

                        if (doc.AttachSourcePaths.Count > 0)
                        {
                            foreach (string fn in doc.AttachSourcePaths)
                            {
                                if (!_backup.AttachSourcePaths.Contains(fn))
                                {
                                    doc.DocTitle = "*" + doc.DocTitle;
                                    return;
                                }
                            }

                            foreach (string fn in _backup.AttachSourcePaths)
                            {
                                if (!doc.AttachSourcePaths.Contains(fn))
                                {
                                    doc.DocTitle = "*" + doc.DocTitle;
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void BtnSaveDoc_Click(object sender, RoutedEventArgs e)
        {
            // if the doc is editable state?
            if (this.tbxDocTitle.IsReadOnly || this.tbxSource.IsReadOnly || this.tbxOriginalTitle.IsReadOnly
                || this.tbxCategory.IsReadOnly || this.tbxAuthor.IsReadOnly || this.tbxDocText.IsReadOnly)
            {
                return;
            }

            DocViewModel doc = this.grdDocInfo.DataContext as DocViewModel;

            if (doc == null)
            {
                return;
            }

            if (doc.DocTitle.StartsWith("*"))
            {
                doc.DocTitle = doc.DocTitle.Substring(1, doc.DocTitle.Length - 1);
            }

            // if name duplication?
            foreach (DocViewModel d in this.lbxDocTitle.ItemsSource)
            {
                if (d.DocTitle != _backup.DocTitle && d.FormerTitle != _backup.FormerTitle && d.DocTitle == doc.DocTitle)
                {
                    MessageBox.Show("文档已存在！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    doc.DocTitle = "*" + doc.DocTitle; // mark unsaved state again

                    this.tbxDocTitle.Focus();
                    return;
                }
            }

            // check illegal characters
            if (doc.DocTitle.Contains('\\') || doc.DocTitle.Contains('/') || doc.DocTitle.Contains(':')
                || doc.DocTitle.Contains('*') || doc.DocTitle.Contains('?') || doc.DocTitle.Contains('"')
                || doc.DocTitle.Contains('<') || doc.DocTitle.Contains('>') || doc.DocTitle.Contains('|'))
            {
                MessageBox.Show("标题不能包含以下字符：\n\\/:*?\"<>|", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            doc.LastRevised = DateTime.Now.ToString();

            int ret = _model.UpdateDoc(doc);
            if (ret != 0)
            {
                MessageBox.Show("文档保存失败！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.tbxDocTitle.IsReadOnly = true;
            this.tbxSource.IsReadOnly = true;
            this.tbxOriginalTitle.IsReadOnly = true;
            this.tbxCategory.IsReadOnly = true;
            this.tbxAuthor.IsReadOnly = true;
            this.tbxDocText.IsReadOnly = true;

            string title = doc.DocTitle;

            // reset global variables
            _paths.Clear();

            // reset doc editing area data context
            this.grdDocInfo.DataContext = null;
            this.tbxDocText.DataContext = null;
            if (this.lbxAttachFileList.ItemsSource != null)
            {
                (this.lbxAttachFileList.ItemsSource as List<ListBoxItem>).Clear();
                this.lbxAttachFileList.ItemsSource = null;
            }
            this.tbxFileCount.Text = "";

            // reset doc title list
            if (this.lbxDocTitle.ItemsSource != null)
            {
                (this.lbxDocTitle.ItemsSource as List<DocViewModel>).Clear();
                this.lbxDocTitle.ItemsSource = null;
            }
            this.lbxDocTitle.ItemsSource = _model.LoadDoc(); // reload doc data

            // Update status info.
            if (_hUpdateStatusInfoHandler != null)
            {
                _hUpdateStatusInfoHandler("总共：" + (this.lbxDocTitle.ItemsSource as List<DocViewModel>).Count);
            }

            foreach (DocViewModel d in this.lbxDocTitle.ItemsSource)
            {
                if (d.DocTitle == title)
                {
                    this.lbxDocTitle.SelectedItem = d;
                    break;
                }
            }
        }

        /// <summary>
        /// Delete selected attached files
        /// </summary>
        private void MiDeleteAttachFiles_Click(object sender, RoutedEventArgs e)
        {
            // if the doc is editable state?
            if (this.tbxDocTitle.IsReadOnly || this.tbxSource.IsReadOnly || this.tbxOriginalTitle.IsReadOnly
                || this.tbxCategory.IsReadOnly || this.tbxAuthor.IsReadOnly || this.tbxDocText.IsReadOnly)
            {
                return;
            }

            if (this.lbxAttachFileList.SelectedItems.Count > 0)
            {
                MessageBoxResult ret = MessageBox.Show("已选择" + this.lbxAttachFileList.SelectedItems.Count + "个文件，确定永久删除吗？！",
                    "", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (ret == MessageBoxResult.Yes)
                {
                    DocViewModel doc = this.grdDocInfo.DataContext as DocViewModel;

                    foreach (ListBoxItem item in this.lbxAttachFileList.SelectedItems)
                    {
                        string fileName = (item.ToolTip as ToolTip).Content.ToString();
                        doc.AttachFileNames.Remove(fileName); // remove AttachFileNames
                        foreach (string path in doc.AttachSourcePaths) // remove AttachSourcePaths
                        {
                            if (path.Contains(fileName))
                            {
                                doc.AttachSourcePaths.Remove(path);
                                break;
                            }
                        }
                        foreach (string path in _paths) // remove path from current path global variable
                        {
                            if (path.Contains(fileName))
                            {
                                _paths.Remove(path);
                                break;
                            }
                        }
                    }

                    LoadAttachFileList(); // reload attached file list

                    if (doc.AttachFileNames.Count != _backup.AttachFileNames.Count)
                    {
                        if (!doc.DocTitle.StartsWith("*"))
                        {
                            doc.DocTitle = "*" + doc.DocTitle;
                        }
                        return;
                    }

                    if (doc.AttachFileNames.Count > 0)
                    {
                        foreach (string fn in doc.AttachFileNames)
                        {
                            if (!_backup.AttachFileNames.Contains(fn))
                            {
                                if (!doc.DocTitle.StartsWith("*"))
                                {
                                    doc.DocTitle = "*" + doc.DocTitle;
                                }
                                return;
                            }
                        }

                        foreach (string fn in _backup.AttachFileNames)
                        {
                            if (!doc.AttachFileNames.Contains(fn))
                            {
                                if (!doc.DocTitle.StartsWith("*"))
                                {
                                    doc.DocTitle = "*" + doc.DocTitle;
                                }
                                return;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Show the attached files in the File Explorer
        /// </summary>
        private void MiOpenExplorer_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem item = this.lbxAttachFileList.SelectedItems[0] as ListBoxItem; // only open the folder that contains the first file selected
            string fileName = ((item.Content as StackPanel).Children[1] as TextBlock).Tag.ToString(); // get the safe file name (i.e., name+extension)

            DocViewModel doc = this.grdDocInfo.DataContext as DocViewModel;
            foreach (string fn in doc.AttachFileNames)
            {
                if (File.Exists(_attachDir + "\\" + _backup.DocTitle + "\\" + fn))
                {
                    try
                    {
                        System.Diagnostics.Process.Start("explorer.exe", _attachDir + "\\" + _backup.DocTitle);
                    }
                    catch (Exception ex)
                    {
                        _logWriter.WriteWarningLog("Documents::MiOpenExplorer_Click >> exception occurs when open the attached file folder: " + ex.Message);
                    }

                    return;
                }
            }

            foreach (string path in doc.AttachSourcePaths)
            {
                if (path.Contains(fileName) && File.Exists(path))
                {
                    try
                    {
                        System.Diagnostics.Process.Start("explorer.exe", System.IO.Path.GetDirectoryName(path));
                    }
                    catch (Exception ex)
                    {
                        _logWriter.WriteWarningLog("Documents::MiOpenExplorer_Click >> exception occurs when open the attached file folder: " + ex.Message);
                    }

                    return;
                }
            }
        }

        #region doc modification detection
        private void TbxDocTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).IsReadOnly)
            {
                return;
            }

            string text = (sender as TextBox).Text;
            DocViewModel doc = (sender as TextBox).DataContext as DocViewModel;

            if (doc == null)
            {
                return;
            }

            if (text != null && text.StartsWith("*"))
            {
                if (text.Substring(1, text.Length - 1) == _backup.DocTitle)
                {
                    if (Equals((sender as TextBox).DataContext as DocViewModel, _backup))
                    {
                        (sender as TextBox).Text = _backup.DocTitle;
                        (sender as TextBox).SelectionStart = (sender as TextBox).Text.Length;
                    }
                }
            }
            else if (text != null && !text.StartsWith("*"))
            {
                if (text != _backup.DocTitle)
                {
                    (sender as TextBox).Text = "*" + text;
                    (sender as TextBox).SelectionStart = (sender as TextBox).Text.Length;
                }
            }
        }

        private void TbxSource_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).IsReadOnly)
            {
                return;
            }

            string text = (sender as TextBox).Text;
            DocViewModel doc = (sender as TextBox).DataContext as DocViewModel;

            if (doc == null)
            {
                return;
            }

            if (text != null && text != _backup.Source && !doc.DocTitle.StartsWith("*"))
            {
                doc.DocTitle = "*" + doc.DocTitle;
                return;
            }

            if (!Equals((sender as TextBox).DataContext as DocViewModel, _backup) && !doc.DocTitle.StartsWith("*"))
            {
                doc.DocTitle = "*" + doc.DocTitle;
            }
            else if (Equals((sender as TextBox).DataContext as DocViewModel, _backup) && doc.DocTitle.StartsWith("*"))
            {
                doc.DocTitle = doc.DocTitle.Substring(1, doc.DocTitle.Length - 1);
            }
        }

        private void TbxOriginalTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).IsReadOnly)
            {
                return;
            }

            string text = (sender as TextBox).Text;
            DocViewModel doc = (sender as TextBox).DataContext as DocViewModel;

            if (doc == null)
            {
                return;
            }

            if (text != null && text != _backup.OriginalTitle && !doc.DocTitle.StartsWith("*"))
            {
                doc.DocTitle = "*" + doc.DocTitle;
                return;
            }

            if (!Equals((sender as TextBox).DataContext as DocViewModel, _backup) && !doc.DocTitle.StartsWith("*"))
            {
                doc.DocTitle = "*" + doc.DocTitle;
            }
            else if (Equals((sender as TextBox).DataContext as DocViewModel, _backup) && doc.DocTitle.StartsWith("*"))
            {
                doc.DocTitle = doc.DocTitle.Substring(1, doc.DocTitle.Length - 1);
            }
        }

        private void TbxCategory_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).IsReadOnly)
            {
                return;
            }

            string text = (sender as TextBox).Text;
            DocViewModel doc = (sender as TextBox).DataContext as DocViewModel;

            if (doc == null)
            {
                return;
            }

            if (text != null && text != _backup.Category && !doc.DocTitle.StartsWith("*"))
            {
                doc.DocTitle = "*" + doc.DocTitle;
                return;
            }

            if (!Equals((sender as TextBox).DataContext as DocViewModel, _backup) && !doc.DocTitle.StartsWith("*"))
            {
                doc.DocTitle = "*" + doc.DocTitle;
            }
            else if (Equals((sender as TextBox).DataContext as DocViewModel, _backup) && doc.DocTitle.StartsWith("*"))
            {
                doc.DocTitle = doc.DocTitle.Substring(1, doc.DocTitle.Length - 1);
            }
        }

        private void TbxAuthor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).IsReadOnly)
            {
                return;
            }

            string text = (sender as TextBox).Text;
            DocViewModel doc = (sender as TextBox).DataContext as DocViewModel;

            if (doc == null)
            {
                return;
            }

            if (text != null && text != _backup.Author && !doc.DocTitle.StartsWith("*"))
            {
                doc.DocTitle = "*" + doc.DocTitle;
                return;
            }

            if (!Equals((sender as TextBox).DataContext as DocViewModel, _backup) && !doc.DocTitle.StartsWith("*"))
            {
                doc.DocTitle = "*" + doc.DocTitle;
            }
            else if (Equals((sender as TextBox).DataContext as DocViewModel, _backup) && doc.DocTitle.StartsWith("*"))
            {
                doc.DocTitle = doc.DocTitle.Substring(1, doc.DocTitle.Length - 1);
            }
        }

        private void TbxDocText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).IsReadOnly)
            {
                return;
            }

            string text = (sender as TextBox).Text;
            DocViewModel doc = (sender as TextBox).DataContext as DocViewModel;

            if (doc == null)
            {
                return;
            }

            if (text != null && text != _backup.DocText && !doc.DocTitle.StartsWith("*"))
            {
                doc.DocTitle = "*" + doc.DocTitle;
                return;
            }

            if (!Equals((sender as TextBox).DataContext as DocViewModel, _backup) && !doc.DocTitle.StartsWith("*"))
            {
                doc.DocTitle = "*" + doc.DocTitle;
            }
            else if (Equals((sender as TextBox).DataContext as DocViewModel, _backup) && doc.DocTitle.StartsWith("*"))
            {
                doc.DocTitle = doc.DocTitle.Substring(1, doc.DocTitle.Length - 1);
            }
        }

        /// <summary>
        /// Compare two doc to determine if they are equal
        /// </summary>
        /// <param name="doc1"></param>
        /// <param name="doc2"></param>
        /// <returns></returns>
        private bool Equals(DocViewModel doc1, DocViewModel doc2)
        {
            if ((doc1 == null && doc2 != null) || (doc1 != null && doc2 == null))
            {
                return false;
            }

            if (doc1.DocTitle != doc2.DocTitle
                && doc1.DocTitle.Substring(1, doc1.DocTitle.Length - 1) != doc2.DocTitle
                && doc2.DocTitle.Substring(1, doc2.DocTitle.Length - 1) != doc1.DocTitle)
            {
                return false;
            }

            if (doc1.OriginalTitle != doc2.OriginalTitle)
            {
                return false;
            }

            if (doc1.Source != doc2.Source)
            {
                return false;
            }

            if (doc1.Category != doc2.Category)
            {
                return false;
            }

            if (doc1.Author != doc2.Author)
            {
                return false;
            }

            if (doc1.AttachSourcePaths.Count != doc2.AttachSourcePaths.Count)
            {
                return false;
            }

            if (doc1.AttachSourcePaths.Count > 0)
            {
                foreach (string path in doc1.AttachSourcePaths)
                {
                    if (!doc2.AttachSourcePaths.Contains(path))
                    {
                        return false;
                    }
                }

                foreach (string path in doc2.AttachSourcePaths)
                {
                    if (!doc1.AttachSourcePaths.Contains(path))
                    {
                        return false;
                    }
                }
            }

            if (doc1.AttachFileNames.Count != doc2.AttachFileNames.Count)
            {
                return false;
            }

            if (doc1.AttachFileNames.Count > 0)
            {
                foreach (string fn in doc1.AttachFileNames)
                {
                    if (!doc2.AttachFileNames.Contains(fn))
                    {
                        return false;
                    }
                }

                foreach (string fn in doc2.AttachFileNames)
                {
                    if (!doc1.AttachFileNames.Contains(fn))
                    {
                        return false;
                    }
                }
            }

            if (doc1.DocText != doc2.DocText)
            {
                return false;
            }

            return true;
        }
        #endregion doc modification detection

        private void BtnEditDoc_Click(object sender, RoutedEventArgs e)
        {
            if (this.grdDocInfo.DataContext == null)
            {
                return;
            }

            this.tbxDocTitle.IsReadOnly = false;
            this.tbxSource.IsReadOnly = false;
            this.tbxOriginalTitle.IsReadOnly = false;
            this.tbxCategory.IsReadOnly = false;
            this.tbxAuthor.IsReadOnly = false;
            this.tbxDocText.IsReadOnly = false;

            this.tbxDocTitle.Focus();
            this.tbxDocTitle.SelectionStart = this.tbxDocTitle.Text.Length; // move carnet to the text end
        }

        /// <summary>
        /// Mouse left key double click to edit doc
        /// </summary>
        private void Tbx_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.grdDocInfo.DataContext == null)
            {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.tbxDocTitle.IsReadOnly = false;
                this.tbxSource.IsReadOnly = false;
                this.tbxOriginalTitle.IsReadOnly = false;
                this.tbxCategory.IsReadOnly = false;
                this.tbxAuthor.IsReadOnly = false;
                this.tbxDocText.IsReadOnly = false;

                TextBox tbx = sender as TextBox;
                tbx.Focus();
                tbx.SelectionStart = tbx.Text.Length; // move carnet to the text end
            }
        }

        private void BtnNewDoc_Click(object sender, RoutedEventArgs e)
        {
            List<DocViewModel> docList = this.lbxDocTitle.ItemsSource as List<DocViewModel>;

            if (this.grdDocInfo.DataContext != null && (this.grdDocInfo.DataContext as DocViewModel).DocTitle.StartsWith("*"))
            {
                MessageBoxResult res = MessageBox.Show("当前编辑的文档未保存，要保存吗？", "", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (res == MessageBoxResult.Yes) // save currently edited doc
                {
                    BtnSaveDoc_Click(sender, e);
                }
                else if (res == MessageBoxResult.No) // recover currently edited doc
                {
                    docList[_index].DocTitle = _backup.DocTitle;
                    docList[_index].OriginalTitle = _backup.OriginalTitle;
                    docList[_index].Source = _backup.Source;
                    docList[_index].Category = _backup.Category;
                    docList[_index].Author = _backup.Author;
                    docList[_index].CreateTime = _backup.CreateTime;
                    docList[_index].LastRevised = _backup.LastRevised;

                    docList[_index].AttachFileNames.Clear();
                    foreach (string str in _backup.AttachFileNames)
                    {
                        docList[_index].AttachFileNames.Add(str);
                    }

                    docList[_index].AttachSourcePaths.Clear();
                    foreach (string str in _backup.AttachSourcePaths)
                    {
                        docList[_index].AttachSourcePaths.Add(str);
                    }

                    docList[_index].DocText = _backup.DocText;
                }
                else // give up new doc
                {
                    return; // keep currently edited doc unsaved
                }
            }

            DocViewModel doc = new DocViewModel();

            // avoid duplicattion of the doc title existing
            if (CheckDuplication(doc))
            {
                doc.DocTitle += " (2)";
                int count = 2;
                while (CheckDuplication(doc))
                {
                    count++;
                    doc.DocTitle = doc.DocTitle.Substring(0, doc.DocTitle.Length - 2) + count + ")";
                }
            }

            doc.FormerTitle = doc.DocTitle;

            int ret = _model.CreateDoc(doc);
            if (ret != 0)
            {
                MessageBox.Show("新建文档失败！", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.lbxDocTitle.ItemsSource = null;
            this.lbxDocTitle.ItemsSource = _model.LoadDoc();

            // Update status info
            if (_hUpdateStatusInfoHandler != null)
            {
                _hUpdateStatusInfoHandler("总共：" + (this.lbxDocTitle.ItemsSource as List<DocViewModel>).Count);
            }

            // select current new doc
            for (int i = 0; i < this.lbxDocTitle.Items.Count; i++)
            {
                if ((this.lbxDocTitle.Items[i] as DocViewModel).DocTitle == doc.DocTitle)
                {
                    this.lbxDocTitle.SelectedIndex = i;
                }
            }
        }

        private void BtnDeleteDoc_Click(object sender, RoutedEventArgs e)
        {
            if (this.lbxDocTitle.SelectedItems.Count > 0)
            {
                // if the doc is editable state?
                if (!this.tbxDocTitle.IsReadOnly || !this.tbxSource.IsReadOnly || !this.tbxOriginalTitle.IsReadOnly
                    || !this.tbxCategory.IsReadOnly || !this.tbxAuthor.IsReadOnly || !this.tbxDocText.IsReadOnly)
                {
                    MessageBox.Show("当前编辑的文档未保存，禁止删除！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBoxResult ret = MessageBox.Show("已选择" + this.lbxDocTitle.SelectedItems.Count + "个文档，确定永久删除吗？！",
                    "", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (ret == MessageBoxResult.Yes)
                {
                    int err = 0;
                    foreach (DocViewModel doc in this.lbxDocTitle.SelectedItems)
                    {
                        err = _model.DeleteDoc(doc);
                    }
                    
                    if (err != 0)
                    {
                        _logWriter.WriteErrorLog("Documents::BtnDeleteDoc_Click >> exception occurs when delete doc(s)");
                        return;
                    }

                    // reset global variables
                    _paths.Clear();

                    // reset doc editing area data context
                    this.grdDocInfo.DataContext = null;
                    this.tbxDocText.DataContext = null;
                    if (this.lbxAttachFileList.ItemsSource != null)
                    {
                        (this.lbxAttachFileList.ItemsSource as List<ListBoxItem>).Clear();
                        this.lbxAttachFileList.ItemsSource = null;
                    }
                    this.tbxFileCount.Text = "";

                    // reload doc data
                    this.lbxDocTitle.ItemsSource = null;
                    this.lbxDocTitle.ItemsSource = _model.LoadDoc();

                    // Update status info
                    if (_hUpdateStatusInfoHandler != null)
                    {
                        _hUpdateStatusInfoHandler("总共：" + (this.lbxDocTitle.ItemsSource as List<DocViewModel>).Count);
                    }
                }
            }
        }

        private void MiDeleteDoc_Click(object sender, RoutedEventArgs e)
        {
            BtnDeleteDoc_Click(sender, e);
        }

        /// <summary>
        /// Check whether specified doc's title is the same as that of existing doc, i.e., duplication of title
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private bool CheckDuplication(DocViewModel doc)
        {
            List<DocViewModel> docList = this.lbxDocTitle.ItemsSource as List<DocViewModel>;

            if (docList == null || docList.Count == 0)
            {
                return false;
            }

            foreach (DocViewModel d in docList)
            {
                if (d.DocTitle == doc.DocTitle)
                {
                    return true;
                }
            }

            return false;
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            // if the doc is editable state?
            if (!this.tbxDocTitle.IsReadOnly || !this.tbxSource.IsReadOnly || !this.tbxOriginalTitle.IsReadOnly
                || !this.tbxCategory.IsReadOnly || !this.tbxAuthor.IsReadOnly || !this.tbxDocText.IsReadOnly)
            {
                if (this.grdDocInfo.DataContext != null && (this.grdDocInfo.DataContext as DocViewModel).DocTitle.StartsWith("*"))
                {
                    MessageBox.Show("当前编辑的文档未保存，请先保存！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else
                {
                    // disable the editing state
                    this.tbxDocTitle.IsReadOnly = true;
                    this.tbxSource.IsReadOnly = true;
                    this.tbxOriginalTitle.IsReadOnly = true;
                    this.tbxCategory.IsReadOnly = true;
                    this.tbxAuthor.IsReadOnly = true;
                    this.tbxDocText.IsReadOnly = true;
                }
            }

            // reset global variables
            _paths.Clear();

            // reset doc editing area data context
            this.grdDocInfo.DataContext = null;
            this.tbxDocText.DataContext = null;
            if (this.lbxAttachFileList.ItemsSource != null)
            {
                (this.lbxAttachFileList.ItemsSource as List<ListBoxItem>).Clear();
                this.lbxAttachFileList.ItemsSource = null;
            }
            this.tbxFileCount.Text = "";

            // reset doc title list
            if (this.lbxDocTitle.ItemsSource != null)
            {
                (this.lbxDocTitle.ItemsSource as List<DocViewModel>).Clear();
                this.lbxDocTitle.ItemsSource = null;
            }
            this.lbxDocTitle.ItemsSource = _model.LoadDoc(); // reload doc data

            // Update status info
            if (_hUpdateStatusInfoHandler != null)
            {
                _hUpdateStatusInfoHandler("总共：" + (this.lbxDocTitle.ItemsSource as List<DocViewModel>).Count);
            }
        }

        private void BtnExport2Txt_Click(object sender, RoutedEventArgs e)
        {
            _model.Export2Txt(this.tbxDocText.DataContext as DocViewModel);
        }
    }
}
