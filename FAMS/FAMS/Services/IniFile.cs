using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FAMS.Services
{
    public class CIniFile
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(
            string lpAppName,   // Section name.
            string lpKeyName,   // Key name.
            string lpString,    // Value of the key.
            string lpFileName   // Destination file path.
            );
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(
            string lpAppName,   // Section name.
            string lpKeyName,   // Key name.
            string lpDefault,   // Default string.
            StringBuilder lpReturnedString,   // Destination buffer.
            int nSize,          // Size of destination buffer.
            string lpFileName   // File path specified.
            );
        [DllImport("kernel32")]
        private static extern uint GetPrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            byte[] lpReturnedString,
            uint nSize,
            string lpFileName
            );

        private string _iniPath;

        public CIniFile(string iniPath)
        {
            _iniPath = iniPath;
        }

        public void WriteValue(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, _iniPath);
        }

        public string ReadValue(string section, string key)
        {
            StringBuilder sb = new StringBuilder(255);
            int i = GetPrivateProfileString(section, key, "", sb, 255, _iniPath);
            return sb.ToString();
        }

        public List<string> GetAllSections()
        {
            List<string> result = new List<string>();
            byte[] buf = new byte[65536];
            uint len = GetPrivateProfileString(null, null, null, buf, (uint)buf.Length, _iniPath);
            int j = 0;
            for (int i = 0; i < len; i++)
            {
                if (buf[i] == 0)
                {
                    result.Add(Encoding.Default.GetString(buf, j, i - j));
                    j = i + 1;
                }
            }
            return result;
        }

        public List<string> GetAllKeys(string section)
        {
            List<string> result = new List<string>();
            byte[] buf = new byte[65536];
            uint len = GetPrivateProfileString(section, null, null, buf, (uint)buf.Length, _iniPath);
            int j = 0;
            for (int i = 0; i < len; i++)
            {
                if (buf[i] == 0)
                {
                    result.Add(Encoding.Default.GetString(buf, j, i - j));
                    j = i + 1;
                }
            }
            return result;
        }
    }
}
