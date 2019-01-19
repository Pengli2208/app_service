using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WindowsFormsApplication1.JS_Service;
//using WindowsFormsApplication1.deviceInfo;
using WindowsFormsApplication1.query;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            deviceInfo devInfo = new deviceInfo();

            try
            {
                devInfo.InitDeviceInfo();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            textBox1.Text = deviceInfo.deviceCode;
            textBox2.Text = deviceInfo.operationGroup;
            textBox3.Text = deviceInfo.region;
            textBox4.Text = Convert.ToString(deviceInfo.folderId);
        }
        
        //end public interface

        private void button1_Click(object sender, EventArgs e)
        {
            string devCode;
            string operationGroup ;
            string region;
            int foldId ;
            string ret = "OK";
            try
            {
                devCode = textBox1.Text.ToString();
                operationGroup = textBox2.Text.ToString();
                region = textBox3.Text.ToString();
                foldId = Convert.ToInt32(textBox4.Text.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            try
            {
               deviceInfo devInfo = new deviceInfo();
               ret = devInfo.SetDeviceInfo(devCode, operationGroup, region, foldId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            MessageBox.Show(ret);
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string ret = "OK";
            append test1 = new append();
            string reporter = "Test Id";
            string productSN = "AllFit";
            string propertyName = "volt";
            string propertyValue = "100";
            string Ext = "Ext";
            string Time = DateTime.Now.ToString("u");

            string problemType = "Unk ";
            Sql_config.InitDataBase();
            int index = comboBox1.SelectedIndex;
            switch(index)
            {
                case 3:
                    for(int idx=0;idx<10;idx++)
                    {
                        try
                        {
                            reporter += idx.ToString();
                            productSN += idx.ToString();
                            propertyName += idx.ToString();
                            propertyValue += idx.ToString();
                            Ext += idx.ToString();

                            ret = test1.NewData(reporter, productSN, propertyName, propertyValue, Ext, Time);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                break;
                case 0:
                for (int idx = 0; idx < 10; idx++)
                {
                    try
                    {
                        reporter += idx.ToString();
                        productSN += idx.ToString();
                        Ext += idx.ToString();
                        ret = test1.ErrNotify(reporter, problemType, Ext, Time);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                break;
                case 1:
                for (int idx = 0; idx < 10; idx++)
                {
                    try
                    {
                        reporter += idx.ToString();
                        productSN += idx.ToString();
                        Ext += idx.ToString();
                       // (string reporter, string productSN, string Ext, string Time)
                        ret = test1.ErrNotify(reporter, problemType, Ext, Time);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                break;
            }
            MessageBox.Show(ret);
        }


    }
}


