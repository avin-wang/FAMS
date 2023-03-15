using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace FAMS.CustomWindow
{
    /// <summary>
    /// The class Window to be inherited below needs 3 necessary modules(references):
    /// WindowsBase.dll, PresentationCore.dll, PresentationFramework.dll
    /// </summary>
    public class CustomMainWindow : Window
    {
        ResourceDictionary _winStyle;
        double _orgWinLeft;
        double _orgWinTop;
        double _orgWinWidth;
        double _orgWinHeight;
        bool _isFullScreen = false;
        bool _isCloseStoryboardCompleted = false;

        public CustomMainWindow()
        {
            _winStyle = new ResourceDictionary();
            _winStyle.Source = new Uri("FAMS.CustomWindow;component/CustomMainWindowStyle.xaml", UriKind.Relative);
            this.Style = (System.Windows.Style)_winStyle["MainWindowStyle"];
            this.Loaded += CustomMainWindow_Loaded;
            this.Closing += new System.ComponentModel.CancelEventHandler(CustomMainWindow_Closing);
        }

        private void CustomMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Get control template of the window style applied to this CustomMainWindow.
            ControlTemplate ct = (ControlTemplate)_winStyle["MainWindowControlTemplate"];

            // Define operations on title bar.
            Border bd = (Border)ct.FindName("borderTitle", this);
            bd.MouseMove += BorderTitle_MouseMove; // Drag move the window.
            bd.MouseLeftButtonDown += Bd_MouseLeftButtonDown; // Full screen displaying.

            // Define minimize button click event handler.
            Button btMinimize = (Button)ct.FindName("buttonMinimize", this);
            btMinimize.Click += ButtonMinimize_Click;

            // Define close button click event handler.
            Button btClose = (Button)ct.FindName("buttonClose", this);
            btClose.Click += ButtonClose_Click;
        }

        /// <summary>
        /// Full screen.
        /// </summary>
        private void Bd_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (_orgWinWidth == 0)
                {
                    _orgWinLeft = this.Left;
                    _orgWinTop = this.Top;
                    _orgWinWidth = this.Width;
                    _orgWinHeight = this.Height;
                }

                if (!_isFullScreen)
                {
                    this.Topmost = false;
                    this.WindowState = System.Windows.WindowState.Normal;
                    this.ResizeMode = System.Windows.ResizeMode.NoResize;

                    this.Left = 0.0;
                    this.Top = 0.0;
                    this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
                    this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;

                    _isFullScreen = true;
                }
                else
                {
                    this.Topmost = false;
                    this.WindowState = System.Windows.WindowState.Normal;
                    this.ResizeMode = System.Windows.ResizeMode.CanResizeWithGrip;

                    this.Left = _orgWinLeft;
                    this.Top = _orgWinTop;
                    this.Width = _orgWinWidth;
                    this.Height = _orgWinHeight;

                    _isFullScreen = false;
                }
            }
        }

        /// <summary>
        /// Drag move.
        /// </summary>
        private void BorderTitle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// Minimize button click event.
        /// </summary>
        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Close button click event.
        /// </summary>
        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Window closing animation.
        /// </summary>
        void CustomMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DoubleAnimation daFirstHalf;
            DoubleAnimation daSecondHalf;

            if (!_isCloseStoryboardCompleted)
            {
                daFirstHalf = new DoubleAnimation();
                daFirstHalf.From = 1;
                daFirstHalf.To = 0.1;
                daFirstHalf.Duration = new Duration(TimeSpan.Parse("0:0:0.8"));

                daSecondHalf = new DoubleAnimation();
                daSecondHalf.From = 1;
                daSecondHalf.To = 0;
                daSecondHalf.Duration = new Duration(TimeSpan.Parse("0:0:0.8"));
                daSecondHalf.BeginTime = TimeSpan.Parse("0:0:0.25");
                daSecondHalf.Completed += new EventHandler(CustomMainWindow_CloseStoryboardCompleted);

                ScaleTransform st = new ScaleTransform();
                st.CenterX = this.Width / 2;
                st.CenterY = this.Height / 2;
                this.RenderTransform = st;
                st.BeginAnimation(ScaleTransform.ScaleYProperty, daFirstHalf);
                st.BeginAnimation(ScaleTransform.ScaleXProperty, daSecondHalf);

                e.Cancel = true;
            }
        }

        /// <summary>
        /// Close this window.
        /// </summary>
        void CustomMainWindow_CloseStoryboardCompleted(object sender, EventArgs e)
        {
            _isCloseStoryboardCompleted = true;
            this.Close();
        }
    }
}
