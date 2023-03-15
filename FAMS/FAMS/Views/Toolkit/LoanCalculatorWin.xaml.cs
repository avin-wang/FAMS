using FAMS.Commons.BaseClasses;
using FAMS.Models.Toolkit;
using FAMS.Services;
using FAMS.ViewModels.Toolkit;
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
using System.Windows.Shapes;

namespace FAMS.Views.Toolkit
{
    /// <summary>
    /// Interaction logic for LoanCalculatorWin.xaml
    /// </summary>
    public partial class LoanCalculatorWin : PopupWindowBase
    {
        private CFamsFileHelper _ffHelper = new CFamsFileHelper();
        private CLogWriter _logWriter = CLogWriter.GetInstance();
        private LoanCalculatorModel _model = new LoanCalculatorModel();

        public LoanCalculatorWin()
        {
            InitializeComponent();
            InitializeContext();
        }

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

            // Data binding
            this.DataContext = new LoanCalculatorViewModel();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.DataContext = _model.Calc(this.DataContext as LoanCalculatorViewModel);
        }

        private void CbxDownPayment_TextChanged(object sender, RoutedEventArgs e)
        {
            this.DataContext = _model.Calc(this.DataContext as LoanCalculatorViewModel);
        }
    }
}
