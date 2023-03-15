using System.ComponentModel;

namespace FAMS.ViewModels.Accounts
{
    class AdvancedSearchViewModel : INotifyPropertyChanged
    {
        private string m_strAccountName;     // Account name
        private int m_nAccountType = 4;      // Account type (0-"普通账号", 1-"财务账号", 2-"工作账号", 3-"政务账号", 4-"全部")
        private string m_strURL;             // Website address
        private string m_strUserName;        // User name, i.e., login name
        private string m_strDisplayName;     // Display name, i.e., nick name
        private string m_strAppendDate;      // Append date
        private string m_strAppendDateFrom;  // Append date range - start date
        private string m_strAppendDateTo;    // Append date range - end date
        private string m_strLastRevised;     // Last revised date
        private string m_strRevisedDateFrom; // Last revised date range - start date
        private string m_strRevisedDateTo;   // Last revised date range - end date
        private int m_nAttachmentFlag = 2;   // Attachment flag (0-"无", 1-"有", 2-"全部")

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

        public int AccountType
        {
            get { return m_nAccountType; }
            set
            {
                m_nAccountType = value;
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

        public string AppendDateFrom
        {
            get { return m_strAppendDateFrom; }
            set
            {
                m_strAppendDateFrom = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AppendDateFrom"));
                }
            }
        }

        public string AppendDateTo
        {
            get { return m_strAppendDateTo; }
            set
            {
                m_strAppendDateTo = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AppendDateTo"));
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

        public string RevisedDateFrom
        {
            get { return m_strRevisedDateFrom; }
            set
            {
                m_strRevisedDateFrom = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("RevisedDateFrom"));
                }
            }
        }

        public string RevisedDateTo
        {
            get { return m_strRevisedDateTo; }
            set
            {
                m_strRevisedDateTo = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("RevisedDateTo"));
                }
            }
        }

        public int AttachmentFlag
        {
            get { return m_nAttachmentFlag; }
            set
            {
                m_nAttachmentFlag = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AttachmentFlag"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
