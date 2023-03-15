using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace FAMS.Views
{
    /// <summary>
    /// Interaction logic for Locker.xaml
    /// </summary>
    public partial class Locker : Page
    {
        string _lockPageName = null;
        string _unlockCode = null;

        public UnlockPageHandler hUnlockPage;
        public delegate void UnlockPageHandler(string pageName);

        public Locker(string lockPageName, string unlockCode, bool savedPwd)
        {
            InitializeComponent();

            _lockPageName = lockPageName;
            _unlockCode = unlockCode;

            if (savedPwd)
            {
                this.pwdUnlockCode.Password = unlockCode;
                this.pwdUnlockCode.GetType()
                    .GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(this.pwdUnlockCode, new object[] { 0, unlockCode.Length }); // carnet to the end
            }

            this.pwdUnlockCode.AddHandler(TextBox.KeyDownEvent, new KeyEventHandler(UnlockCodeBox_KeyDown));
            this.pwdUnlockCode.Focus();
        }

        private void UnlockCodeBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BtnUnlock_Click(sender, e);
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.pwdUnlockCode.Password = "";
            }
        }

        private void BtnUnlock_Click(object sender, RoutedEventArgs e)
        {
            if (this.pwdUnlockCode.Password == _unlockCode)
            {
                if (hUnlockPage != null)
                {
                    hUnlockPage(_lockPageName);
                }
            }
            else
            {
                this.lbWarning.Opacity = 1;
            }
        }

        private void pwdUnlockCode_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.pwdUnlockCode.Password == "")
            {
                this.lbWarning.Opacity = 0;
            }
        }
    }
}
