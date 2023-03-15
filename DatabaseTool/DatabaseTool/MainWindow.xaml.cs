using ADOX;
using FAMS.Data.Accounts;
using FAMS.Services;
using FAMS.ViewModels.Accounts;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DatabaseTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Create a new database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCreateNewDb_Click(object sender, RoutedEventArgs e)
        {
            CAccessHelper accHelper = new CAccessHelper();
            //Directory.CreateDirectory("./database");
            //string fileName = "./database/" + this.tbxDbFileName.Text;
            string fileName = this.tbxDbFileName.Text;

            if (File.Exists(fileName))
            {
                MessageBoxResult result = MessageBox.Show("Database exists! Delete it? ", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    File.Delete(fileName);
                }
                else
                {
                    return;
                }
            }

            if (accHelper.CreateDatabase(fileName, this.tbxDbFilePwd.Text == "Use default" ?
                "fams.pwd.0334.057" : this.tbxDbFilePwd.Text)) // accHelper.CreateDatabase(fileName, null)
            {
                List<Column> columns = new List<Column>();

                Column column = new Column();
                column.Name = "AccountName";
                column.Type = DataTypeEnum.adLongVarWChar;
                column.Attributes = ColumnAttributesEnum.adColNullable;
                columns.Add(column);

                column = new Column();
                column.Name = "AccountType";
                column.Type = DataTypeEnum.adInteger;
                column.Attributes = ColumnAttributesEnum.adColNullable;
                columns.Add(column);

                column = new Column();
                column.Name = "URL";
                column.Type = DataTypeEnum.adLongVarWChar;
                column.Attributes = ColumnAttributesEnum.adColNullable;
                columns.Add(column);

                column = new Column();
                column.Name = "UserName";
                column.Type = DataTypeEnum.adLongVarWChar;
                column.Attributes = ColumnAttributesEnum.adColNullable;
                columns.Add(column);

                column = new Column();
                column.Name = "Password";
                column.Type = DataTypeEnum.adVarWChar;
                column.DefinedSize = 128; // for DataTypeEnum.adLongVarWChar(long text), do not need to set this parameter
                column.Attributes = ColumnAttributesEnum.adColNullable;
                columns.Add(column);

                column = new Column();
                column.Name = "DisplayName";
                column.Type = DataTypeEnum.adVarWChar;
                column.DefinedSize = 128;
                column.Attributes = ColumnAttributesEnum.adColNullable;
                columns.Add(column);

                column = new Column();
                column.Name = "Email";
                column.Type = DataTypeEnum.adVarWChar;
                column.DefinedSize = 128;
                column.Attributes = ColumnAttributesEnum.adColNullable;
                columns.Add(column);

                column = new Column();
                column.Name = "Telephone";
                column.Type = DataTypeEnum.adVarWChar;
                column.DefinedSize = 128;
                column.Attributes = ColumnAttributesEnum.adColNullable;
                columns.Add(column);

                column = new Column();
                column.Name = "PaymentCode";
                column.Type = DataTypeEnum.adVarWChar;
                column.DefinedSize = 128;
                column.Attributes = ColumnAttributesEnum.adColNullable;
                columns.Add(column);

                column = new Column();
                column.Name = "AppendDate";
                column.Type = DataTypeEnum.adDate;
                column.Attributes = ColumnAttributesEnum.adColNullable;
                columns.Add(column);

                column = new Column();
                column.Name = "LastRevised";
                column.Type = DataTypeEnum.adDate;
                column.Attributes = ColumnAttributesEnum.adColNullable;
                columns.Add(column);

                column = new Column();
                column.Name = "FormerAccountNames";
                column.Type = DataTypeEnum.adLongVarWChar;
                column.Attributes = ColumnAttributesEnum.adColNullable;
                columns.Add(column);

                column = new Column();
                column.Name = "AttachmentFlag";
                column.Type = DataTypeEnum.adInteger;
                column.Attributes = ColumnAttributesEnum.adColNullable;
                columns.Add(column);

                column = new Column();
                column.Name = "AttachedFileNames";
                column.Type = DataTypeEnum.adLongVarWChar;
                column.Attributes = ColumnAttributesEnum.adColNullable;
                columns.Add(column);

                column = new Column();
                column.Name = "Remarks";
                column.Type = DataTypeEnum.adLongVarWChar;
                column.Attributes = ColumnAttributesEnum.adColNullable;
                columns.Add(column);

                column = new Column();
                column.Name = "KeyWords";
                column.Type = DataTypeEnum.adLongVarWChar;
                column.Attributes = ColumnAttributesEnum.adColNullable;
                columns.Add(column);

                try
                {
                    if (accHelper.CreateTable(fileName, "AccountTable", columns, "fams.pwd.0334.057"))
                    {
                        MessageBox.Show("Create database succeeded!", "", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            MessageBox.Show("Create database failed!", "", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Merge database.
        /// Attention:
        /// (1) dabatase 1 must be version before 2020/03/04!
        /// (2) database 2 must be empty!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMergeDb_Click(object sender, RoutedEventArgs e)
        {
            string dbFilePath1 = this.tbxDbFilePath1.Text;
            string dbFilePath2 = this.tbxDbFilePath2.Text;
            string dbPwd1 = "fams.pwd.0334.057";
            string dbPwd2 = "fams.pwd.0334.057";           

            // Connect to database.
            CAccessHelper accHelper1 = new CAccessHelper();
            accHelper1.Open(dbFilePath1, dbPwd1);
            _accHelper2.Open(dbFilePath2, dbPwd2);

            // Get all fields' names of the account table.
            List<string> fieldNames = new List<string>();
            fieldNames = accHelper1.GetAllFieldNames("AccountTable");

            // Construct sql statement.
            string sql = "SELECT ";
            foreach (string field in fieldNames)
            {
                sql += "AccountTable." + field + ", ";
            }
            sql = sql.Substring(0, sql.Length - 2) + " FROM AccountTable";

            // Get data set from the database.
            DataSet ds = accHelper1.Query(sql);

            // Read account data, and insert new ones to current database.
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
                        case "AttachmentFlag":
                            if (ds.Tables[0].Rows[i]["AttachmentFlag"] != DBNull.Value)
                            {
                                account.AttachmentFlag = Convert.ToInt32(ds.Tables[0].Rows[i]["AttachmentFlag"]);
                            }
                            break;
                        case "AttachedFileName":
                            if (ds.Tables[0].Rows[i]["AttachedFileName"] != DBNull.Value)
                            {
                                account.AttachedFileName = ds.Tables[0].Rows[i]["AttachedFileName"].ToString();
                            }
                            break;
                        case "Remark":
                            if (ds.Tables[0].Rows[i]["Remark"] != DBNull.Value)
                            {
                                account.Remark = ds.Tables[0].Rows[i]["Remark"].ToString();
                            }
                            break;
                    }
                }

                // Insert.
                Insert(ConvertToViewModel(account));
            }

            accHelper1.Close();
            _accHelper2.Close();

            MessageBox.Show("Merge database completed!", "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private AccountViewModel ConvertToViewModel(Account account)
        {
            AccountViewModel vmAccount = new AccountViewModel();

            vmAccount.AccountName = account.AccountName; // Account name.
            switch (account.AccountType)                 // Account type(0-general, 1-financial, 2-work, 3-governmental).
            {
                case 0:
                    vmAccount.AccountType = "general";
                    break;
                case 1:
                    vmAccount.AccountType = "financial";
                    break;
                case 2:
                    vmAccount.AccountType = "work";
                    break;
                case 3:
                    vmAccount.AccountType = "governmental";
                    break;
            }
            vmAccount.URL = account.URL;                 // Website address.
            vmAccount.UserName = account.UserName;       // User name, i.e., login name.
            vmAccount.Password = account.Password;       // Password to login.
            vmAccount.DisplayName = account.DisplayName; // Display name, i.e., nick name.
            vmAccount.Email = account.Email;             // Registration e-mail.
            vmAccount.Telephone = account.Telephone;     // Registration telephone.
            vmAccount.PaymentCode = account.PaymentCode; // Payment code.
            vmAccount.AppendDate = account.AppendDate;   // Append date.
            switch (account.AttachmentFlag)              // Attachment flag(0-no, 1-yes).
            {
                case 0:
                    vmAccount.AttachmentFlag = "no";
                    break;
                case 1:
                    vmAccount.AttachmentFlag = "yes";
                    break;
            }
            vmAccount.AttachedFileName = account.AttachedFileName; // Attached file name with extension(without directory path).
            vmAccount.Remark = account.Remark;           // Remark.

            return vmAccount;
        }

        private CAccessHelper _accHelper2 = new CAccessHelper();
        private int Insert(AccountViewModel vmAccount)
        {
            // Get current new account's info.
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

            // Construct sql statement.
            string sqlInsert = "INSERT INTO AccountTable (";
            string sqlValue = "VALUES (";
            foreach (KeyValuePair<string, string> info in accoInfo)
            {
                string fieldName = info.Key;

                if (fieldName == "AttachedFileName")
                {
                    fieldName = "AttachedFileNames";
                }
                else if (fieldName == "Remark")
                {
                    fieldName = "Remarks";
                }

                if (info.Value != "")
                {
                    sqlInsert += "[" + fieldName + "], "; // Use "[]" to avoid error reporting when fields defined by user conflicts with the SQL reserved words.
                    switch (fieldName)
                    {
                        case "AccountType":
                            switch (info.Value) // Account type: 0-general, 1-financial, 2-work, 3-governmental.
                            {
                                case "general":
                                    sqlValue += "'0', ";
                                    break;
                                case "financial":
                                    sqlValue += "'1', ";
                                    break;
                                case "work":
                                    sqlValue += "'2', ";
                                    break;
                                case "governmental":
                                    sqlValue += "'3', ";
                                    break;
                            }
                            break;
                        case "AttachmentFlag":
                            switch (vmAccount.AttachmentFlag) // Attachment flag: 0-no, 1-yes.
                            {
                                case "no":
                                    sqlValue += "'0', ";
                                    break;
                                case "yes":
                                    sqlValue += "'1', ";
                                    break;
                            }
                            break;
                        default:
                            sqlValue += "'" + info.Value + "', ";
                            break;
                    }

                    if (info.Key == "AppendDate")
                    {
                        sqlInsert += "[LastRevised], ";
                        sqlValue += "'" + info.Value + "', ";
                    }
                }
            }
            string sql;
            sql = sqlInsert.Substring(0, sqlInsert.Length - 2) + ") ";
            sql += sqlValue.Substring(0, sqlValue.Length - 2) + ")";

            _accHelper2.ExecuteNonQuery(sql);

            return 0;
        }
    }
}
