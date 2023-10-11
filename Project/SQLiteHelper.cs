using System;
using System.Collections.Generic;
using System.Text;
using Finisar.SQLite;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;

namespace DefaultNS
{
    static class SQLite
    {
        static string db_file_name_default = Application.StartupPath + "\\AppData.db3";
        static public bool ExecuteNonQuery(string db_file_name, string query_string)
        {
            string connString = "Data Source=" + db_file_name + ";Version=3;New=False;Compress=True;";
            bool bCommOK = false;
            try
            {
                SQLiteConnection myConnection = new SQLiteConnection(connString);
                myConnection.Open();
                SQLiteCommand myCommand = new SQLiteCommand(query_string, myConnection);
                myCommand.ExecuteNonQuery();
                myConnection.Close();
                bCommOK = true;
            }
            catch (Exception ex)
            {
                Global.WriteLogFile("[ExecuteNonQuery] - [" + query_string + "] - " + ex.ToString());
                MessageBox.Show("[ExecuteNonQuery] - [" + query_string + "] - " + ex.ToString());
            }

            return bCommOK;
        }

        static public bool ExecuteNonQuery(string query_string)
        {
            return ExecuteNonQuery(db_file_name_default, query_string);
        }

        static public DataTable ExecuteDataTable(string db_file_name, string query_string)
        {
            string connString = "Data Source=" + db_file_name + ";Version=3;New=False;Compress=True;";
            DataTable data_table = null;
            try
            {
                SQLiteConnection myConnection = new SQLiteConnection(connString);
                myConnection.Open();
                SQLiteDataAdapter myAdapter = new SQLiteDataAdapter();
                myAdapter.SelectCommand = new SQLiteCommand(query_string, myConnection);
                DataSet dataSet = new DataSet();
                try
                {
                    myAdapter.Fill(dataSet);
                }
                catch (Exception ex)
                {
                    Global.WriteLogFile("[ExecuteDataTable/myAdapter.Fill(dataSet)] - [" + query_string + "] - " + ex.ToString());
                    MessageBox.Show("[ExecuteDataTable/myAdapter.Fill(dataSet)] - [" + query_string + "] - " + ex.ToString());
                }

                if (dataSet.Tables.Count > 0)
                    data_table = dataSet.Tables[0];
                myConnection.Close();
            }
            catch (Exception ex)
            {
                Global.WriteLogFile("[ExecuteDataTable] - [" + query_string + "] - " + ex.ToString());
                MessageBox.Show("[ExecuteDataTable] - [" + query_string + "] - " + ex.ToString());
            }

            return data_table;
        }

        static public DataTable ExecuteDataTable(string query_string)
        {
            return ExecuteDataTable(db_file_name_default, query_string);
        }
		
        static public object ExecuteScalar(string db_file_name, string query_string)
        {
            string connString = "Data Source=" + db_file_name + ";Version=3;New=False;Compress=True;";
            object retOBJ = null;
            try
            {
                SQLiteConnection myConnection = new SQLiteConnection(connString);
                myConnection.Open();
                SQLiteCommand myCommand = new SQLiteCommand(query_string, myConnection);
                retOBJ = myCommand.ExecuteScalar();
                myConnection.Close();
            }
            catch (Exception ex)
            {
                Global.WriteLogFile("[ExecuteScalar] - [" + query_string + "] - " + ex.ToString());
                MessageBox.Show("[ExecuteScalar] - [" + query_string + "] - " + ex.ToString());
            }
            return retOBJ;
        }

        static public object ExecuteScalar(string query_string)
        {
            return ExecuteScalar(db_file_name_default, query_string);
        }
		
    }
}
