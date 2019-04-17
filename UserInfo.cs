using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate
{
    public class UserInfo
    {
        public const string NO_USER_MSG = "Please set the user";
        
        public int id;
        public string name;
        public string scanCode;

        public UserInfo(string scanCode)
        {
            using (SqlConnection con = new SqlConnection(AutoUpdater.CON_STR))
            {
                con.Open();
                using (SqlDataReader rdr = new SqlCommand("SELECT Id, FullName, ScanCode FROM dbo.AutoUpdate_Users WHERE ScanCode = '" + scanCode + "'", con).ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        id = rdr.GetInt32(0);
                        name = rdr.GetString(1);
                        this.scanCode = scanCode;
                    }
                    else
                    {
                        id = 0;
                        name = "";
                        scanCode = "";
                    }
                }
                con.Close();
            }
        }

        public UserInfo()
        {
            id = 0;
            name = "";
            scanCode = "";
        }
    }
}
