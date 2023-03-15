using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using FAMS.Commons.BaseClasses;
using FAMS.Services;
using FAMS.ViewModels.Login;
using System.Threading;
using System.IO;

namespace FAMS
{
    /// <summary>
    /// Interaction logic for LoginWin.xaml
    /// </summary>
    public partial class LoginWin : LoginWindowBase
    {
        private CFamsFileHelper _ffHelper = new CFamsFileHelper();
        private CLogWriter _logWriter = CLogWriter.GetInstance();
        private Dictionary<string, UserInfoViewModel> _users = new Dictionary<string, UserInfoViewModel>();

        public LoginWin()
        {
            InitializeComponent();
            InitializeContext();
        }

        private void InitializeContext()
        {
            // Initialize *.fams file helper (fix system config file path)
            _ffHelper.Init("./config/system.fams");

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

            LoadUserLoginInfos(); // load user login infos

            // if set auto login
            if (this.ckbAutoLogin.IsChecked.Value)
            {
                // record current login user
                _ffHelper.WriteData("sys_login_user", "last_login_user", (this.cbxUserName.SelectedItem as UserInfoViewModel).UserName);

                MainWindow main = new MainWindow(this.cbxUserName.Text.Trim());
                main.Show(); // enter main window
                this.Close();
            }

            // get focus
            if (this.pwdBox.Password.Trim() == "")
            {
                this.pwdBox.Focus();
            }
            else
            {
                this.btnLogin.Focus();
            }
        }

        /// <summary>
        /// Load user login infos: usernames, passwords, etc
        /// </summary>
        private void LoadUserLoginInfos()
        {
            try
            {
                List<string> usernames = _ffHelper.GetNames("sys_login_pwd");

                for (int i = 0; i < usernames.Count; i++)
                {
                    string username = usernames[i];
                    string pwd = _ffHelper.GetData("sys_login_pwd", username); // password
                    bool saved = _ffHelper.GetData("sys_login_pwd_saved", username) == "1" ? true : false; // saving state
                    bool auto = _ffHelper.GetData("sys_login_auto", username) == "1" ? true : false; // auto login state
                    UserInfoViewModel user = new UserInfoViewModel();
                    user.UserName = username;
                    user.LoginPassword = pwd;
                    user.LoginSaved = saved;
                    user.AutoLogin = auto;

                    if (_users.ContainsKey(user.UserName))
                    {
                        _users[user.UserName] = user;
                    }
                    else
                    {
                        _users.Add(user.UserName, user);
                    }
                }

                foreach (UserInfoViewModel user in _users.Values)
                {
                    this.cbxUserName.Items.Add(user);
                }

                this.cbxUserName.SelectedIndex = this.cbxUserName.Items.Count - 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载登录信息失败！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                _logWriter.WriteErrorLog("LoginWin::LoadUserLoginInfos >> Load login infos failed: " + ex.Message);

                this.Close();
            }
        }

        /// <summary>
        /// Login button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = this.cbxUserName.Text.Trim();
            string pwd = this.pwdBox.Password.Trim();

            // Username & password non-empty validation
            string prompt = string.Empty;
            if (username == string.Empty)
            {
                prompt = "用户名不能为空！";
                this.cbxUserName.Focus();
            }
            else if (pwd == string.Empty)
            {
                prompt = "密码不能为空！";
                this.pwdBox.Focus();
            }

            if (prompt != string.Empty)
            {
                this.lbPrompt.Content = prompt;
                this.lbPrompt.Visibility = Visibility.Visible;
                return;
            }

            ThreadStart thrs = new ThreadStart(CheckLoginInfo);
            Thread thr = new Thread(thrs);
            thr.SetApartmentState(ApartmentState.STA);
            thr.IsBackground = true;
            thr.Start();
        }

        private void CheckLoginInfo()
        {
            this.Dispatcher.Invoke(new Action(CheckUserLoginInfo));
        }

        private void CheckUserLoginInfo()
        {
            try
            {
                string username = this.cbxUserName.Text.Trim();
                string pwd = this.pwdBox.Password.Trim();
                bool saved = this.ckbSave.IsChecked.Value;
                bool auto = this.ckbAutoLogin.IsChecked.Value;

                UserInfoViewModel user = new UserInfoViewModel();
                user.UserName = username;
                user.LoginPassword = pwd;
                user.LoginSaved = saved;
                user.AutoLogin = auto;

                string prompt = LoginInfoVerify(user);

                if (prompt == "登录信息验证正确！")
                {
                    SaveLoginInfo(user);

                    MainWindow main = new MainWindow(user.UserName);
                    main.Show(); // enter main window
                    this.Close();
                }
                else
                {
                    this.lbPrompt.Content = prompt;
                    this.lbPrompt.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("LoginWin::CheckUserLoginInfo >> Exception occurs in login info checking: " + ex.Message);
            }
        }

        private string LoginInfoVerify(UserInfoViewModel user)
        {
            string username = user.UserName;
            string pwd = user.LoginPassword;
            string prompt = string.Empty;

            foreach (UserInfoViewModel userInfo in _users.Values)
            {
                if (userInfo.UserName == username && userInfo.LoginPassword == pwd) // correct username and password
                {
                    prompt = "登录信息验证正确！";
                    return prompt;
                }
                else if (userInfo.UserName == username && userInfo.LoginPassword != pwd) // correct username, wrong password
                {
                    prompt = "密码错误！";
                    return prompt;
                }
                else if (userInfo.UserName != username && userInfo.LoginPassword == pwd) // correct password, wrong username
                {
                    prompt = "用户名错误！";
                }
                else
                {
                    prompt = "登录信息不存在！"; // login info does not exist
                }
            }

            return prompt;
        }

        private void SaveLoginInfo(UserInfoViewModel user)
        {
            try
            {
                if (_users.ContainsKey(user.UserName))
                {
                    _users.Remove(user.UserName);
                }

                List<UserInfoViewModel> userList = _users.Values.ToList();
                userList.Add(user);

                foreach (UserInfoViewModel userInfo in userList)
                {
                    _ffHelper.WriteData("sys_login_pwd", userInfo.UserName, userInfo.LoginPassword); // save username & password
                    _ffHelper.WriteData("sys_login_pwd_saved", userInfo.UserName, userInfo.LoginSaved ? "1" : "0"); // save saving state
                    _ffHelper.WriteData("sys_login_auto", userInfo.UserName, userInfo.AutoLogin ? "1" : "0"); // save auto login state
                }

                _ffHelper.WriteData("sys_login_user", "last_login_user", user.UserName); // record current login user
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("LoginWin::SaveLoginInfo >> Save login info failed: " + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cbxUserName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserInfoViewModel user = (UserInfoViewModel)this.cbxUserName.SelectedItem;

            if (user != null && user.LoginSaved)
            {
                this.pwdBox.Password = user.LoginPassword;
                this.ckbSave.IsChecked = true;

                if (user.AutoLogin)
                {
                    this.ckbAutoLogin.IsChecked = true;
                }
            }
            else
            {
                this.pwdBox.Password = "";
                this.ckbSave.IsChecked = false;
            }
        }

        private void pwdBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }

        private void ckbAutoLogin_Checked(object sender, RoutedEventArgs e)
        {
            this.ckbSave.IsChecked = true;
        }
    }
}
