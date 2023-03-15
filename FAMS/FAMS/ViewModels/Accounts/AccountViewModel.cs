using System;
using System.ComponentModel;

namespace FAMS.ViewModels.Accounts
{
    class AccountViewModel : INotifyPropertyChanged
    {
        private string m_strAccountName;                                    // Account name
        private string m_strAccountType = "普通账号";                        // Account type ("普通账号", "财务账号", "工作账号", "政务账号")
        private string m_strURL;                                            // Website address
        private string m_strUserName;                                       // User name, i.e., login name
        private string m_strPassword;                                       // Password to login
        private string m_strDisplayName;                                    // Display name, i.e., nick name
        private string m_strEmail;                                          // Registration e-mail
        private string m_strTelephone;                                      // Registration telephone
        private string m_strPaymentCode;                                    // Payment code
        private string m_strAppendDate = DateTime.Now.ToShortDateString();  // Append date
        private string m_strLastRevised = DateTime.Now.ToShortDateString(); // Last revised date
        private string m_strFormerAccountNames;                             // Former accounts names
        private string m_strAttachmentFlag = "无";                          // Attachment flag("有", "无")
        private string m_strAttachedFileNames;                              // Attached file names with extension(without directory path)
        private string m_strRemarks;                                        // Remarks
        private string m_strKeyWords;                                       // Key words

        // DateTime.Now.ToString("yyyy-MM-dd") => "2020-03-11"
        // DateTime.Now.ToString("yyyy-M-d") => "2020-3-11"
        // DateTime.Now.ToShortDateString() => "2020/03/11"

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

        public string LastRevised
        {
            get { return m_strLastRevised; }
            set
            {
                m_strLastRevised = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("LastRevised"));
                }
            }
        }

        public string FormerAccountNames
        {
            get { return m_strFormerAccountNames; }
            set
            {
                m_strFormerAccountNames = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("FormerAccountNames"));
                }
            }
        }

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

        public string AttachedFileNames
        {
            get { return m_strAttachedFileNames; }
            set
            {
                m_strAttachedFileNames = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AttachedFileNames"));
                }
            }
        }

        public string Remarks
        {
            get { return m_strRemarks; }
            set
            {
                m_strRemarks = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Remarks"));
                }
            }
        }

        public string KeyWords
        {
            get { return m_strKeyWords; }
            set
            {
                m_strKeyWords = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("KeyWords"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
