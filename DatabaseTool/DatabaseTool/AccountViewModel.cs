using System;
using System.ComponentModel;

namespace FAMS.ViewModels.Accounts
{
    class AccountViewModel : INotifyPropertyChanged
    {
        private string m_strAccountName; // Account name.

        public string AccountName
        {
            get { return m_strAccountName; }
            set
            {
                m_strAccountName = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AccountName"));
                }
            }
        }

        private string m_strAccountType = "general"; // Account type("general", "financial", "work").

        public string AccountType
        {
            get { return m_strAccountType; }
            set
            {
                m_strAccountType = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AccountType"));
                }
            }
        }

        private string m_strURL; // Website address.

        public string URL
        {
            get { return m_strURL; }
            set
            {
                m_strURL = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("URL"));
                }
            }
        }

        private string m_strUserName; // User name, i.e., login name.

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

        private string m_strPassword; // Password to login.

        public string Password
        {
            get { return m_strPassword; }
            set
            {
                m_strPassword = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Password"));
                }
            }
        }

        private string m_strDisplayName; // Display name, i.e., nick name.

        public string DisplayName
        {
            get { return m_strDisplayName; }
            set
            {
                m_strDisplayName = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DisplayName"));
                }
            }
        }

        private string m_strEmail; // Registration e-mail.

        public string Email
        {
            get { return m_strEmail; }
            set
            {
                m_strEmail = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Email"));
                }
            }
        }

        private string m_strTelephone; // Registration telephone.

        public string Telephone
        {
            get { return m_strTelephone; }
            set
            {
                m_strTelephone = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Telephone"));
                }
            }
        }

        private string m_strPaymentCode; // Payment code.

        public string PaymentCode
        {
            get { return m_strPaymentCode; }
            set
            {
                m_strPaymentCode = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("PaymentCode"));
                }
            }
        }

        private string m_strAppendDate = DateTime.Now.ToString("yyyy-MM-dd"); // Append date.

        public string AppendDate
        {
            get { return m_strAppendDate; }
            set
            {
                m_strAppendDate = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AppendDate"));
                }
            }
        }

        private string m_strAttachmentFlag = "no"; // Attachment flag("yes", "no").

        public string AttachmentFlag
        {
            get { return m_strAttachmentFlag; }
            set
            {
                m_strAttachmentFlag = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AttachmentFlag"));
                }
            }
        }

        private string m_strAttachedFileName; // Attached file name with extension(without directory path).

        public string AttachedFileName
        {
            get { return m_strAttachedFileName; }
            set
            {
                m_strAttachedFileName = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AttachedFileName"));
                }
            }
        }

        private string m_strRemark; // Remark.

        public string Remark
        {
            get { return m_strRemark; }
            set
            {
                m_strRemark = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Remark"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
