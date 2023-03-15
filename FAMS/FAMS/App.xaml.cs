using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using FAMS.Views;
using FAMS.Views.Accounts;
using FAMS.Services;
using System.IO;
using FAMS.Views.Documents;

namespace FAMS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private System.Threading.Mutex _mutex;

        private DispatcherTimer _lostFocusTimer; // Start timing when this app loses focus
        private DispatcherTimer _mouseMotionlessTimer; // Start timing when mouse is motionless in a long time
        private Point _mousePos; // Mouse's(also cursor's) last position
        private int _detectCount = 0; // Count of detecting whether the mouse is motionless

        private Frame _frameContent = null;
        private Accounts _accounts = null;
        private Documents _documents = null;

        private CFamsFileHelper _ffHelper = new CFamsFileHelper();

        private string _pageUnlockPwd = null;
        private bool _pageUnlockSaved = false;
        private bool _pageUnlockAuto = false;
        private int _pageLockDelay = 0; // page locking delay time (in units of second)

        public App()
        {
            this.Startup += new StartupEventHandler(App_Startup); // Run system in singleton mode
        }

        #region Event handlers.
        /// <summary>
        /// Run this method when this application starts up
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e); // Application starts up
            _mousePos = GetMousePos(); // Get mouse's current position

            // When this app is activated
            _mouseMotionlessTimer = new DispatcherTimer();
            _mouseMotionlessTimer.Tick += new EventHandler(MouseMotionlessTimer_Tick);
            _mouseMotionlessTimer.Interval = new TimeSpan(0, 0, 1); // run detecting in every second
            _mouseMotionlessTimer.Start();

            // When this app is deactivated
            _lostFocusTimer = new DispatcherTimer(); // app deactivated timer
            _lostFocusTimer.Tick += new EventHandler(LostFocusTimer_Tick);
            _lostFocusTimer.Interval = new TimeSpan(0, 0, 1);

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\config"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\config");
            }

            _ffHelper.Init(Directory.GetCurrentDirectory() + "\\config\\system.fams");

            if (!File.Exists(Directory.GetCurrentDirectory() + "\\config\\system.fams"))
            {
                InitSysConfig(); // initialize systyem configurations
            }

            string lastLoginUser = _ffHelper.GetData("sys_login_user", "last_login_user"); // get last login user
            _pageUnlockPwd = _ffHelper.GetData("page_unlock_pwd", lastLoginUser); // get system page unlock password
            _pageUnlockSaved = _ffHelper.GetData("page_unlock_pwd_saved", lastLoginUser) == "1" ? true : false; // system page unlock password saved
            _pageUnlockAuto = _ffHelper.GetData("page_unlock_auto", lastLoginUser) == "1" ? true : false; // system page auto unlock
            _pageLockDelay = int.Parse(_ffHelper.GetData("page_lock_delay", lastLoginUser)); // get system page locking delay time
        }

        /// <summary>
        /// Initialize systyem configurations
        /// </summary>
        private void InitSysConfig()
        {
            // general config
            _ffHelper.WriteData("config", "autorun", "1"); // system autorun ("1"-autorun, "0"-not autorun)

            // database config
            _ffHelper.WriteData("config", "acc_db_dir", "\\acc"); // account database directory (all directories end without charactor '\\'!)
            _ffHelper.WriteData("config", "acc_db_name", "FAMS-DB-Account.accdb"); // account database file name
            _ffHelper.WriteData("config", "acc_db_pwd", "fams.pwd.0334.057"); // account database file (FAMS-DB-Account.accdb) password
            _ffHelper.WriteData("config", "acc_attach_dir", "\\acc\\attached_files"); // account attached file directory

            // log config
            _ffHelper.WriteData("config", "log_dir", "\\log"); // log file directory
            _ffHelper.WriteData("config", "log_name", "FAMS.log"); // log file name
            _ffHelper.WriteData("config", "update_log_name", "FAMS-Log-Update.fams"); // update log file name
            _ffHelper.WriteData("config", "todo_log_name", "FAMS-Log-Todo.fams"); // todo log file name

            // document config
            _ffHelper.WriteData("config", "doc_dir", "\\doc"); // document directory
            _ffHelper.WriteData("config", "doc_attach_dir", "\\doc\\attached_files"); // document attached file directory

            // login config
            _ffHelper.WriteData("sys_login_pwd", "public", "123"); // system login password for user "public"
            _ffHelper.WriteData("sys_login_pwd_saved", "public", "1"); // if save the password: "1"-yes, "0"-no
            _ffHelper.WriteData("sys_login_auto", "public", "0"); // if auto login to the system: "1"-yes, "0"-no
            _ffHelper.WriteData("sys_login_user", "last_login_user", "public"); // record last login user, so as to get page locking delay time when app is on

            // page locking config
            _ffHelper.WriteData("page_unlock_pwd", "public", "123"); // page unlock password for user "public"
            _ffHelper.WriteData("page_unlock_pwd_saved", "public", "1"); // if save the page unlock passowrd: "1"-yes, "0"-no
            _ffHelper.WriteData("page_unlock_auto", "public", "0"); // if auto unlock all system pages: "1"-yes, "0"-no
            _ffHelper.WriteData("page_lock_delay", "public", "300"); // page locking delay time. (in unit of second, e.g., 300s by default)
        }

        /// <summary>
        /// Run system in singleton mode
        /// </summary>
        /// <history time="2018/03/02">create this method</history>
        void App_Startup(object sender, StartupEventArgs e)
        {
            bool createdNew;
            _mutex = new System.Threading.Mutex(true, "FAMS", out createdNew);
            if (!createdNew)
            {
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Run this method when this app is activated
        /// </summary>
        private void App_Activated(object sender, EventArgs e)
        {
            _mouseMotionlessTimer.Start();
            _lostFocusTimer.Stop();
        }

        /// <summary>
        /// Run this method when this app is deactivated
        /// </summary>
        private void App_Deactivated(object sender, EventArgs e)
        {
            _mouseMotionlessTimer.Stop();
            _lostFocusTimer.Start();
        }
        #endregion

        /// <summary>
        /// Run this method in every second when this app is activated
        /// </summary>
        private void MouseMotionlessTimer_Tick(object sender, EventArgs e)
        {
            // only when the MainWindow (loginWin) is closed will the timer start counting
            if (!IsMouseMoved() && this.MainWindow == null && !_pageUnlockAuto)
            {
                // note that this.MainWindow is the LoginWin, which now is null since the LoginWin had been closed
                Grid grdMain = (this.Windows[0] as Window).Content as Grid;
                Grid grdFrame = grdMain.Children[1] as Grid;
                _frameContent = grdFrame.Children[0] as Frame;

                if (_frameContent.Content is Locker)
                {
                    _detectCount = 0; // Keep the count equal to 0 when current page is a Locker
                }
                else
                {
                    _detectCount++;
                }

                if (_detectCount == _pageLockDelay)
                {
                    _detectCount = 0;

                    if (_frameContent.Content is Accounts)
                    {
                        _accounts = _frameContent.Content as Accounts;
                        // Lock the page
                        Locker locker = new Locker("Accounts", _pageUnlockPwd, _pageUnlockSaved);
                        locker.hUnlockPage = new Locker.UnlockPageHandler(UnlockPage);
                        _frameContent.Content = locker;
                    }
                    else if (_frameContent.Content is Documents)
                    {
                        _documents = _frameContent.Content as Documents;
                        // Lock the page
                        Locker locker = new Locker("Documents", _pageUnlockPwd, _pageUnlockSaved);
                        locker.hUnlockPage = new Locker.UnlockPageHandler(UnlockPage);
                        _frameContent.Content = locker;
                    }
                }
            }
            else
            {
                _detectCount = 0; // Recount the detecting when the mouse's movement is detected
            }
        }

        /// <summary>
        /// Run this method when this app keep deactivated long enough
        /// time (time that is longer than _pageLockDelay)
        /// </summary>
        private void LostFocusTimer_Tick(object sender, EventArgs e)
        {
            if (this.MainWindow == null && !_pageUnlockAuto) // only when the MainWindow (loginWin) is closed will the timer start counting
            {
                // note that this.MainWindow is the LoginWin, which now is null since the LoginWin had been closed
                Grid grdMain = (this.Windows[0] as Window).Content as Grid;
                Grid grdFrame = grdMain.Children[1] as Grid;
                _frameContent = grdFrame.Children[0] as Frame;

                if (_frameContent.Content is Locker)
                {
                    _detectCount = 0; // Keep the count equal to 0 when current page is a Locker
                }
                else
                {
                    _detectCount++;
                }

                if (_detectCount == _pageLockDelay)
                {
                    _detectCount = 0;

                    if (_frameContent.Content is Accounts)
                    {
                        _accounts = _frameContent.Content as Accounts;
                        // Lock the page
                        Locker locker = new Locker("Accounts", _pageUnlockPwd, _pageUnlockSaved);
                        locker.hUnlockPage = new Locker.UnlockPageHandler(UnlockPage);
                        _frameContent.Content = locker;
                    }
                    else if (_frameContent.Content is Documents)
                    {
                        _documents = _frameContent.Content as Documents;
                        // Lock the page
                        Locker locker = new Locker("Documents", _pageUnlockPwd, _pageUnlockSaved);
                        locker.hUnlockPage = new Locker.UnlockPageHandler(UnlockPage);
                        _frameContent.Content = locker;
                    }
                }
            }
        }

        /// <summary>
        /// Unlock the page
        /// </summary>
        /// <param name="pageName">name of the page to be unlocked</param>
        private void UnlockPage(string pageName)
        {
            switch (pageName)
            {
                case "Home":
                    //_frameContent.Content = _home;
                    break;
                case "Finance":
                    //_frameContent.Content = _finance;
                    break;
                case "Accounts":
                    _frameContent.Content = _accounts;
                    break;
                case "Contacts":
                    //_frameContent.Content = _contacts;
                    break;
                case "Documents":
                    _frameContent.Content = _documents;
                    break;
                case "Pictures":
                    //_frameContent.Content = _pictures;
                    break;
                case "Videos":
                    //_frameContent.Content = _videos;
                    break;
                case "Mails":
                    //_frameContent.Content = _mails;
                    break;
                case "Toolkit":
                    //_frameContent.Content = _toolkit;
                    break;
            }
        }

        /// <summary>
        /// Check if the mouse is moved
        /// </summary>
        /// <returns>true-yes, false-no</returns>
        private bool IsMouseMoved()
        {
            Point point = GetMousePos();
            if (point == _mousePos)
            {
                return false;
            }
            _mousePos = point;
            return true;
        }

        /// <summary>
        /// Get current position of the mouse
        /// </summary>
        /// <returns>the mouse's coordinate in Point</returns>
        public Point GetMousePos()
        {
            MousePos mp = new MousePos();
            GetCursorPos(out mp);
            Point p = new Point(mp.X, mp.Y);
            return p;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetCursorPos(out MousePos mp); // Get the cursor's position

        [StructLayout(LayoutKind.Sequential)]
        private struct MousePos
        {
            public int X;
            public int Y;

            public MousePos(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }
    }
}
