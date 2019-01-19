using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using WindowsFormsApplication1.JS_Service;
using System.Data;
namespace WindowsFormsApplication1
{
    class deviceInfo
    {
        public static string deviceCode ="";
        public static string operationGroup = "";
        public static string region = "";
        public static int folderId = 0;

        public string GetProductType(string productSN)
        {
            Service1SoapClient test1 = new Service1SoapClient();
            return test1.ReturnDevice_SN_Info(region, productSN);
        }


        public string SetDeviceInfo(string _deviceCode, string _operationGroup, string _region, int _folderId)
        {
            deviceCode = _deviceCode;
            operationGroup = _operationGroup;
            region = _region;
            folderId = _folderId;
            using (SQLiteConnection conn = new SQLiteConnection(Sql_config.DataSource))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    conn.Open();

                    SQLiteHelper sh = new SQLiteHelper(cmd);

                   // int count = sh.ExecuteScalar<int>("select count(*) from Config;") + 1;

                    var dic = new Dictionary<string, object>();
                    dic["deviceCode"] = deviceCode;
                    dic["operationGroup"] = operationGroup;
                    dic["region"] = region;
                    dic["folderId"] = folderId;
                    sh.Insert("Config", dic);
                    
                    conn.Close();

                    
                }
            }

            //todo
            //get
          
            return "OK";
        }

        public string InitDeviceInfo()
        {            
            using (SQLiteConnection conn = new SQLiteConnection(Sql_config.DataSource))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    conn.Open();

                    SQLiteHelper sh = new SQLiteHelper(cmd);
                    const string sqlCmd = "Select * From Config limit ((select count(ID) from Config)-1),1";
                    DataTable result;
                    try
                    {
                        result = sh.Select(sqlCmd);
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("select function failed" + ex.Message);
                    }
                    try
                    {
                        if (result.Rows.Count >= 1)
                        {
                            deviceCode = result.Rows[0]["deviceCode"].ToString();
                            operationGroup = result.Rows[0]["operationGroup"].ToString();
                            region = result.Rows[0]["region"].ToString();
                            folderId = Convert.ToInt32(result.Rows[0]["folderId"].ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("No valid data found!" + ex.Message);
                    }
                }
            }
            return "OK";
        }
    }
}
