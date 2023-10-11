using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;


namespace DefaultNS
{
    class RestApiHelper
    {
        public static string Url = "";
        public static string GetSNStatusApiString = "";
        public static string InsertPassFailDetailTestCaseApiString = "";
        public static string InsertDetailTestCaseApiString = "";
        public static string UpdateFirmwareInfoApiString = "";
        public static string GetSNInfoApiString = "";


        public static void InitGlobalVarial()
        {
            IniFile ini = new IniFile(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\AppConfig.ini");
            Url = ini.IniReadValue("API_CONNECTION", "URL", "http://10.139.3.21:81/DIPAPI/");
            GetSNStatusApiString = ini.IniReadValue("API_CONNECTION", "GetSNStatusApiString", "api/Factory/getSNStatus");
            InsertPassFailDetailTestCaseApiString = ini.IniReadValue("API_CONNECTION", "InsertPassFailDetailTestCaseApiString", "api/Factory/insertPassFailDetailTestCase");
            InsertDetailTestCaseApiString = ini.IniReadValue("API_CONNECTION", "InsertDetailTestCaseApiString", "api/Factory/insertDetailTestCase");
            UpdateFirmwareInfoApiString = ini.IniReadValue("API_CONNECTION", "UpdateFirmwareInfoApiString", "api/Factory/updateFirmwareInfo");
            GetSNInfoApiString = ini.IniReadValue("API_CONNECTION", "GetSNInfoApiString", "api/Factory/getsninfo");
        }

        public static string Post(string url, string api_name, string content)
        {
            string res = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + api_name);
            
            try
            {
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = content.Length;
                StreamWriter requestWriter = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII);
                requestWriter.Write(content);
                requestWriter.Close();

                HttpWebResponse httpWebResponse = (HttpWebResponse)request.GetResponse();

                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    Stream webStream = httpWebResponse.GetResponseStream();
                    StreamReader responseReader = new StreamReader(webStream);
                    string response = responseReader.ReadToEnd();
                    res = response;
                }

                
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("-----------------");
                Console.Out.WriteLine(ex.Message);
            }
            return res;
        }



        private static string GetValueJsonResult(string json, string keyword)
        {
            string res = json;
            if (res.Length > 0)
            {
                res = res.Replace(": ", ":");
                res = res.Replace("{", "");
                res = res.Replace("}", "");
                string[] separatingStrings = { ":" };

                string[] sItems1 = res.Split(',');
                if (sItems1.Length > 0)
                {
                    for(int i = 0; i < sItems1.Length; i++)
                    {
                        string temp = sItems1[i].Replace("\\","");
                        temp = temp.Replace("\"", "");
                        string[] sItems = temp.Split(separatingStrings, StringSplitOptions.None);
                        if (sItems.Length > 1)
                        {
                            if (sItems[0] == keyword)
                            {
                                return sItems[1];
                            }
                        }
                    }
                    
                }
                
            }
            return "";
        }


        public static string CreateContentInput(string HeaderInput, string Data)
        {
            string res = "";
            res = "{\"Inputs\": {\"" + HeaderInput + "\": \"" + Data + "\"}}";
            return res;
        }

        public static string CreateDataGetSnInput(string serial, string machine_name)
        {
            //{\"SN\":\"S02HS11310000001\",\"MachineName\":\"TEST01_PT_1\"}
            string res = "";
            res = "{\\\"SN\\\":\\\"" + serial + "\\\",\\\"MachineName\\\":\\\"" + machine_name + "\\\"}";
            return res;
        }

        public static string CreateDataInsertPassFailDetailTestCaseInput(string serial, string machine_name, string user_id, DataTable dtabTestCase)
        {
            //[{\"SN\":\"S02HS11310000001\",\"MachineName\":\"TEST01_PT_1\",\"UserID\":\"1196\",\"TestItem\":\"\",\"LowLimit\":\"\",\"Value\":\"\",\"HighLimit\":\"\",\"TestResult\":\"OK\",\"TestTime\":\"\"},
            //{\"SN\":\"S02HS11310000001\",\"MachineName\":\"TEST01_PT_1\",\"UserID\":\"1196\",\"TestItem\":\"\",\"LowLimit\":\"\",\"Value\":\"\",\"HighLimit\":\"\",\"TestResult\":\"OK\",\"TestTime\":\"\"}]
            if (dtabTestCase.Rows.Count <= 0) return "";
            string res = "";
            try
            {
                for (int i = 0; i < dtabTestCase.Rows.Count; i++)
                {
                    if(i == 0) res += "[";
                    if (i > 0) res += ",";
                    res += "{\\\"SN\\\":\\\"" + serial + "\\\",\\\"MachineName\\\":\\\"" + machine_name + "\\\",\\\"UserID\\\":\\\"" + user_id + "\\\",\\\"TestItem\\\":\\\"" + Conv.atos(dtabTestCase.Rows[i]["TestName"]) + "\\\",\\\"LowLimit\\\":\\\"" + Conv.atos(dtabTestCase.Rows[i]["LowValue"]) + "\\\",\\\"Value\\\":\\\"" + Conv.atos(dtabTestCase.Rows[i]["ReadValue"]) + "\\\",\\\"HighLimit\\\":\\\"" + Conv.atos(dtabTestCase.Rows[i]["HighValue"]) + "\\\",\\\"TestResult\\\":\\\"" + Conv.atos(dtabTestCase.Rows[i]["Result"]) + "\\\",\\\"TestTime\\\":\\\"" + Conv.atos(dtabTestCase.Rows[i]["TestTime"]) + "\\\"}";
                    if (i == dtabTestCase.Rows.Count -1) res += "]";
                }
                return res;
            }
            catch (Exception ex)
            {
                Global.WriteLogFile("[CreateDataInsertDetailTestCaseInput] - " + ex.Message);
            }
            
            return "";
        }

        public static string CreateDataInsertDetailTestCaseInput(string serial, string machine_name, DataTable dtabTestCase)
        {
            //[{\"SN\":\"20KSA650M5K0007\",\"MachineName\":\"A26 CPU  M1\",\"TestItem\":\"ABC\",\"LowLimit\":\"11\",\"Value\":\"AAA\",\"HighLimit\":\"1000\",\"TestResult\":\"12\",\"TestTime\":\"2020-05-29 09:09:09\"},
            //{\"SN\":\"20KSA650M5K0007\",\"MachineName\":\"A26 CPU  M1\",\"TestItem\":\"DEF\",\"LowLimit\":\"12\",\"Value\":\"BBB\",\"HighLimit\":\"2000\",\"TestResult\":\"33\",\"TestTime\":\"2020-05-29 09:09:09\"}]

            if (dtabTestCase.Rows.Count <= 0) return "";
            string res = "";
            try
            {
                for (int i = 0; i < dtabTestCase.Rows.Count; i++)
                {
                    if (i == 0) res += "[";
                    if (i > 0) res += ",";
                    res += "{\\\"SN\\\":\\\"" + serial + "\\\",\\\"MachineName\\\":\\\"" + machine_name + "\\\",\\\"TestItem\\\":\\\"" + Conv.atos(dtabTestCase.Rows[i]["TestName"]) + "\\\",\\\"LowLimit\\\":\\\"" + Conv.atos(dtabTestCase.Rows[i]["LowValue"]) + "\\\",\\\"Value\\\":\\\"" + Conv.atos(dtabTestCase.Rows[i]["ReadValue"]) + "\\\",\\\"HighLimit\\\":\\\"" + Conv.atos(dtabTestCase.Rows[i]["HighValue"]) + "\\\",\\\"TestResult\\\":\\\"" + Conv.atos(dtabTestCase.Rows[i]["Result"]) + "\\\",\\\"TestTime\\\":\\\"" + Conv.atos(dtabTestCase.Rows[i]["TestTime"]) + "\\\"}";
                    if (i == dtabTestCase.Rows.Count - 1) res += "]";
                }
                return res;
            }
            catch (Exception ex)
            {
                Global.WriteLogFile("[CreateDataInsertDetailTestCaseInput] - " + ex.Message);
            }

            return "";
        }
        public static string CreateDataUpdateFirmwareInfo(string serial, string machine_name, string user_name, string firmware, string firmware_version)
        {
            //{\"SN\":\"S02HS11310000001\",\"MachineName\":\"TEST01_PT_1\"}
            string res = "";
            res = "{\\\"SN\\\":\\\"" + serial + "\\\",\\\"MachineName\\\":\\\"" + machine_name + "\\\",\\\"UserName\\\":\\\"" + user_name + "\\\",\\\"Firmware\\\":\\\"" + firmware + "\\\",\\\"FirmwareVersion\\\":\\\"" + firmware_version + "\\\"}";
            return res;
        }

        public static string CreateDataGetInfoSnInput(string serial, string machine_name, string user_name)
        {
            //{\"SN\":\"S02HS11310000001\",\"MachineName\":\"TEST01_PT_1\"}
            string res = "";
            res = "{\\\"SN\\\":\\\"" + serial + "\\\",\\\"MachineName\\\":\\\"" + machine_name + "\\\",\\\"UserName\\\":\\\"" + user_name + "\\\"}";
            return res;
        }

        #region TEST
        /*
        public static string GetSnStatusTest(string serial, string machine_name)
        {
            string res = "";
            res = Post(RestApiHelper.Url, "api/Factory/getSNStatus", RestApiHelper.CreateContentInput("InputGetSNStatus", CreateDataGetSnInput("S02HS11310000001", "TEST01_PT_1"))); ;
            return res;
        }

        */
        #endregion
    }
}
