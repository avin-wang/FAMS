using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using SysWin = System.Windows;
using SysWinForm = System.Windows.Forms;
using FAMS.Services;
using FAMS.Data.Accounts;
using FAMS.ViewModels.Accounts;

namespace FAMS.Models.Accounts
{
    class AccountsModel
    {
        private CFamsFileHelper _ffHelper = new CFamsFileHelper();
        private CAccessHelper _accHelper = new CAccessHelper();
        private CLogWriter _logWriter = CLogWriter.GetInstance();

        private bool _dbOpen = false;
        private List<Account> _accountList = new List<Account>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public AccountsModel()
        {
            // Initialize *.fams file helper (fix system config file path)
            _ffHelper.Init(Directory.GetCurrentDirectory() + "\\config\\system.fams");

            // Get log file info
            string logDir = Directory.GetCurrentDirectory() + _ffHelper.GetData("config", "log_dir");
            string logName = _ffHelper.GetData("config", "log_name");

            // Initialize log writer
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            _logWriter.Init(logDir + "\\", logName);
            _logWriter.SetCurrLevel(5);

            // Connect to database
            string fileName = Directory.GetCurrentDirectory() + _ffHelper.GetData("config", "acc_db_dir") + "\\" + _ffHelper.GetData("config", "acc_db_name");
            string pwd = _ffHelper.GetData("config", "acc_db_pwd");
            if (!File.Exists(fileName))
            {
                SysWin.MessageBox.Show("Database file not found!", "Error", SysWin.MessageBoxButton.OK, SysWin.MessageBoxImage.Error);
                _logWriter.WriteErrorLog("AccountsModel::Connect to database >> Database file not found!");

                return;
            }

            try
            {
                _dbOpen = _accHelper.Open(fileName, pwd);
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("AccountsModel::Connect to database >> " + ex.Message);
            }
        }

        #region Event handlers
        /// <summary>
        /// Load account data
        /// </summary>
        /// <param name="conditions">conditions for query</param>
        /// <returns></returns>
        public List<AccountViewModel> LoadAccountData(string conditions = null)
        {
            if (!_dbOpen)
            {
                return null;
            }

            // Get all fields' names of the account table.
            List<string> fieldNames = new List<string>();
            try
            {
                fieldNames = _accHelper.GetAllFieldNames("AccountTable");
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("AccountsModel::GetAllFieldNames >> " + ex.Message);
                return null;
            }

            // Get data set from the database.
            DataSet ds = DbQuery(fieldNames, conditions);
            if (ds == null)
            {
                return null;
            }

            // Read account data.
            _accountList.Clear();
            for (int i = 0; i < ds.Tables[0].Rows.Count; ++i)
            {
                Account account = new Account(); // with 16 properties
                foreach (string fieldName in fieldNames)
                {
                    switch (fieldName)
                    {
                        case "AccountName":
                            if (ds.Tables[0].Rows[i]["AccountName"] != DBNull.Value)
                            {
                                account.AccountName = ds.Tables[0].Rows[i]["AccountName"].ToString();
                            }
                            break;
                        case "AccountType":
                            if (ds.Tables[0].Rows[i]["AccountType"] != DBNull.Value)
                            {
                                account.AccountType = Convert.ToInt32(ds.Tables[0].Rows[i]["AccountType"]);
                            }
                            break;
                        case "URL":
                            if (ds.Tables[0].Rows[i]["URL"] != DBNull.Value)
                            {
                                account.URL = ds.Tables[0].Rows[i]["URL"].ToString();
                            }
                            break;
                        case "UserName":
                            if (ds.Tables[0].Rows[i]["UserName"] != DBNull.Value)
                            {
                                account.UserName = ds.Tables[0].Rows[i]["UserName"].ToString();
                            }
                            break;
                        case "Password":
                            if (ds.Tables[0].Rows[i]["Password"] != DBNull.Value)
                            {
                                account.Password = ds.Tables[0].Rows[i]["Password"].ToString();
                            }
                            break;
                        case "DisplayName":
                            if (ds.Tables[0].Rows[i]["DisplayName"] != DBNull.Value)
                            {
                                account.DisplayName = ds.Tables[0].Rows[i]["DisplayName"].ToString();
                            }
                            break;
                        case "Email":
                            if (ds.Tables[0].Rows[i]["Email"] != DBNull.Value)
                            {
                                account.Email = ds.Tables[0].Rows[i]["Email"].ToString();
                            }
                            break;
                        case "Telephone":
                            if (ds.Tables[0].Rows[i]["Telephone"] != DBNull.Value)
                            {
                                account.Telephone = ds.Tables[0].Rows[i]["Telephone"].ToString();
                            }
                            break;
                        case "PaymentCode":
                            if (ds.Tables[0].Rows[i]["PaymentCode"] != DBNull.Value)
                            {
                                account.PaymentCode = ds.Tables[0].Rows[i]["PaymentCode"].ToString();
                            }
                            break;
                        case "AppendDate":
                            if (ds.Tables[0].Rows[i]["AppendDate"] != DBNull.Value)
                            {
                                // (DateTime)ds.Tables[0].Rows[i]["AppendDate"] => {2017-05-02 0:00:00}	System.DateTime
                                account.AppendDate = ((DateTime)ds.Tables[0].Rows[i]["AppendDate"]).ToShortDateString();
                            }
                            break;
                        case "LastRevised":
                            if (ds.Tables[0].Rows[i]["LastRevised"] != DBNull.Value)
                            {
                                account.LastRevised = ((DateTime)ds.Tables[0].Rows[i]["LastRevised"]).ToShortDateString();
                            }
                            break;
                        case "FormerAccountNames":
                            if (ds.Tables[0].Rows[i]["FormerAccountNames"] != DBNull.Value)
                            {
                                account.FormerAccountNames = ds.Tables[0].Rows[i]["FormerAccountNames"].ToString();
                            }
                            break;
                        case "AttachmentFlag":
                            if (ds.Tables[0].Rows[i]["AttachmentFlag"] != DBNull.Value)
                            {
                                account.AttachmentFlag = Convert.ToInt32(ds.Tables[0].Rows[i]["AttachmentFlag"]);
                            }
                            break;
                        case "AttachedFileNames":
                            if (ds.Tables[0].Rows[i]["AttachedFileNames"] != DBNull.Value)
                            {
                                account.AttachedFileNames = ds.Tables[0].Rows[i]["AttachedFileNames"].ToString();
                            }
                            break;
                        case "Remarks":
                            if (ds.Tables[0].Rows[i]["Remarks"] != DBNull.Value)
                            {
                                account.Remarks = ds.Tables[0].Rows[i]["Remarks"].ToString();
                            }
                            break;
                        case "KeyWords":
                            if (ds.Tables[0].Rows[i]["KeyWords"] != DBNull.Value)
                            {
                                account.KeyWords = ds.Tables[0].Rows[i]["KeyWords"].ToString();
                            }
                            break;
                    }
                }
                _accountList.Add(account);
            }

            // Data conversion.
            List<AccountViewModel> vmAccounts = new List<AccountViewModel>();
            foreach (Account account in _accountList)
            {
                vmAccounts.Add(ConvertToViewModel(account));
            }

            return vmAccounts;
        }

        /// <summary>
        /// Search
        /// </summary>
        /// <param name="keyWord">key word</param>
        /// <param name="field">field name</param>
        /// <history time="2020/03/04">add: all fields searching/specified field searching</history>
        /// <returns></returns>
        public List<AccountViewModel> Search(string keyWord, string field = "全部")
        {
            if (keyWord != null && keyWord != "")
            {
                string conditions = "";

                if (field == "全部")
                {
                    conditions += "AccountTable.AccountName LIKE '%" + keyWord + "%'";
                    switch (keyWord.ToLower()) // Account type (0-"普通账号", 1-"财务账号", 2-"工作账号", 3-政务账号)
                    {
                        case "普通账号":
                            conditions += " OR AccountTable.AccountType LIKE '%0%'";
                            break;
                        case "财务账号":
                            conditions += " OR AccountTable.AccountType LIKE '%1%'";
                            break;
                        case "工作账号":
                            conditions += " OR AccountTable.AccountType LIKE '%2%'";
                            break;
                        case "政务账号":
                            conditions += " OR AccountTable.AccountType LIKE '%3%'";
                            break;
                    }
                    conditions += " OR AccountTable.URL LIKE '%" + keyWord + "%'";
                    conditions += " OR AccountTable.UserName LIKE '%" + keyWord + "%'";
                    conditions += " OR AccountTable.DisplayName LIKE '%" + keyWord + "%'";
                    conditions += " OR AccountTable.Email LIKE '%" + keyWord + "%'";
                    conditions += " OR AccountTable.Telephone LIKE '%" + keyWord + "%'";
                    conditions += " OR AccountTable.FormerAccountNames LIKE '%" + keyWord + "%'";
                    conditions += " OR AccountTable.Remarks LIKE '%" + keyWord + "%'";
                    conditions += " OR AccountTable.KeyWords LIKE '%" + keyWord + "%'";
                }
                else
                {
                    if (field == "AccountType")
                    {
                        switch (keyWord.ToLower())
                        {
                            case "普通账号":
                                conditions = "AccountTable.AccountType LIKE '%0%'";
                                break;
                            case "财务账号":
                                conditions = "AccountTable.AccountType LIKE '%1%'";
                                break;
                            case "工作账号":
                                conditions = "AccountTable.AccountType LIKE '%2%'";
                                break;
                            case "政务账号":
                                conditions = "AccountTable.AccountType LIKE '%3%'";
                                break;
                        }
                    }
                    else
                    {
                        conditions = "AccountTable." + field + " LIKE '%" + keyWord + "%'";
                    }
                }

                return LoadAccountData(conditions);
            }

            return LoadAccountData();
        }

        /// <summary>
        /// Advanced search
        /// </summary>
        /// <param name="vmAS">advanced search view model</param>
        /// <returns></returns>
        public List<AccountViewModel> AdvancedSearch(AdvancedSearchViewModel vmAS)
        {
            string conditions = "";

            // Account name
            if (vmAS.AccountName != null && vmAS.AccountName != "")
            {
                conditions += "AccountTable.AccountName LIKE '%" + vmAS.AccountName + "%'";
            }

            // Account type
            if (vmAS.AccountType != 4) // Account type (0-"普通账号", 1-"财务账号", 2-"工作账号", 3-政务账号, 4-"全部")
            {
                conditions += " AND AccountTable.AccountType LIKE '%" + vmAS.AccountType + "%'";
            }

            // URL
            if (vmAS.URL != null && vmAS.URL != "")
            {
                conditions += " AND AccountTable.URL LIKE '%" + vmAS.URL + "%'";
            }

            // User name
            if (vmAS.UserName != null && vmAS.UserName != "")
            {
                conditions += " AND AccountTable.UserName LIKE '%" + vmAS.UserName + "%'";
            }

            // Display name/nickname
            if (vmAS.DisplayName != null && vmAS.DisplayName != "")
            {
                conditions += " AND AccountTable.DisplayName LIKE '%" + vmAS.DisplayName + "%'";
            }

            // Append date
            if (vmAS.AppendDate != null && vmAS.AppendDate != "")
            {
                string dateFrom = null;
                string dateTo = null;
                if (vmAS.AppendDate.ToLower() != "select a date range...")
                {
                    switch (vmAS.AppendDate.ToLower())
                    {
                        case "within a week":
                            // DateTime.Now.ToString() => "2017-05-12 9:45:34" string
                            dateFrom = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd") + " 0:00:00";
                            dateTo = DateTime.Now.ToString();
                            break;
                        case "within a month":
                            dateFrom = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd") + " 0:00:00";
                            dateTo = DateTime.Now.ToString();
                            break;
                        case "within three months":
                            dateFrom = DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd") + " 0:00:00";
                            dateTo = DateTime.Now.ToString();
                            break;
                        case "within half a year":
                            dateFrom = DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd") + " 0:00:00";
                            dateTo = DateTime.Now.ToString();
                            break;
                        case "this year":
                            dateFrom = DateTime.Now.Year.ToString() + "-01-01 0:00:00";
                            dateTo = DateTime.Now.ToString();
                            break;
                        case "last year":
                            dateFrom = (DateTime.Now.Year - 1).ToString() + "-01-01 0:00:00";
                            dateTo = (DateTime.Now.Year - 1).ToString() + "-12-31 23:59:59";
                            break;
                        case "the year before last":
                            dateFrom = (DateTime.Now.Year - 2).ToString() + "-01-01 0:00:00";
                            dateTo = (DateTime.Now.Year - 2).ToString() + "-12-31 23:59:59";
                            break;
                    }
                }
                else
                {
                    if (vmAS.AppendDateFrom != null)
                    {
                        dateFrom = vmAS.AppendDateFrom + " 0:00:00";
                    }
                    if (vmAS.AppendDateTo != null)
                    {
                        dateTo = vmAS.AppendDateTo + " 23:59:59";
                    }
                }
                if (dateFrom != null && dateTo != null)
                {
                    conditions += " AND AccountTable.AppendDate BETWEEN #" + dateFrom + "# AND #" + dateTo + "#";
                }
            }

            // Last revised date
            if (vmAS.LastRevised != null && vmAS.LastRevised != "")
            {
                string dateFrom = null;
                string dateTo = null;
                if (vmAS.LastRevised.ToLower() != "select a date range...")
                {
                    switch (vmAS.LastRevised.ToLower())
                    {
                        case "within a week":
                            dateFrom = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd") + " 0:00:00";
                            dateTo = DateTime.Now.ToString();
                            break;
                        case "within a month":
                            dateFrom = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd") + " 0:00:00";
                            dateTo = DateTime.Now.ToString();
                            break;
                        case "within three months":
                            dateFrom = DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd") + " 0:00:00";
                            dateTo = DateTime.Now.ToString();
                            break;
                        case "within half a year":
                            dateFrom = DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd") + " 0:00:00";
                            dateTo = DateTime.Now.ToString();
                            break;
                        case "this year":
                            dateFrom = DateTime.Now.Year.ToString() + "-01-01 0:00:00";
                            dateTo = DateTime.Now.ToString();
                            break;
                        case "last year":
                            dateFrom = (DateTime.Now.Year - 1).ToString() + "-01-01 0:00:00";
                            dateTo = (DateTime.Now.Year - 1).ToString() + "-12-31 23:59:59";
                            break;
                        case "the year before last":
                            dateFrom = (DateTime.Now.Year - 2).ToString() + "-01-01 0:00:00";
                            dateTo = (DateTime.Now.Year - 2).ToString() + "-12-31 23:59:59";
                            break;
                    }
                }
                else
                {
                    if (vmAS.RevisedDateFrom != null)
                    {
                        dateFrom = vmAS.RevisedDateFrom + " 0:00:00";
                    }
                    if (vmAS.RevisedDateTo != null)
                    {
                        dateTo = vmAS.RevisedDateTo + " 23:59:59";
                    }
                }
                if (dateFrom != null && dateTo != null)
                {
                    conditions += " AND AccountTable.LastRevised BETWEEN #" + dateFrom + "# AND #" + dateTo + "#";
                }
            }

            // Attachments
            if (vmAS.AttachmentFlag != 2) // Attachment flag (0-"无", 1-"有", 2-"全部")
            {
                conditions += " AND AccountTable.AttachmentFlag LIKE '%" + vmAS.AttachmentFlag + "%'";
            }

            // If user does not specify account name
            if (conditions != "" && conditions.Substring(0, 5) == " AND ")
            {
                conditions = conditions.Substring(5, conditions.Length - 5);
            }

            // Execute query.
            return LoadAccountData(conditions);
        }

        /// <summary>
        /// Insert, i.e., append a new account to the database
        /// </summary>
        /// <param name="vmAccount">newly-added account</param>
        /// <returns>number of rows affected by the command</returns>
        public int Insert(AccountViewModel vmAccount)
        {
            if (!_dbOpen)
            {
                return 0;
            }

            // Get current new account's info
            Dictionary<string, string> accoInfo = new Dictionary<string, string>(); // Dictionary<PropertyName, PropertyValue>
            Type type = vmAccount.GetType();
            foreach (PropertyInfo pi in type.GetProperties())
            {
                accoInfo.Add(pi.Name, pi.GetValue(vmAccount) == null ? "" : pi.GetValue(vmAccount).ToString());
            }
            if (accoInfo == null)
            {
                return 0;
            }

            // Construct sql statement
            string sqlInsert = "INSERT INTO AccountTable (";
            string sqlValue = "VALUES (";
            foreach (KeyValuePair<string, string> info in accoInfo)
            {
                if (info.Value != "")
                {
                    sqlInsert += "[" + info.Key + "], "; // Use "[]" to avoid error reporting when fields defined by user conflicts with the SQL reserved words
                    switch (info.Key)
                    {
                        case "AccountType":
                            switch (info.Value) // Account type: 0-普通账号, 1-财务账号, 2-工作账号, 3-政务账号
                            {
                                case "普通账号":
                                    sqlValue += "'0', ";
                                    break;
                                case "财务账号":
                                    sqlValue += "'1', ";
                                    break;
                                case "工作账号":
                                    sqlValue += "'2', ";
                                    break;
                                case "政务账号":
                                    sqlValue += "'3', ";
                                    break;
                            }
                            break;
                        case "AttachmentFlag":
                            switch (vmAccount.AttachmentFlag) // Attachment flag: 0-无, 1-有
                            {
                                case "无":
                                    sqlValue += "'0', ";
                                    break;
                                case "有":
                                    sqlValue += "'1', ";
                                    break;
                            }
                            break;
                        default:
                            sqlValue += "'" + info.Value + "', ";
                            break;
                    }
                }
            }
            string sql;
            sql = sqlInsert.Substring(0, sqlInsert.Length - 2) + ") ";
            sql += sqlValue.Substring(0, sqlValue.Length - 2) + ")";

            int rowNum = DbExecuteNonQuery(sql);
            if (rowNum < 0)
            {
                return -1;
            }

            return rowNum;
        }

        /// <summary>
        /// Delete accounts from the database
        /// </summary>
        /// <param name="vmAccounts">account view model</param>
        /// <history time="2018/02/26">delete attached file folder</history>
        /// <returns>number of rows affected by the command</returns>
        public int Delete(List<AccountViewModel> vmAccounts)
        {
            int rowNum = 0;
            foreach (AccountViewModel vmAccount in vmAccounts)
            {
                // Delete selected account from the database
                string sql = "DELETE FROM AccountTable" + " WHERE AccountName='" + vmAccount.AccountName + "'";
                int num = DbExecuteNonQuery(sql);
                if (num < 0)
                {
                    return -1;
                }
                else
                {
                    // Delete the corresponding attached file folder
                    string dir = Directory.GetCurrentDirectory() + _ffHelper.GetData("config", "acc_attach_dir"); // attached file directory
                    if (Directory.Exists(dir + "\\" + vmAccount.AccountName))
                    {
                        Directory.Delete(dir + "\\" + vmAccount.AccountName, true);
                    }

                    rowNum += num;
                }
            }

            return rowNum;
        }

        /// <summary>
        /// Update an account, i.e., modify an account.
        /// </summary>
        /// <param name="newAccount">newly modified account</param>
        /// <param name="oldAccount">old(original) account</param>
        /// <returns></returns>
        public int Update(AccountViewModel newAccount, AccountViewModel oldAccount)
        {
            string sql = "UPDATE AccountTable SET ";

            // Account name
            if (newAccount.AccountName != oldAccount.AccountName)
            {
                sql += "[AccountName]='" + (newAccount.AccountName == null ? "" : newAccount.AccountName) + "', "; // use '[]' to avoid conflicting with Access key words
            }

            // Account type
            if (newAccount.AccountType != oldAccount.AccountType)
            {
                switch (newAccount.AccountType.ToLower())
                {
                    case "普通账号":
                        sql += "[AccountType]='0', ";
                        break;
                    case "财务账号":
                        sql += "[AccountType]='1', ";
                        break;
                    case "工作账号":
                        sql += "[AccountType]='2', ";
                        break;
                    case "政务账号":
                        sql += "[AccountType]='3', ";
                        break;
                }
            }

            // URL
            if (newAccount.URL != oldAccount.URL)
            {
                sql += "[URL]='" + (newAccount.URL == null ? "" : newAccount.URL) + "', ";
            }

            // User name
            if (newAccount.UserName != oldAccount.UserName)
            {
                sql += "[UserName]='" + (newAccount.UserName == null ? "" : newAccount.UserName) + "', ";
            }

            // Password
            if (newAccount.Password != oldAccount.Password)
            {
                sql += "[Password]='" + (newAccount.Password == null ? "" : newAccount.Password) + "', ";
            }

            // Display name/nickname
            if (newAccount.DisplayName != oldAccount.DisplayName)
            {
                sql += "[DisplayName]='" + (newAccount.DisplayName == null ? "" : newAccount.DisplayName) + "', ";
            }

            // E-mail
            if (newAccount.Email != oldAccount.Email)
            {
                sql += "[Email]='" + (newAccount.Email == null ? "" : newAccount.Email) + "', ";
            }

            // Telephone
            if (newAccount.Telephone != oldAccount.Telephone)
            {
                sql += "[Telephone]='" + (newAccount.Telephone == null ? "" : newAccount.Telephone) + "', ";
            }

            // Payment code
            if (newAccount.PaymentCode != oldAccount.PaymentCode)
            {
                sql += "[PaymentCode]='" + (newAccount.PaymentCode == null ? "" : newAccount.PaymentCode) + "', ";
            }

            // Append date
            if (newAccount.AppendDate != oldAccount.AppendDate)
            {
                sql += "[AppendDate]='" + (newAccount.AppendDate == null ? "" : newAccount.AppendDate) + "', ";
            }

            // Last revised
            if (newAccount.LastRevised != oldAccount.LastRevised)
            {
                sql += "[LastRevised]='" + (newAccount.LastRevised == null ? "" : newAccount.LastRevised) + "', ";
            }

            // Former account names
            if (newAccount.FormerAccountNames != oldAccount.FormerAccountNames)
            {
                sql += "[FormerAccountNames]='" + (newAccount.FormerAccountNames == null ? "" : newAccount.FormerAccountNames) + "', ";
            }

            // Attachment flag
            if (newAccount.AttachmentFlag != oldAccount.AttachmentFlag)
            {
                switch (newAccount.AttachmentFlag.ToLower())
                {
                    case "无":
                        sql += "[AttachmentFlag]='0', ";
                        break;
                    case "有":
                        sql += "[AttachmentFlag]='1', ";
                        break;
                }
            }

            // Attached file names (One can add new attachments, or clear all existing attachments)
            if (newAccount.AttachedFileNames != oldAccount.AttachedFileNames)
            {
                sql += "[AttachedFileNames]='" + (newAccount.AttachedFileNames == null ? "" : newAccount.AttachedFileNames) + "', ";
            }

            // Remarks
            if (newAccount.Remarks != oldAccount.Remarks)
            {
                sql += "[Remarks]='" + (newAccount.Remarks == null ? "" : newAccount.Remarks) + "', ";
            }

            // Key words
            if (newAccount.KeyWords != oldAccount.KeyWords)
            {
                sql += "[KeyWords]='" + (newAccount.KeyWords == null ? "" : newAccount.KeyWords) + "', ";
            }

            sql = sql.Substring(0, sql.Length - 2) + " WHERE AccountTable.AccountName='" + oldAccount.AccountName + "'";
            int rowNum = DbExecuteNonQuery(sql);
            if (rowNum < 0)
            {
                return -1;
            }

            return rowNum;
        }

        /// <summary>
        /// Merge database
        /// </summary>
        /// <param name="dbFileName"></param>
        /// <param name="dbPwd"></param>
        /// <returns></returns>
        public List<int> Merge(string dbFileName, string dbPwd = null)
        {
            // Connect to database
            CAccessHelper accHelper = new CAccessHelper();
            try
            {
                accHelper.Open(dbFileName, dbPwd);
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("AccountsModel::Merge >> " + ex.Message);
                return null;
            }

            // Get all fields' names of the account table
            List<string> fieldNames = new List<string>();
            try
            {
                fieldNames = accHelper.GetAllFieldNames("AccountTable");
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("AccountsModel::Merge >> " + ex.Message);
                accHelper.Close();
                return null;
            }

            // Get data set from the database
            string sql = "SELECT * FROM AccountTable"; // '*' represent all fields, i.e., all columns of the table
            DataSet ds = null;
            try
            {
                ds = accHelper.Query(sql); // ds will not be null even if the database is empty
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("AccountsModel::Merge >> " + ex.Message + " >> " + sql);
                accHelper.Close();
                return null;
            }

            if (ds == null)
            {
                accHelper.Close();
                return null;
            }

            List<int> num = new List<int>();
            int insertNum = 0;
            int updateNum = 0;
            string insertAccs = "";
            string updateAccs = "";

            // Read account data, and insert new ones to current database
            List<Account> accountList = new List<Account>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; ++i)
            {
                Account account = new Account();
                foreach (string fieldName in fieldNames)
                {
                    switch (fieldName)
                    {
                        case "AccountName":
                            if (ds.Tables[0].Rows[i]["AccountName"] != DBNull.Value)
                            {
                                account.AccountName = ds.Tables[0].Rows[i]["AccountName"].ToString();
                            }
                            break;
                        case "AccountType":
                            if (ds.Tables[0].Rows[i]["AccountType"] != DBNull.Value)
                            {
                                account.AccountType = Convert.ToInt32(ds.Tables[0].Rows[i]["AccountType"]);
                            }
                            break;
                        case "URL":
                            if (ds.Tables[0].Rows[i]["URL"] != DBNull.Value)
                            {
                                account.URL = ds.Tables[0].Rows[i]["URL"].ToString();
                            }
                            break;
                        case "UserName":
                            if (ds.Tables[0].Rows[i]["UserName"] != DBNull.Value)
                            {
                                account.UserName = ds.Tables[0].Rows[i]["UserName"].ToString();
                            }
                            break;
                        case "Password":
                            if (ds.Tables[0].Rows[i]["Password"] != DBNull.Value)
                            {
                                account.Password = ds.Tables[0].Rows[i]["Password"].ToString();
                            }
                            break;
                        case "DisplayName":
                            if (ds.Tables[0].Rows[i]["DisplayName"] != DBNull.Value)
                            {
                                account.DisplayName = ds.Tables[0].Rows[i]["DisplayName"].ToString();
                            }
                            break;
                        case "Email":
                            if (ds.Tables[0].Rows[i]["Email"] != DBNull.Value)
                            {
                                account.Email = ds.Tables[0].Rows[i]["Email"].ToString();
                            }
                            break;
                        case "Telephone":
                            if (ds.Tables[0].Rows[i]["Telephone"] != DBNull.Value)
                            {
                                account.Telephone = ds.Tables[0].Rows[i]["Telephone"].ToString();
                            }
                            break;
                        case "PaymentCode":
                            if (ds.Tables[0].Rows[i]["PaymentCode"] != DBNull.Value)
                            {
                                account.PaymentCode = ds.Tables[0].Rows[i]["PaymentCode"].ToString();
                            }
                            break;
                        case "AppendDate":
                            if (ds.Tables[0].Rows[i]["AppendDate"] != DBNull.Value)
                            {
                                // (DateTime)ds.Tables[0].Rows[i]["AppendDate"] => {2017-05-02 0:00:00}	System.DateTime
                                account.AppendDate = ((DateTime)ds.Tables[0].Rows[i]["AppendDate"]).ToShortDateString();
                            }
                            break;
                        case "LastRevised":
                            if (ds.Tables[0].Rows[i]["LastRevised"] != DBNull.Value)
                            {
                                account.LastRevised = ((DateTime)ds.Tables[0].Rows[i]["LastRevised"]).ToShortDateString();
                            }
                            break;
                        case "FormerAccountNames":
                            if (ds.Tables[0].Rows[i]["FormerAccountNames"] != DBNull.Value)
                            {
                                account.FormerAccountNames = ds.Tables[0].Rows[i]["FormerAccountNames"].ToString();
                            }
                            break;
                        case "AttachmentFlag":
                            if (ds.Tables[0].Rows[i]["AttachmentFlag"] != DBNull.Value)
                            {
                                account.AttachmentFlag = Convert.ToInt32(ds.Tables[0].Rows[i]["AttachmentFlag"]);
                            }
                            break;
                        case "AttachedFileNames":
                            if (ds.Tables[0].Rows[i]["AttachedFileNames"] != DBNull.Value)
                            {
                                account.AttachedFileNames = ds.Tables[0].Rows[i]["AttachedFileNames"].ToString();
                            }
                            break;
                        case "Remarks":
                            if (ds.Tables[0].Rows[i]["Remarks"] != DBNull.Value)
                            {
                                account.Remarks = ds.Tables[0].Rows[i]["Remarks"].ToString();
                            }
                            break;
                        case "KeyWords":
                            if (ds.Tables[0].Rows[i]["KeyWords"] != DBNull.Value)
                            {
                                account.KeyWords = ds.Tables[0].Rows[i]["KeyWords"].ToString();
                            }
                            break;
                    }
                }

                bool exist = false;
                Account acc = new Account();

                foreach (Account a in _accountList)
                {
                    if (account.AccountName == a.AccountName)
                    {
                        exist = true;
                        acc = a;
                        break;
                    }

                    if (account.FormerAccountNames != null)
                    {
                        List<System.String> list = new List<System.String>(account.FormerAccountNames.Split('|'));                        
                        if (list.Contains(a.AccountName))
                        {
                            exist = true;
                            acc = a;
                            break;
                        }
                    }

                    if (a.FormerAccountNames != null)
                    {
                        List<System.String> list = new List<System.String>(a.FormerAccountNames.Split('|'));
                        if (list.Contains(account.AccountName))
                        {
                            exist = true;
                            acc = a;
                            break;
                        }
                    }
                }

                // Update if exist, or insert
                if (exist)
                {
                    DateTime dt1 = DateTime.ParseExact(account.LastRevised, "yyyy/M/d", System.Globalization.CultureInfo.CurrentCulture);
                    DateTime dt2 = DateTime.ParseExact(acc.LastRevised, "yyyy/M/d", System.Globalization.CultureInfo.CurrentCulture);

                    if (DateTime.Compare(dt1, dt2) > 0) // update only when the merging account is a newer version
                    {
                        int rowNum = Update(ConvertToViewModel(account), ConvertToViewModel(acc));
                        updateNum += rowNum > 0 ? rowNum : 0;
                        updateAccs += account.AccountName + ", ";

                        WriteAccountUpdateLog(account, acc); // write log
                    }
                }
                else
                {
                    int rowNum = Insert(ConvertToViewModel(account));
                    insertNum += rowNum > 0 ? rowNum : 0;
                    insertAccs += account.AccountName + ", ";

                    WriteAccountUpdateLog(account); // write log
                }
            }

            // Write log
            insertAccs = insertAccs != "" ? insertAccs.Substring(0, insertAccs.Length - 2) : insertAccs;
            updateAccs = updateAccs != "" ? updateAccs.Substring(0, updateAccs.Length - 2) : updateAccs;

            _logWriter.WriteInfoLog("AccountsModel::Merge >> Newly-merged account(s)(" + (insertNum + updateNum)
                + "): (insert " + insertNum + ")" + insertAccs + "; (update " + updateNum + ")" + updateAccs);

            num.Add(insertNum);
            num.Add(updateNum);
            accHelper.Close();

            return num;
        }

        /// <summary>
        /// Export accounts' data to file of specified type
        /// </summary>
        /// <param name="vmAccounts">selected accounts</param>
        /// <returns>failed if return -1; canceled or succeeded if return 0</returns>
        public int Export(List<AccountViewModel> vmAccounts)
        {
            List<Account> accountList = new List<Account>();
            foreach (AccountViewModel vmAccount in  vmAccounts)
            {
                foreach (Account account in _accountList)
                {
                    if (account.AccountName == vmAccount.AccountName)
                    {
                        accountList.Add(account);
                        break;
                    }
                }
            }

            if (accountList.Count == 0)
            {
                if (vmAccounts.Count > 0)
                {
                    string accountNames = "";
                    foreach (AccountViewModel vmAccount in vmAccounts)
                    {
                        accountNames += vmAccount.AccountName + ",";
                    }
                    accountNames = accountNames.Substring(0, accountNames.Length - 1);
                    _logWriter.WriteErrorLog("AccountsModel::Export >> Data not found: " + accountNames);
                    return -1;
                }
            }

            // Get output path name
            SysWinForm.FolderBrowserDialog dlg = new SysWinForm.FolderBrowserDialog();
            dlg.ShowNewFolderButton = true;
            dlg.Description = "导出到：";
            if (dlg.ShowDialog() == SysWinForm.DialogResult.OK)
            {
                string txtFileName = dlg.SelectedPath + "\\"
                    + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "-FAMS-Accounts.txt";
                if (File.Exists(txtFileName))
                {
                    SysWin.MessageBoxResult option = SysWin.MessageBox.Show("文件已存在，要覆盖吗？", "",
                    SysWin.MessageBoxButton.YesNoCancel, SysWin.MessageBoxImage.Question, SysWin.MessageBoxResult.Cancel);

                    if (option == SysWin.MessageBoxResult.Cancel)
                    {
                        return 0;
                    }
                    else if (option == SysWin.MessageBoxResult.No)
                    {
                        int num = 2;
                        txtFileName = txtFileName.Substring(0, txtFileName.Length - 4) + " (" + num.ToString() + ").txt";
                        while (File.Exists(txtFileName))
                        {
                            ++num;
                            txtFileName = num < 10 ? txtFileName.Substring(0, txtFileName.Length - 6) : txtFileName.Substring(0, txtFileName.Length - 7);
                            txtFileName += num.ToString() + ").txt";
                        }
                    }
                }

                // Construct output text string
                string txtString = "";
                foreach (Account account in accountList)
                {
                    txtString += "● " + account.AccountName + "\r\n";

                    switch (account.AccountType)
                    {
                        case 0:
                            txtString += "账号类型：        普通账号\r\n";
                            break;
                        case 1:
                            txtString += "账号类型：        财务账号\r\n";
                            break;
                        case 2:
                            txtString += "账号类型：        工作账号\r\n";
                            break;
                        case 3:
                            txtString += "账号类型：        政务账号\r\n";
                            break;
                    }

                    txtString += "网址：            " + account.URL + "\r\n";
                    txtString += "用户名/登录名：   " + account.UserName + "\r\n";
                    txtString += "密码：            " + account.Password + "\r\n";
                    txtString += "显示名称/昵称：   " + account.DisplayName + "\r\n";
                    txtString += "电子邮箱：        " + account.Email + "\r\n";
                    txtString += "手机号码：        " + account.Telephone + "\r\n";
                    txtString += "支付密码：        " + account.PaymentCode + "\r\n";
                    txtString += "提交日期：        " + account.AppendDate + "\r\n";
                    txtString += "最后修改：        " + account.LastRevised + "\r\n";

                    if (account.FormerAccountNames != null)
                    {
                        txtString += "曾用账号名称：    " + account.FormerAccountNames.Replace("|", ", ") + "\r\n";
                    }
                    else
                    {
                        txtString += "曾用账号名称：\r\n";
                    }
                    
                    switch (account.AttachmentFlag)
                    {
                        case 0:
                            txtString += "附件：            无\r\n";
                            break;
                        case 1:
                            txtString += "附件：            有\r\n";
                            break;
                    }

                    if (account.KeyWords != null)
                    {
                        txtString += "关键字：          " + account.KeyWords.Replace("|", ", ") + "\r\n";
                    }
                    else
                    {
                        txtString += "关键字：\r\n";
                    }
                    
                    if (account.Remarks != null)
                    {
                        
                            txtString += "备注：            " + account.Remarks.Replace("\r\n", "\r\n                  ") + "\r\n\r\n";
                    }
                    else
                    {
                        txtString += "备注：\r\n\r\n";
                    }
                }

                // Export data
                if (WriteTxt(txtFileName, FileMode.Create, txtString) < 0)
                {
                    return -1;
                }
                else
                {
                    // Open export directory in Explorer
                    if (SysWin.MessageBox.Show("已成功导出" + accountList.Count + "个账号，打开输出目录？", "", SysWin.MessageBoxButton.YesNo,
                        SysWin.MessageBoxImage.Question, SysWin.MessageBoxResult.Yes)
                        == SysWin.MessageBoxResult.Yes)
                    {
                        try
                        {
                            System.Diagnostics.Process.Start("explorer.exe", dlg.SelectedPath);
                        }
                        catch (Exception ex)
                        {
                            _logWriter.WriteWarningLog("AccountsModel::Export >> Exception occurs when open the export folder: "
                                + ex.Message);
                        }
                    }
                }
            }

            return 0;
        }
        #endregion

        #region Model functions
        private List<string> GetFieldNames()
        {
            // 此处从配置文件或直接从数据库文件获取账户数据库表的所有字段名
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newAccount"></param>
        /// <param name="oldAccount"></param>
        /// <returns></returns>
        private int WriteAccountUpdateLog(Account newAccount, Account oldAccount)
        {
            string txtString = "● {update}\r\n";

            // Account name
            if (newAccount.AccountName != oldAccount.AccountName)
            {
                txtString += "Account Name:         {" + oldAccount.AccountName + "} → {" + newAccount.AccountName + "}\r\n";
            }
            else
            {
                txtString += "Account Name:         " + oldAccount.AccountName + "\r\n";
            }

            // Account type
            if (newAccount.AccountType != oldAccount.AccountType)
            {
                string newType = newAccount.AccountType == 0 ? "普通账号"
                    : (newAccount.AccountType == 1 ? "财务账号"
                    : (newAccount.AccountType == 2 ? "工作账号" : "政务账号"));
                string oldType = oldAccount.AccountType == 0 ? "普通账号"
                    : (oldAccount.AccountType == 1 ? "财务账号"
                    : (oldAccount.AccountType == 2 ? "工作账号" : "政务账号"));
                txtString += "Account Type:         {" + oldType + "} → {" + newType + "}\r\n";
            }
            else
            {
                string type = oldAccount.AccountType == 0 ? "普通账号"
                    : (oldAccount.AccountType == 1 ? "财务账号"
                    : (oldAccount.AccountType == 2 ? "工作账号" : "政务账号"));
                txtString += "Account Type:         " + type + "\r\n";
            }

            // URL
            if (newAccount.URL != oldAccount.URL)
            {
                txtString += "URL:                  {" + oldAccount.URL + "} → {" + newAccount.URL + "}\r\n";
            }
            else
            {
                txtString += "URL:                  " + oldAccount.URL + "\r\n";
            }

            // User name
            if (newAccount.UserName != oldAccount.UserName)
            {
                txtString += "User Name:            {" + oldAccount.UserName + "} → {" + newAccount.UserName + "}\r\n";
            }
            else
            {
                txtString += "User Name:            " + oldAccount.UserName + "\r\n";
            }

            // Password
            if (newAccount.Password != oldAccount.Password)
            {
                txtString += "Password:             {" + oldAccount.Password + "} → {" + newAccount.Password + "}\r\n";
            }
            else
            {
                txtString += "Password:             " + oldAccount.Password + "\r\n";
            }

            // Display name
            if (newAccount.DisplayName != oldAccount.DisplayName)
            {
                txtString += "Display Name:         {" + oldAccount.DisplayName + "} → {" + newAccount.DisplayName + "}\r\n";
            }
            else
            {
                txtString += "Display Name:         " + oldAccount.DisplayName + "\r\n";
            }

            // E-mail
            if (newAccount.Email != oldAccount.Email)
            {
                txtString += "E-mail:               {" + oldAccount.Email + "} → {" + newAccount.Email + "}\r\n";
            }
            else
            {
                txtString += "E-mail:               " + oldAccount.Email + "\r\n";
            }

            // Telephone
            if (newAccount.Telephone != oldAccount.Telephone)
            {
                txtString += "Telephone:            {" + oldAccount.Telephone + "} → {" + newAccount.Telephone + "}\r\n";
            }
            else
            {
                txtString += "Telephone:            " + oldAccount.Telephone + "\r\n";
            }

            // Payment code
            if (newAccount.PaymentCode != oldAccount.PaymentCode)
            {
                txtString += "Payment Code:         {" + oldAccount.PaymentCode + "} → {" + newAccount.PaymentCode + "}\r\n";
            }
            else
            {
                txtString += "Payment Code:         " + oldAccount.PaymentCode + "\r\n";
            }

            // Append date
            txtString += "Append Date:          " + oldAccount.AppendDate + "\r\n"; // append date is not editable

            // Last revised date
            if (newAccount.LastRevised != oldAccount.LastRevised)
            {
                txtString += "Last Revised:         {" + oldAccount.LastRevised + "} → {" + newAccount.LastRevised + "}\r\n";
            }
            else
            {
                txtString += "Last Revised:         " + oldAccount.LastRevised + "\r\n";
            }

            // Former account names
            if (newAccount.FormerAccountNames != oldAccount.FormerAccountNames)
            {
                if (oldAccount.FormerAccountNames != null)
                {
                    txtString += "Former Account Names: {" + oldAccount.FormerAccountNames.Replace("|", ", ") + "} → {";
                }
                else
                {
                    txtString += "Former Account Names: {} → {";
                }

                if (newAccount.FormerAccountNames != null)
                {
                    txtString += newAccount.FormerAccountNames.Replace("|", ", ") + "}\r\n";
                }
                else
                {
                    txtString += "}\r\n";
                }
            }
            else
            {
                if (oldAccount.FormerAccountNames != null)
                {
                    txtString += "Former Account Names: " + oldAccount.FormerAccountNames.Replace("|", ", ") + "\r\n";
                }
                else
                {
                    txtString += "Former Account Names:\r\n";
                }
            }

            // Attachments
            if (newAccount.AttachmentFlag != oldAccount.AttachmentFlag)
            {
                string flagNew = newAccount.AttachmentFlag == 0 ? "无" : "有";
                string flagOld = oldAccount.AttachmentFlag == 0 ? "无" : "有";
                txtString += "Attachments:          {" + flagOld + "} → {" + flagNew + "}\r\n";
            }
            else
            {
                string flag = oldAccount.AttachmentFlag == 0 ? "无" : "有";
                txtString += "Attachments:          " + flag + "\r\n";
            }

            // Key words
            if (newAccount.KeyWords != oldAccount.KeyWords)
            {
                if (oldAccount.KeyWords != null)
                {
                    txtString += "Key Words:            {" + oldAccount.KeyWords.Replace("|", ", ") + "} → {";
                }
                else
                {
                    txtString += "Key Words:            {} → {";
                }

                if (newAccount.KeyWords != null)
                {
                    txtString += newAccount.KeyWords.Replace("|", ", ") + "}\r\n";
                }
                else
                {
                    txtString += "}\r\n";
                }
            }
            else
            {
                if (oldAccount.KeyWords != null)
                {
                    txtString += "Key Words:            " + oldAccount.KeyWords.Replace("|", ", ") + "\r\n";
                }
                else
                {
                    txtString += "Key Words:\r\n";
                }
            }

            // Remarks
            if (newAccount.Remarks != oldAccount.Remarks)
            {
                if (oldAccount.Remarks != null)
                {
                    txtString += "Remarks:              {" + oldAccount.Remarks.Replace("\r\n", "\r\n                      ") + "} →\r\n                      {";
                }
                else
                {
                    txtString += "Remarks:              {" + oldAccount.Remarks + "} →\r\n                      {";
                }

                if (newAccount.Remarks != null)
                {
                    txtString += newAccount.Remarks.Replace("\r\n", "\r\n                      ") + "}\r\n\r\n";
                }
                else
                {
                    txtString += newAccount.Remarks + "}\r\n\r\n";
                }
            }
            else
            {
                if (oldAccount.Remarks != null)
                {
                    txtString += "Remarks:              " + oldAccount.Remarks.Replace("\r\n", "\r\n                      ") + "\r\n\r\n";
                }
                else
                {
                    txtString += "Remarks:\r\n\r\n";
                }
            }

            // DateTime.Now.ToString("yyyy-M-d") => "2020-3-11"
            // DateTime.Now.ToShortDateString() => "2020/03/11"
            string fileName = Directory.GetCurrentDirectory() + _ffHelper.GetData("config", "log_dir") + "\\" + DateTime.Now.ToString("yyyy-M-d") + "-FAMS-AccountDbUpdate.log";

            // Export data.
            int ret = 0;
            if (File.Exists(fileName))
            {
                ret = WriteTxt(fileName, FileMode.Append, txtString);
            }
            else
            {
                ret = WriteTxt(fileName, FileMode.Create, txtString);
            }

            return ret;
        }

        /// <summary>
        /// Write account update log
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        private int WriteAccountUpdateLog(Account account)
        {
            string txtString = "● {Add}\r\n";

            txtString += "Account Name:         " + account.AccountName + "\r\n"; // Account name

            // Account type
            string type = account.AccountType == 0 ? "普通账号"
                    : (account.AccountType == 1 ? "财务账号"
                    : (account.AccountType == 2 ? "工作账号" : "政务账号"));
            txtString += "Account Type:         " + type + "\r\n";

            txtString += "URL:                  " + account.URL + "\r\n"; // URL           
            txtString += "User Name:            " + account.UserName + "\r\n"; // User name            
            txtString += "Password:             " + account.Password + "\r\n"; // Password            
            txtString += "Display Name:         " + account.DisplayName + "\r\n"; // Display name            
            txtString += "E-mail:               " + account.Email + "\r\n"; // E-mail          
            txtString += "Telephone:            " + account.Telephone + "\r\n"; // Telephone           
            txtString += "Payment Code:         " + account.PaymentCode + "\r\n"; // Payment code           
            txtString += "Append Date:          " + account.AppendDate + "\r\n"; // Append date            
            txtString += "Last Revised:         " + account.LastRevised + "\r\n"; // Last revised date

            // Former account names
            if (account.FormerAccountNames != null)
            {
                txtString += "Former Account Names: " + account.FormerAccountNames.Replace("|", ", ") + "\r\n";
            }
            else
            {
                txtString += "Former Account Names:\r\n";
            }

            // Attachments
            string flag = account.AttachmentFlag == 0 ? "无" : "有";
            txtString += "Attachments:          " + flag + "\r\n";

            // Key words
            if (account.KeyWords != null)
            {
                txtString += "Key Words:            " + account.KeyWords.Replace("|", ", ") + "\r\n";
            }
            else
            {
                txtString += "Key Words:\r\n";
            }

            // Remarks
            if (account.Remarks != null)
            {
                txtString += "Remarks:              " + account.Remarks.Replace("\r\n", "\r\n                      ") + "\r\n\r\n";
            }
            else
            {
                txtString += "Remarks:\r\n\r\n";
            }

            string fileName = Directory.GetCurrentDirectory() + _ffHelper.GetData("config", "log_dir") + "\\" + DateTime.Now.ToString("yyyy-M-d") + "-FAMS-AccountDbUpdate.log";

            // Export data.
            int ret = 0;
            if (File.Exists(fileName))
            {
                ret = WriteTxt(fileName, FileMode.Append, txtString);
            }
            else
            {
                ret = WriteTxt(fileName, FileMode.Create, txtString);
            }

            return ret;
        }

        /// <summary>
        /// Write text
        /// </summary>
        /// <param name="txtFileName">full path name of the specified text file</param>
        /// <param name="mode">open mode</param>
        /// <param name="txtString">content string to be written</param>
        /// <returns>0-succeeded, others-failed</returns>
        public int WriteTxt(string txtFileName, FileMode mode, string txtString)
        {
            FileStream fs = new FileStream(txtFileName, mode);
            StreamWriter sw = new StreamWriter(fs);
            try
            {
                sw.Write(txtString);
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("AccountsModel::WriteTxt >> " + ex.Message);
                sw.Flush();
                sw.Close();
                fs.Close();
                return -1;
            }
            sw.Flush(); // Clear all buffers for the current writer.
            sw.Close(); // Close the current StreamWriter object.
            fs.Close(); // Close the current stream and release any resources.

            return 0;
        }

        /// <summary>
        /// Copy source folder to destination folder.
        /// </summary>
        /// <param name="srcFolderPathName">source folder path</param>
        /// <param name="dstFolderPathName">destination folder path</param>
        private void CopyFolder(string srcFolderPathName, string dstFolderPathName)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(srcFolderPathName);
            Directory.CreateDirectory(dstFolderPathName);
            foreach (FileSystemInfo fsInfo in dirInfo.GetFileSystemInfos())
            {
                string dstName = System.IO.Path.Combine(dstFolderPathName, fsInfo.Name);
                if (fsInfo is FileInfo)
                {
                    File.Copy(fsInfo.FullName, dstName);
                }
                else
                {
                    Directory.CreateDirectory(dstName);
                    CopyFolder(fsInfo.FullName, dstName);
                }
            }
        }
        #endregion

        #region Data conversion
        /// <summary>
        /// Convert account from model mode to view model mode
        /// </summary>
        /// <param name="account">account in model mode</param>
        /// <returns></returns>
        public AccountViewModel ConvertToViewModel(Account account)
        {
            AccountViewModel vmAccount = new AccountViewModel();

            vmAccount.AccountName = account.AccountName; // Account name
            switch (account.AccountType)                 // Account type(0-普通账号, 1-财务账号, 2-工作账号, 3-政务账号)
            {
                case 0:
                    vmAccount.AccountType = "普通账号";
                    break;
                case 1:
                    vmAccount.AccountType = "财务账号";
                    break;
                case 2:
                    vmAccount.AccountType = "工作账号";
                    break;
                case 3:
                    vmAccount.AccountType = "政务账号";
                    break;
            }
            vmAccount.URL = account.URL;                 // Website address
            vmAccount.UserName = account.UserName;       // User name, i.e., login name
            vmAccount.Password = account.Password;       // Password to login
            vmAccount.DisplayName = account.DisplayName; // Display name, i.e., nick name
            vmAccount.Email = account.Email;             // Registration e-mail
            vmAccount.Telephone = account.Telephone;     // Registration telephone
            vmAccount.PaymentCode = account.PaymentCode; // Payment code
            vmAccount.AppendDate = account.AppendDate;   // Append date
            vmAccount.LastRevised = account.LastRevised; // Last Revised date
            vmAccount.FormerAccountNames = account.FormerAccountNames; // Former account names
            switch (account.AttachmentFlag)              // Attachment flag(0-无, 1-有)
            {
                case 0:
                    vmAccount.AttachmentFlag = "无";
                    break;
                case 1:
                    vmAccount.AttachmentFlag = "有";
                    break;
            }
            vmAccount.AttachedFileNames = account.AttachedFileNames; // Attached file name with extension (without directory path)
            vmAccount.Remarks = account.Remarks;           // Remark
            vmAccount.KeyWords = account.KeyWords;         // Key words

            return vmAccount;
        }

        /// <summary>
        /// Convert account from view model mode to model mode
        /// </summary>
        /// <param name="vmAccount">account in view model mode</param>
        /// <returns></returns>
        public Account ConvertToModel(AccountViewModel vmAccount)
        {
            Account account = new Account();

            account.AccountName = vmAccount.AccountName; // Account name
            switch (vmAccount.AccountType)               // Account type (0-普通账号, 1-财务账号, 2-工作账号, 3-政务账号)
            {
                case "普通账号":
                    account.AccountType = 0;
                    break;
                case "财务账号":
                    account.AccountType = 1;
                    break;
                case "工作账号":
                    account.AccountType = 2;
                    break;
                case "政务账号":
                    account.AccountType = 3;
                    break;
            }
            account.URL = vmAccount.URL;                 // Website address
            account.UserName = vmAccount.UserName;       // User name, i.e., login name
            account.Password = vmAccount.Password;       // Password to login
            account.DisplayName = vmAccount.DisplayName; // Display name, i.e., nick name
            account.Email = vmAccount.Email;             // Registration e-mail
            account.Telephone = vmAccount.Telephone;     // Registration telephone
            account.PaymentCode = vmAccount.PaymentCode; // Payment code
            account.AppendDate = vmAccount.AppendDate;   // Append date
            account.LastRevised = vmAccount.LastRevised; // Last revised date
            account.FormerAccountNames = vmAccount.FormerAccountNames;   // Former account names
            switch (vmAccount.AttachmentFlag)            // Attachment flag (0-无, 1-有)
            {
                case "无":
                    account.AttachmentFlag = 0;
                    break;
                case "有":
                    account.AttachmentFlag = 1;
                    break;
            }
            account.AttachedFileNames = vmAccount.AttachedFileNames; // Attached file name with extension (without directory path)
            account.Remarks = vmAccount.Remarks;           // Remarks
            account.KeyWords = vmAccount.KeyWords;         // Key words

            return account;
        }
        #endregion

        #region Database access
        /// <summary>
        /// Query
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="conditions"></param>
        /// <returns></returns>
        private DataSet DbQuery(List<string> fields, string conditions = null)
        {
            // Construct sql statement.
            string sql = "SELECT ";
            foreach (string field in fields)
            {
                sql += "AccountTable." + field + ", ";
            }
            sql = sql.Substring(0, sql.Length - 2) + " FROM AccountTable";

            if (conditions != null && conditions != "")
            {
                sql += " WHERE " + conditions;
            }

            // Execute query.
            try
            {
                return _accHelper.Query(sql);
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("AccountsModel::DbQuery >> " + ex.Message + " >> " + sql);
                return null;
            }
        }

        /// <summary>
        /// Execute without query
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>number of rows affected by the command</returns>
        private int DbExecuteNonQuery(string sql)
        {
            try
            {
                return _accHelper.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                _logWriter.WriteErrorLog("AccountsModel::DbExecuteNonQuery >> " + ex.Message + " >> " + sql);
                return -1;
            }
        }
        #endregion
    }
}
