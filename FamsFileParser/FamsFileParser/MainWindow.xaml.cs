using FAMS.Services;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FamsFileParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CFamsFileHelper _ffHelper = new CFamsFileHelper();

        public MainWindow()
        {
            InitializeComponent();

            // Initialize *.fams file helper. Fix system config file path. <2020/03/04, add>
            _ffHelper.Init("system.fams");
            SetInitDefaultConfig(); // set initial default config. 

            // Test illegal data writting.
            TestIllegalData();
        }

        private void TestIllegalData()
        {
            try
            {
                _ffHelper.WriteData("music_albumn", "westlife", "♩");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                this.Close(); // if exception occurs, then exit
            }
        }

        /// <summary>
        /// Set initial default configuration parameters.
        /// Do not run this method in this solution! in stead, please create a new solution.
        /// </summary>
        /// <history time="2020/03/04">create this method</history>
        private void SetInitDefaultConfig()
        {
            // Write data.
            _ffHelper.WriteData("config", "autorun", "1");
            _ffHelper.WriteData("config", "acc_db_dir", "./database/"); // all directories end with charactor '/'
            _ffHelper.WriteData("config", "acc_db_name", "FAMS-DB-Account.accdb");
            _ffHelper.WriteData("config", "acc_db_pwd", "fams.pwd.0334.057"); // open password of the account database file "FAMS-DB-Account.accdb"
            _ffHelper.WriteData("config", "log_dir", "./log/");
            _ffHelper.WriteData("config", "log_name", "FAMS.log");
            _ffHelper.WriteData("sys_login_pwd", "public", "123");
            _ffHelper.WriteData("sys_login_pwd", "avin", "123");
            _ffHelper.WriteData("sys_login_pwd", "flin", "123");
            _ffHelper.WriteData("acc_login_pwd", "public", "123");
            _ffHelper.WriteData("acc_login_pwd", "avin", "avin");
            _ffHelper.WriteData("acc_login_pwd", "flin", "flin");

            // Get data.
            List<string> secNames = _ffHelper.GetNames();
            List<string> keyNames = _ffHelper.GetNames("sys_login_pwd");
            Dictionary<string, string> configs = _ffHelper.GetData("config");
            Dictionary<string, string> sysLoginPwds = _ffHelper.GetData("sys_login_pwd");
            Dictionary<string, string> accLoginPwds = _ffHelper.GetData("acc_login_pwd");
            string sysLoginPwd = _ffHelper.GetData("sys_login_pwd", "public");
            string accLoginPwd = _ffHelper.GetData("acc_login_pwd", "avin");
        }
    }
}
