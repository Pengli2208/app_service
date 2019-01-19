using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using WindowsFormsApplication1.JS_Service;
using System.Data;
using System.IO;
using System.Threading;

namespace WindowsFormsApplication1.query
{
    class append
    {
        private string[] tableNames = new string[]{
            "errorData","fileData","jobData","processData",
            "updatedInfo"
        };

        private const int tableMax = 4;
        private static int jobNew = 0;

        private int syncJobID = 0;
        private int syncProcessDataID = 0;
        private int syncFileID = 0;
        private int syncErrorID = 0;
        
        private string ErrNotify_Service(string reporter,string problemType,string Ext, string Time)
        {
           // deviceInfo devInfo = new deviceInfo;
            Service1SoapClient test1 = new Service1SoapClient();
            string ret;
            //string productSN = devInfo.GetProductType(
            try
            {
                ret = test1.CollectDevice_MES_TPMData_New(reporter,deviceInfo.deviceCode,problemType,Ext,Time);
                
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return ret;
        }
        private string StartJob_Service(string reporter, string productSN, string Ext, string Time)
        {
            deviceInfo devInfo = new deviceInfo();
            Service1SoapClient test1 = new Service1SoapClient();
            string productType = devInfo.GetProductType(productSN);
            jobNew++;
            string ret;
            try
            {
                ret = test1.CollectDevice_MES_StartJob_New(reporter, deviceInfo.deviceCode, productType, productSN, deviceInfo.operationGroup, Ext, Time);
                if (ret == "OK")
                {
                    string ret1 = test1.CollectDevice_MES_ControlJob_New(reporter, deviceInfo.deviceCode, productType, productSN, deviceInfo.operationGroup, Ext, "0", Time);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
            return ret;
        }
        private string EndJob_Service(string reporter, string productSN, string Ext, string Time)
        {
            string ret;
             deviceInfo devInfo = new deviceInfo();
            Service1SoapClient test1 = new Service1SoapClient();
            string productType = devInfo.GetProductType(productSN);
            try
            {
                if (jobNew > 0)
                {
                    jobNew--;
                    string ret1 = test1.CollectDevice_MES_ControlJob_New(reporter, deviceInfo.deviceCode, productType, productSN, deviceInfo.operationGroup, Ext, "1", Time);

                }
                ret = test1.CollectDevice_MES_EndJob_New(reporter, deviceInfo.deviceCode, productType, productSN, deviceInfo.operationGroup, Ext, Time);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
              
            return ret;
        }
        private string NewData_Service(string reporter, string productSN, string propertyName, string propertyValue, string Ext, string Time)
        {
            string ret = "ERROR";
             deviceInfo devInfo = new deviceInfo();
            Service1SoapClient test1 = new Service1SoapClient();
            string productType = devInfo.GetProductType(productSN);
            try
            {
                ret = test1.CollectDevice_MES_ProcessData_New(reporter, deviceInfo.deviceCode, productType, productSN, propertyName, propertyValue, Ext, Time);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return ret;
        }
        private string NewFile_Service(string reporter, string productSN, string docType, string fileName, string filePath, string Ext, string Time)
        {
            string ret = "ERROR";
            deviceInfo devInfo = new deviceInfo();
            Service1SoapClient test1 = new Service1SoapClient();
            string productType = devInfo.GetProductType(productSN);
            string filePathAll = filePath + fileName;
            using (FileStream fileStream = new FileStream(filePathAll, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // 读取文件的 byte[]  
                byte[] bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                fileStream.Close();

    /*
                // 把 byte[] 转换成 Stream  
                Stream stream = new MemoryStream(bytes);
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始 
                stream.Seek(0, SeekOrigin.Begin);
    */
                try
                {
                    ret = test1.CollectDevice_MES_Doc_New(reporter, deviceInfo.deviceCode, productType, productSN, docType, deviceInfo.folderId, fileName, bytes, Ext, Time);

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            return ret;
    }
        private string SyncErrorData(DataTable data, SQLiteHelper sh)
        {
            string ret = "ERROR";
            for (int i = 0; i < data.Rows.Count; i++)
            {
                string reporter = data.Rows[i]["reporter"].ToString();
                string problemType = data.Rows[i]["problemType"].ToString();
                string Ext = data.Rows[i]["ext"].ToString();
                string Time = data.Rows[i]["time"].ToString();
               
                ret = ErrNotify_Service( reporter, problemType, Ext,  Time);
                if (ret == "OK")
                {
                    Dictionary<string, object> dicData = new Dictionary<string, object>();
                    dicData["ID"] = data.Rows[i]["ID"].ToString();
                    var dic3 = new Dictionary<string, object>();
                    dic3["updated"] = 1;

                    sh.Update("errorData", dicData, dic3);
                    syncErrorID = Convert.ToInt32(data.Rows[i]["ID"].ToString());
                }
                else
                {
                    break;
                }

            }

            return ret;

        }
        private string SyncFileData(DataTable data, SQLiteHelper sh)
        {
            string ret = "ERROR";
            for (int i = 0; i < data.Rows.Count; i++)
            {
                string reporter = data.Rows[i]["reporter"].ToString();
                string productSN = data.Rows[i]["productSN"].ToString();
                string docType = data.Rows[i]["docType"].ToString();
                string fileName = data.Rows[i]["fileName"].ToString();
                string filePath = data.Rows[i]["filePath"].ToString();
                string Ext = data.Rows[i]["ext"].ToString();
                string Time = data.Rows[i]["time"].ToString();

                ret = NewFile_Service(reporter, productSN,docType,fileName,filePath, Ext, Time);
                if (ret == "OK")
                {
                    Dictionary<string, object> dicData = new Dictionary<string, object>();
                    dicData["ID"] = data.Rows[i]["ID"].ToString();
                    var dic3 = new Dictionary<string, object>();
                    dic3["updated"] = "1";
                    syncFileID = Convert.ToInt32(data.Rows[i]["ID"].ToString());
                    sh.Update("fileData", dicData, dic3);
                }
                else
                {
                    break;
                }

            }

            return ret;

        }
        private string SyncJobData(DataTable data, SQLiteHelper sh)
        {
            string ret = "ERROR";
            for (int i = 0; i < data.Rows.Count; i++)
            {
                string reporter = data.Rows[i]["reporter"].ToString();
                string productSN = data.Rows[i]["productSN"].ToString();
                string jobType = data.Rows[i]["jobType"].ToString();
                string Ext = data.Rows[i]["ext"].ToString();
                string Time = data.Rows[i]["time"].ToString();

                if(jobType == "1")
                    ret = StartJob_Service(reporter, productSN, Ext, Time);
                else
                    ret = EndJob_Service(reporter, productSN, Ext, Time);
                if (ret == "OK")
                {
                    Dictionary<string, object> dicData = new Dictionary<string, object>();
                    dicData["ID"] = data.Rows[i]["ID"].ToString();
                    var dic3 = new Dictionary<string, object>();
                    dic3["updated"] = "1";
                    syncJobID = Convert.ToInt32(data.Rows[i]["ID"].ToString());
                    sh.Update("jobData", dicData, dic3);
                }
                else
                {
                    break;
                }

            }

            return ret;

        }
        private string SyncProcessData(DataTable data, SQLiteHelper sh)
        {
            string ret = "ERROR";
            for (int i = 0; i < data.Rows.Count; i++)
            {

                string reporter = data.Rows[i]["reporter"].ToString();
                string productSN = data.Rows[i]["productSN"].ToString();
                string propertyName = data.Rows[i]["propertyName"].ToString();
                string propertyValue = data.Rows[i]["propertyValue"].ToString();
                string Ext = data.Rows[i]["ext"].ToString();
                string Time = data.Rows[i]["time"].ToString();

                ret = NewData_Service(reporter, productSN,propertyName,propertyValue, Ext, Time);
                if (ret == "OK")
                {
                    Dictionary<string, object> dicData = new Dictionary<string, object>();
                    dicData["ID"] = data.Rows[i]["ID"].ToString();
                    var dic3 = new Dictionary<string, object>();
                    dic3["updated"] = "1";
                    syncProcessDataID = Convert.ToInt32(data.Rows[i]["ID"].ToString());
                    sh.Update("processData", dicData, dic3);
                }
                else
                {
                    break;
                }

            }

            return ret;

        }

        public void SyncData()
        {
            string ret = "ERROR";
            while (true)
            {

                using (SQLiteConnection conn = new SQLiteConnection(Sql_config.DataSource))
                {
                    using (SQLiteCommand cmd = new SQLiteCommand())
                    {
                        cmd.Connection = conn;
                        conn.Open();

                        SQLiteHelper sh = new SQLiteHelper(cmd);
                        for (int idx = 0; idx < tableMax; idx++)
                        {
                            string sqlCmd = string.Format("Select * From {0} where updated == 0;", tableNames[idx]);

                            DataTable result;
                            try
                            {
                                result = sh.Select(sqlCmd);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("sync select function failed " + idx.ToString() + ex.Message);
                            }
                            try
                            {
                                if (result.Rows.Count >= 1)
                                {
                                    switch (idx)
                                    {
                                        case 0:
                                            ret = SyncErrorData(result, sh);
                                            break;
                                        case 1:
                                            ret = SyncFileData(result, sh);
                                            break;
                                        case 2:
                                            ret = SyncJobData(result, sh);
                                            break;
                                        case 3:
                                            ret = SyncProcessData(result, sh);
                                            break;
                                        default:
                                            ret = "ERROR";
                                            break;

                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("sync No valid data found!" + ex.Message);
                            }
                        }
                        conn.Close();
                    }
                }
                Thread.Sleep(1000);
            }
           // return ret;
        }
        public string BackGroundTask()
        {
            Thread mythread = new Thread(SyncData);
            mythread.Start();
            return "OK";
        }
        public string ErrNotify(string reporter,string problemType,string Ext, string Time)
        {
            int updated = 0;
            string ret = ErrNotify_Service(reporter,problemType,Ext,Time);
            if(ret.Equals("OK"))
                updated = 1;
            using (SQLiteConnection conn = new SQLiteConnection(Sql_config.DataSource))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    conn.Open();

                    SQLiteHelper sh = new SQLiteHelper(cmd);

                    

                    var dic = new Dictionary<string, object>();
                    dic["reporter"] = reporter;
                    dic["problemType"] = problemType;
                    dic["ext"] = Ext;
                    dic["time"] = Time;
                    dic["updated"] = updated;                    
                    sh.Insert("errorData", dic);                
                    conn.Close();                    
                }
            }
            return ret;
        }
        public string StartJob(string reporter, string productSN, string Ext, string Time)
        {
            int updated = 0;
            string ret = StartJob_Service(reporter, productSN, Ext, Time);
            if (ret.Equals("OK"))
                updated = 1;
            using (SQLiteConnection conn = new SQLiteConnection(Sql_config.DataSource))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    conn.Open();

                    SQLiteHelper sh = new SQLiteHelper(cmd);

                    

                    var dic = new Dictionary<string, object>();
                    dic["reporter"] = reporter;
                    dic["productSN"] = productSN;
                    dic["ext"] = Ext;
                    dic["time"] = Time;
                    dic["updated"] = updated;
                    dic["jobType"] = "1";
                    sh.Insert("jobData", dic);

                    conn.Close();
                }
            }
            return ret;
        }
        public string EndJob(string reporter, string productSN, string Ext, string Time)
        {
            int updated = 0;
            string ret = EndJob_Service(reporter, productSN, Ext, Time);
            if (ret.Equals("OK"))
                updated = 1;
            using (SQLiteConnection conn = new SQLiteConnection(Sql_config.DataSource))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    conn.Open();

                    SQLiteHelper sh = new SQLiteHelper(cmd);

                   

                    var dic = new Dictionary<string, object>();
                    dic["reporter"] = reporter;
                    dic["productSN"] = productSN;
                    dic["ext"] = Ext;
                    dic["time"] = Time;
                    dic["updated"] = updated;
                    dic["jobType"] = "0";
                    sh.Insert("jobData", dic);

                    conn.Close();
                }
            }
            return ret;
        }

        public string NewData(string reporter, string productSN, string propertyName, string propertyValue, string Ext, string Time)
        {
            int updated = 0;
            string ret = NewData_Service(reporter, productSN, propertyName, propertyValue, Ext, Time);
            if (ret.Equals("OK"))
                updated = 1;
            using (SQLiteConnection conn = new SQLiteConnection(Sql_config.DataSource))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    conn.Open();

                    SQLiteHelper sh = new SQLiteHelper(cmd);

                    

                    var dic = new Dictionary<string, object>();
                    dic["reporter"] = reporter;
                    dic["productSN"] = productSN;
                    dic["ext"] = Ext;
                    dic["propertyName"] = propertyName;
                    dic["propertyValue"] = propertyValue;
                    dic["time"] = Time;
                    dic["updated"] = updated;
                    sh.Insert("processData", dic);
                    conn.Close();
                }
            }
            return ret;
        }
        public string NewFile(string reporter, string productSN, string docType, string fileName, string filePath, string Ext, string Time)
        {
            int updated = 0;
            string ret = NewFile_Service(reporter, productSN, docType, fileName, filePath, Ext, Time);
            if (ret.Equals("OK"))
                updated = 1;
            using (SQLiteConnection conn = new SQLiteConnection(Sql_config.DataSource))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    conn.Open();

                    SQLiteHelper sh = new SQLiteHelper(cmd);
                

                    var dic = new Dictionary<string, object>();
                    dic["reporter"] = reporter;
                    dic["productSN"] = productSN;
                    dic["docType"] = docType;
                    dic["fileName"] = fileName;
                    dic["filePath"] = filePath;
                    dic["ext"] = Ext;
                    dic["time"] = Time;
                    dic["updated"] = updated;

                    sh.Insert("fileData", dic);
                    conn.Close();
                }
            }
            return ret;
        }



    }
}
