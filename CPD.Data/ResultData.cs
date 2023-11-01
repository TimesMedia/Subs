using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;

namespace CPD.Data
{

    public class History
    {
        public int ResultId { get; set;}
        public string Publication { get; set; } = "";
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


        // public static void Quit(int ResultId)
        // {   
        //    DataSet1TableAdapters.ResultTableAdapter lResultAdapter = new DataSet1TableAdapters.ResultTableAdapter();
        //    lResultAdapter.AttachConnection();
        //    try
        //     {

        //         lResultAdapter.Quit(ResultId);
        //     }
        //     catch (Exception ex)
        //     {
        //         if (ex.InnerException == null)
        //         {
        //             ExceptionData.WriteException(1, ex.Message, "static ResultData", "Quit", "");
        //             throw new Exception("static ResultData" + " : " + "Quit" + " : ", ex);
        //         }
        //         else
        //         {
        //             throw ex; // Just bubble it up
        //         }
        //     }

        // }


        //public static bool BuzyWithTest(int CustomerId)
        //{
        //    DataSet1.HistoryDataTable lHistory = new DataSet1.HistoryDataTable();
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

        public static List<History> GetHistory(string pBy, int pId, DateTime? pDateId = null )
        {
            try
            {
                List<History> lResults = new List<History>();
               

                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = Settings.CPDConnectionString;
                    connection.Open();
                    DbCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = "[dbo].[ResultData.History.FillBy]";
                    command.CommandType = CommandType.StoredProcedure;

                    DbParameter lParameter1 = command.CreateParameter();
                    lParameter1.ParameterName = "@By";
                    lParameter1.DbType = DbType.String;
                    lParameter1.Value = pBy;
                    command.Parameters.Add(lParameter1);

                    DbParameter lParameter2 = command.CreateParameter();
                    lParameter2.ParameterName = "@Id";
                    lParameter2.DbType = DbType.Int32;
                    lParameter2.Value = pId;
                    command.Parameters.Add(lParameter2);

                    if (pDateId != null)
                    {
                        DbParameter lParameter3 = command.CreateParameter();
                        lParameter3.ParameterName = "@DateId";
                        lParameter3.DbType = DbType.DateTime;
                        lParameter3.Value = pDateId;
                        command.Parameters.Add(lParameter3);
                    }
                    using (DbDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            History lHistory = new History();

                            lHistory.ResultId = (int)dataReader[nameof(History.ResultId)];
                            lHistory.ModuleId = (int)dataReader[nameof(History.ModuleId)];
                            lHistory.Publication = (string)dataReader[nameof(History.Publication)];
                            lHistory.Issue = (string)dataReader[nameof(History.Issue)];
                            lHistory.Module = (string)dataReader[nameof(History.Module)];
                            lHistory.Attempt = (Int16)dataReader[nameof(History.Attempt)];

                            if (dataReader[nameof(History.URL)] != System.DBNull.Value)
                            {
                                lHistory.URL = (string)dataReader[nameof(History.URL)];
                            }

                            if (dataReader[nameof(History.Datum)] != System.DBNull.Value)
                            {
                                lHistory.Datum = (DateTime)dataReader[nameof(History.Datum)];
                            }


                            if (dataReader[nameof(History.Score)] != System.DBNull.Value)
                            {
                                lHistory.Score = (int)dataReader[nameof(History.Score)];
                            } 

                            lHistory.Verdict = (string)dataReader[nameof(History.Verdict)];

                            if (dataReader[nameof(History.DateIssued)] != System.DBNull.Value)
                                {
                                    lHistory.DateIssued = (DateTime)dataReader[nameof(History.DateIssued)];
                                }

                            lHistory.NormalPoints = (decimal)dataReader[nameof(History.NormalPoints)];
                            lHistory.EthicsPoints = (decimal)dataReader[nameof(History.EthicsPoints)];
                            lHistory.CustomerId = (int)dataReader[nameof(History.CustomerId)];
                            lHistory.Surname = (string)dataReader[nameof(History.Surname)];

                            lResults.Add(lHistory);
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "static ResultData", "GetHistory", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw;
            }
        }
     

        //public static List<History> GetHistory(int pCustomerId)
        //{
        //    try
        //    {
        //        DbProviderFactory factory = SqlClientFactory.Instance;

        //        // Now get the connection object.
        //        using (DbConnection connection = factory.CreateConnection())
        //        {
        //            connection.ConnectionString = Settings.CPDConnectionString;
        //            connection.Open();
        //            DbCommand command = factory.CreateCommand();
        //            command.Connection = connection;
        //            command.CommandText = "[DataSet1.History.FillBy]";
        //            command.CommandType = CommandType.StoredProcedure;

        //            DbParameter lParameter1 = command.CreateParameter();
        //            lParameter1.ParameterName = "By";
        //            lParameter1.DbType = DbType.String;
        //            lParameter1.Value = "History";
        //            command.Parameters.Add(lParameter1);

        //            DbParameter lParameter2 = command.CreateParameter();
        //            lParameter2.ParameterName = "Id";
        //            lParameter2.DbType = DbType.Int32;
        //            lParameter2.Value = pCustomerId;
        //            command.Parameters.Add(lParameter2);

        //            return HistoryReader(ref command);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.InnerException == null)
        //        {
        //            ExceptionData.WriteException(1, ex.Message, "static ResultData", "GetHistory", "");
        //            throw new Exception("static ResultData" + " : " + "GetHistory" + " : ", ex);
        //        }
        //        else
        //        {
        //            throw ex; // Just bubble it up
        //        }
        //    }
        //}


        //public static List<History> GetByResultId(int pResultId)
        //{
        //    try
        //    {
        //        DbProviderFactory factory = SqlClientFactory.Instance;

        //        // Now get the connection object.
        //        using (DbConnection connection = factory.CreateConnection())
        //        {
        //            connection.ConnectionString = Settings.CPDConnectionString;
        //            connection.Open();
        //            DbCommand command = factory.CreateCommand();
        //            command.Connection = connection;
        //            command.CommandText = "[DataSet1.History.FillBy]";
        //            command.CommandType = CommandType.StoredProcedure;

        //            DbParameter lParameter1 = command.CreateParameter();
        //            lParameter1.ParameterName = "By";
        //            lParameter1.DbType = DbType.String;
        //            lParameter1.Value = "ResultId";
        //            command.Parameters.Add(lParameter1);

        //            DbParameter lParameter2 = command.CreateParameter();
        //            lParameter2.ParameterName = "Id";
        //            lParameter2.DbType = DbType.Int32;
        //            lParameter2.Value = pResultId;
        //            command.Parameters.Add(lParameter2);

        //            return HistoryReader(ref command);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.InnerException == null)
        //        {
        //            ExceptionData.WriteException(1, ex.Message, "static ResultData", "GetByResultId", "");
        //            throw new Exception("static ResultData" + " : " + "GetByResultId" + " : ", ex);
        //        }
        //        else
        //        {
        //            throw ex; // Just bubble it up
        //        }
        //    }
        //  }

        //public static int Initialise(int CustomerId, int ModuleId)
        //{    
        //    DataSet1TableAdapters.ResultTableAdapter lResultAdapter = new DataSet1TableAdapters.ResultTableAdapter();
        //    DataSet1TableAdapters.ResultDetailTableAdapter lResultDetailAdapter = new DataSet1TableAdapters.ResultDetailTableAdapter();
        //    //QuestionareDocTableAdapters.ArticleTableAdapter lArticleAdapter = new QuestionareDocTableAdapters.ArticleTableAdapter();
        //    DataSet1TableAdapters.QuestionTableAdapter lQuestionAdapter = new DataSet1TableAdapters.QuestionTableAdapter();
        //    lResultAdapter.AttachConnection();
        //    lResultDetailAdapter.AttachConnection();
        //    //lArticleAdapter.AttachConnection();
        //    lQuestionAdapter.AttachConnection();
        //    try
        //    {
        //        // Local objects

        //        DataSet1 lDataSet1 = new DataSet1();

        //        short CurrentAttempt = (short)GetAttempt(CustomerId, ModuleId);

        //        // Exit if you are too far

        //        if (CurrentAttempt == 2)
        //        {
        //            throw new Exception("You cannot initialise more than one attempt.");
        //        }

        //        // Create a single Result row

        //        DataSet1.ResultRow lResultRow = lDataSet1.Result.NewResultRow();
        //        lResultRow.CustomerId = CustomerId;
        //        lResultRow.ModuleId = ModuleId;
        //        lResultRow.Datum = DateTime.Now;
        //        lResultRow.Attempt = CurrentAttempt;
        //        lResultRow.Attempt++;
        //        lResultRow.Pass = false;
        //        lDataSet1.Result.AddResultRow(lResultRow);
        //        lResultAdapter.Update(lResultRow);
        //        lDataSet1.Result.AcceptChanges();

        //        // Create the child ResultDetail rows
        //        //lArticleAdapter.FillBy(lQuestionareDoc.Article, ModuleId);
        //        lQuestionAdapter.FillBy(lDataSet1.Question, ModuleId);

        //        foreach (DataSet1.QuestionRow lQuestionRow in lDataSet1.Question)
        //        {
        //            DataSet1.ResultDetailRow lResultDetailRow = lDataSet1.ResultDetail.NewResultDetailRow();
        //            lResultDetailRow.ResultId = lResultRow.ResultId;
        //            lResultDetailRow.QuestionId = lQuestionRow.QuestionId;
        //            lResultDetailRow.Datum = DateTime.Now;
        //            lResultDetailRow.Answer = 0;
        //            lDataSet1.ResultDetail.AddResultDetailRow(lResultDetailRow);
        //        }

        //        lResultDetailAdapter.Update(lDataSet1.ResultDetail);
        //        lDataSet1.ResultDetail.AcceptChanges();

        //        return lResultRow.ResultId;
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.InnerException == null)
        //        {
        //            ExceptionData.WriteException(typeof(WarningException) == ex.GetType() ? 3 : 1, ex.Message, "static ResultData", "Initialise", "");
        //            throw new Exception("static ResultData" + " : " + "Initialise" + " : ", ex);
        //        }
        //        else
        //        {
        //            throw ex; // Just bubble it up
        //        }
        //    }
        //}


        //private static int GetCorrectAnswersByQuestion(int QuestionId)
        //{
        //    CPD2.Data.DataSet1TableAdapters.AnswerTableAdapter lAnswerAdapter = new DataSet1TableAdapters.AnswerTableAdapter();
        //    lAnswerAdapter.AttachConnection();
        //    try
        //    {
        //        DataSet1.AnswerDataTable lAnswer = new DataSet1.AnswerDataTable();

        //        lAnswerAdapter.FillBy(lAnswer, "QuestionId", QuestionId);


        //        if (lAnswer.Count == 0)
        //        {
        //            throw new Exception("There are no answers for question with id = " + QuestionId.ToString());
        //        }

        //        int Answer = 0;
        //        int i = 0;

        //        foreach (DataSet1.AnswerRow lAnswerRow in lAnswer)
        //        {
        //            if (lAnswerRow.Correct)
        //            {
        //                Answer = Answer + (int)Math.Pow(2, i);
        //            }
        //            i++;
        //        }

        //        return Answer;
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.InnerException == null)
        //        {
        //            ExceptionData.WriteException(typeof(WarningException) == ex.GetType() ? 3 : 1, ex.Message, "static ResultData", "GetCorrectAnswersByQuestion", "");
        //            throw new Exception("static ResultData" + " : " + "GetCorrectAnswersByQuestion" + " : ", ex);
        //        }
        //        else
        //        {
        //            throw ex; // Just bubble it up
        //        }
        //    }
        //}


        //public static void SetCorrectAnswerBySurvey(int SurveyId)
        //{
        //    DataSet1TableAdapters.QuestionTableAdapter lQuestionAdapter = new DataSet1TableAdapters.QuestionTableAdapter();
        //    lQuestionAdapter.AttachConnection();
        //    try
        //    {
        //        //Get all the relevant questions


        //        DataSet1.QuestionDataTable lQuestion = lQuestionAdapter.GetBy("SurveyId", SurveyId);

        //        if (lQuestion.Count == 0)
        //        {
        //            throw new Exception("There are no questions for survey with id = " + SurveyId.ToString());
        //        }

        //        foreach (DataSet1.QuestionRow lQuestionRow in lQuestion)
        //        {
        //            lQuestionRow.CorrectAnswer = GetCorrectAnswersByQuestion(lQuestionRow.QuestionId);
        //        }

        //        lQuestionAdapter.Update(lQuestion);
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.InnerException == null)
        //        {
        //            ExceptionData.WriteException(typeof(WarningException) == ex.GetType() ? 3 : 1, ex.Message, "static ResultData", "SetCorrectAnswerBySurvey", "");
        //            throw new Exception("static ResultData" + " : " + "SetCorrectAnswerBySurvey" + " : ", ex);
        //        }
        //        else
        //        {
        //            throw ex; // Just bubble it up
        //        }
        //    }
        //}


        //public static DataSet1.AnswerDataTable GetAnswer(int CustomerId, int QuestionId)
        //{
        //    DataSet1TableAdapters.AnswerTableAdapter lAnswerAdapter = new DataSet1TableAdapters.AnswerTableAdapter();
        //    lAnswerAdapter.AttachConnection();
        //    try
        //    {
        //        // Read in the potential answers options

        //        DataSet1.AnswerDataTable lAnswer = new DataSet1.AnswerDataTable();
        //        DataSet1.HistoryDataTable lHistory = new DataSet1.HistoryDataTable();

        //        lAnswerAdapter.FillBy(lAnswer, "QuestionId", QuestionId);

        //        // Apply the actual answers and pass that on

        //        DataSet1TableAdapters.ResultDetailTableAdapter lDetailAdapter = new DataSet1TableAdapters.ResultDetailTableAdapter();

        //        if (!GetCurrentTest(CustomerId, ref lHistory))
        //        {
        //            return lAnswer;
        //        }

        //        int Number = 0;
        //        try
        //        {
        //            Number = (int)lDetailAdapter.GetAnswer("QuestionId", lHistory[0].ResultId, QuestionId);
        //        }
        //        catch
        //        {

        //        }

        //        for (int i = lAnswer.Count - 1; i >= 0; i--)
        //        {
        //            if (Number >= (int)Math.Pow(2, i))
        //            {
        //                lAnswer[i].Correct = true;
        //                Number = Number - (int)Math.Pow(2, i);
        //            }
        //            else
        //            {
        //                lAnswer[i].Correct = false;
        //            }
        //        }

        //        return lAnswer;
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

        //        DataSet1.HistoryDataTable lHistory = new DataSet1.HistoryDataTable();
        //        if (GetCurrentTest(CustomerId, ref lHistory))
        //        {
        //            DataSet1TableAdapters.ResultDetailTableAdapter lDetailAdapter = new DataSet1TableAdapters.ResultDetailTableAdapter();
        //            lDetailAdapter.AttachConnection();
        //            lDetailAdapter.SetAnswer(lHistory[0].ResultId, QuestionId, Answer);
        //            lDetailAdapter.Connection.Close();
        //        }
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

        //public static int Mark(int ResultId)
        //{
        //    DataSet1TableAdapters.ResultTableAdapter lResultAdapter = new DataSet1TableAdapters.ResultTableAdapter();
        //    lResultAdapter.AttachConnection();

        //    int Test = (int)lResultAdapter.Mark(ResultId);
        //    //lResultAdapter.Connection.Close();
        //    return Test;
        //}
    }
}
