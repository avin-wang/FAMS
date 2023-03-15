using System.ComponentModel;

namespace FAMS.ViewModels.Login
{
    public class UserInfoViewModel
    {
        // Login info
        private string m_strUserName;       // login username
        private string m_strLoginPassword;  // login password
        private bool m_bLoginSaved = false; // login password saved state
        private bool m_bAutoLogin = false;  // auto login

        // page unlock info
        private string m_strPagePassword;    // page locking password
        private bool m_bUnlockSaved = false; // page locking password saved state
        private bool m_bAutoUnlock = false;  // page auto unlock
        private int m_nPageLockDelay = 300;  // page locking delay time

        public string UserName
        {
            get { return m_strUserName; }
            set
            {
                m_strUserName = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("UserName"));
                }
            }
        }

        public string LoginPassword
        {
            get { return m_strLoginPassword; }
            set
            {
                m_strLoginPassword = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("LoginPassword"));
                }
            }
        }

        public bool LoginSaved
        {
            get { return m_bLoginSaved; }
            set
            {
                m_bLoginSaved = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("LoginSaved"));
                }
            }
        }

        public bool AutoLogin
        {
            get { return m_bAutoLogin; }
            set
            {
                m_bAutoLogin = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AutoLogin"));
                }
            }
        }

        public string PagePassword
        {
            get { return m_strPagePassword; }
            set
            {
                m_strPagePassword = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("PagePassword"));
                }
            }
        }

        public bool UnlockSaved
        {
            get { return m_bUnlockSaved; }
            set
            {
                m_bUnlockSaved = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("UnlockSaved"));
                }
            }
        }

        public bool AutoUnlock
        {
            get { return m_bAutoUnlock; }
            set
            {
                m_bAutoUnlock = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AutoUnlock"));
                }
            }
        }

        public int PageLockDelay
        {
            get { return m_nPageLockDelay; }
            set
            {
                m_nPageLockDelay = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("PageLockDelay"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return m_strUserName;
        }
    }
}
