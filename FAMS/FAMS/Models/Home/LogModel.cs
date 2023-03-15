using FAMS.Services;
using FAMS.ViewModels.Home;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAMS.Models.Home
{
    /// <summary>
    /// Update log & todo log model class
    /// </summary>
    class LogModel
    {
        private CFamsFileHelper _cfgHelper = new CFamsFileHelper(); // config file access
        private CFamsFileHelper _logHelper = new CFamsFileHelper(); // log file access
        private CLogWriter _logWriter = CLogWriter.GetInstance();

        private string _logDir = string.Empty;
        private string _updatePath = string.Empty; // update log file path
        private string _todoPath = string.Empty; // todo log file path

        /// <summary>
        /// Constructor.
        /// </summary>
        public LogModel()
        {
            // Initialize *.fams file helper (fix system config file path)
            _cfgHelper.Init(Directory.GetCurrentDirectory() + "\\config\\system.fams");

            // Get log file info
            string logDir = Directory.GetCurrentDirectory() + _cfgHelper.GetData("config", "log_dir");
            string logName = _cfgHelper.GetData("config", "log_name");

            // Initialize log writer
            if (!System.IO.Directory.Exists(logDir))
            {
                System.IO.Directory.CreateDirectory(logDir);
            }
            _logWriter.Init(logDir + "\\", logName);
            _logWriter.SetCurrLevel(5);

            // Initialize update & todo log file
            _updatePath = logDir + "\\" + _cfgHelper.GetData("config", "update_log_name");
            _todoPath = logDir + "\\" + _cfgHelper.GetData("config", "todo_log_name");

            // If file exists, access its copy version
            if (File.Exists(_updatePath))
            {
                File.Copy(_updatePath, _updatePath.Remove(_updatePath.Length - 5) + "-cache.fams");
                _updatePath = _updatePath.Remove(_updatePath.Length - 5) + "-cache.fams";
            }

            if (File.Exists(_todoPath))
            {
                File.Copy(_todoPath, _todoPath.Remove(_todoPath.Length - 5) + "-cache.fams");
                _todoPath = _todoPath.Remove(_todoPath.Length - 5) + "-cache.fams";
            }
        }

        #region Model functions
        /// <summary>
        /// Get update log
        /// </summary>
        /// <returns></returns>
        public LogViewModel GetUpdateLog()
        {
            LogViewModel vmLog = new LogViewModel();

            try
            {
                _logHelper.Init(_updatePath);

                if (File.Exists(_updatePath))
                {
                    vmLog.CreateTime = _logHelper.GetData("content", "create_time");
                    vmLog.LastRevisedTime = _logHelper.GetData("content", "last_revised_time");
                    vmLog.LogText = _logHelper.GetData("content", "log_text");
                }
                else
                {
                    _logHelper.WriteData("content", "create_time", vmLog.CreateTime);
                    _logHelper.WriteData("content", "last_revised_time", vmLog.LastRevisedTime);
                    _logHelper.WriteData("content", "log_text", vmLog.LogText);
                }
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("LogModel::GetUpdateLog >> get update log failed: " + ex.Message);
            }

            return vmLog;
        }

        /// <summary>
        /// Get todo log
        /// </summary>
        /// <returns></returns>
        public LogViewModel GetTodoLog()
        {
            LogViewModel vmLog = new LogViewModel();

            try
            {
                _logHelper.Init(_todoPath);

                if (File.Exists(_todoPath))
                {
                    vmLog.CreateTime = _logHelper.GetData("content", "create_time");
                    vmLog.LastRevisedTime = _logHelper.GetData("content", "last_revised_time");
                    vmLog.LogText = _logHelper.GetData("content", "log_text");
                }
                else
                {
                    _logHelper.WriteData("content", "create_time", vmLog.CreateTime);
                    _logHelper.WriteData("content", "last_revised_time", vmLog.LastRevisedTime);
                    _logHelper.WriteData("content", "log_text", vmLog.LogText);
                }
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("LogModel::GetTodoLog >> get todo log failed: " + ex.Message);
            }

            return vmLog;
        }

        /// <summary>
        /// Write update log
        /// </summary>
        /// <param name="vmLog">log view model instance</param>
        /// <returns></returns>
        public int WriteUpdateLog(LogViewModel vmLog)
        {
            try
            {
                _logHelper.Init(_updatePath);

                _logHelper.WriteData("content", "create_time", vmLog.CreateTime);
                _logHelper.WriteData("content", "last_revised_time", vmLog.LastRevisedTime);
                _logHelper.WriteData("content", "log_text", vmLog.LogText);

                // save update log file
                if (_updatePath.EndsWith("-cache.fams"))
                {
                    File.Copy(_updatePath, _updatePath.Remove(_updatePath.Length - 11) + ".fams", true);
                }
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("LogModel::WriteUpdateLog >> write update log failed: " + ex.Message);
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Write todo log
        /// </summary>
        /// <param name="vmLog">log view model instance</param>
        /// <returns></returns>
        public int WriteTodoLog(LogViewModel vmLog)
        {
            try
            {
                _logHelper.Init(_todoPath);

                _logHelper.WriteData("content", "create_time", vmLog.CreateTime);
                _logHelper.WriteData("content", "last_revised_time", vmLog.LastRevisedTime);
                _logHelper.WriteData("content", "log_text", vmLog.LogText);

                // save todo log file
                if (_todoPath.EndsWith("-cache.fams"))
                {
                    File.Copy(_todoPath, _todoPath.Remove(_todoPath.Length - 11) + ".fams", true);
                }
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("LogModel::WriteTodoLog >> write todo log failed: " + ex.Message);
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// When log file access ends, run this method to delete cache file
        /// </summary>
        public void Close()
        {
            try
            {
                if (_updatePath.EndsWith("-cache.fams"))
                {
                    File.Delete(_updatePath);
                }

                if (_todoPath.EndsWith("-cache.fams"))
                {
                    File.Delete(_todoPath);
                }
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("LogModel::Close >> delete cache file failed: " + ex.Message);
            }
        }
        #endregion
    }
}
