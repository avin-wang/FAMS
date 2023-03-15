using ADOX;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace FAMS.Services
{
    /// <summary>
    /// Project including this class should add two references below (both are COM modules):
    /// Microsoft ADO Ext. 2.8 for DDL and Security
    /// Microsoft ActiveX Data Objects 2.8 Library
    /// </summary>
    public class CAccessHelper
    {
        private string m_Provider = "Microsoft.ACE.OLEDB.12.0";
        private OleDbConnection m_conn = null;

        #region Create database.
        /// <summary>
        /// Create a new database.
        /// </summary>
        /// <param name="fileName">full file path of the database to be created</param>
        /// <param name="pwd">password</param>
        /// <returns>true-succeeded, false-failed</returns>
        public bool CreateDatabase(string fileName, string pwd = null)
        {
            if (File.Exists(fileName))
            {
                throw new Exception("File with the same name already exists!");
            }

            // Construct connection string.
            string connStr = null;
            if (pwd == null)
            {
                connStr = "Provider=" + m_Provider + ";Data Source=" + fileName; // with password.
            }
            else
            {
                connStr = "Provider=" + m_Provider + ";Data Source=" + fileName + ";Jet OLEDB:Database Password=" + pwd; // without password.
            }

            // Create a database.
            Catalog catalog = new Catalog();
            try
            {
                catalog.Create(connStr);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return true;
        }

        /// <summary>
        /// Create a new table.
        /// </summary>
        /// <param name="fileName">full file path of the database specified</param>
        /// <param name="tableName">tabel name</param>
        /// <param name="columns">columns to be created</param>
        /// <param name="pwd">password of the database</param>
        /// <returns>true-succeeded, false-failed</returns>
        public bool CreateTable(string fileName, string tableName, List<Column> columns, string pwd = null)
        {
            if (!File.Exists(fileName))
            {
                throw new Exception("File specified do not exist!");
            }

            // Construct connection string.
            string connStr = null;
            if (pwd == null)
            {
                connStr = "Provider=" + m_Provider + ";Data Source=" + fileName; // with password.
            }
            else
            {
                connStr = "Provider=" + m_Provider + ";Data Source=" + fileName + ";Jet OLEDB:Database Password=" + pwd; // without password.
            }

            // Connect to the database.
            ADODB.Connection conn = new ADODB.Connection();
            try
            {
                conn.Open(connStr, null, null, -1);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            Catalog catalog = new Catalog();
            catalog.ActiveConnection = conn;

            // Create a table with specified columns.
            Table table = new Table();
            table.Name = tableName;
            try
            {
                // Add an auto-increasing column.
                Column col = new Column();
                col.ParentCatalog = catalog;
                col.Type = DataTypeEnum.adInteger; // must set data type first!
                col.Name = "ID";
                col.DefinedSize = 9;
                col.Properties["AutoIncrement"].Value = true;
                table.Columns.Append(col);
                // Add all other custom columns.
                foreach (var column in columns)
                {
                    table.Columns.Append(column);
                }

                catalog.Tables.Append(table);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            // Close.
            table = null;
            catalog = null;
            conn.Close();

            return true;
        }

        /// <summary>
        /// Create a new table.
        /// </summary>
        /// <param name="fileName">full file path of the database specified</param>
        /// <param name="tableName">tabel name</param>
        /// <param name="primaryKey">primary key</param>
        /// <param name="foreignKey">foreign key</param>
        /// <param name="relatedTable">related table</param>
        /// <param name="relatedColumn">related column</param>
        /// <param name="columns">columns to be created</param>
        /// <param name="pwd">password of the database</param>
        /// <returns>true-succeeded, false-failed</returns>
        public bool CreateTable(string fileName, string tableName, Column primaryKey, Column foreignKey, string relatedTable, string relatedColumn, List<Column> columns, string pwd = null)
        {
            if (!File.Exists(fileName))
            {
                throw new Exception("File specified do not exist!");
            }

            // Construct connect string.
            string connStr = null;
            if (pwd == null)
            {
                connStr = "Provider=" + m_Provider + ";Data Source=" + fileName; // with password.
            }
            else
            {
                connStr = "Provider=" + m_Provider + ";Data Source=" + fileName + ";Jet OLEDB:Database Password=" + pwd; // without password.
            }

            // Connect to the database.
            ADODB.Connection conn = new ADODB.Connection();
            try
            {
                conn.Open(connStr, null, null, -1);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            Catalog catalog = new Catalog();
            catalog.ActiveConnection = conn;

            // Create a table with specified columns.
            Table table = new Table();
            table.Name = tableName;
            try
            {
                table.Keys.Append("PrimaryKey", KeyTypeEnum.adKeyPrimary, primaryKey, "", ""); // Set primary key.
                table.Keys.Append("ForeignKey", KeyTypeEnum.adKeyForeign, foreignKey, relatedTable, relatedColumn); // Set foreign key.
                // Add all other custom columns.
                foreach (var column in columns)
                {
                    table.Columns.Append(column);
                }

                catalog.Tables.Append(table);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            // Close.
            table = null;
            catalog = null;
            conn.Close();

            return true;
        }
        #endregion

        #region Access database.
        /// <summary>
        /// Open and create a database connection.
        /// </summary>
        /// <param name="fileName">full file path of the database specified</param>
        /// <param name="pwd">password of the database</param>
        /// <returns>true-succeeded, false-failed</returns>
        public bool Open(string fileName, string pwd = null)
        {
            string connectionString = "Provider=" + m_Provider + ";Data Source=" + fileName;
            if (pwd != null)
            {
                connectionString += ";Jet OLEDB:Database Password=" + pwd;
            }

            try
            {
                if (m_conn != null)
                {
                    m_conn.Close();
                    m_conn = null;
                }
                m_conn = new OleDbConnection(connectionString);
                m_conn.Open();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return true;
        }

        /// <summary>
        /// Close current database connection.
        /// </summary>
        public void Close()
        {
            if (m_conn != null)
            {
                m_conn.Close();
                m_conn = null;
            }
        }

        /// <summary>
        /// Execute query.
        /// </summary>
        /// <param name="sql">sql statement</param>
        /// <returns>resulting data set</returns>
        public DataSet Query(string sql)
        {
            DataSet ds = new DataSet();

            try
            {
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(sql, m_conn))
                {
                    adapter.Fill(ds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return ds;
        }

        /// <summary>
        /// Execute adding, deleting, modifying, etc.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>number of rows affected</returns>
        public int ExecuteNonQuery(string sql)
        {
            int result = 0;

            try
            {
                using (OleDbCommand cmd = m_conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sql;
                    cmd.Connection = m_conn;
                    result = cmd.ExecuteNonQuery(); // return -1 if exception occurs.
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Get all tables' names.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllTableNames()
        {
            List<string> tableNames = new List<string>();

            try
            {
                DataTable dt = m_conn.GetSchema("Tables");
                foreach (DataRow row in dt.Rows)
                {
                    if (row[3].ToString() == "TABLE")
                    {
                        tableNames.Add(row[2].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return tableNames;
        }

        /// <summary>
        /// Get all fields' names of the specified table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public List<string> GetAllFieldNames(string tableName)
        {
            List<string> fieldNames = new List<string>();

            try
            {
                using (OleDbCommand cmd = new OleDbCommand())
                {
                    cmd.CommandText = "SELECT TOP 1 * FROM [" + tableName + "]";
                    cmd.Connection = m_conn;
                    OleDbDataReader dr = cmd.ExecuteReader();
                    for (int i = 0; i < dr.FieldCount; ++i)
                    {
                        fieldNames.Add(dr.GetName(i));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return fieldNames;
        }
        #endregion
    }
}
