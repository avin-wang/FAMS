using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using FAMS.Commons.BaseClasses;
using FAMS.Services;
using FAMS.ViewModels.Accounts;

namespace FAMS.Views.Accounts
{
    /// <summary>
    /// Interaction logic for AdvancedSearchWin.xaml
    /// </summary>
    public partial class AdvancedSearchWin : PopupWindowBase
    {
        private AdvancedSearchViewModel _dataContext = new AdvancedSearchViewModel();
        private CFamsFileHelper _ffHelper = new CFamsFileHelper();
        private CLogWriter _logWriter = CLogWriter.GetInstance();

        public AdvancedSearchWin()
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
            if (!System.IO.Directory.Exists(logDir))
            {
                System.IO.Directory.CreateDirectory(logDir);
            }
            _logWriter.Init(logDir + "\\", logName);
            _logWriter.SetCurrLevel(5);

            // Data binding
            this.DataContext = _dataContext;

            // Event handler registration
            this.cbxAppendDate.SelectionChanged += CbxAppendDate_SelectionChanged;
            this.dpkAppendDateFrom.Loaded += DpkDateFrom_Loaded;
            this.dpkAppendDateTo.Loaded += DpkDateTo_Loaded;
            this.dpkAppendDateFrom.SelectedDateChanged += DpkAppendDateFrom_SelectedDateChanged;
            this.dpkAppendDateTo.SelectedDateChanged += DpkAppendDateTo_SelectedDateChanged;
            this.cbxLastRevised.SelectionChanged += CbxRevisedDate_SelectionChanged;
            this.dpkRevisedDateFrom.Loaded += DpkDateFrom_Loaded;
            this.dpkRevisedDateTo.Loaded += DpkDateTo_Loaded;
            this.dpkRevisedDateFrom.SelectedDateChanged += DpkRevisedDateFrom_SelectedDateChanged;
            this.dpkRevisedDateTo.SelectedDateChanged += DpkRevisedDateTo_SelectedDateChanged;
            this.KeyDown += AdvancedSearchWin_KeyDown;
        }

        /// <summary>
        /// Append date selection changed
        /// </summary>
        private void CbxAppendDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (7 == this.cbxAppendDate.SelectedIndex)
            {
                UnfoldAppendDateCalendar();
            }
            else
            {
                FoldAppendDateCalendar();
            }
        }

        /// <summary>
        /// Append date from calendar selection changed
        /// </summary>
        private void DpkAppendDateFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_dataContext.AppendDateFrom == "" || _dataContext.AppendDateFrom == null)
            {
                return;
            }

            DateTime dtStart = new DateTime();
            try
            {
                dtStart = DateTime.Parse(_dataContext.AppendDateFrom);
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("AdvancedSearchWin::DpkAppendDateFrom_SelectedDateChanged >> " + ex.Message);
                _dataContext.AppendDateFrom = "";
                return;
            }

            if (_dataContext.AppendDateTo != null && _dataContext.AppendDateTo != "")
            {
                DateTime dtEnd = new DateTime();
                try
                {
                    dtEnd = DateTime.Parse(_dataContext.AppendDateTo);
                }
                catch (Exception ex)
                {
                    _logWriter.WriteErrorLog("AdvancedSearchWin::DpkAppendDateFrom_SelectedDateChanged >> " + ex.Message);
                    return;
                }

                if (dtStart > dtEnd)
                {
                    MessageBox.Show("开始日期不能晚于结束日期！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _dataContext.AppendDateFrom = "";
                }
            }
        }

        /// <summary>
        /// Append date to calendar selection changed
        /// </summary>
        private void DpkAppendDateTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_dataContext.AppendDateTo == "" || _dataContext.AppendDateTo == null)
            {
                return;
            }

            DateTime dtEnd = new DateTime();
            try
            {
                dtEnd = DateTime.Parse(_dataContext.AppendDateTo);
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("AdvancedSearchWin::DpkAppendDateTo_SelectedDateChanged >> " + ex.Message);
                _dataContext.AppendDateTo = "";
                return;
            }

            if (_dataContext.AppendDateFrom != null && _dataContext.AppendDateFrom != "")
            {
                DateTime dtStart = new DateTime();
                try
                {
                    dtStart = DateTime.Parse(_dataContext.AppendDateFrom);
                }
                catch (Exception ex)
                {
                    _logWriter.WriteErrorLog("AdvancedSearchWin::DpkAppendDateTo_SelectedDateChanged >> " + ex.Message);
                    return;
                }

                if (dtStart > dtEnd)
                {
                    MessageBox.Show("结束日期不能早于开始日期！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _dataContext.AppendDateTo = "";
                }
            }
        }

        /// <summary>
        /// Last revised date selection changed
        /// </summary>
        /// <history time="2020/03/04">create this method</history>
        private void CbxRevisedDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (7 == this.cbxLastRevised.SelectedIndex)
            {
                UnfoldRevisedDateCalendar();
            }
            else
            {
                FoldRevisedDateCalendar();
            }
        }

        /// <summary>
        /// Last revised date from calendar selection changed
        /// </summary>
        /// <history time="2020/03/04">create this method</history>
        private void DpkRevisedDateFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_dataContext.RevisedDateFrom == "" || _dataContext.RevisedDateFrom == null)
            {
                return;
            }

            DateTime dtStart = new DateTime();
            try
            {
                dtStart = DateTime.Parse(_dataContext.RevisedDateFrom);
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("AdvancedSearchWin::DpkRevisedDateFrom_SelectedDateChanged >> " + ex.Message);
                _dataContext.RevisedDateFrom = "";
                return;
            }

            if (_dataContext.RevisedDateTo != null && _dataContext.RevisedDateTo != "")
            {
                DateTime dtEnd = new DateTime();
                try
                {
                    dtEnd = DateTime.Parse(_dataContext.RevisedDateTo);
                }
                catch (Exception ex)
                {
                    _logWriter.WriteErrorLog("AdvancedSearchWin::DpkRevisedDateFrom_SelectedDateChanged >> " + ex.Message);
                    return;
                }

                if (dtStart > dtEnd)
                {
                    MessageBox.Show("开始日期不能晚于结束日期！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _dataContext.RevisedDateFrom = "";
                }
            }
        }

        /// <summary>
        /// Last revised date to calendar selection changed
        /// </summary>
        /// <history time="2020/03/04">create this method</history>
        private void DpkRevisedDateTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_dataContext.RevisedDateTo == "" || _dataContext.RevisedDateTo == null)
            {
                return;
            }

            DateTime dtEnd = new DateTime();
            try
            {
                dtEnd = DateTime.Parse(_dataContext.RevisedDateTo);
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("AdvancedSearchWin::DpkRevisedDateTo_SelectedDateChanged >> " + ex.Message);
                _dataContext.RevisedDateTo = "";
                return;
            }

            if (_dataContext.RevisedDateFrom != null && _dataContext.RevisedDateFrom != "")
            {
                DateTime dtStart = new DateTime();
                try
                {
                    dtStart = DateTime.Parse(_dataContext.RevisedDateFrom);
                }
                catch (Exception ex)
                {
                    _logWriter.WriteErrorLog("AdvancedSearchWin::DpkRevisedDateTo_SelectedDateChanged >> " + ex.Message);
                    return;
                }

                if (dtStart > dtEnd)
                {
                    MessageBox.Show("结束日期不能早于开始日期！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _dataContext.RevisedDateTo = "";
                }
            }
        }

        /// <summary>
        /// Unfold the append date calendar
        /// </summary>
        private void UnfoldAppendDateCalendar()
        {
            if (this.hidedRow1.Height != new GridLength(0))
            {
                return;
            }

            GridLengthAnimation glaHeight1 = new GridLengthAnimation();
            GridLengthAnimation glaHeight2 = new GridLengthAnimation();
            GridLengthAnimation glaHeight3 = new GridLengthAnimation();
            GridLengthAnimation glaHeight4 = new GridLengthAnimation();
            DoubleAnimation daDateFromHeight = new DoubleAnimation();
            DoubleAnimation daDateToHeight = new DoubleAnimation();

            glaHeight1.From = new GridLength(0);
            glaHeight1.To = new GridLength(30);
            glaHeight2.From = new GridLength(0);
            glaHeight2.To = new GridLength(10);
            glaHeight3.From = new GridLength(0);
            glaHeight3.To = new GridLength(30);
            glaHeight4.From = new GridLength(0);
            glaHeight4.To = new GridLength(10);
            daDateFromHeight.From = 0;
            daDateFromHeight.To = 30;
            daDateToHeight.From = 0;
            daDateToHeight.To = 30;

            Duration duration = new Duration(TimeSpan.FromMilliseconds(200));
            glaHeight1.Duration = duration;
            glaHeight2.Duration = duration;
            glaHeight3.Duration = duration;
            glaHeight4.Duration = duration;
            daDateFromHeight.Duration = duration;
            daDateToHeight.Duration = duration;

            this.hidedRow1.BeginAnimation(RowDefinition.HeightProperty, glaHeight1);
            this.hidedRow2.BeginAnimation(RowDefinition.HeightProperty, glaHeight2);
            this.hidedRow3.BeginAnimation(RowDefinition.HeightProperty, glaHeight3);
            this.hidedRow4.BeginAnimation(RowDefinition.HeightProperty, glaHeight4);
            this.dpkAppendDateFrom.BeginAnimation(DatePicker.HeightProperty, daDateFromHeight);
            this.dpkAppendDateTo.BeginAnimation(DatePicker.HeightProperty, daDateToHeight);
        }

        /// <summary>
        /// Fold the append date calendar
        /// </summary>
        private void FoldAppendDateCalendar()
        {
            if (this.hidedRow1.Height == new GridLength(0))
            {
                return;
            }

            GridLengthAnimation glaHeight1 = new GridLengthAnimation();
            GridLengthAnimation glaHeight2 = new GridLengthAnimation();
            GridLengthAnimation glaHeight3 = new GridLengthAnimation();
            GridLengthAnimation glaHeight4 = new GridLengthAnimation();
            DoubleAnimation daDateFromHeight = new DoubleAnimation();
            DoubleAnimation daDateToHeight = new DoubleAnimation();

            glaHeight1.From = new GridLength(30);
            glaHeight1.To = new GridLength(0);
            glaHeight2.From = new GridLength(10);
            glaHeight2.To = new GridLength(0);
            glaHeight3.From = new GridLength(30);
            glaHeight3.To = new GridLength(0);
            glaHeight4.From = new GridLength(10);
            glaHeight4.To = new GridLength(0);
            daDateFromHeight.From = 30;
            daDateFromHeight.To = 0;
            daDateToHeight.From = 30;
            daDateToHeight.To = 0;

            Duration duration = new Duration(TimeSpan.FromMilliseconds(200));
            glaHeight1.Duration = duration;
            glaHeight2.Duration = duration;
            glaHeight3.Duration = duration;
            glaHeight4.Duration = duration;
            daDateFromHeight.Duration = duration;
            daDateToHeight.Duration = duration;

            this.hidedRow1.BeginAnimation(RowDefinition.HeightProperty, glaHeight1);
            this.hidedRow2.BeginAnimation(RowDefinition.HeightProperty, glaHeight2);
            this.hidedRow3.BeginAnimation(RowDefinition.HeightProperty, glaHeight3);
            this.hidedRow4.BeginAnimation(RowDefinition.HeightProperty, glaHeight4);
            this.dpkAppendDateFrom.BeginAnimation(DatePicker.HeightProperty, daDateFromHeight);
            this.dpkAppendDateTo.BeginAnimation(DatePicker.HeightProperty, daDateToHeight);
        }

        /// <summary>
        /// Unfold the revised date calendar
        /// </summary>
        private void UnfoldRevisedDateCalendar()
        {
            if (this.hidedRow5.Height != new GridLength(0))
            {
                return;
            }

            GridLengthAnimation glaHeight1 = new GridLengthAnimation();
            GridLengthAnimation glaHeight2 = new GridLengthAnimation();
            GridLengthAnimation glaHeight3 = new GridLengthAnimation();
            GridLengthAnimation glaHeight4 = new GridLengthAnimation();
            DoubleAnimation daDateFromHeight = new DoubleAnimation();
            DoubleAnimation daDateToHeight = new DoubleAnimation();

            glaHeight1.From = new GridLength(0);
            glaHeight1.To = new GridLength(30);
            glaHeight2.From = new GridLength(0);
            glaHeight2.To = new GridLength(10);
            glaHeight3.From = new GridLength(0);
            glaHeight3.To = new GridLength(30);
            glaHeight4.From = new GridLength(0);
            glaHeight4.To = new GridLength(10);
            daDateFromHeight.From = 0;
            daDateFromHeight.To = 30;
            daDateToHeight.From = 0;
            daDateToHeight.To = 30;

            Duration duration = new Duration(TimeSpan.FromMilliseconds(200));
            glaHeight1.Duration = duration;
            glaHeight2.Duration = duration;
            glaHeight3.Duration = duration;
            glaHeight4.Duration = duration;
            daDateFromHeight.Duration = duration;
            daDateToHeight.Duration = duration;

            this.hidedRow5.BeginAnimation(RowDefinition.HeightProperty, glaHeight1);
            this.hidedRow6.BeginAnimation(RowDefinition.HeightProperty, glaHeight2);
            this.hidedRow7.BeginAnimation(RowDefinition.HeightProperty, glaHeight3);
            this.hidedRow8.BeginAnimation(RowDefinition.HeightProperty, glaHeight4);
            this.dpkRevisedDateFrom.BeginAnimation(DatePicker.HeightProperty, daDateFromHeight);
            this.dpkRevisedDateTo.BeginAnimation(DatePicker.HeightProperty, daDateToHeight);
        }

        /// <summary>
        /// Fold the revised date calendar
        /// </summary>
        private void FoldRevisedDateCalendar()
        {
            if (this.hidedRow5.Height == new GridLength(0))
            {
                return;
            }

            GridLengthAnimation glaHeight1 = new GridLengthAnimation();
            GridLengthAnimation glaHeight2 = new GridLengthAnimation();
            GridLengthAnimation glaHeight3 = new GridLengthAnimation();
            GridLengthAnimation glaHeight4 = new GridLengthAnimation();
            DoubleAnimation daDateFromHeight = new DoubleAnimation();
            DoubleAnimation daDateToHeight = new DoubleAnimation();

            glaHeight1.From = new GridLength(30);
            glaHeight1.To = new GridLength(0);
            glaHeight2.From = new GridLength(10);
            glaHeight2.To = new GridLength(0);
            glaHeight3.From = new GridLength(30);
            glaHeight3.To = new GridLength(0);
            glaHeight4.From = new GridLength(10);
            glaHeight4.To = new GridLength(0);
            daDateFromHeight.From = 30;
            daDateFromHeight.To = 0;
            daDateToHeight.From = 30;
            daDateToHeight.To = 0;

            Duration duration = new Duration(TimeSpan.FromMilliseconds(200));
            glaHeight1.Duration = duration;
            glaHeight2.Duration = duration;
            glaHeight3.Duration = duration;
            glaHeight4.Duration = duration;
            daDateFromHeight.Duration = duration;
            daDateToHeight.Duration = duration;

            this.hidedRow5.BeginAnimation(RowDefinition.HeightProperty, glaHeight1);
            this.hidedRow6.BeginAnimation(RowDefinition.HeightProperty, glaHeight2);
            this.hidedRow7.BeginAnimation(RowDefinition.HeightProperty, glaHeight3);
            this.hidedRow8.BeginAnimation(RowDefinition.HeightProperty, glaHeight4);
            this.dpkRevisedDateFrom.BeginAnimation(DatePicker.HeightProperty, daDateFromHeight);
            this.dpkRevisedDateTo.BeginAnimation(DatePicker.HeightProperty, daDateToHeight);
        }

        /// <summary>
        /// Modify the watermark of the begin date calendar
        /// </summary>
        private void DpkDateFrom_Loaded(object sender, RoutedEventArgs e)
        {
            var dp = sender as DatePicker;
            if (dp == null) return;

            var tb = GetChildOfType<DatePickerTextBox>(dp);
            if (tb == null) return;

            var wm = tb.Template.FindName("PART_Watermark", tb) as ContentControl;
            if (wm == null) return;

            wm.Content = "开始";
            wm.FontStyle = FontStyles.Italic;
        }

        /// <summary>
        /// Modify the watermark of the end date calendar
        /// </summary>
        private void DpkDateTo_Loaded(object sender, RoutedEventArgs e)
        {
            var dp = sender as DatePicker;
            if (dp == null) return;

            var tb = GetChildOfType<DatePickerTextBox>(dp);
            if (tb == null) return;

            var wm = tb.Template.FindName("PART_Watermark", tb) as ContentControl;
            if (wm == null) return;

            wm.Content = "结束";
            wm.FontStyle = FontStyles.Italic;
        }

        /// <summary>
        /// Get child of specified type of dependency object
        /// </summary>
        /// <typeparam name="T">type of child</typeparam>
        /// <param name="depObj">dependency object</param>
        /// <returns>child</returns>
        private static T GetChildOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        /// <summary>
        /// Search
        /// </summary>
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (this.cbxAppendDate.Text == "Select a date range...")
            {
                if (_dataContext.AppendDateFrom == null || _dataContext.AppendDateFrom == "")
                {
                    MessageBox.Show("提交日期\"开始\"不能为空！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (_dataContext.AppendDateTo == null || _dataContext.AppendDateTo == "")
                {
                    MessageBox.Show("提交日期\"结束\"不能为空！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (this.cbxLastRevised.Text == "Select a date range...")
            {
                if (_dataContext.RevisedDateFrom == null || _dataContext.RevisedDateFrom == "")
                {
                    MessageBox.Show("最后修改日期\"开始\"不能为空！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (_dataContext.RevisedDateTo == null || _dataContext.RevisedDateTo == "")
                {
                    MessageBox.Show("最后修改日期\"结束\"不能为空！", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            this.DialogResult = true;
        }

        /// <summary>
        /// Shortcuts
        /// </summary>
        private void AdvancedSearchWin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.DialogResult = true;
            }
            else if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
            }
        }

        /// <summary>
        /// Drag and move this window
        /// </summary>
        private void AdvancedSearchWin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch (Exception ex)
            {
                _logWriter.WriteWarningLog("AdvancedSearchWin::AdvancedSearchWin_MouseLeftButtonDown >> " + ex.Message);
            }
        }
    }
}
