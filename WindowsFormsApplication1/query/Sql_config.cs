using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Resources;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace WindowsFormsApplication1
{
    class Sql_config
    {
        public static string DatabaseFile = @"C:\All-Fit\history_data.db";
        public static string DataSource
        {
            get
            {
                InitDataBase();
                return string.Format("data source={0}", DatabaseFile);
            }
        }
        public static string InitDataBase()
        {
            if (File.Exists(DatabaseFile))
            {

            }
            else
            {
                System.Resources.ResourceManager rm = Properties.Resources.ResourceManager;

                string str1 = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;// Process.GetCurrentProcess().MainModule.loc;
                DatabaseFile = string.Format("{0}history_data.db", str1);//存放到临时文件夹内
                FileStream Stream = new FileStream(DatabaseFile, FileMode.OpenOrCreate);


                BinaryFormatter bin = new BinaryFormatter();


                try
                {
                    bin.Serialize(Stream, rm.GetObject("history_data", null));
                    Stream.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return "OK";

        }
    }
}
