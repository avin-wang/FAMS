using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using SysWinForm = System.Windows.Forms;
using FAMS.Models.Accounts;
using FAMS.Services;
using FAMS.ViewModels.Accounts;
using ADOX;

namespace FAMS.Views.Accounts
{
    /// <summary>
    /// Interaction logic for Accounts.xaml
    /// </summary>
    public partial class Accounts : Page
    {
        private CFamsFileHelper _ffHelper = new CFamsFileHelper();
        private CLogWriter _logWriter = CLogWriter.GetInstance();

        private string _attachDir = string.Empty; // account attached file directory
        private string _newAccountName = null;

        private AccountsModel _model = new AccountsModel();

        // Update status info.
        public UpdateStatusInfoHandler _hUpdateStatusInfoHandler;
        public delegate void UpdateStatusInfoHandler(string infoStr);

        public Accounts()
        {
            InitializeComponent();
            InitializeContext();
        }

        /// <summary>
        /// Global initialization.
        /// </summary>
        private void InitializeContext()
        {
            // Initialize *.fams file helper (fix system config file path)
            _ffHelper.Init(Directory.GetCurrentDirectory() + "\\config\\system.fams");

            // Get log file info
            string logDir = Directory.GetCurrentDirectory() + _ffHelper.GetData("config", "log_dir");
            string logName = _ffHelper.GetData("config", "log_name");

            // Initialize log writer
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            _logWriter.Init(logDir + "\\", logName);
            _logWriter.SetCurrLevel(5);

            // Get attached file directory
            _attachDir = Directory.GetCurrentDirectory() + _ffHelper.GetData("config", "acc_attach_dir");

            // Data binding
            this.grdNewAccount.DataContext = new AccountViewModel();
            this.lvAccountList.ItemsSource = _model.LoadAccountData();

            // Add handlers.
            this.tbxSearchKeyWord.AddHandler(TextBox.KeyDownEvent, new KeyEventHandler(SearchBox_KeyDown));
            this.tbxSearchKeyWord.AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(SearchBox_TextChanged));
            this.btnSearch.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnSearch_Click));
            this.btnAdvancedSearch.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnAdvancedSearch_Click));
            this.btnEdit.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnEdit_Click));
            this.btnDelete.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnDelete_Click));
            this.btnMergeInto.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnMergeInto_Click));
            this.btnExport.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnExport_Click));
            this.btnAddNewDb.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnAddNewDb_Click));
            this.btnAddNewAccount.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnAddNewAccount_Click));
            this.btnSave.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnSave_Click));
            this.btnCancel.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnCancel_Click));
            this.btnAddAttachment.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnAddAttachment_Click));
            this.lvAccountList.MouseDoubleClick += LvAccountList_MouseDoubleClick;
            this.lvAccountList.KeyDown += LvAccountList_KeyDown;
            this.lvAccountList.SelectionChanged += LvAccountList_SelectionChanged;

            this.tbxSearchKeyWord.Focus();
        }

        /// <summary>
        /// Double click the selected item of the account list
        /// </summary>
        private void LvAccountList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && this.lvAccountList.SelectedItems.Count > 0)
            {
                BtnEdit_Click(sender, e);
            }
        }

        /// <summary>
        /// Shortcuts
        /// </summary>
        private void LvAccountList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F2)
            {
                BtnEdit_Click(sender, e); // Press F2 to edit current selected account
            }
            else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == System.Windows.Input.Key.E)
            {
                BtnExport_Click(sender, e); // Press Ctrl+E to export current selected account
            }
            else if (e.Key == System.Windows.Input.Key.Delete)
            {
                BtnDelete_Click(sender, e); // Press Delete to delete current selected account(s)
            }
        }

        /// <summary>
        /// TextChanged event of the search box
        /// </summary>
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (this.tbxSearchKeyWord.Text == "")
            //{
            //    BtnSearch_Click(sender, e);
            //}

            // Open real-time searching (uncomment above lines to close it)
            BtnSearch_Click(sender, e);
        }

        /// <summary>
        /// KeyDown event of the search box
        /// </summary>
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BtnSearch_Click(sender, e);
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.tbxSearchKeyWord.Text = "";
            }
        }

        /// <summary>
        /// Search
        /// </summary>
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.lvAccountList.ItemsSource = _model.Search(this.tbxSearchKeyWord.Text);

            // Update status info
            if (_hUpdateStatusInfoHandler != null)
            {
                if (this.tbxSearchKeyWord.Text != "")
                {
                    _hUpdateStatusInfoHandler("搜索结果：" + this.lvAccountList.Items.Count);
                }
                else
                {
                    _hUpdateStatusInfoHandler("总共：" + this.lvAccountList.Items.Count);
                }
            }
        }

        /// <summary>
        /// Advanced search
        /// </summary>
        private void BtnAdvancedSearch_Click(object sender, RoutedEventArgs e)
        {
            AdvancedSearchWin win = new AdvancedSearchWin();
            if (win.ShowDialog() == true)
            {
                this.lvAccountList.ItemsSource = _model.AdvancedSearch(win.DataContext as AdvancedSearchViewModel);

                // Update status info
                if (_hUpdateStatusInfoHandler != null)
                {
                    _hUpdateStatusInfoHandler("搜索结果：" + this.lvAccountList.Items.Count);
                }
            }
        }

        /// <summary>
        /// Edit an account selected
        /// </summary>
        /// <history time="2020/03/04">modify 'Update' function interface</history>
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (this.lvAccountList.SelectedItem == null)
            {
                MessageBox.Show("请选择一个账号！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AccountViewModel newAccount = this.lvAccountList.SelectedItem as AccountViewModel;
            AccountViewModel oldAccount = new AccountViewModel();
            oldAccount.AccountName = newAccount.AccountName;
            oldAccount.AccountType = newAccount.AccountType;
            oldAccount.URL = newAccount.URL;
            oldAccount.UserName = newAccount.UserName;
            oldAccount.Password = newAccount.Password;
            oldAccount.DisplayName = newAccount.DisplayName;
            oldAccount.Email = newAccount.Email;
            oldAccount.Telephone = newAccount.Telephone;
            oldAccount.PaymentCode = newAccount.PaymentCode;
            oldAccount.AppendDate = newAccount.AppendDate;
            oldAccount.LastRevised = newAccount.LastRevised;
            oldAccount.FormerAccountNames = newAccount.FormerAccountNames;
            oldAccount.AttachmentFlag = newAccount.AttachmentFlag;
            oldAccount.AttachedFileNames = newAccount.AttachedFileNames;
            oldAccount.Remarks = newAccount.Remarks;
            oldAccount.KeyWords = newAccount.KeyWords;

            EditWin win = new EditWin() { DataContext = newAccount };
            if (win.ShowDialog() == true)
            {
                if (_model.Update(newAccount, oldAccount) < 0)
                {
                    MessageBox.Show("修改失败！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    this.lvAccountList.ItemsSource = _model.LoadAccountData(); // Reload.

                    // Update status info
                    if (_hUpdateStatusInfoHandler != null)
                    {
                        _hUpdateStatusInfoHandler("总共：" + this.lvAccountList.Items.Count);
                    }
                }
            }
        }

        private void MiEdit_Click(object sender, RoutedEventArgs e)
        {
            BtnEdit_Click(sender, e);
        }

        /// <summary>
        /// Delete selected accounts
        /// </summary>
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (this.lvAccountList.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("已选择" + this.lvAccountList.SelectedItems.Count + "个账号，确定删除？", "",
                    MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                {
                    List<AccountViewModel> selectedAccounts = new List<AccountViewModel>();
                    foreach (AccountViewModel vmAccount in this.lvAccountList.SelectedItems)
                    {
                        selectedAccounts.Add(vmAccount);
                    }
                    _model.Delete(selectedAccounts);
                    this.lvAccountList.ItemsSource = _model.LoadAccountData(); // Reload.

                    // Update status info
                    if (_hUpdateStatusInfoHandler != null)
                    {
                        _hUpdateStatusInfoHandler("总共：" + this.lvAccountList.Items.Count);
                    }
                }
            }
            else
            {
                MessageBox.Show("请选择至少一个账号！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MiDelete_Click(object sender, RoutedEventArgs e)
        {
            BtnDelete_Click(sender, e);
        }

        /// <summary>
        /// Merge specified database into current database.
        /// </summary>
        private void BtnMergeInto_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Access文件(*.accdb)|*.accdb";
            ofd.FilterIndex = 0;
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<int> num = _model.Merge(ofd.FileName, _ffHelper.GetData("config", "acc_db_pwd"));
                if (num != null)
                {
                    MessageBox.Show("成功并入" + (num[0] + num[1]) + "个账号！" + "\n（新增：" + num[0] + "，更新：" + num[1] + "）",
                        "", MessageBoxButton.OK, MessageBoxImage.Information);

                    this.lvAccountList.ItemsSource = _model.LoadAccountData(); // Reload

                    // Update status info
                    if (_hUpdateStatusInfoHandler != null)
                    {
                        _hUpdateStatusInfoHandler("总共：" + this.lvAccountList.Items.Count + "  新增：" + num[0] + "  更新：" + num[1]);
                    }
                }
                else
                {
                    MessageBox.Show("数据库并入失败！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Export selected accounts
        /// </summary>
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (this.lvAccountList.SelectedItems.Count > 0)
            {
                List<AccountViewModel> vmAccounts = new List<AccountViewModel>();
                foreach (AccountViewModel vmAccount in this.lvAccountList.SelectedItems)
                {
                    vmAccounts.Add(vmAccount);
                }

                if (_model.Export(vmAccounts) < 0)
                {
                    MessageBox.Show("导出失败！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("请选择至少一个账号！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MiExport_Click(object sender, RoutedEventArgs e)
        {
            BtnExport_Click(sender, e);
        }

        /// <summary>
        /// New a database
        /// </summary>
        private void BtnAddNewDb_Click(object sender, RoutedEventArgs e)
        {
            SysWinForm.FolderBrowserDialog dlg = new SysWinForm.FolderBrowserDialog();
            dlg.ShowNewFolderButton = true;
            dlg.Description = "保存新数据库至：";
            if (dlg.ShowDialog() == SysWinForm.DialogResult.OK)
            {
                string dbFileName = dlg.SelectedPath + "\\" + "FAMS-DB-Account.accdb";

                if (File.Exists(dbFileName))
                {
                    MessageBoxResult result = MessageBox.Show("数据库已存在，要覆盖吗？", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        File.Delete(dbFileName);
                    }
                    else
                    {
                        return;
                    }
                }

                CAccessHelper accHelper = new CAccessHelper();
                string dbPwd = _ffHelper.GetData("config", "acc_db_pwd");

                if (accHelper.CreateDatabase(dbFileName, dbPwd)) // use accHelper.CreateDatabase(fileName, null) to create a database without password
                {
                    List<Column> columns = new List<Column>();

                    Column column = new Column();
                    column.Name = "AccountName";
                    column.Type = DataTypeEnum.adLongVarWChar;
                    column.Attributes = ColumnAttributesEnum.adColNullable;
                    columns.Add(column);

                    column = new Column();
                    column.Name = "AccountType";
                    column.Type = DataTypeEnum.adInteger;
                    column.Attributes = ColumnAttributesEnum.adColNullable;
                    columns.Add(column);

                    column = new Column();
                    column.Name = "URL";
                    column.Type = DataTypeEnum.adLongVarWChar;
                    column.Attributes = ColumnAttributesEnum.adColNullable;
                    columns.Add(column);

                    column = new Column();
                    column.Name = "UserName";
                    column.Type = DataTypeEnum.adLongVarWChar;
                    column.Attributes = ColumnAttributesEnum.adColNullable;
                    columns.Add(column);

                    column = new Column();
                    column.Name = "Password";
                    column.Type = DataTypeEnum.adVarWChar;
                    column.DefinedSize = 128; // for DataTypeEnum.adLongVarWChar(long text), do not need to set this parameter
                    column.Attributes = ColumnAttributesEnum.adColNullable;
                    columns.Add(column);

                    column = new Column();
                    column.Name = "DisplayName";
                    column.Type = DataTypeEnum.adVarWChar;
                    column.DefinedSize = 128;
                    column.Attributes = ColumnAttributesEnum.adColNullable;
                    columns.Add(column);

                    column = new Column();
                    column.Name = "Email";
                    column.Type = DataTypeEnum.adVarWChar;
                    column.DefinedSize = 128;
                    column.Attributes = ColumnAttributesEnum.adColNullable;
                    columns.Add(column);

                    column = new Column();
                    column.Name = "Telephone";
                    column.Type = DataTypeEnum.adVarWChar;
                    column.DefinedSize = 128;
                    column.Attributes = ColumnAttributesEnum.adColNullable;
                    columns.Add(column);

                    column = new Column();
                    column.Name = "PaymentCode";
                    column.Type = DataTypeEnum.adVarWChar;
                    column.DefinedSize = 128;
                    column.Attributes = ColumnAttributesEnum.adColNullable;
                    columns.Add(column);

                    column = new Column();
                    column.Name = "AppendDate";
                    column.Type = DataTypeEnum.adDate;
                    column.Attributes = ColumnAttributesEnum.adColNullable;
                    columns.Add(column);

                    column = new Column();
                    column.Name = "LastRevised";
                    column.Type = DataTypeEnum.adDate;
                    column.Attributes = ColumnAttributesEnum.adColNullable;
                    columns.Add(column);

                    column = new Column();
                    column.Name = "FormerAccountNames";
                    column.Type = DataTypeEnum.adLongVarWChar;
                    column.Attributes = ColumnAttributesEnum.adColNullable;
                    columns.Add(column);

                    column = new Column();
                    column.Name = "AttachmentFlag";
                    column.Type = DataTypeEnum.adInteger;
                    column.Attributes = ColumnAttributesEnum.adColNullable;
                    columns.Add(column);

                    column = new Column();
                    column.Name = "AttachedFileNames";
                    column.Type = DataTypeEnum.adLongVarWChar;
                    column.Attributes = ColumnAttributesEnum.adColNullable;
                    columns.Add(column);

                    column = new Column();
                    column.Name = "Remarks";
                    column.Type = DataTypeEnum.adLongVarWChar;
                    column.Attributes = ColumnAttributesEnum.adColNullable;
                    columns.Add(column);

                    column = new Column();
                    column.Name = "KeyWords";
                    column.Type = DataTypeEnum.adLongVarWChar;
                    column.Attributes = ColumnAttributesEnum.adColNullable;
                    columns.Add(column);

                    try
                    {
                        if (accHelper.CreateTable(dbFileName, "AccountTable", columns, dbPwd))
                        {
                            MessageBox.Show("数据库创建成功！", "", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logWriter.WriteFaultLog("Accounts::BtnAddNewDb >> exception occurs when create database: " + ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Add a new account
        /// </summary>
        private void BtnAddNewAccount_Click(object sender, RoutedEventArgs e)
        {
            if (this.hidedCol.Width != new GridLength(400))
            {
                Unfold();
                this.tbxAccountName.Focus();
            }
            else
            {
                AccountViewModel newAccount = this.grdNewAccount.DataContext as AccountViewModel;
                if (newAccount.AttachedFileNames != null && newAccount.AttachedFileNames != ""
                    && Directory.Exists(_attachDir + "\\" + newAccount.AccountName))
                {
                    Directory.Delete(_attachDir + "\\" + newAccount.AccountName, true); // delete the newly-created directory
                }

                this.grdNewAccount.DataContext = new AccountViewModel(); // Reset.
                Fold();
            }
        }


        /// <summary>
        /// Save the new account
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            AccountViewModel vmNewAccount = this.grdNewAccount.DataContext as AccountViewModel;

            // It will be null if no input ever, while it will be "" if the input has been cleared
            if (vmNewAccount.AccountName == null || vmNewAccount.AccountName == "")
            {
                MessageBox.Show("\"账号名称\" 不能为空！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.tbxAccountName.Focus();
                return;
            }

            // If current account to be added already existed?
            if (_model.Search(vmNewAccount.AccountName, "AccountName").Count > 0
                || _model.Search(vmNewAccount.AccountName, "FormerAccountNames").Count > 0)
            {
                MessageBox.Show("账号已存在！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.tbxAccountName.Focus();
                return;
            }

            // If any files attached?
            vmNewAccount.AttachmentFlag = (vmNewAccount.AttachedFileNames != null && vmNewAccount.AttachedFileNames != "") ? "有" : "无";
            // Rename the attached file folder if the account name was modified
            if (_newAccountName != null && _newAccountName != vmNewAccount.AccountName)
            {
                RenameFolder(_attachDir + "\\" + _newAccountName, _attachDir + "\\" + vmNewAccount.AccountName);
            }

            if (_model.Insert(vmNewAccount) < 0)
            {
                MessageBox.Show("账号创建失败！", "", MessageBoxButton.OK, MessageBoxImage.Error);

                // Delete attached files if added
                if (vmNewAccount.AttachedFileNames != null && vmNewAccount.AttachedFileNames != ""
                    && Directory.Exists(_attachDir + "\\" + vmNewAccount.AccountName))
                {
                    Directory.Delete(_attachDir + "\\" + vmNewAccount.AccountName, true);
                }
            }
            else
            {
                this.lvAccountList.ItemsSource = _model.LoadAccountData(); // Reload

                // Update status info
                if (_hUpdateStatusInfoHandler != null)
                {
                    _hUpdateStatusInfoHandler("总共：" + this.lvAccountList.Items.Count);
                }
            }
            this.grdNewAccount.DataContext = new AccountViewModel(); // Reset.

            Fold();
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

            Directory.Delete(orgDirName, true); // Delete the old folder.
        }

        /// <summary>
        /// Cancel adding the new account
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            AccountViewModel newAccount = this.grdNewAccount.DataContext as AccountViewModel;
            if (newAccount.AttachedFileNames != null && newAccount.AttachedFileNames != ""
                && Directory.Exists(_attachDir + "\\" + newAccount.AccountName))
            {
                Directory.Delete(_attachDir + "\\" + newAccount.AccountName, true); // delete the newly-created directory
            }

            this.grdNewAccount.DataContext = new AccountViewModel(); // Reset
            Fold();
        }

        private void Fold()
        {
            GridLengthAnimation glaWidth = new GridLengthAnimation();

            // Specify start and end points
            glaWidth.From = new GridLength(400);
            glaWidth.To = new GridLength(0);

            // Specify time duration
            glaWidth.Duration = new Duration(TimeSpan.FromMilliseconds(300));

            // Apply animation to specified row
            this.hidedCol.BeginAnimation(ColumnDefinition.WidthProperty, glaWidth);
        }

        private void Unfold()
        {
            GridLengthAnimation glaWidth = new GridLengthAnimation();

            // Specify start and end points
            glaWidth.From = new GridLength(0);
            glaWidth.To = new GridLength(400);

            // Specify time duration
            glaWidth.Duration = new Duration(TimeSpan.FromMilliseconds(300));

            // Apply animation to specified row
            this.hidedCol.BeginAnimation(ColumnDefinition.WidthProperty, glaWidth);
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

        /// <summary>
        /// Add attached files
        /// </summary>
        private void BtnAddAttachment_Click(object sender, RoutedEventArgs e)
        {
            AccountViewModel newAccount = this.grdNewAccount.DataContext as AccountViewModel;

            if (newAccount.AccountName == null || newAccount.AccountName == "")
            {
                MessageBox.Show("\"账号名称\"不能为空！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _newAccountName = newAccount.AccountName;

            // If current account to be added already existed?
            if (_model.Search(_newAccountName, "AccountName").Count > 0
                || _model.Search(_newAccountName, "FormerAccountNames").Count > 0)
            {
                MessageBox.Show("账号已存在！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "文件(*.png;*.bmp;*.jpg)|*.png;*.bmp;*.jpg|全部文件(*.*)|*.*";
            ofd.FilterIndex = 0;
            ofd.Multiselect = true;
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string fileName in ofd.FileNames)
                {
                    string safeFileName = System.IO.Path.GetFileName(fileName);
                    newAccount.AttachedFileNames += "|" + safeFileName;

                    if (!Directory.Exists(_attachDir + "\\" + _newAccountName))
                    {
                        Directory.CreateDirectory(_attachDir + "\\" + _newAccountName);
                    }
                    if (!File.Exists(_attachDir + "\\" + _newAccountName + "\\" + safeFileName))
                    {
                        File.Copy(fileName, _attachDir + "\\" + _newAccountName + "\\" + safeFileName);
                    }
                }

                if (newAccount.AttachedFileNames.StartsWith("|"))
                {
                    newAccount.AttachedFileNames = newAccount.AttachedFileNames.Substring(1, newAccount.AttachedFileNames.Length - 1);
                }
            }
        }

        /// <summary>
        /// Update status bar
        /// </summary>
        private void LvAccountList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update status info
            if (_hUpdateStatusInfoHandler != null)
            {
                _hUpdateStatusInfoHandler("总共：" + this.lvAccountList.Items.Count + "  选择：" + this.lvAccountList.SelectedItems.Count);
            }
        }
    }

    internal class GridLengthAnimation : AnimationTimeline
    {
        public static readonly DependencyProperty FromProperty;
        public static readonly DependencyProperty ToProperty;

        static GridLengthAnimation()
        {
            FromProperty = DependencyProperty.Register("From", typeof(GridLength), typeof(GridLengthAnimation));
            ToProperty = DependencyProperty.Register("To", typeof(GridLength), typeof(GridLengthAnimation));
        }

        public GridLength From
        {
            get { return (GridLength)GetValue(GridLengthAnimation.FromProperty); }
            set { SetValue(GridLengthAnimation.FromProperty, value); }
        }

        public GridLength To
        {
            get { return (GridLength)GetValue(GridLengthAnimation.ToProperty); }
            set { SetValue(GridLengthAnimation.ToProperty, value); }
        }

        public override Type TargetPropertyType
        {
            get { return typeof(GridLength); }
        }

        protected override System.Windows.Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            double fromVal = ((GridLength)GetValue(GridLengthAnimation.FromProperty)).Value;
            double toVal = ((GridLength)GetValue(GridLengthAnimation.ToProperty)).Value;

            if (fromVal > toVal)
            {
                return new GridLength((1 - animationClock.CurrentProgress.Value) * (fromVal - toVal) + toVal,
                    ((GridLength)GetValue(GridLengthAnimation.FromProperty)).GridUnitType);
            }
            else
            {
                return new GridLength(animationClock.CurrentProgress.Value * (toVal - fromVal) + fromVal,
                    ((GridLength)GetValue(GridLengthAnimation.ToProperty)).GridUnitType);
            }
        }
    }
}
