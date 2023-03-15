using System;
using System.IO;


namespace FAMS.Services
{
    public class CLogWriter
    {
        private FileStream m_fileStream = null;
        private StreamWriter m_streamWriter = null;
        private int m_nLogLevel = 2;
        private string m_strMode = "Debug";
        private string m_strLogName = "";
        private string m_strLogPath = "";
        private volatile static CLogWriter _instance = null;
        private static readonly object lockObj = new object();
        public Boolean IsInit = false;

        private CLogWriter()
        {
        }

        /// <summary>
        /// Get singleton instance.
        /// </summary>
        /// <returns></returns>
        public static CLogWriter GetInstance()
        {
            if (_instance == null)
            {
                lock (lockObj)
                {
                    if (_instance == null)
                    {
                        _instance = new CLogWriter();
                    }
                }
            }

            return _instance;
        }

        /// <summary>
        /// Initialize log info (create or open log file).
        /// </summary>
        /// <param name="logDir"></param>
        /// <param name="logName"></param>
        public void Init(string logDir, string logName)
        {
            m_strLogName = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + logName; // 2017-8-23-FAMS.log
            m_strLogPath = logDir + m_strLogName;
            this.IsInit = true;
        }

        /// <summary>
        /// Set current log level. (default is 2)
        /// </summary>
        /// <param name="level"></param>
        public void SetCurrLevel(int level)
        {
            m_nLogLevel = level; // Log level: 1-Fault, 2-Error, 3-Warning, 4-Debug, 5-Info
        }

        public void WriteFaultLog(string logText)
        {
            m_strMode = "Fault";
            WriteLog(1, logText);
        }

        public void WriteErrorLog(string logText)
        {
            m_strMode = "Error";
            WriteLog(2, logText);
        }

        public void WriteWarningLog(string logText)
        {
            m_strMode = "Warning";
            WriteLog(3, logText);
        }

        public void WriteDebugLog(string logText)
        {
            m_strMode = "Debug";
            WriteLog(4, logText);
        }

        public void WriteInfoLog(string logText)
        {
            m_strMode = "Info";
            WriteLog(5, logText);
        }

        private void WriteLog(int level, string logText)
		{
            string logMsgText;
            if (level > m_nLogLevel)
            {
                return;
            }
            
            if (m_strMode== "Warning")
            {
                logMsgText = "[" + m_strMode + "] [" + DateTime.Now.ToLongTimeString() + "]   " + logText;
            }
            else if (m_strMode == "Info")
            {
                logMsgText = "[" + m_strMode + "]    [" + DateTime.Now.ToLongTimeString() + "]   " + logText;
            }
            else
            {
                logMsgText = "[" + m_strMode + "]   [" + DateTime.Now.ToLongTimeString() + "]   " + logText;
            }

            if (File.Exists(m_strLogPath))
            {
                // Open in append mode while specified log file exists (append new log text to the end of the file).
                m_fileStream = File.Open(m_strLogPath, FileMode.Append);
            }
            else
            {
                m_fileStream = File.Create(m_strLogPath);
            }

            m_streamWriter = new StreamWriter(m_fileStream);
            if (m_streamWriter != null)
            {
                m_streamWriter.WriteLine(logMsgText);
                m_streamWriter.Flush(); // Clear all buffers.
                m_streamWriter.Close(); // Close current StreamWriter.
                m_fileStream.Close(); // Close current stream and release any resources.
            }
        }
    }
}
