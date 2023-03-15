using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FAMS.Services
{
    /****************************************** About *.fams File Type ****************************************
     Intro:
           The *.fams file is a customed file type defined by FAMS's author (avinwang@163.com).
           It stores data coded as a string in a predetermined protocol.

     Data access Protocol:
           The original data is stored as a string which is constructed in a convention below:

           The data string is constructed as(use separator ";" and ","):
           section1,key1,value1;section2,key2,value2;...

           Example(use separator ";" and ","):
           config,db_filename,D:\Program Files\fams\database;config,log_filename,D:\Program Files\fams\log;login_pwd,
           avin,avin;login_pwd,flin,flin;acc_pwd,avin,fams.pwd.acc.avin;acc_pwd,flin,fams.pwd.acc.flin;

           Note that, the separators used to concatenate the data segments can also be specified as
           other characters, such as "♩" and "♪", etc.

           For the sake of security, the data string will be encrypted before written to the file.
    **********************************************************************************************************/

    public class CFamsFileHelper
    {
        private CEncryptor _encrypter = new CEncryptor();
        private string _fileName = null;
        private string _password = null;
        private int _size = 0;
        private List<FamsFileData> _dataList = new List<FamsFileData>();

        private string _separator1 = "♩"; // section separator
        private string _separator2 = "♪"; // key-value separator
        private char _sep1;
        private char _sep2;

        public CFamsFileHelper()
        {
        }

        /// <summary>
        /// Initialization.
        /// </summary>
        /// <param name="fileName">file path</param>
        /// <param name="password">encryption password of the file</param>
        /// <param name="size">buffer size</param>
        public void Init(string fileName, string password = null, int size = 0)
        {
            _sep1 = (_separator1.ToCharArray())[0];
            _sep2 = (_separator2.ToCharArray())[0];

            _fileName = fileName;
            _password = password;
            _size = size;

            if (File.Exists(fileName))
            {
                LoadData();
            }
        }

        /// <summary>
        /// Get all section names.
        /// </summary>
        /// <returns></returns>
        public List<string> GetNames()
        {
            List<string> names = new List<string>();

            foreach (FamsFileData data in _dataList)
            {
                if (!names.Contains(data.Section))
                {
                    names.Add(data.Section);
                }
            }

            return names;
        }

        /// <summary>
        /// Get all key names related to the specified section.
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        public List<string> GetNames(string sec)
        {
            List<string> names = new List<string>();

            foreach (FamsFileData data in _dataList)
            {
                if (data.Section == sec && !names.Contains(data.Key))
                {
                    names.Add(data.Key);
                }
            }

            return names;
        }

        /// <summary>
        /// Get data related to the specified section and key.
        /// </summary>
        /// <param name="sec">section name</param>
        /// <param name="key">key name</param>
        /// <returns></returns>
        public string GetData(string sec, string key)
        {
            if (_dataList.Count == 0)
            {
                return null;
            }

            foreach (FamsFileData data in _dataList)
            {
                if (data.Section == sec && data.Key == key)
                {
                    return data.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Get all key-value data involved in the specified section.
        /// </summary>
        /// <param name="sec">section name</param>
        /// <returns>key-value pairs</returns>
        public Dictionary<string, string> GetData(string sec)
        {
            Dictionary<string, string> dt = new Dictionary<string, string>();

            foreach (FamsFileData data in _dataList)
            {
                if (data.Section == sec)
                {
                    dt.Add(data.Key, data.Value);
                }
            }

            return dt;
        }

        /// <summary>
        /// Write data to the file. 
        /// </summary>
        /// <param name="sec">section name</param>
        /// <param name="key">key name</param>
        /// <param name="value">data value</param>
        /// <returns></returns>
        public int WriteData(string sec, string key, string value)
        {
            // Data validation.
            if (sec.Contains(_separator1) || sec.Contains(_separator2)
                || key.Contains(_separator1) || key.Contains(_separator2)
                || value.Contains(_separator1) || value.Contains(_separator2))
            {
                throw new System.Exception("Illegal charactors \"" + _separator1 + "\" \"" + _separator2 + "\" detected!");
            }

            // If already exists, remove it.
            foreach (FamsFileData data in _dataList)
            {
                if (data.Section == sec && data.Key == key)
                {
                    _dataList.Remove(data);
                    break;
                }
            }

            // Add current new data.
            FamsFileData dt = new FamsFileData();
            dt.Section = sec;
            dt.Key = key;
            dt.Value = value;
            _dataList.Add(dt);

            // Construct metadata string.
            StringBuilder sb = new StringBuilder();
            foreach (FamsFileData data in _dataList)
            {
                sb.Append(data.Section + _separator2 + data.Key + _separator2 + data.Value + _separator1);
            }

            // Write metadata to the file.
            //File.Delete(_fileName);
            _encrypter.EncryptFile(_fileName, sb.ToString());

            return 0;
        }

        /// <summary>
        /// Delete data related to the specified section and key.
        /// </summary>
        /// <param name="sec">section name</param>
        /// <param name="key">key name</param>
        /// <returns></returns>
        public int DeleteData(string sec, string key)
        {
            // Remove data item.
            foreach (FamsFileData data in _dataList)
            {
                if (data.Section == sec && data.Key == key)
                {
                    _dataList.Remove(data);
                    break;
                }
            }

            // Construct metadata string.
            StringBuilder sb = new StringBuilder();
            foreach (FamsFileData data in _dataList)
            {
                sb.Append(data.Section + _separator2 + data.Key + _separator2 + data.Value + _separator1);
            }

            // Write metadata to the file.
            _encrypter.EncryptFile(_fileName, sb.ToString());

            return 0;
        }

        /// <summary>
        /// Delete data related to the specified section.
        /// </summary>
        /// <param name="sec">section name</param>
        /// <returns></returns>
        public int DeleteData(string sec)
        {
            // Remove data item.
            foreach (FamsFileData data in _dataList)
            {
                if (data.Section == sec)
                {
                    _dataList.Remove(data);
                }
            }

            // Construct metadata string.
            StringBuilder sb = new StringBuilder();
            foreach (FamsFileData data in _dataList)
            {
                sb.Append(data.Section + _separator2 + data.Key + _separator2 + data.Value + _separator1);
            }

            // Write metadata to the file.
            _encrypter.EncryptFile(_fileName, sb.ToString());

            return 0;
        }

        /// <summary>
        /// Clear all data.
        /// </summary>
        /// <returns></returns>
        public int ClearData()
        {
            _dataList.Clear();
            _encrypter.EncryptFile(_fileName, "");

            return 0;
        }

        /// <summary>
        /// Determines whether the specified section exists.
        /// </summary>
        /// <param name="sec">section name</param>
        /// <returns></returns>
        public bool Exist(string sec)
        {
            foreach (FamsFileData data in _dataList)
            {
                if (data.Section == sec)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified key related to the specified section exists.
        /// </summary>
        /// <param name="sec">section name</param>
        /// <param name="key">key name</param>
        /// <returns></returns>
        public bool Exist(string sec, string key)
        {
            foreach (FamsFileData data in _dataList)
            {
                if (data.Section == sec && data.Key == key)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Load data.
        /// </summary>
        private void LoadData()
        {
            // Data structure: section_name1{_separator2}key_name1{_separator2}value{_separator1}section_name2{_separator2}key_name2{_separator2}value2{_separator1}...
            string metadata = _encrypter.DecryptFile(_fileName, _password, _size);
            if (!metadata.Contains(_separator1))
            {
                return;
            }

            metadata = metadata.Substring(0, metadata.LastIndexOf(_sep1)); // The last segment is invalid due to the encryption
            _dataList.Clear(); // Clear current data list

            // Parse data.
            string[] sections = metadata.Split(_sep1);
            for (int i = 0; i < sections.Length; i++)
            {
                string[] section = sections[i].Split(_sep2);
                FamsFileData data = new FamsFileData();
                data.Section = section[0];
                data.Key = section[1];
                data.Value = section[2];
                _dataList.Add(data);
            }
        }
    }

    public struct FamsFileData
    {
        public string Section; // Section name.
        public string Key;     // Key name.
        public string Value;   // Value.
    }
}
