using System;
using System.IO;
using System.Windows.Documents;
using FAMS.Commons.BaseClasses;
using FAMS.Services;
using FAMS.ViewModels.Home;
using FAMS.Models.Home;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;

namespace FAMS.Views.Home
{
    /// <summary>
    /// Interaction logic for AboutWin.xaml
    /// </summary>
    public partial class AboutWin : PopupWindowBase
    {
        private CFamsFileHelper _ffHelper = new CFamsFileHelper();
        private CLogWriter _logWriter = CLogWriter.GetInstance();

        private LogModel _logModel = new LogModel(); 

        public AboutWin()
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

            // Even handlers binding
            this.tbxUpdate.MouseDoubleClick += Tbx_MouseDoubleClick;
            this.tbxUpdate.KeyDown += TbxUpdate_KeyDown;
            this.tbxTodo.MouseDoubleClick += Tbx_MouseDoubleClick;
            this.tbxTodo.KeyDown += TbxTodo_KeyDown;

            // Load logs
            this.gbxUpdateLog.DataContext = _logModel.GetUpdateLog();
            this.gbxTodoLog.DataContext = _logModel.GetTodoLog();
        }

        /// <summary>
        /// Ctrl+S to save todo log
        /// </summary>
        private void TbxTodo_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Keyboard.IsKeyDown(Key.S))
            {
                BtnSaveTodoLog_Click(sender, e); // save todo log
            }
        }

        /// <summary>
        /// Ctrl+S to save update log
        /// </summary>
        private void TbxUpdate_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Keyboard.IsKeyDown(Key.S))
            {
                BtnSaveUpdateLog_Click(sender, e); // save update log
            }
        }

        /// <summary>
        /// Mouse left key double click to edit log
        /// </summary>
        private void Tbx_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                TextBox tbx = sender as TextBox;
                tbx.IsReadOnly = false;
                tbx.Focus();
                tbx.SelectionStart = 0; // move carnet to the text start
            }
        }

        /// <summary>
        /// Update log edit button click event handler
        /// </summary>
        private void BtnEditUpdateLog_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.tbxUpdate.IsReadOnly = false;
            this.tbxUpdate.Focus();
            //this.tbxUpdate.SelectionStart = 0; // move carnet to the text start
        }

        /// <summary>
        /// Update log save button click event handler
        /// </summary>
        private void BtnSaveUpdateLog_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.tbxUpdate.IsReadOnly)
            {
                return;
            }

            LogViewModel vm = this.gbxUpdateLog.DataContext as LogViewModel;
            vm.LastRevisedTime = DateTime.Now.ToString(); // e.g., DateTime.Now.ToString() => "2020/3/16 10:50:25"

            int ret = _logModel.WriteUpdateLog(vm);
            if (ret != 0)
            {
                MessageBox.Show("保存更新日志失败！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.tbxUpdate.IsReadOnly = true;
        }

        /// <summary>
        /// Todo log edit button click event handler
        /// </summary>
        private void BtnEditTodoLog_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.tbxTodo.IsReadOnly = false;
            this.tbxTodo.Focus();
            //this.tbxTodo.SelectionStart = 0; // move carnet to the text start
        }

        /// <summary>
        /// Todo log save button click event handler
        /// </summary>
        private void BtnSaveTodoLog_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.tbxTodo.IsReadOnly)
            {
                return;
            }

            LogViewModel vm = this.gbxTodoLog.DataContext as LogViewModel;
            vm.LastRevisedTime = DateTime.Now.ToString();

            int ret = _logModel.WriteTodoLog(vm);
            if (ret != 0)
            {
                MessageBox.Show("保存代办日志失败！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.tbxTodo.IsReadOnly = true;
        }

        private void AboutWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _logModel.Close();
        }
    }
}
