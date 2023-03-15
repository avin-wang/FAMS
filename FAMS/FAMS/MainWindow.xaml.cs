using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using System.Windows.Threading;
using SysWinForm = System.Windows.Forms;
using FAMS.CustomWindow;
using FAMS.Views;
using FAMS.Views.Home;
using FAMS.Views.Finance;
using FAMS.Views.Accounts;
using FAMS.Views.Contacts;
using FAMS.Views.Documents;
using FAMS.Views.Pictures;
using FAMS.Views.Videos;
using FAMS.Views.Mails;
using FAMS.Views.Toolkit;
using FAMS.Services;
using System.IO;

namespace FAMS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : CustomMainWindow
    {
        private DispatcherTimer _currDateTimer = new DispatcherTimer();
        private CFamsFileHelper _ffHelper = new CFamsFileHelper(); // <2020/03/04, add>
        private CLogWriter _logWriter = CLogWriter.GetInstance();

        private List<Image> _imgNaviButtonList = new List<Image>();

        private Home _home = new Home();
        private Finance _finance = new Finance();
        private Accounts _accounts = new Accounts();
        private Contacts _contacts = new Contacts();
        private Documents _documents = new Documents();
        private Pictures _pictures = new Pictures();
        private Videos _videos = new Videos();
        private Mails _mails = new Mails();
        private Toolkit _toolkit = new Toolkit();

        private SysWinForm.NotifyIcon _notifyIcon = new SysWinForm.NotifyIcon();
        private bool _isMinimizedToNotificationArea;

        // Page locking info
        private string _pwd = null;
        private bool _unlockSaved = false;
        private bool _autoUnlock = false;

        // current login user name
        private string _username = null;

        public MainWindow(string username)
        {
            InitializeComponent();
            InitializeContext(username);
        }

        /// <summary>
        /// Initialize context (global initialization).
        /// </summary>
        private void InitializeContext(string username)
        {
            // Initialize window location
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen; // Window's initial location.

            // Initialize window size
            double workWidth = SystemParameters.WorkArea.Width; // Width of working area (i.e., without task-bar).
            double workHeight = SystemParameters.WorkArea.Height; // Height of working area (i.e., without task-bar).
            double screenWidth = SystemParameters.PrimaryScreenWidth; // Width of the whole screen.
            double screenHeight = SystemParameters.PrimaryScreenHeight; // Height of the whole screen.
            this.Width = (int)(workWidth * 0.85) > 1650 ? (int)(workWidth * 0.85) : 1650; // Set current window's width.
            this.Height = (int)(workHeight * 0.8) > 800 ? (int)(workHeight * 0.8) : 800; // Set current window's height.

            // Initialize *.fams file helper (fix system config file path)
            _ffHelper.Init("./config/system.fams");

            // Get log file info
            string logDir = Directory.GetCurrentDirectory() + _ffHelper.GetData("config", "log_dir");
            string logName = _ffHelper.GetData("config", "log_name");

            _username = username; // current login user name

            // Get page locking info
            _pwd = _ffHelper.GetData("page_unlock_pwd", _username); // get system page unlock password
            _unlockSaved = _ffHelper.GetData("page_unlock_pwd_saved", _username) == "1" ? true : false; // system page unlock password saved
            _autoUnlock = _ffHelper.GetData("page_unlock_auto", _username) == "1" ? true : false; // system page auto unlock

            // Initialize log writer
            if (!System.IO.Directory.Exists(logDir))
            {
                System.IO.Directory.CreateDirectory(logDir);
            }
            _logWriter.Init(logDir + "\\", logName);
            _logWriter.SetCurrLevel(5);

            // Initialize global configurations
            GlobalConfig();

            // Initialize navigation bar.
            _imgNaviButtonList.Add(this.imgHome);
            _imgNaviButtonList.Add(this.imgFinance);
            _imgNaviButtonList.Add(this.imgAccounts);
            _imgNaviButtonList.Add(this.imgContacts);
            _imgNaviButtonList.Add(this.imgDocuments);
            _imgNaviButtonList.Add(this.imgPictures);
            _imgNaviButtonList.Add(this.imgVideos);
            _imgNaviButtonList.Add(this.imgMails);
            _imgNaviButtonList.Add(this.imgToolkit);
            foreach (Image imgbutton in _imgNaviButtonList)
            {
                NavigationButton.SetClickedState(imgbutton, false);
                imgbutton.MouseEnter += NaviButton_MouseEnter;
                imgbutton.MouseLeave += NaviButton_MouseLeave;
                imgbutton.MouseLeftButtonDown += NaviButton_MouseLeftButtonDown;
            }

            // Set default startup frame
            this.imgHome.Opacity = 1;
            this.imgHome.Width = 30;
            this.imgHome.Height = 30;
            this.frameContent.Content = _home;
            NavigationButton.SetClickedState(this.imgHome, true);

            // Initialize status bar's date & time
            _currDateTimer.Tick += new EventHandler(CurrDateTimer_Tick);
            _currDateTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            _currDateTimer.Start();

            // Delegates
            _accounts._hUpdateStatusInfoHandler = new Accounts.UpdateStatusInfoHandler(UpdateStatusInfo);
            _documents._hUpdateStatusInfoHandler = new Documents.UpdateStatusInfoHandler(UpdateStatusInfo);

            // Initialize window's minimized state
            _isMinimizedToNotificationArea = GetMinimizedState();
            if (_isMinimizedToNotificationArea)
            {
                CreateNotifyIcon();
            }
        }

        /// <summary>
        /// Initialize global configurations.
        /// </summary>
        /// <history time="2018/03/10">create this method</history>
        private void GlobalConfig()
        {
            // Set autorun status.
            SetAutoRun();
        }

        /// <summary>
        /// Set autorun status.
        /// </summary>
        /// <history time="2018/04/14">create this method</history>
        private void SetAutoRun()
        {
            CRegEditor regEditor = new CRegEditor(RegistryHive.CurrentUser);

            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string exeName = exePath.Substring(exePath.LastIndexOf(@"\") + 1, 4); // "FAMS"
            string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            string shortcutDirectory = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp";
            string shortcutPath = System.IO.Path.Combine(shortcutDirectory, string.Format("{0}.lnk", exeName));

            if (_ffHelper.GetData("config", "autorun") == "1" ? true : false)
            {
                // Generate registry.
                if (!regEditor.Exists(subKey, exeName))
                {
                    regEditor.CreateKeyValue(subKey, exeName, exePath);
                }

                // Generate shortcut.
                if (!System.IO.File.Exists(shortcutPath))
                {
                    GenerateShortcut(shortcutDirectory, exePath, exeName);
                }
            }
            else
            {
                // Delete registry.
                if (regEditor.Exists(subKey, exeName))
                {
                    regEditor.DeleteKeyValue(subKey, exeName);
                }

                // Delete shortcut.
                if (System.IO.File.Exists(shortcutPath))
                {
                    System.IO.File.Delete(shortcutPath);
                }
            }
        }

        #region Generate shortcut.
        /// <summary>
        /// Generate shortcut.
        /// </summary>
        /// <param name="shortcutDirectory">shortcut directory</param>
        /// <param name="targetPath">full path of the file(i.e., .exe) to generate shortcut</param>
        /// <param name="shortcutName">name of the shortcut</param>
        /// <param name="description">description on the shortcut</param>
        /// <param name="iconLocation">icon location of the shortcut</param>
        /// <history time="2018/03/10">create this method</history>
        /// <remarks>to run this method, one must add this reference module: COM>Windows Script Host Object Model</remarks>
        public void GenerateShortcut(string shortcutDirectory, string targetPath, string shortcutName = null, string description = null, string iconLocation = null)
        {
            if (!System.IO.Directory.Exists(shortcutDirectory))
            {
                System.IO.Directory.CreateDirectory(shortcutDirectory);
            }

            shortcutName = shortcutName == null ? targetPath.Substring(targetPath.LastIndexOf('\\') + 1) : shortcutName;
            string shortcutPath = System.IO.Path.Combine(shortcutDirectory, string.Format("{0}.lnk", shortcutName));
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(targetPath);
            shortcut.WindowStyle = 1;
            shortcut.Description = description;
            shortcut.IconLocation = string.IsNullOrEmpty(iconLocation) ? targetPath : iconLocation;
            try
            {
                shortcut.Save();
            }
            catch (Exception ex)
            {
                // 可能发生异常：拒绝访问。 (异常来自 HRESULT:0x80070005 (E_ACCESSDENIED))
                _logWriter.WriteErrorLog("MainWindow.xaml.cs::GenerateShortcut >> " + ex.Message);
            }
        }

        /// <summary>
        /// Generate shortcut on desktop.
        /// </summary>
        /// <param name="targetPath">full path of the file(i.e., .exe) to generate shortcut</param>
        /// <param name="shortcutName">name of the shortcut</param>
        /// <param name="description">description on the shortcut</param>
        /// <param name="iconLocation">icon location of the shortcut</param>
        /// <history time="2018/03/10">create this method</history>
        public void GenerateShortcutOnDesktop(string targetPath, string shortcutName = null, string description = null, string iconLocation = null)
        {
            string desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory); // Get directory of desktop.
            GenerateShortcut(desktopDirectory, targetPath, shortcutName, description, iconLocation);
        }
        #endregion

        #region Notification area icon settings.
        /// <summary>
        /// Create icon to be shown in the notification area.
        /// </summary>
        /// <history time="2018/02/26">create this method</history>
        private void CreateNotifyIcon()
        {
            _notifyIcon.Text = "家庭事务管理系统-让生活井然有序！";
            _notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(SysWinForm.Application.ExecutablePath);
            _notifyIcon.Visible = true;

            // Show main window.
            SysWinForm.MenuItem miShow = new SysWinForm.MenuItem("显示主窗口");
            miShow.Click += new EventHandler(ShowMainWindow);

            // Exit system.
            SysWinForm.MenuItem miExit = new SysWinForm.MenuItem("退出");
            miExit.Click += new EventHandler(Exit);

            SysWinForm.MenuItem[] childen = new SysWinForm.MenuItem[] { miShow, miExit };
            _notifyIcon.ContextMenu = new SysWinForm.ContextMenu(childen);
            _notifyIcon.MouseClick += new SysWinForm.MouseEventHandler((obj, arg) => // MouseClick or MouseDoubleClick
            {
                if (arg.Button == SysWinForm.MouseButtons.Left)
                {
                    this.SwitchWindowState(obj, arg);
                }
            });
        }

        /// <summary>
        /// Switch window's state.
        /// </summary>
        /// <history time="2018/02/26">create this method</history>
        private void SwitchWindowState(object obj, SysWinForm.MouseEventArgs arg)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            }
            else if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Minimized;
            }
        }

        /// <summary>
        /// Get default window's state when minimized(i.e., minimized to toolbar or notification area).
        /// </summary>
        /// <history time="2018/02/26">create this method</history>
        /// <returns>true-minimized to notification area, false-minimized to toolbar</returns>
        private bool GetMinimizedState()
        {
            // Read in config file.
            return true;
        }

        /// <summary>
        /// Run this method when this window's state is changed.
        /// </summary>
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                if (_isMinimizedToNotificationArea)
                {
                    this.Hide();
                }
            }
        }

        /// <summary>
        /// Show main window.
        /// </summary>
        private void ShowMainWindow(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        /// <summary>
        /// Exit system.
        /// </summary>
        private void Exit(object sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
            MiExitRightNow_Click(sender, new RoutedEventArgs());
        }
        #endregion

        /// <summary>
        /// Refresh date & time in the status bar.
        /// </summary>
        private void CurrDateTimer_Tick(object sender, EventArgs e)
        {
            string curDateTime = "";
            curDateTime = DateTime.Now.ToString("yyyy-MM-dd") + "  "; // Get year, month, day.
            curDateTime += DateTime.Now.ToString("HH:mm:ss") + "  "; // Get hour, minute, second.
            curDateTime += DateTime.Now.ToString("dddd", new System.Globalization.CultureInfo("zh-cn")); // Get week.("en-us", "zh-cn", "ar-iq", "de-de")
            this.labelCurrDateTime.Content = curDateTime;
        }

        /// <summary>
        /// Navigation button mouse enter.
        /// </summary>
        private void NaviButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Image imgButton = sender as Image;

            if (!NavigationButton.GetClickedState(imgButton))
            {
                imgButton.Opacity = 1;

                DoubleAnimation daWidth = new DoubleAnimation();
                DoubleAnimation daHeight = new DoubleAnimation();

                // Specify start and end points.
                daWidth.From = 20;
                daWidth.To = 30;
                daHeight.From = 20;
                daHeight.To = 30;

                // Specify time duration.
                Duration duration = new Duration(TimeSpan.FromMilliseconds(150));
                daWidth.Duration = duration;
                daHeight.Duration = duration;

                // Apply animation to specified row.
                imgButton.BeginAnimation(Image.WidthProperty, daWidth);
                imgButton.BeginAnimation(Image.HeightProperty, daHeight);
            }
        }

        /// <summary>
        /// Navigation button mouse leave.
        /// </summary>
        private void NaviButton_MouseLeave(object sender, MouseEventArgs e)
        {
            Image imgButton = sender as Image;

            if (!NavigationButton.GetClickedState(imgButton))
            {
                DoubleAnimation daWidth = new DoubleAnimation();
                DoubleAnimation daHeight = new DoubleAnimation();

                // Specify start and end points.
                daWidth.From = 30;
                daWidth.To = 20;
                daHeight.From = 30;
                daHeight.To = 20;

                // Specify time duration.
                Duration duration = new Duration(TimeSpan.FromMilliseconds(150));
                daWidth.Duration = duration;
                daHeight.Duration = duration;

                // Apply animation to specified row.
                imgButton.BeginAnimation(Image.WidthProperty, daWidth);
                imgButton.BeginAnimation(Image.HeightProperty, daHeight);

                imgButton.Opacity = 0.5;
            }
        }

        /// <summary>
        /// Navigation button mouse left button down (i.e., single click).
        /// </summary>
        private void NaviButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image imgButton = sender as Image;

            if (e.ClickCount == 1 && !NavigationButton.GetClickedState(imgButton))
            {
                foreach (Image imgNaviButton in _imgNaviButtonList)
                {
                    if (NavigationButton.GetClickedState(imgNaviButton))
                    {
                        // Close current content frame.
                        switch (imgNaviButton.Name)
                        {
                            case "imgHome":
                                //_home.CloseProc();
                                break;
                            case "imgFinance":
                                //_finance.CloseProc();
                                break;
                            case "imgAccounts":
                                //_accounts.CloseProc();
                                break;
                            case "imgContacts":
                                //_contacts.CloseProc();
                                break;
                            case "imgDocuments":
                                //_documents.CloseProc();
                                break;
                            case "imgPictures":
                                //_pictures.CloseProc();
                                break;
                            case "imgVideos":
                                //_videos.CloseProc();
                                break;
                            case "imgMails":
                                //_mails.CloseProc();
                                break;
                            case "imgToolkit":
                                //_toolkit.CloseProc();
                                break;
                        }
                        NavigationButton.SetClickedState(imgNaviButton, false);
                        NaviButton_MouseLeave(imgNaviButton, e);
                    }
                }

                switch (imgButton.Name)
                {
                    case "imgHome":
                        this.frameContent.Content = _home;
                        break;
                    case "imgFinance":
                        this.frameContent.Content = _finance;
                        break;
                    case "imgAccounts":
                        // Lock the page.
                        if (_autoUnlock)
                        {
                            this.frameContent.Content = _accounts;
                        }
                        else
                        {
                            Locker locker = new Locker("Accounts", _pwd, _unlockSaved); // <2020/03/04, add>
                            locker.hUnlockPage = new Locker.UnlockPageHandler(UnlockPage);
                            this.frameContent.Content = locker;
                        }
                        break;
                    case "imgContacts":
                        this.frameContent.Content = _contacts;
                        break;
                    case "imgDocuments":
                        //this.frameContent.Content = _documents;
                        // Lock the page.
                        if (_autoUnlock)
                        {
                            this.frameContent.Content = _documents;
                        }
                        else
                        {
                            Locker locker = new Locker("Documents", _pwd, _unlockSaved);
                            locker.hUnlockPage = new Locker.UnlockPageHandler(UnlockPage);
                            this.frameContent.Content = locker;
                        }
                        break;
                    case "imgPictures":
                        this.frameContent.Content = _pictures;
                        break;
                    case "imgVideos":
                        this.frameContent.Content = _videos;
                        break;
                    case "imgMails":
                        this.frameContent.Content = _mails;
                        break;
                    case "imgToolkit":
                        this.frameContent.Content = _toolkit;
                        break;
                }

                NavigationButton.SetClickedState(imgButton, true);

                // Reset status info.
                UpdateStatusInfo("");
            }
        }

        /// <summary>
        /// Unlock page.
        /// </summary>
        /// <param name="pageName">name of page to be unlocked</param>
        private void UnlockPage(string pageName)
        {
            switch (pageName)
            {
                case "Home":
                    this.frameContent.Content = _home;
                    break;
                case "Finance":
                    this.frameContent.Content = _finance;
                    break;
                case "Accounts":
                    this.frameContent.Content = _accounts;
                    break;
                case "Contacts":
                    this.frameContent.Content = _contacts;
                    break;
                case "Documents":
                    this.frameContent.Content = _documents;
                    break;
                case "Pictures":
                    this.frameContent.Content = _pictures;
                    break;
                case "Videos":
                    this.frameContent.Content = _videos;
                    break;
                case "Mails":
                    this.frameContent.Content = _mails;
                    break;
                case "Toolkit":
                    this.frameContent.Content = _toolkit;
                    break;
            }
        }

        /// <summary>
        /// Update status info.
        /// </summary>
        void UpdateStatusInfo(string infoStr)
        {
            this.labelStatusInfo.Content = infoStr;
        }

        /// <summary>
        /// Drag and move the main window.
        /// </summary>
        private void StatusBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// Options.
        /// </summary>
        /// <history time="2018/03/10">create this method</history>
        private void MiSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingWin win = new SettingWin();
            win.ShowDialog();
        }

        private void MiAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWin win = new AboutWin();
            win.ShowDialog();
        }

        /// <summary>
        /// Exit system right now.
        /// </summary>
        /// <history time="2018/02/25">create this method</history>
        private void MiExitRightNow_Click(object sender, RoutedEventArgs e)
        {
            _notifyIcon.Dispose();
            this.Close(); // It may not terminate other threads in multi-thread cases(e.g., sub window was opened)!
        }

        /// <summary>
        /// Exit system after a period of time.
        /// </summary>
        private void MiExitDelay_Click(object sender, RoutedEventArgs e)
        {
            _counter = 60; // seconds
            this.labelCountDown.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler(ExitDelay_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            timer.Start();
        }

        private int _counter = 0; // Countdown counter
        private void ExitDelay_Tick(object sender, EventArgs e)
        {
            string prompt = "系统将在" + _counter.ToString() + "秒后退出！          ";
            this.labelCountDown.Content = prompt;

            if (_counter == 0)
            {
                _notifyIcon.Dispose();
                this.Close();
            }

            _counter -= 1;
        }

        /// <summary>
        /// Restart system.
        /// </summary>
        /// <history time="2018/03/02">create this method</history>
        private void MiRestart_Click(object sender, RoutedEventArgs e)
        {
            SysWinForm.Application.Restart();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Dispose resource.
        /// </summary>
        /// <history time="2020/02/26">create this method</param>
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _notifyIcon.Dispose();
        }
    }

    /// <summary>
    /// Attached property of navigation button.
    /// </summary>
    class NavigationButton : DependencyObject
    {
        public static bool GetClickedState(DependencyObject obj)
        {
            return (bool)obj.GetValue(ClickedStateProperty);
        }

        public static void SetClickedState(DependencyObject obj, bool value)
        {
            obj.SetValue(ClickedStateProperty, value);
        }

        // Using a DependencyProperty as the backing store for ClickedState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClickedStateProperty =
            DependencyProperty.RegisterAttached("ClickedState", typeof(bool), typeof(NavigationButton), new PropertyMetadata(false));
    }
}
