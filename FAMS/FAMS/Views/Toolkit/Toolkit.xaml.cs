using System;
using System.IO;
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
using FAMS.Services;

namespace FAMS.Views.Toolkit
{
    /// <summary>
    /// Interaction logic for Toolkit.xaml
    /// </summary>
    public partial class Toolkit : Page
    {
        private CFamsFileHelper _ffHelper = new CFamsFileHelper();
        private CLogWriter _logWriter = CLogWriter.GetInstance();

        public Toolkit()
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
        }

        private void btnLoanCalc_Click(object sender, RoutedEventArgs e)
        {
            LoanCalculatorWin win = new LoanCalculatorWin();
            win.ShowDialog();
        }
    }
}
