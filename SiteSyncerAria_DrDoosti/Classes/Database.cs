using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;

namespace SiteSyncerAria_DrDoosti
{
    class Database
    {
        public static string CONNECTION_STRING = "";
        public static string SQLCONNECTION_STRING = "";

        public static DataTable TableSqlQuery(string sql)
        {
            try
            {
                DataTable dt = new DataTable();
                OleDbDataAdapter da = new OleDbDataAdapter(sql, CONNECTION_STRING);                
                da.Fill(dt);
                return dt;
            }
            catch //(Exception ex)
            {                
                return null;
            }        
        }

        public static int ExecuteSql(string sql)
        {
            int result;

            using (OleDbConnection dbConn = new OleDbConnection(CONNECTION_STRING))
            {
                dbConn.Open();

                using (OleDbTransaction dbTrans = dbConn.BeginTransaction())
                {
                    try
                    {
                        using (OleDbCommand dbCommand = new OleDbCommand(sql, dbConn, dbTrans))
                        {
                            result = dbCommand.ExecuteNonQuery();
                        }

                        dbTrans.Commit();
                    }
                    catch // (Exception ex)
                    {
                        dbTrans.Rollback();                        
                        result = -1;
                    }
                }

                dbConn.Close();
                return result;
            }
        }


       

       
    }
}

