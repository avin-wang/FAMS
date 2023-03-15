using System;
using System.ComponentModel;

namespace FAMS.ViewModels.Home
{
    class LogViewModel : INotifyPropertyChanged
    {
        // update log
        private string m_strCreateTime = DateTime.Now.ToString();      // create time (e.g., DateTime.Now.ToString() => "2020/3/16 10:50:25")
        private string m_strLastRevisedTime = DateTime.Now.ToString(); // last revised time
        private string m_strLogText = string.Empty;                    // text content

        public string CreateTime
        {
            get { return m_strCreateTime; }
            set
            {
                m_strCreateTime = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CreateTime"));
                }
            }
        }

        public string LastRevisedTime
        {
            get { return m_strLastRevisedTime; }
            set
            {
                m_strLastRevisedTime = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("LastRevisedTime"));
                }
            }
        }

        public string LogText
        {
            get { return m_strLogText; }
            set
            {
                m_strLogText = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("LogText"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
