using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FAMS.ViewModels.Documents
{
    public class DocViewModel : INotifyPropertyChanged
    {
        private string m_strDocTitle = "Untitled";                        // doc title
        private string m_strOriginalTitle = string.Empty;                 // original title
        private string m_strSource = string.Empty;                        // doc source (original or digested)
        private string m_strCategory = "Uncategorized";                   // notes category
        private string m_strAuthor = "Avin Wang";                         // author
        private string m_strCreateTime = DateTime.Now.ToString();         // create time
        private string m_strLastRevised = DateTime.Now.ToString();        // last revised time
        private string m_strFormerTitle = "Untitled";                     // use former title to mark whether the title is modified
        private List<string> m_lstAttachFileNames = new List<string>();   // attached file names (name+suffix)
        private List<string> m_lstAttachSourcePaths = new List<string>(); // source attached file paths (full path)
        private string m_strDocText = string.Empty;                       // doc content text

        public string DocTitle
        {
            get { return m_strDocTitle; }
            set
            {
                m_strDocTitle = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DocTitle"));
                }
            }
        }

        public string OriginalTitle
        {
            get { return m_strOriginalTitle; }
            set
            {
                m_strOriginalTitle = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("OriginalTitle"));
                }
            }
        }

        public string Source
        {
            get { return m_strSource; }
            set
            {
                m_strSource = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Source"));
                }
            }
        }

        // Category, e.g., health, work, music, auto, financial, life, education, study&research, affairs handling, etc
        // it is user customized
        public string Category
        {
            get { return m_strCategory; }
            set
            {
                m_strCategory = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Category"));
                }
            }
        }

        public string Author
        {
            get { return m_strAuthor; }
            set
            {
                m_strAuthor = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Author"));
                }
            }
        }

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

        public string FormerTitle
        {
            get { return m_strFormerTitle; }
            set
            {
                m_strFormerTitle = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("FormerTitle"));
                }
            }
        }

        public List<string> AttachFileNames
        {
            get { return m_lstAttachFileNames; }
            set
            {
                m_lstAttachFileNames = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AttachFileNames"));
                }
            }
        }

        public List<string> AttachSourcePaths
        {
            get { return m_lstAttachSourcePaths; }
            set
            {
                m_lstAttachSourcePaths = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AttachSourcePaths"));
                }
            }
        }

        public string DocText
        {
            get { return m_strDocText; }
            set
            {
                m_strDocText = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DocText"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
