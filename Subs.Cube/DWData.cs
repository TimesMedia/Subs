using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Specialized;
using Subs.Data;

namespace DW
{
    public static class DWData
    {
        public static List<int> GetNewLiabilityEntries()
        {
            List<int> lNewList = new List<int>();
            try
            {
                SqlConnection lConnection = new SqlConnection();
                SqlCommand Command = new SqlCommand();
                SqlDataAdapter Adaptor = new SqlDataAdapter();
                lConnection.ConnectionString = Settings.SUBSDWConnectionString;
                lConnection.Open();
                Command.Connection = lConnection;
                Command.CommandType = CommandType.StoredProcedure;
                Command.CommandText = "SUBSDW.[dbo].[LiabilityNewEntries]";

                SqlDataReader lReader = Command.ExecuteReader();


                while (lReader.Read())
                {
                    lNewList.Add((lReader.GetInt32(0)));
                }

                return lNewList;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "static DWData", "GetNewLiabilityEntries", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

               throw ex;
            }


        }





    }
}
