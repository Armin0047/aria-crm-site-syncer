using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace SiteSyncerAria_DrDoosti
{
    internal class DatabaseSQL
    {
        private static void ValidateSettings(string dbservername, string dbdatabasename, string dbusername, string dbpassword)
        {
            if (string.IsNullOrWhiteSpace(dbservername) ||
                string.IsNullOrWhiteSpace(dbdatabasename) ||
                string.IsNullOrWhiteSpace(dbusername) ||
                string.IsNullOrWhiteSpace(dbpassword))
            {
                throw new InvalidOperationException("SQL Server connection settings must be provided in Settings.ini.");
            }
        }

        internal static void InitConnectionString(string dbservername, string dbdatabasename, string dbusername, string dbpassword)
        {
            ValidateSettings(dbservername, dbdatabasename, dbusername, dbpassword);

            var builder = new OleDbConnectionStringBuilder
            {
                Provider = "SQLOLEDB.1",
                DataSource = dbservername
            };
            builder["Initial Catalog"] = dbdatabasename;
            builder["User ID"] = dbusername;
            builder["Password"] = dbpassword;
            Database.CONNECTION_STRING = builder.ConnectionString;
        }

        internal static void InitConnectionStringSQLConnection(string dbservername, string dbdatabasename, string dbusername, string dbpassword)
        {
            ValidateSettings(dbservername, dbdatabasename, dbusername, dbpassword);

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = dbservername,
                InitialCatalog = dbdatabasename,
                UserID = dbusername,
                Password = dbpassword,
                IntegratedSecurity = false
            };
            Database.SQLCONNECTION_STRING = builder.ConnectionString;
        }

    }
}
