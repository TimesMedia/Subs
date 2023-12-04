using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using Subs.Data;

namespace CPD.Data
{

    public class History
    {
        public int ResultId { get; set;}
        public string Publication { get; set; } = "";
        public string AccreditationNumber { get; set; } = "";
        public string Issue { get; set; } = "";
        public string Module { get; set; } = "";
        public int ModuleId { get; set; }
        public Int16 Attempt { get; set; }
        public string URL { get; set; }
        public DateTime Datum { get; set; }
        public int Score { get; set; }
        public string Verdict { get; set; } = "";
        public DateTime DateIssued { get; set; }
        public decimal NormalPoints { get; set; }
        public decimal EthicsPoints { get; set; }
        public int CustomerId { get; set; }
        public string Surname { get; set; } = "";
    }


    public class ResultData
    {
        //private List<History> gHistory = new List<History>();

         public static int GetAttempt(int pCustomerId, int pModuleId)
         {
            try
            {
                // Now get the connection object.
                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = Settings.CPDConnectionString;
                    connection.Open();
                    DbCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = "[ResultData.Attempt]";
                    command.CommandType = CommandType.StoredProcedure;

                    DbParameter lParameter1 = command.CreateParameter();
                    lParameter1.ParameterName = "CustomerId";
                    lParameter1.DbType = DbType.String;
                    lParameter1.Value = "pCustomerId";
                    command.Parameters.Add(lParameter1);

                    DbParameter lParameter2 = command.CreateParameter();
                    lParameter2.ParameterName = "ModuleId";
                    lParameter2.DbType = DbType.Int32;
                    lParameter2.Value = pModuleId;
                    command.Parameters.Add(lParameter2);
                    return (int)command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                 if (ex.InnerException == null)
                 {
                     ExceptionData.WriteException(1, ex.Message, "static ResultData", "GetAttempt", "");
                     throw new Exception( "static ResultData" + " : " + "GetAttempt" + " : ", ex);
                 }
                 else
                 {
                     throw; // Just bubble it up
                 }
            }
         }


        public static void Quit(int ResultId)
        {
            DataSet1TableAdapters.ResultTableAdapter lResultAdapter = new DataSet1TableAdapters.ResultTableAdapter();
            lResultAdapter.AttachConnection();
            try
            {

                lResultAdapter.Quit(ResultId);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ExceptionData.WriteException(1, ex.Message, "static ResultData", "Quit", "");
                    throw new Exception("static ResultData" + " : " + "Quit" + " : ", ex);
                }
                else
                {
                    throw ex; // Just bubble it up
                }
            }

        }


        //public static bool BuzyWithTest(int CustomerId)
        //{
        //    DataSet1. lHistory = new DataSet1.HistoryDataTable();
        //    GetCurrentTest(CustomerId, ref lHistory);
        //    if (lHistory.Count == 0)
        //    {
        //        // You are not buzy with any test
        //        return false;
        //    }
        //    else
        //    {
        //        // You are buzy with at least one test
        //        return true;
        //    }
        //}


        //public static bool GetCurrentTest(int CustomerId, ref List<History> pHistory)
        //{
        //    List<History> lResults = new List<History>();

        //    try
        //    {
        //        using (SqlConnection lConnection = new SqlConnection())
        //        {
        //            SqlCommand Command = new SqlCommand();
        //            SqlDataAdapter Adaptor = new SqlDataAdapter();
        //            lConnection.ConnectionString = Settings.CPDConnectionString;
        //            lConnection.Open();
        //            Command.Connection = lConnection;
        //            Command.CommandType = CommandType.StoredProcedure;
        //            Command.CommandText = "[ResultData.History.FillBy]";
        //            SqlCommandBuilder.DeriveParameters(Command);
        //            Command.Parameters["@By"].Value = "CurrentTest";
        //            Command.Parameters["@Id"].Value = CustomerId;

        //            Adaptor.SelectCommand = Command;
        //            pHistory.Clear();
        //            Adaptor.Fill(pHistory);
        //            if (pHistory.Count == 1)
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                //ExceptionData.WriteException(5, "Current test count for customer " + CustomerId.ToString() + " = " + pHistory.Count.ToString(), "static ResultData", "GetCurrentTest", "");
        //                return false;
        //            }
        //        }
        //    }

        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "static ResultData", "GetCurrentTest", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        return false;
        //    }
        //}



        public List<History> GetHistory(string pBy, int pId, DateTime? pDateId = null )
        {
            SqlDataReader lReader;
            List<History> lResults = new List<History>();
            SqlConnection connection = new SqlConnection();
            try
            {
                
                connection.ConnectionString = Settings.CPDConnectionString;
                connection.Open();
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandText = "[dbo].[ResultData.History.FillBy]";
                command.CommandType = CommandType.StoredProcedure;

                SqlParameter lParameter1 = command.CreateParameter();
                lParameter1.ParameterName = "@By";
                lParameter1.DbType = DbType.String;
                lParameter1.Value = pBy;
                command.Parameters.Add(lParameter1);

                SqlParameter lParameter2 = command.CreateParameter();
                lParameter2.ParameterName = "@Id";
                lParameter2.DbType = DbType.Int32;
                lParameter2.Value = pId;
                command.Parameters.Add(lParameter2);

               
                SqlParameter lParameter3 = command.CreateParameter();
                lParameter3.ParameterName = "@DateId";
                lParameter3.DbType = DbType.DateTime;
                if (pDateId != null)
                {
                    lParameter3.Value = pDateId;
                }
                command.Parameters.Add(lParameter3);
  
                   
                lReader = command.ExecuteReader();

                if (lReader.HasRows)
                {
                   return AssignHistory(lReader);
                }
                else throw new Exception("No history found");
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "static ResultData", "GetHistory", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw;
            }
        }


        private List<History> AssignHistory(SqlDataReader pReader)
        {
            List<History> Results = new List<History>();
            int lOffset = 0;
            try 
            { 
                while (pReader.Read())
                {
                    History lHistory = new History();

                    lOffset = 0; lHistory.ResultId = (int)pReader[lOffset];
                    lOffset++; lHistory.Publication = (string)pReader[lOffset];
                    lOffset++; lHistory.AccreditationNumber = (string)pReader[lOffset];
                    lOffset++; lHistory.Issue = (string)pReader[lOffset];
                    lOffset++; lHistory.Module = (string)pReader[lOffset];
                    lOffset++; lHistory.ModuleId = (int)pReader[lOffset];
                    lOffset++; lHistory.Attempt = (Int16)pReader[lOffset];

                    lOffset++; if (pReader[lOffset] != System.DBNull.Value)
                    {
                        lHistory.URL = (string)pReader[lOffset];
                    }


                    lOffset++; if (pReader[lOffset] != System.DBNull.Value)
                    {
                        lHistory.Datum = (DateTime)pReader[lOffset];
                    }


                    lOffset++; if (pReader[lOffset] != System.DBNull.Value)
                    {
                        lHistory.Score = (int)pReader[lOffset];
                    }

                    lOffset++; lHistory.Verdict = (string)pReader[lOffset];

                    lOffset++; if (pReader[lOffset] != System.DBNull.Value)
                    {
                        lHistory.DateIssued = (DateTime)pReader[lOffset];
                    }

                    lOffset++; lHistory.NormalPoints = (decimal)pReader[lOffset];
                    lOffset++; lHistory.EthicsPoints = (decimal)pReader[lOffset];
                    lOffset++; lHistory.CustomerId = (int)pReader[lOffset];
                    lOffset++; lHistory.Surname = (string)pReader[lOffset];

                    Results.Add(lHistory);
                } // End of while
           
                return Results;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "ResultData", "AssignHistory", "Offset = " + lOffset.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);
                throw ex;
            }

        }


        public List<History> GetByResultId(int pResultId)
        {
            SqlDataReader lReader;
            List<History> lResults = new List<History>();
            SqlConnection connection = new SqlConnection();

            try
            {
                // Now get the connection object.
            
                connection.ConnectionString = Settings.CPDConnectionString;
                connection.Open();
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandText = "[dbo].[ResultData.History.FillBy]";
                command.CommandType = CommandType.StoredProcedure;

                SqlParameter lParameter1 = command.CreateParameter();
                lParameter1.ParameterName = "@By";
                lParameter1.DbType = DbType.String;
                lParameter1.Value = "ResultId";
                command.Parameters.Add(lParameter1);

                SqlParameter lParameter2 = command.CreateParameter();
                lParameter2.ParameterName = "@Id";
                lParameter2.DbType = DbType.Int32;
                lParameter2.Value = pResultId;
                command.Parameters.Add(lParameter2);

                SqlParameter lParameter3 = command.CreateParameter();
                lParameter3.ParameterName = "@DateId";
                lParameter3.DbType = DbType.DateTime;
                command.Parameters.Add(lParameter3);

                lReader = command.ExecuteReader();

                if (lReader.HasRows)
                {
                    return AssignHistory(lReader);
                }
                else throw new Exception("No history found");
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "ResultData", "GetByResultId", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }

        public static int Initialise(int CustomerId, int ModuleId)
        {
            DataSet1TableAdapters.ResultTableAdapter lResultAdapter = new DataSet1TableAdapters.ResultTableAdapter();
            DataSet1TableAdapters.ResultDetailTableAdapter lResultDetailAdapter = new DataSet1TableAdapters.ResultDetailTableAdapter();
            DataSet1TableAdapters.QuestionTableAdapter lQuestionAdapter = new DataSet1TableAdapters.QuestionTableAdapter();
            lResultAdapter.AttachConnection();
            lResultDetailAdapter.AttachConnection();
            lQuestionAdapter.AttachConnection();
            try
            {
                // Local objects

                DataSet1 lDataSet1 = new DataSet1();

                short CurrentAttempt = (short)GetAttempt(CustomerId, ModuleId);

                // Exit if you are too far

                if (CurrentAttempt == 2)
                {
                    throw new Exception("You cannot initialise more than one attempt.");
                }

                // Create a single Result row

                DataSet1.ResultRow lResultRow = lDataSet1.Result.NewResultRow();
                lResultRow.CustomerId = CustomerId;
                lResultRow.ModuleId = ModuleId;
                lResultRow.Datum = DateTime.Now;
                lResultRow.Attempt = CurrentAttempt;
                lResultRow.Attempt++;
                lResultRow.Pass = false;
                lDataSet1.Result.AddResultRow(lResultRow);
                lResultAdapter.Update(lResultRow);
                lDataSet1.Result.AcceptChanges();

                // Create the child ResultDetail rows
               
                lQuestionAdapter.FillBy(lDataSet1.Question, ModuleId);

                foreach (DataSet1.QuestionRow lQuestionRow in lDataSet1.Question)
                {
                    DataSet1.ResultDetailRow lResultDetailRow = lDataSet1.ResultDetail.NewResultDetailRow();
                    lResultDetailRow.ResultId = lResultRow.ResultId;
                    lResultDetailRow.QuestionId = lQuestionRow.QuestionId;
                    lResultDetailRow.Datum = DateTime.Now;
                    lResultDetailRow.Answer = 0;
                    lDataSet1.ResultDetail.AddResultDetailRow(lResultDetailRow);
                }

                lResultDetailAdapter.Update(lDataSet1.ResultDetail);
                lDataSet1.ResultDetail.AcceptChanges();

                return lResultRow.ResultId;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "static Initialise", "Initialise", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }

        private static int GetCorrectAnswersByQuestion(int QuestionId)
        {
            CPD.Data.DataSet1TableAdapters.AnswerTableAdapter lAnswerAdapter = new DataSet1TableAdapters.AnswerTableAdapter();
            lAnswerAdapter.AttachConnection();
            try
            {
                DataSet1.AnswerDataTable lAnswer = new DataSet1.AnswerDataTable();

                lAnswerAdapter.FillBy(lAnswer, "QuestionId", QuestionId);


                if (lAnswer.Count == 0)
                {
                    throw new Exception("There are no answers for question with id = " + QuestionId.ToString());
                }

                int Answer = 0;
                int i = 0;

                foreach (DataSet1.AnswerRow lAnswerRow in lAnswer)
                {
                    if (lAnswerRow.Correct)
                    {
                        Answer = Answer + (int)Math.Pow(2, i);
                    }
                    i++;
                }

                return Answer;
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ExceptionData.WriteException(typeof(WarningException) == ex.GetType() ? 3 : 1, ex.Message, "static ResultData", "GetCorrectAnswersByQuestion", "");
                    throw new Exception("static ResultData" + " : " + "GetCorrectAnswersByQuestion" + " : ", ex);
                }
                else
                {
                    throw ex; // Just bubble it up
                }
            }
        }


        public static void SetCorrectAnswerBySurvey(int SurveyId)
        {
            DataSet1TableAdapters.QuestionTableAdapter lQuestionAdapter = new DataSet1TableAdapters.QuestionTableAdapter();
            lQuestionAdapter.AttachConnection();
            try
            {
                //Get all the relevant questions

                DataSet1.QuestionDataTable lQuestion = new DataSet1.QuestionDataTable();
                    
                lQuestionAdapter.FillBy1(lQuestion, "SurveyId", SurveyId);

                if (lQuestion.Count == 0)
                {
                    throw new Exception("There are no questions for survey with id = " + SurveyId.ToString());
                }

                foreach (DataSet1.QuestionRow lQuestionRow in lQuestion)
                {
                    lQuestionRow.CorrectAnswer = GetCorrectAnswersByQuestion(lQuestionRow.QuestionId);
                }

                lQuestionAdapter.Update(lQuestion);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ExceptionData.WriteException(typeof(WarningException) == ex.GetType() ? 3 : 1, ex.Message, "static ResultData", "SetCorrectAnswerBySurvey", "");
                    throw new Exception("static ResultData" + " : " + "SetCorrectAnswerBySurvey" + " : ", ex);
                }
                else
                {
                    throw ex; // Just bubble it up
                }
            }
        }


     
        //public static DataSet1.AnswerDataTable GetAnswer(int CustomerId, int QuestionId)
        //{
        //    DataSet1TableAdapters.AnswerTableAdapter lAnswerAdapter = new DataSet1TableAdapters.AnswerTableAdapter();
        //    lAnswerAdapter.AttachConnection();
        //    try
        //    {
        //        // Read in the potential answers options

        //        DataSet1.AnswerDataTable lAnswerTable = new DataSet1.AnswerDataTable();
                
        //        lAnswerAdapter.FillBy(lAnswerTable, "QuestionId", QuestionId);


        //        // Apply the actual answers and pass that on

        //        List<History> lHistoryList = new List<History>();
        //        DataSet1TableAdapters.ResultDetailTableAdapter lDetailAdapter = new DataSet1TableAdapters.ResultDetailTableAdapter();

        //        lHistoryList = GetHistory("CurrentTest", CustomerId );

        //        int lCustomerAnswer = 0;
        //        lCustomerAnswer = (int)lDetailAdapter.GetAnswer("QuestionId", lHistoryList[0].ResultId, QuestionId);
                               
        //        for (int i = lAnswerTable.Count - 1; i >= 0; i--)
        //        {
        //            if (lCustomerAnswer >= (int)Math.Pow(2, i))
        //            {
        //                lAnswerTable[i].Correct = true;
        //                lCustomerAnswer = lCustomerAnswer - (int)Math.Pow(2, i);
        //            }
        //            else
        //            {
        //                lAnswerTable[i].Correct = false;
        //            }
        //        }

        //        return lAnswerTable;
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.InnerException == null)
        //        {
        //            ExceptionData.WriteException(1, ex.Message, "static ResultData", "GetAnswer", "");
        //            throw new Exception("static ResultData" + " : " + "GetAnswer" + " : ", ex);
        //        }
        //        else
        //        {
        //            throw ex; // Just bubble it up
        //        }
        //    }
        //    finally
        //    {
        //        lAnswerAdapter.Connection.Close();
        //    }
        //}


        //public static void SetAnswer(int CustomerId, int QuestionId, int Answer)
        //{
        //    try
        //    {
        //        // Get all the answers

        //        List<History> lHistoryList = GetHistory("CurrentTest", CustomerId);
               
        //        DataSet1TableAdapters.ResultDetailTableAdapter lDetailAdapter = new DataSet1TableAdapters.ResultDetailTableAdapter();
        //        lDetailAdapter.AttachConnection();
        //        lDetailAdapter.SetAnswer(lHistoryList[0].ResultId, QuestionId, Answer);
        //        lDetailAdapter.Connection.Close();

        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.InnerException == null)
        //        {
        //            ExceptionData.WriteException(typeof(WarningException) == ex.GetType() ? 3 : 1, ex.Message, "static ResultData", "SetAnswer", "");
        //            throw new Exception("static ResultData" + " : " + "SetAnswer" + " : ", ex);
        //        }
        //        else
        //        {
        //            throw ex; // Just bubble it up
        //        }
        //    }
        //}

 
        public static int Mark(int ResultId)
        {
            DataSet1TableAdapters.ResultTableAdapter lResultAdapter = new DataSet1TableAdapters.ResultTableAdapter();
            lResultAdapter.AttachConnection();

            int Test = (int)lResultAdapter.Mark(ResultId);
            //lResultAdapter.Connection.Close();
            return Test;
        }
    }
}
