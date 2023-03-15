using Microsoft.Win32;

namespace FAMS.Services
{
    public class CRegEditor
    {
        private RegistryHive _hKey; // Registry hive.

        public CRegEditor(RegistryHive hKey = RegistryHive.CurrentUser)
        {
            _hKey = hKey;
        }

        public void CreateSubKey(string subKey)
        {
            RegistryKey bKey = null;
            switch (_hKey)
            {
                case RegistryHive.ClassesRoot:
                    bKey = Registry.ClassesRoot;
                    break;
                case RegistryHive.CurrentUser:
                    bKey = Registry.CurrentUser;
                    break;
                case RegistryHive.LocalMachine:
                    bKey = Registry.LocalMachine;
                    break;
                case RegistryHive.Users:
                    bKey = Registry.Users;
                    break;
                case RegistryHive.CurrentConfig:
                    bKey = Registry.CurrentConfig;
                    break;
            }

            if (!subKey.Contains(@"\"))
            {
                // When only sub key safe name is given, do adding under SOFTWARE by default.
                subKey = @"SOFTWARE\" + subKey;
            }

            RegistryKey sKey = bKey.CreateSubKey(subKey); // @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"
            sKey.Close();
            bKey.Close();
        }

        public void DeleteSubKey(string subKey)
        {
            RegistryKey bKey = null;
            switch (_hKey)
            {
                case RegistryHive.ClassesRoot:
                    bKey = Registry.ClassesRoot;
                    break;
                case RegistryHive.CurrentUser:
                    bKey = Registry.CurrentUser;
                    break;
                case RegistryHive.LocalMachine:
                    bKey = Registry.LocalMachine;
                    break;
                case RegistryHive.Users:
                    bKey = Registry.Users;
                    break;
                case RegistryHive.CurrentConfig:
                    bKey = Registry.CurrentConfig;
                    break;
            }

            if (!subKey.Contains(@"\"))
            {
                // When only sub key safe name is given, do deleting under SOFTWARE by default.
                subKey = @"SOFTWARE\" + subKey;
            }

            bKey.DeleteSubKey(subKey, false); // true/false - throw/do not throw exception when sub key is missing.
            bKey.Close();
        }

        public bool Exists(string subKey)
        {
            RegistryKey bKey = null;
            switch (_hKey)
            {
                case RegistryHive.ClassesRoot:
                    bKey = Registry.ClassesRoot;
                    break;
                case RegistryHive.CurrentUser:
                    bKey = Registry.CurrentUser;
                    break;
                case RegistryHive.LocalMachine:
                    bKey = Registry.LocalMachine;
                    break;
                case RegistryHive.Users:
                    bKey = Registry.Users;
                    break;
                case RegistryHive.CurrentConfig:
                    bKey = Registry.CurrentConfig;
                    break;
            }

            string subKeyName;
            if (subKey.Contains(@"\"))
            {
                subKeyName = subKey.Substring(subKey.LastIndexOf(@"\") + 1, subKey.Length - (subKey.LastIndexOf(@"\") + 1));
                subKey = subKey.Substring(0, subKey.LastIndexOf(@"\") + 1);
            }
            else
            {
                // When only sub key safe name is given, do searching under SOFTWARE by default.
                subKeyName = subKey;
                subKey = "SOFTWARE";
            }

            RegistryKey sKey = bKey.OpenSubKey(subKey);
            string[] subKeyNames = sKey.GetSubKeyNames(); // Get all sub keys' names in current sub key's parent key.
            foreach (string keyName in subKeyNames)
            {
                if (keyName == subKeyName)
                {
                    sKey.Close();
                    bKey.Close();
                    return true;
                }
            }
            sKey.Close();
            bKey.Close();
            return false;
        }


        public void CreateKeyValue(string subKey, string name, string value)
        {
            RegistryKey bKey = null;
            switch (_hKey)
            {
                case RegistryHive.ClassesRoot:
                    bKey = Registry.ClassesRoot;
                    break;
                case RegistryHive.CurrentUser:
                    bKey = Registry.CurrentUser;
                    break;
                case RegistryHive.LocalMachine:
                    bKey = Registry.LocalMachine;
                    break;
                case RegistryHive.Users:
                    bKey = Registry.Users;
                    break;
                case RegistryHive.CurrentConfig:
                    bKey = Registry.CurrentConfig;
                    break;
            }

            if (!subKey.Contains(@"\"))
            {
                // When only sub key safe name is given, do deleting under SOFTWARE by default.
                subKey = @"SOFTWARE\" + subKey;
            }

            RegistryKey sKey = bKey.OpenSubKey(subKey, true);
            sKey.SetValue(name, value, RegistryValueKind.String);
            sKey.Close();
            bKey.Close();
        }

        public string GetKeyValue(string subKey, string name)
        {
            RegistryKey bKey = null;
            switch (_hKey)
            {
                case RegistryHive.ClassesRoot:
                    bKey = Registry.ClassesRoot;
                    break;
                case RegistryHive.CurrentUser:
                    bKey = Registry.CurrentUser;
                    break;
                case RegistryHive.LocalMachine:
                    bKey = Registry.LocalMachine;
                    break;
                case RegistryHive.Users:
                    bKey = Registry.Users;
                    break;
                case RegistryHive.CurrentConfig:
                    bKey = Registry.CurrentConfig;
                    break;
            }

            if (!subKey.Contains(@"\"))
            {
                // When only sub key safe name is given, do deleting under SOFTWARE by default.
                subKey = @"SOFTWARE\" + subKey;
            }

            RegistryKey sKey = bKey.OpenSubKey(subKey, true);
            string value = sKey.GetValue(name).ToString();
            sKey.Close();
            bKey.Close();

            return value;
        }

        public void DeleteKeyValue(string subKey, string name)
        {
            RegistryKey bKey = null;
            switch (_hKey)
            {
                case RegistryHive.ClassesRoot:
                    bKey = Registry.ClassesRoot;
                    break;
                case RegistryHive.CurrentUser:
                    bKey = Registry.CurrentUser;
                    break;
                case RegistryHive.LocalMachine:
                    bKey = Registry.LocalMachine;
                    break;
                case RegistryHive.Users:
                    bKey = Registry.Users;
                    break;
                case RegistryHive.CurrentConfig:
                    bKey = Registry.CurrentConfig;
                    break;
            }

            if (!subKey.Contains(@"\"))
            {
                // When only sub key safe name is given, do deleting under SOFTWARE by default.
                subKey = @"SOFTWARE\" + subKey;
            }

            RegistryKey sKey = bKey.OpenSubKey(subKey, true);
            sKey.DeleteValue(name, false);
            sKey.Close();
            bKey.Close();
        }

        public bool Exists(string subKey, string name)
        {
            RegistryKey bKey = null;
            switch (_hKey)
            {
                case RegistryHive.ClassesRoot:
                    bKey = Registry.ClassesRoot;
                    break;
                case RegistryHive.CurrentUser:
                    bKey = Registry.CurrentUser;
                    break;
                case RegistryHive.LocalMachine:
                    bKey = Registry.LocalMachine;
                    break;
                case RegistryHive.Users:
                    bKey = Registry.Users;
                    break;
                case RegistryHive.CurrentConfig:
                    bKey = Registry.CurrentConfig;
                    break;
            }

            if (!subKey.Contains(@"\"))
            {
                // When only sub key safe name is given, do deleting under SOFTWARE by default.
                subKey = @"SOFTWARE\" + subKey;
            }

            RegistryKey sKey = bKey.OpenSubKey(subKey);
            string[] valueNames = sKey.GetValueNames();
            foreach (string valueName in valueNames)
            {
                if (valueName == name)
                {
                    sKey.Close();
                    bKey.Close();
                    return true;
                }
            }
            sKey.Close();
            bKey.Close();
            return false;
        }
    }
}
