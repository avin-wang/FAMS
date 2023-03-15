using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FAMS.Commons.BaseClasses
{
    /// <summary>
    /// The class Window to be inherited below needs 3 necessary modules(references):
    /// WindowsBase.dll, PresentationCore.dll, PresentationFramework.dll
    /// </summary>
    public class LoginWindowBase : Window
    {
        ResourceDictionary _winStyle;

        public LoginWindowBase()
        {
            _winStyle = new ResourceDictionary();
            _winStyle.Source = new Uri("../../Styles/WindowStyle.xaml", UriKind.Relative);
            this.Style = (System.Windows.Style)_winStyle["LoginWindowStyle"];
            this.Loaded += LoginWindowBase_Loaded;
        }

        private void LoginWindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            // Get control template of the window style.
            ControlTemplate ct = (ControlTemplate)_winStyle["LoginWindowControlTemplate"];

            // Define operations on title bar.
            Border bdTitle = (Border)ct.FindName("borderTitle", this);
            bdTitle.MouseMove += BorderTitle_MouseMove; // Drag move the window.

            // Define logo.
            Border bdLogo = (Border)ct.FindName("borderLogo", this);
            // 下面的logo文件载入方式为权益之策，该策略要求图标文件必须随程序一起拷贝，并放在exe所在目录下的Icons文件夹内，否则程序奔溃！后期再改善
            Uri uri = new Uri(System.IO.Directory.GetCurrentDirectory() + this.LogoPath, UriKind.Absolute);
            bdLogo.Background = new ImageBrush(new BitmapImage(uri)) { Stretch = Stretch.Uniform };
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



        public string LogoPath
        {
            get { return (string)GetValue(LogoPathProperty); }
            set { SetValue(LogoPathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LogoPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LogoPathProperty =
            DependencyProperty.Register("LogoPath", typeof(string), typeof(LoginWindowBase), new PropertyMetadata(null));


    }
}
