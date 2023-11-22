using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Subs.Data;


namespace CPD.Data
{
	/// <summary>
	/// Summary description for ExceptionData.
	/// </summary>
	public static class ExceptionData
	{
		private static SqlConnection gConnection = new SqlConnection();

        static ExceptionData()
        {
            gConnection.ConnectionString = Settings.CPDConnectionString;

		}

        public static void WriteExceptionToFile(string FileName, int Severity, string Message, string Object, string Method,
            string Comment)
        {
                StreamWriter lStreamWriter = new StreamWriter(FileName, true);
                string FullMessage = "\n" + DateTime.Now.ToString() + " Severity: " + Severity.ToString() + " ";
                FullMessage = FullMessage + Message + " ";
                FullMessage = FullMessage + Object + " ";
                FullMessage = FullMessage + Method + " ";
                FullMessage = FullMessage + Comment;
                lStreamWriter.WriteLine(FullMessage);
                lStreamWriter.Close();
         }


		public static void WriteException(int Severity, string Message, string Object, string Method, 
			string Comment) 
		{
			try
			{
                 //Remember the stuff in the database

				SqlCommand Command = new SqlCommand(); 
				SqlDataAdapter Adaptor = new SqlDataAdapter(); 
				
                gConnection.Open();
				Command.Connection = gConnection;
				Command.CommandType = CommandType.StoredProcedure;
				Command.CommandText = "[dbo].[MExceptionData001]";
				SqlCommandBuilder.DeriveParameters(Command);

				Command.Parameters["@Severity"].Value = Severity;
				Command.Parameters["@Message"].Value = Message;
				Command.Parameters["@Object"].Value = Object;
				Command.Parameters["@Method"].Value = Method;
				Command.Parameters["@Comment"].Value = Comment;

				Command.ExecuteScalar();

			}
			finally
			{
				gConnection.Close();
			}

		}
	}
}
