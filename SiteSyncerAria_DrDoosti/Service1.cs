using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SiteSyncerAria_DrDoosti.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Script.Serialization;

namespace SiteSyncerAria_DrDoosti
{
    public partial class Service1 : ServiceBase
    {
        private readonly SemaphoreSlim _productGate = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _invoiceGate = new SemaphoreSlim(1, 1);
        System.Timers.Timer timer = new System.Timers.Timer();
        System.Timers.Timer timersandogh = new System.Timers.Timer();
        private static readonly HttpClient client = new HttpClient();
        string _connectionString = "";
        string _url;
        string _APIKEY;

        public Service1()
        {
            var startupPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            SiteSyncerAria.IniFile ini = new SiteSyncerAria.IniFile(@"" + startupPath + @"\Settings.ini");
            UserConnection.server = ini.IniReadValue("Form", "Server");
            UserConnection.database = ini.IniReadValue("Form", "dataBase");
            UserConnection.usernamesql = ini.IniReadValue("Form", "user");
            UserConnection.passwordsql = ini.IniReadValue("Form", "pass");

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

            WriteToFile("Service is started at " + DateTime.Now);

            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 3000;

            timersandogh.Elapsed += new ElapsedEventHandler(OnElapsedtimersandogh);
            timersandogh.Interval = 5000;

            try
            {
                DatabaseSQL.InitConnectionStringSQLConnection(UserConnection.server, UserConnection.database, UserConnection.usernamesql, UserConnection.passwordsql);
                DatabaseSQL.InitConnectionString(UserConnection.server, UserConnection.database, UserConnection.usernamesql, UserConnection.passwordsql);
                _url = Database.TableSqlQuery(@"select val from setting where tag='sitesyncer' and [key]='UrlCRM'").Rows[0][0].ToString();
                _APIKEY = Database.TableSqlQuery(@"select val from setting where tag='sitesyncer' and [key]='APIKEY'").Rows[0][0].ToString();
                _connectionString = $@"Data Source={UserConnection.server};Initial Catalog={UserConnection.database};User ID={UserConnection.usernamesql};Password={UserConnection.passwordsql};Integrated Security=false";
                timer.Enabled = true;
                timersandogh.Enabled = true;
            }
            catch (Exception ex)
            {
                WriteToFile("Service startup failed: " + ex);
                throw;
            }


        }

        protected override void OnStop()
        {
            timer.Stop();
            timersandogh.Stop();
            WriteToFile("Service is stopped at " + DateTime.Now);
        }

        private async void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            //await senttosite();
            await RunProductTick();
        }

        public async Task senttosite()
        {
            try
            {
                string connectionString = $@"Data Source={UserConnection.server};Initial Catalog={UserConnection.database};User ID={UserConnection.usernamesql};Password={UserConnection.passwordsql};Integrated Security=false";
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    DataTable dt = new DataTable();
                    using (SqlCommand command = new SqlCommand("sp_sent_details_to_site", con))
                    {
                        command.Parameters.AddWithValue("@state", 0);
                        command.CommandType = CommandType.StoredProcedure;
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(dt);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        ListProduct link = new ListProduct();
                        int Codekala = Convert.ToInt32(dt.Rows[0]["external_id"].ToString());
                        link.description = dt.Rows[0]["description"].ToString();
                        link.rate = dt.Rows[0]["rate"] == DBNull.Value ? 0m : Convert.ToDecimal(dt.Rows[0]["rate"]);
                        link.unit = dt.Rows[0]["unit"].ToString();
                        link.external_id = dt.Rows[0]["external_id"].ToString() ?? "";

                        await SendWebrequest_Post_Method(link, Codekala);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToFile(" Error  senttosite : " + ex.Message);
            }
        }

        public async Task resultinsert(string type, responseBody data)
        {
            try
            {
                bool _success = Convert.ToBoolean(JsonConvert.SerializeObject(data.success));
                string _responseJson = JsonConvert.SerializeObject(data.dtos);
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    using (SqlCommand com = new SqlCommand())
                    {
                        com.Connection = con;
                        com.CommandText = $@"insert into logcrmservice([type],success,message,result)
                                                        values(@type,@success,@message,@result)";
                        com.Parameters.Add(new SqlParameter("@type", SqlDbType.VarChar, 10)).Value = type.ToString();
                        com.Parameters.Add(new SqlParameter("@success", SqlDbType.Bit)).Value = _success;
                        com.Parameters.Add("@message", SqlDbType.VarChar, 250).Value = (object)data.message ?? DBNull.Value;
                        com.Parameters.Add("@result", SqlDbType.NVarChar, -1).Value = (object)_responseJson ?? DBNull.Value;
                        if (con.State != ConnectionState.Open)
                        {
                            await con.OpenAsync();
                        }
                        await com.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToFile(" Error  Update logcrmservice : " + ex.Message);
            }
        }

        private async Task SendWebrequest_Post_Method(ListProduct link, int id)
        {
            var service = new ProductStockService(_url,client);
            try
            {
                var response = await service.ProductS(link, _APIKEY);

                await resultinsert("product", response);
                await updatedata("product", response.success, id);
            }
            catch (Exception ex)
            {
                WriteToFile($"sent to product post {id.ToString()} : " + ex.Message);
            }
        }

        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }

        private async void OnElapsedtimersandogh(object source, ElapsedEventArgs e)
        {
            //await senttositeinvoice();
            await RunInvoiceTick();
        }

        public async Task senttositeinvoice()
        {
            try
            {


                string connectionString = $@"Data Source={UserConnection.server};Initial Catalog={UserConnection.database};User ID={UserConnection.usernamesql};Password={UserConnection.passwordsql};Integrated Security=false";

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand("sp_sent_details_to_site", con))
                    {
                        command.Parameters.Add("@state", SqlDbType.Int).Value = 1;
                        command.CommandType = CommandType.StoredProcedure;
                        await con.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (!reader.HasRows)
                            {

                                return;
                            }

                            Invoice invoices = new Invoice ();

                            int idPayment = 0;
                            bool isHeaderRead = false;

                            while (await reader.ReadAsync())
                            {
                                // فقط در اولین ردیف، اطلاعات عمومی فاکتور را می‌خوانیم
                                if (!isHeaderRead)
                                {
                                    idPayment = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0;
                                    invoices.phonenumber = reader["phonenumber"]?.ToString() ?? string.Empty;
                                    invoices.company = reader["company"]?.ToString() ?? string.Empty;
                                    invoices.date = reader["date"]?.ToString() ?? string.Empty;
                                    invoices.external_id = reader["external_id"].ToString() ?? "";
                                    isHeaderRead = true;
                                }

                                // برای تمام ردیف‌ها، اقلام فاکتور خوانده و اضافه می‌شوند
                                invoices.items.Add(new item
                                {
                                    description = reader["description"]?.ToString() ?? string.Empty,
                                    unit = reader["unit"]?.ToString() ?? string.Empty,
                                    rate = reader["rate"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["rate"]),
                                    qty = Convert.ToInt32(reader["qty"]?.ToString() ?? string.Empty),
                                });
                            }
                            try
                            {
                                // ارسال داده‌ها به وب‌سرویس
                                await SendWebinvoice_Post_Method(invoices, idPayment);
                            }
                            catch (Exception ex)
                            {
                                WriteToFile(ex.Message);
                            }
                        }
                    }

                }
            }
            catch (SqlException sqlEx)
            {
                // این لاگ دقیقاً مشخص می‌کند خطای SQL چیست
                WriteToFile($"SQL Error [{sqlEx.Number}]: {sqlEx.Message} (Line: {sqlEx.LineNumber})");
            }
        }

        private async Task SendWebinvoice_Post_Method(Invoice factor, int IdSandogh)
        {

            var service = new ProductStockService(_url, client);
            try
            {
                var response = await service.invoicedetails(factor, _APIKEY);
                await resultinsert("Invoice", response);
                await updatedata("Invoice", response.success, IdSandogh);
            }
            catch (Exception ex)
            {
                WriteToFile($"sent to invoice post {IdSandogh.ToString()} : " + ex.Message);
            }
        }

        public async Task updatedata(string type, bool success, int id)
        {
            try
            {
                string query = "";
                string tablename = type == "product" ? "kala" : "sandogh";
                string columnname = type == "product" ? "code" : "sandogh";
                if (success)
                {
                    query = $@"update {tablename} set Senttosite=1 where {columnname}=@code;";
                }
                else
                {
                    query = $@"update {tablename} set Senttosite=0,SenttoSitetime=getdate() where {columnname}=@code;";
                }

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    using (SqlCommand com = new SqlCommand())
                    {
                        com.Connection = con;
                        com.CommandText = query;
                        com.Parameters.Add(new SqlParameter("@code", SqlDbType.Int)).Value = id;
                        if (con.State != ConnectionState.Open)
                        {
                            await con.OpenAsync();
                        }
                        await com.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToFile(" Error  Update kala-sandogh : " + ex.Message);
            }
        }

        private async Task RunProductTick()
        {
            if (!await _productGate.WaitAsync(0))
                return;

            try
            {
                await senttosite();
            }
            catch (Exception ex)
            {
                WriteToFile("Product timer failed: " + ex);
            }
            finally
            {
                _productGate.Release();
            }
        }

        private async Task RunInvoiceTick()
        {
            if (!await _invoiceGate.WaitAsync(0))
                return;

            try
            {
                await senttositeinvoice();
            }
            catch (Exception ex)
            {
                WriteToFile("Invoice timer failed: " + ex);
            }
            finally
            {
                _invoiceGate.Release();
            }
        }
    }
}
