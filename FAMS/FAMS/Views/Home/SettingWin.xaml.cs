using System.Windows;
using FAMS.Commons.BaseClasses;
using FAMS.ViewModels.Home;
using FAMS.Services;
using System.Collections.Generic;
using FAMS.ViewModels.Login;
using System;
using System.Threading;
using System.Linq;
using System.Windows.Controls;
using System.IO;

namespace FAMS.Views.Home
{
    /// <summary>
    /// Interaction logic for SettingWin.xaml
    /// </summary>
    public partial class SettingWin : PopupWindowBase
    {
        private CFamsFileHelper _ffHelper = new CFamsFileHelper();
        private CLogWriter _logWriter = CLogWriter.GetInstance();

        private bool _autorun = true;
        private Dictionary<string, UserInfoViewModel> _users = new Dictionary<string, UserInfoViewModel>();

        public SettingWin()
        {
            InitializeComponent();
            InitializeContext();
        }

        /// <summary>
        /// Initialize context
        /// </summary>
        /// <history time="2018/03/10">create this method</history>
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

            // Initialize data context
            _autorun = _ffHelper.GetData("config", "autorun") == "1" ? true : false;
            this.cbxAutoRuns.IsChecked = _autorun;

            // Initialize event handlers
            this.cbxAutoRuns.Checked += CbxAutoRuns_Checked;
            this.cbxAutoRuns.Unchecked += CbxAutoRuns_Unchecked;

            LoadUserInfos(); // load user infos
        }

        /// <summary>
        /// Set auto running
        /// </summary>
        /// <history time="2018/03/10">create this method</history>
        private void CbxAutoRuns_Checked(object sender, RoutedEventArgs e)
        {
            if (_autorun != true)
            {
                _ffHelper.WriteData("config", "autorun", "1");
                _autorun = true;
                MessageBox.Show("设置成功，重启软件生效！", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Cancel auto running
        /// </summary>
        /// <history time="2018/03/10">create this method</history>
        private void CbxAutoRuns_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_autorun != false)
            {
                _ffHelper.WriteData("config", "autorun", "0");
                _autorun = false;
                MessageBox.Show("设置成功，重启软件生效！", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Load user infos
        /// </summary>
        private void LoadUserInfos()
        {
            try
            {
                List<string> usernames = _ffHelper.GetNames("sys_login_pwd");

                _users.Clear();
                this.cbxLoginUserName.Items.Clear();
                this.cbxPageUserName.Items.Clear();

                for (int i = 0; i < usernames.Count; i++)
                {
                    string username = usernames[i];

                    // login info
                    string loginPwd = _ffHelper.GetData("sys_login_pwd", username); // password
                    bool loginSaved = _ffHelper.GetData("sys_login_pwd_saved", username) == "1" ? true : false; // saving state
                    bool autoLogin = _ffHelper.GetData("sys_login_auto", username) == "1" ? true : false; // auto login state

                    // page locking info
                    string pagePwd = _ffHelper.GetData("page_unlock_pwd", username);
                    bool unlockSaved = _ffHelper.GetData("page_unlock_pwd_saved", username) == "1" ? true : false;
                    bool autoUnlock = _ffHelper.GetData("page_unlock_auto", username) == "1" ? true : false;
                    int lockDelay = int.Parse(_ffHelper.GetData("page_lock_delay", username));

                    UserInfoViewModel user = new UserInfoViewModel();
                    user.UserName = username;
                    user.LoginPassword = loginPwd;
                    user.LoginSaved = loginSaved;
                    user.AutoLogin = autoLogin;
                    user.PagePassword = pagePwd;
                    user.UnlockSaved = unlockSaved;
                    user.AutoUnlock = autoUnlock;
                    user.PageLockDelay = lockDelay;

                    _users.Add(user.UserName, user);

                    // Binding
                    this.cbxLoginUserName.Items.Add(user); // login info
                    this.cbxPageUserName.Items.Add(user); // page locking info
                }

                this.cbxLoginUserName.SelectedIndex = this.cbxLoginUserName.Items.Count - 1;
                this.cbxPageUserName.SelectedIndex = this.cbxPageUserName.Items.Count - 1;
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("SettingWin::LoadUserInfos >> Load user infos failed: " + ex.Message);
            }
        }

        private void btnModifyLoginInfo_Click(object sender, RoutedEventArgs e)
        {
            this.cbxLoginUserName.IsEnabled = true;
            this.pwdLogin.Visibility = Visibility.Hidden;
            this.tbxLoginPwd.Visibility = Visibility.Visible;
            this.ckbLoginSaved.IsEnabled = true;
            this.ckbLoginAuto.IsEnabled = true;
        }

        private void btnSaveLoginInfo_Click(object sender, RoutedEventArgs e)
        {
            string username = this.cbxLoginUserName.Text.Trim();
            string pwd = this.pwdLogin.Password.Trim();
            bool saved = this.ckbLoginSaved.IsChecked.Value;
            bool auto = this.ckbLoginAuto.IsChecked.Value;

            // Username & password non-empty validation
            string prompt = string.Empty;
            if (username == string.Empty)
            {
                prompt = "用户名不能为空！";
                this.cbxLoginUserName.Focus();
            }
            else if (pwd == string.Empty)
            {
                prompt = "密码不能为空！";
                this.pwdLogin.Focus();
            }

            if (prompt != string.Empty)
            {
                this.lbLoginPrompt.Content = prompt;
                this.lbLoginPrompt.Visibility = Visibility.Visible;
                return;
            }

            bool isNew = false;
            if (!_users.ContainsKey(username))
            {
                isNew = true;
            }

            // Save login info
            try
            {
                if (isNew)
                {
                    UserInfoViewModel user = new UserInfoViewModel();
                    user.UserName = username;
                    user.LoginPassword = pwd;
                    user.LoginSaved = saved;
                    user.AutoLogin = auto;

                    // set default page locking info
                    user.PagePassword = user.UserName;
                    user.UnlockSaved = false;
                    user.AutoUnlock = false;
                    user.PageLockDelay = 300;

                    // write login info
                    _ffHelper.WriteData("sys_login_pwd", user.UserName, user.LoginPassword); // save username & password
                    _ffHelper.WriteData("sys_login_pwd_saved", user.UserName, user.LoginSaved ? "1" : "0"); // save saving state
                    _ffHelper.WriteData("sys_login_auto", user.UserName, user.AutoLogin ? "1" : "0"); // save auto login state

                    // write page info
                    _ffHelper.WriteData("page_unlock_pwd", user.UserName, user.PagePassword); // page unlock password
                    _ffHelper.WriteData("page_unlock_pwd_saved", user.UserName, user.UnlockSaved ? "1" : "0"); // if save the page unlock passowrd: "1"-yes, "0"-no
                    _ffHelper.WriteData("page_unlock_auto", user.UserName, user.AutoUnlock ? "1" : "0"); // if auto unlock all system pages: "1"-yes, "0"-no
                    _ffHelper.WriteData("page_lock_delay", user.UserName, user.PageLockDelay.ToString()); // page locking delay time. (in unit of second)

                    _users.Add(username, user);
                }
                else
                {
                    // Update current users dictionary
                    _users[username].LoginPassword = pwd;
                    _users[username].LoginSaved = saved;
                    _users[username].AutoLogin = auto;

                    // write login info
                    _ffHelper.WriteData("sys_login_pwd", username, pwd); // save username & password
                    _ffHelper.WriteData("sys_login_pwd_saved", username, saved ? "1" : "0"); // save saving state
                    _ffHelper.WriteData("sys_login_auto", username, auto ? "1" : "0"); // save auto login state
                }

                // debinding
                this.cbxLoginUserName.Items.Clear();
                this.cbxPageUserName.Items.Clear();

                // rebinding
                foreach (UserInfoViewModel user in _users.Values)
                {
                    this.cbxLoginUserName.Items.Add(user);
                    this.cbxPageUserName.Items.Add(user);
                }

                this.cbxLoginUserName.SelectedItem = _users[username];
                this.cbxPageUserName.SelectedItem = _users[username];

                this.cbxLoginUserName.IsEnabled = false;
                this.pwdLogin.Visibility = Visibility.Visible;
                this.tbxLoginPwd.Visibility = Visibility.Hidden;
                this.ckbLoginSaved.IsEnabled = false;
                this.ckbLoginAuto.IsEnabled = false;
                this.lbLoginPrompt.Visibility = Visibility.Collapsed;

                MessageBox.Show("保存登录信息成功！", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存登录信息失败", "", MessageBoxButton.OK, MessageBoxImage.Error);
                _logWriter.WriteErrorLog("SettingWin::btnSaveLoginInfo_Click >> Save login info failed: " + ex.Message);
            }
        }

        private void cbxLoginUserName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserInfoViewModel user = (UserInfoViewModel)this.cbxLoginUserName.SelectedItem;

            if (user != null && user.LoginSaved)
            {
                this.pwdLogin.Password = user.LoginPassword;
                this.ckbLoginSaved.IsChecked = true;

                if (user.AutoLogin)
                {
                    this.ckbLoginAuto.IsChecked = true;
                }
            }
            else
            {
                this.pwdLogin.Password = "";
                this.ckbLoginSaved.IsChecked = false;
                this.ckbLoginAuto.IsChecked = false;
            }
        }

        private void btnDeleteLogin_Click(object sender, RoutedEventArgs e)
        {
            if (this.cbxLoginUserName.Items.Count > 1)
            {
                if (MessageBox.Show("确定删除该项登录信息？", "", MessageBoxButton.YesNo, MessageBoxImage.Question)
                    == MessageBoxResult.Yes)
                {
                    Button btn = sender as Button;
                    UserInfoViewModel user = btn.DataContext as UserInfoViewModel;

                    try
                    {
                        // Delete login info
                        _ffHelper.DeleteData("sys_login_pwd", user.UserName);
                        _ffHelper.DeleteData("sys_login_pwd_saved", user.UserName);
                        _ffHelper.DeleteData("sys_login_auto", user.UserName);

                        // Delete page locking info
                        _ffHelper.DeleteData("page_unlock_pwd", user.UserName);
                        _ffHelper.DeleteData("page_unlock_pwd_saved", user.UserName);
                        _ffHelper.DeleteData("page_unlock_auto", user.UserName);
                        _ffHelper.DeleteData("page_lock_delay", user.UserName);

                        _users.Remove(user.UserName);

                        this.cbxLoginUserName.Items.Remove(user);
                        this.cbxPageUserName.Items.Remove(user);

                        this.cbxLoginUserName.SelectedIndex = this.cbxLoginUserName.Items.Count - 1;
                        this.cbxPageUserName.SelectedIndex = this.cbxPageUserName.Items.Count - 1;

                        // update last login user
                        _ffHelper.WriteData("sys_login_user", "last_login_user", this.cbxLoginUserName.Text.Trim());

                        this.cbxLoginUserName.IsEnabled = false;
                        this.pwdLogin.Visibility = Visibility.Visible;
                        this.tbxLoginPwd.Visibility = Visibility.Hidden;
                        this.ckbLoginSaved.IsEnabled = false;
                        this.ckbLoginAuto.IsEnabled = false;
                        this.lbLoginPrompt.Visibility = Visibility.Collapsed;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("删除登录信息失败！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                        _logWriter.WriteErrorLog("SettingWin::btnDeleteLogin_Click >> Delete login info failed: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("仅剩唯一登录信息，禁止删除！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void cbxPageUserName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserInfoViewModel user = (UserInfoViewModel)this.cbxPageUserName.SelectedItem;

            if (user != null && user.UnlockSaved)
            {
                this.pwdPage.Password = user.PagePassword;
                this.ckbPageUnlockSaved.IsChecked = true;

                if (user.AutoUnlock)
                {
                    this.ckbPageUnlockAuto.IsChecked = true;
                }
            }
            else
            {
                this.pwdPage.Password = "";
                this.ckbPageUnlockSaved.IsChecked = false;
                this.ckbPageUnlockAuto.IsChecked = false;
            }

            if (user != null)
            {
                this.tbxLockDelay.Text = user.PageLockDelay.ToString();
            }
            else
            {
                this.tbxLockDelay.Text = "";
            }
        }

        private void btnModifyPageUnlockInfo_Click(object sender, RoutedEventArgs e)
        {
            this.cbxPageUserName.IsEnabled = true;
            this.pwdPage.Visibility = Visibility.Hidden;
            this.tbxPagePwd.Visibility = Visibility.Visible;
            this.ckbPageUnlockSaved.IsEnabled = true;
            this.ckbPageUnlockAuto.IsEnabled = true;
            this.tbxLockDelay.IsEnabled = true;
        }

        private void btnSavePageUnlockInfo_Click(object sender, RoutedEventArgs e)
        {
            string username = this.cbxPageUserName.Text.Trim();
            string pwd = this.pwdPage.Password.Trim();
            bool saved = this.ckbPageUnlockSaved.IsChecked.Value;
            bool auto = this.ckbPageUnlockAuto.IsChecked.Value;
            int lockDelay = 0;

            try
            {
                lockDelay = int.Parse(this.tbxLockDelay.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("时间数据类型错误：" + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Username & password non-empty validation
            string prompt = string.Empty;
            if (!_users.ContainsKey(username))
            {
                prompt = "用户名不能为空！";
                this.cbxPageUserName.Focus();
            }
            else if (pwd == string.Empty)
            {
                prompt = "密码不能为空！";
                this.pwdPage.Focus();
            }
            else if (lockDelay <= 0)
            {
                prompt = "锁定延时必须大于0！";
                this.tbxLockDelay.Focus();
            }

            if (prompt != string.Empty)
            {
                this.lbPagePrompt.Content = prompt;
                this.lbPagePrompt.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                // Save page locking info
                _ffHelper.WriteData("page_unlock_pwd", username, pwd);
                _ffHelper.WriteData("page_unlock_pwd_saved", username, saved ? "1" : "0");
                _ffHelper.WriteData("page_unlock_auto", username, auto ? "1" : "0");
                _ffHelper.WriteData("page_lock_delay", username, lockDelay.ToString());

                // Update current user dictionary
                _users[username].PagePassword = pwd;
                _users[username].UnlockSaved = saved;
                _users[username].AutoUnlock = auto;
                _users[username].PageLockDelay = lockDelay;

                // Update data binding
                this.cbxPageUserName.Items.Clear();
                foreach (UserInfoViewModel user in _users.Values)
                {
                    this.cbxPageUserName.Items.Add(user);
                }

                this.cbxPageUserName.SelectedItem = _users[username];

                this.cbxPageUserName.IsEnabled = false;
                this.pwdPage.Visibility = Visibility.Visible;
                this.tbxPagePwd.Visibility = Visibility.Hidden;
                this.tbxLockDelay.IsEnabled = false;
                this.ckbPageUnlockSaved.IsEnabled = false;
                this.ckbPageUnlockAuto.IsEnabled = false;
                this.lbPagePrompt.Visibility = Visibility.Collapsed;

                MessageBox.Show("保存锁定信息成功！", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存锁定信息失败！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                _logWriter.WriteErrorLog("SettingWin::btnSavePageUnlockInfo_Click >> Save page locking info failed: " + ex.Message);
            }
        }
    }
}
