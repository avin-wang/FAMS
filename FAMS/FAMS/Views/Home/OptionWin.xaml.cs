using System.Windows;
using FAMS.Commons.BaseClasses;
using FAMS.ViewModels.Home;
using FAMS.Services;

namespace FAMS.Views.Home
{
    /// <summary>
    /// Interaction logic for OptionWin.xaml
    /// </summary>
    public partial class OptionWin : PopupWindowBase
    {
        private CFamsFileHelper _ffHelper = new CFamsFileHelper(); // <2020/03/04, add>
        private CLogWriter _logWriter = CLogWriter.GetInstance();

        //private GeneralViewModel _dcGeneral = new GeneralViewModel();
        private bool _autorun;

        public OptionWin()
        {
            InitializeComponent();
            InitializeContext();
        }

        /// <summary>
        /// Initialize context.
        /// </summary>
        /// <history time="2018/03/10">create this method</history>
        private void InitializeContext()
        {
            // Initialize *.fams file helper. Fix system config file path. <2020/03/04, add>
            _ffHelper.Init("./config/system.fams");

            // Initialize data context. <2020/03/04, modify>
            //_dcGeneral.AutoRuns = _ffHelper.GetData("config", "autorun") == "1" ? true : false;
            //_autorun = _dcGeneral.AutoRuns;
            _autorun = _ffHelper.GetData("config", "autorun") == "1" ? true : false;
            this.cbxAutoRuns.IsChecked = _autorun;
            //this.spGeneral.DataContext = _dcGeneral;

            // Initialize event handlers.
            this.cbxAutoRuns.Checked += CbxAutoRuns_Checked;
            this.cbxAutoRuns.Unchecked += CbxAutoRuns_Unchecked;
        }

        /// <summary>
        /// Set auto running.
        /// </summary>
        /// <history time="2018/03/10">create this method</history>
        private void CbxAutoRuns_Checked(object sender, RoutedEventArgs e)
        {
            if (_autorun != true)
            {
                _ffHelper.WriteData("config", "autorun", "1");
                //_dcGeneral.AutoRuns = true;
                _autorun = true;
                MessageBox.Show("Apply setting successfully! Restart to take effect.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Cancel auto running.
        /// </summary>
        /// <history time="2018/03/10">create this method</history>
        private void CbxAutoRuns_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_autorun != false)
            {
                _ffHelper.WriteData("config", "autorun", "0");
                //_dcGeneral.AutoRuns = false;
                _autorun = false;
                MessageBox.Show("Apply setting successfully! Restart to take effect.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
