using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using System.Reflection;
using Subs.Data;

namespace CPD.Data
{
    public struct AvailableSurvey
    {
        public int SurveyId { get; set; }
        public string Publication { get; set; }
        public int IssueId { get; set; }
        public string EBookURL { get; set; }
        public string ExpirationDate { get; set; }
        public int? Facility { get; set; }
    }


    public struct AvailableModule
    { 
        public int ModuleId { get; set; }
        public string Publication { get; set; }
        public string Issue { get; set; }
        public string Module { get; set; }
        public int? NormalPoints { get; set; }
        public int? EthicsPoints { get; set; }
        public int? Passrate { get; set; }
        public string Expiration {get; set; }
        public string AdvertURL { get; set; }
    }
  
       
    public static class ModuleData
    {
        public static List<AvailableModule> GetAvailableModules(int pSurveyId)
            {
                try
                {
                    DbProviderFactory factory = SqlClientFactory.Instance;

                    // Now get the connection object.
                    using (SqlConnection connection = new SqlConnection())
                    {
                        connection.ConnectionString = Settings.CPDConnectionString;
                        connection.Open();
                        SqlCommand command = new SqlCommand();
                        command.Connection = connection;
                        command.CommandText = "[dbo].[ModuleData.GetAvailableModules]";
                        command.CommandType = CommandType.StoredProcedure;
                        DbParameter lParameter1 = command.CreateParameter();
                        lParameter1.ParameterName = "SurveyId";
                        lParameter1.DbType = DbType.Int32;
                        lParameter1.Value = pSurveyId;
                        command.Parameters.Add(lParameter1);
                       
                        List<AvailableModule> lResults = new List<AvailableModule>();

                        using (DbDataReader dataReader = command.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                AvailableModule lAvailibleModule = new AvailableModule()
                                {
                                    ModuleId = (int)dataReader[nameof(AvailableModule.ModuleId)],
                                    Publication = (string)dataReader[nameof(AvailableModule.Publication)],
                                    Issue = (string)dataReader[nameof(AvailableModule.Issue)],
                                    Module = (string)dataReader[nameof(AvailableModule.Module)],
                                    NormalPoints = (int?)dataReader[nameof(AvailableModule.NormalPoints)],
                                    EthicsPoints = (int?)dataReader[nameof(AvailableModule.EthicsPoints)],
                                    Passrate = (int?)dataReader[nameof(AvailableModule.Passrate)],
                                    Expiration = (string)dataReader[nameof(AvailableModule.Expiration)]
                                };

                            lResults.Add(lAvailibleModule);
                            }
                        }
                        return lResults;
                    }
                }
                catch (Exception ex)

                {
                    //Display all the exceptions

                    Exception CurrentException = ex;
                    int ExceptionLevel = 0;
                    do
                    {
                        ExceptionLevel++;
                        ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, nameof(ModuleData), nameof(GetAvailableModules), "");
                        CurrentException = CurrentException.InnerException;
                    } while (CurrentException != null);

                    throw;
                }
            }

        public static List<AvailableSurvey> GetAvailableTest(int pCustomerId)
        {
            try 
            { 
                List<AvailableSurvey> lSurveys = new List<AvailableSurvey>();

                DbProviderFactory factory = SqlClientFactory.Instance;

                // Now get the connection object.
                using (SqlConnection connection = new SqlConnection())
                { 
                    connection.ConnectionString = Settings.CPDConnectionString;
                    connection.Open();
                    DbCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = "[dbo].[ModuleData.GetAvailableTest]";
                    command.CommandType = CommandType.StoredProcedure;
                   
                    using (DbDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            AvailableSurvey lSurvey = new AvailableSurvey()
                            { 

                            SurveyId = (int)dataReader[nameof(AvailableSurvey.SurveyId)],
                            Publication = (string)dataReader[nameof(AvailableSurvey.Publication)],
                            IssueId = (int)dataReader[nameof(AvailableSurvey.IssueId)],
                            EBookURL = (string)dataReader[nameof(AvailableSurvey.EBookURL)],
                            ExpirationDate = (string)dataReader[nameof(AvailableSurvey.ExpirationDate)] };

                            lSurveys.Add(lSurvey);
                        }
                    }
                    return lSurveys;
                }
            }
            catch(Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, nameof(ModuleData), nameof(GetAvailableTest), "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw;
            }
        }


        public static List<AvailableSurvey> GetAvailableRead(int pCustomerId)
        {
            try
            {
                List<AvailableSurvey> lSurveys = new List<AvailableSurvey>();

                DbProviderFactory factory = SqlClientFactory.Instance;

                // Now get the connection object.
                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = Settings.CPDConnectionString;
                    connection.Open();
                    DbCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = "[dbo].[ModuleData.GetAvailableRead]";
                    command.CommandType = CommandType.StoredProcedure;
                    DbParameter lParameter1 = command.CreateParameter();
                    lParameter1.ParameterName = "CustomerId";
                    lParameter1.DbType = DbType.Int32;
                    lParameter1.Value = pCustomerId;
                    command.Parameters.Add(lParameter1);

                    using (DbDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            AvailableSurvey lSurvey = new AvailableSurvey()
                            {
                                SurveyId = (int)dataReader[nameof(AvailableSurvey.SurveyId)],
                                Publication = (string)dataReader[nameof(AvailableSurvey.Publication)],
                                IssueId = (int)dataReader[nameof(AvailableSurvey.IssueId)],
                                EBookURL = (string)dataReader[nameof(AvailableSurvey.EBookURL)],
                                ExpirationDate = (string)dataReader[nameof(AvailableSurvey.ExpirationDate)]
                            };

                            lSurveys.Add(lSurvey);
                        }
                    }
                    return lSurveys;
                }
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, nameof(ModuleData), nameof(GetAvailableRead), "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw;
            }
        }




        //public static int GetIssueId(int ModuleId)
        //{
        //    try
        //    {
        //        // Get the corresponding IssueIdQuestionareDoc

        //        DataSet1TableAdapters.ModuleTableAdapter lModuleAdapter = new DataSet1TableAdapters.ModuleTableAdapter();
        //        return (int)lModuleAdapter.GetIssueId(ModuleId);
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.InnerException == null)
        //        {
        //            ExceptionData.WriteException(1, ex.Message, "static ResultData", "GetIssueId", "");
        //            throw new Exception("static ResultData" + " : " + "GetIssueId" + " : ", ex);
        //        }
        //        else
        //        {
        //            throw ex; // Just bubble it up
        //        }
        //    }
        //}

        public static string GetQuestions(int pModuleId)
        {

            try
            {
                DataSet1 lDataSet1 = new DataSet1();
                DataSet1TableAdapters.ModuleTableAdapter lModuleTableAdapter = new Data.DataSet1TableAdapters.ModuleTableAdapter();
                DataSet1TableAdapters.ArticleTableAdapter lArticleAdapter = new Data.DataSet1TableAdapters.ArticleTableAdapter();
                DataSet1TableAdapters.QuestionTableAdapter lQuestionAdapter = new Data.DataSet1TableAdapters.QuestionTableAdapter();
                lModuleTableAdapter.FillBy(lDataSet1.Module, pModuleId);
                lArticleAdapter.FillBy(lDataSet1.Article, pModuleId);
                lQuestionAdapter.FillBy(lDataSet1.Question, pModuleId);


                // Get the data into string format
                MemoryStream lMemoryStream1 = new MemoryStream(0);
                MemoryStream lMemoryStream2 = new MemoryStream(0);
                MemoryStream lMemoryStream3 = new MemoryStream(0);

                lDataSet1.WriteXml(lMemoryStream1, System.Data.XmlWriteMode.IgnoreSchema);

                // Replace the first line
                lMemoryStream1.Position = 0;
                StreamReader lReader = new StreamReader(lMemoryStream1);
                StreamWriter lWriter = new StreamWriter(lMemoryStream2);
                lReader.ReadLine(); // Read the first line.
                lWriter.WriteLine("<DataSet1>"); // Write another first line

                while (!lReader.EndOfStream)  // Copy the rest of the lines
                {
                    lWriter.WriteLine(lReader.ReadLine());
                }

                lWriter.Flush();
                return Encoding.UTF8.GetString(lMemoryStream2.GetBuffer(), 0, (int)lMemoryStream2.Length);


            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "static ModuleData", "GetQuestions", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return "Error in GetQuestions: " + ex.Message;
            }

        }

    }
}
