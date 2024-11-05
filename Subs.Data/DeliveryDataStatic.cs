using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Subs.Data
{
    public class Dormant
    {
        public int SubscriptionId { get; set; }

        public DateTime LastDeliveryDate { get; set; }
        public string ProductName { get; set; }

        public int PayerId { get; set; }

        public decimal Liability { get; set; }

        public int LastSequenceByProduct { get; set; }

        public int LastSequenceBySubscription { get; set; }

        public int LagByIssues { get; set; }

        public int DeliverableIssueId { get; set; }

        public string DeliverableIssueName { get; set; }
  
       
    }






    public static class DeliveryDataStatic
    {
        private static MIMSDataContext gMimsDataContext = new MIMSDataContext(Settings.ConnectionString);


        #region Generate deliverylists


        public static string LoadActive(ref DeliveryDoc pDeliveryDoc)
        {
            foreach (int lIssueId in ProductDataStatic.CurrentIssues())
            {

                {
                    string lResult;

                    if ((lResult = Load(lIssueId, ref pDeliveryDoc)) != "OK")
                    {
                        return lResult;
                    }
                }
            }
            return "OK";
        }

        public static string Load(int IssueId, ref DeliveryDoc pDoc)
        {
            SqlConnection lConnection = new SqlConnection();
            try
            {
                // Get new data

                SqlCommand lCommand = new SqlCommand();
                SqlDataAdapter lAdaptor = new SqlDataAdapter();
                lConnection.ConnectionString = Settings.ConnectionString;
                lConnection.Open();
                lCommand.Connection = lConnection;
                lCommand.CommandType = CommandType.StoredProcedure;
                lCommand.CommandText = "dbo.[MIMS.DeliveryDataStatic.Load]";
                SqlCommandBuilder.DeriveParameters(lCommand);
                lAdaptor.SelectCommand = lCommand;
                lCommand.Parameters["@IssueId"].Value = IssueId;
                lCommand.Parameters["@Status"].Value = SubStatus.Deliverable;

                lAdaptor.Fill(pDoc.DeliveryRecord);

                return "OK";
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "DeliveryDataStatic", "Load", "IssueId = " + IssueId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return ex.Message;
            }
            finally
            {
                lConnection.Close();
            }
        }

        public static bool LoadLabelBySubscription(int SubscriptionId, ref DeliveryDoc pDeliveryDoc)
        {
            SqlConnection lConnection = new SqlConnection(Settings.ConnectionString);
            SqlCommand Command = new SqlCommand();
            SqlDataAdapter Adaptor = new SqlDataAdapter();
            try
            {
                // Get new data

                lConnection.Open();
                Command.Connection = lConnection;
                Command.CommandType = CommandType.StoredProcedure;
                Command.CommandText = "dbo.[MIMS.DeliveryData.LoadLabelByCustomer]";
                SqlCommandBuilder.DeriveParameters(Command);
                Adaptor.SelectCommand = Command;
                Command.Parameters["@Type"].Value = "BySubscription";
                Command.Parameters["@Id"].Value = SubscriptionId;
                pDeliveryDoc.Label.Clear();
                Adaptor.Fill(pDeliveryDoc.Label);

                if (pDeliveryDoc.Label.Rows.Count == 0)
                {
                    ExceptionData.WriteException(5, "There was no data for subscription", "DeliveryData", "LoadLabelBySubscription", SubscriptionId.ToString());
                    return false;
                }
                return true;
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "DeliveryData", "LoadLabelBySubscription", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return false;
            }
            finally
            {
                lConnection.Close();
            }

        }

        public static string GetDeliverableElectronic(int pReceiverId, out List<MIMS_DeliveryDataStatic_DeliverableElectronicResult> pDeliverables)
        {
            try
            {
                pDeliverables = gMimsDataContext.MIMS_DeliveryDataStatic_DeliverableElectronic(pReceiverId).ToList();
                return "OK";
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "DeliveryDataStatic", " GetDeliverableElectronic", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                pDeliverables = null;
                return ex.Message;
            }
        }

        public static bool CreateDeliveryList(ref DeliveryDoc pDeliveryDoc)
        {
            int CurrentSubscriptionId = 0; // Used for diagnostic purposes
            SqlConnection lConnection = new SqlConnection(Settings.ConnectionString);
            SqlCommand Command = new SqlCommand();
            SqlDataAdapter Adaptor = new SqlDataAdapter();
            try
            {
                // Cleanup before you start a new one

                pDeliveryDoc.DeliveryList1.Clear();

                // Get new data

                lConnection.Open();
                Command.Connection = lConnection;
                Command.CommandType = CommandType.StoredProcedure;
                Command.CommandText = "MIMS.DeliveryDataStatic.CreateDeliveryList";
                SqlCommandBuilder.DeriveParameters(Command);
                Adaptor.SelectCommand = Command;
                pDeliveryDoc.DeliveryList1.Clear();

                foreach (DataRowView lRowView in pDeliveryDoc.DeliveryRecord.DefaultView)
                {
                    DeliveryDoc.DeliveryRecordRow lRow = (DeliveryDoc.DeliveryRecordRow)lRowView.Row;
                    CurrentSubscriptionId = lRow.SubscriptionId;
                    Command.Parameters["@SubscriptionId"].Value = lRow.SubscriptionId;
                    Adaptor.Fill(pDeliveryDoc.DeliveryList1);
                }
                return true;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "static DeliveryData", "LoadDeliveryList", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return false;
            }

            finally
            {
                lConnection.Close();
            }

        }

        #endregion

        //public static string ReverseDelivery(int IssueToReverseId, DateTime DeliveryDate)
        //{
        //    SqlConnection lConnection = new SqlConnection();
        //    try
        //    {
        //        SqlCommand Command = new SqlCommand();
        //        lConnection.ConnectionString = Settings.ConnectionString;
        //        lConnection.Open();
        //        Command.Connection = lConnection;
        //        Command.CommandType = CommandType.StoredProcedure;
        //        Command.CommandText = "dbo.[MIMS.DeliveryDataStatic.ReverseDelivery]";
        //        SqlCommandBuilder.DeriveParameters(Command);
        //        //Command.Parameters["@DeliveriesReversed"].Direction = ParameterDirection.Output;
        //        //Command.Parameters["@DeliveriesReversed"].IsNullable = false;

        //        Command.Parameters["@IssueToReverseId"].Value = IssueToReverseId;
        //        Command.Parameters["@DeliveryDate"].Value = DeliveryDate;

        //        Command.ExecuteNonQuery();

        //        //int DeliveriesReversed = (int)Command.Parameters["@DeliveriesReversed"].Value;

        //        ExceptionData.WriteException(5, "I have reversed all deliveries for issue " + IssueToReverseId.ToString() + " executed on " + DeliveryDate.ToString("yyyymmddHHss"), "DeliveryDataStatic", "ReverseDelivery", "");
        //        return "OK";

        //    }
        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "DeliveryDataStatic", "ReverseDelivery", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        return ex.Message;
        //    }

        //    finally
        //    {
        //        lConnection.Close();
        //    }
        //}

        public static List<Dormant> GetDormants()
        {
            int lCurrentSubscriptionId = 0;
            SqlConnection lConnection = new SqlConnection();
            try
            {
                List<Dormant> lDormants = new List<Dormant>();
                
                SqlCommand Command = new SqlCommand();
                SqlDataAdapter Adaptor = new SqlDataAdapter();
                lConnection.ConnectionString = Settings.ConnectionString;
                lConnection.Open();
                Command.Connection = lConnection;
                Command.CommandType = CommandType.StoredProcedure;
                Command.CommandText = "[dbo].[MIMS.DeliveryDataStatic.Dormants]";

                SqlDataReader lReader = Command.ExecuteReader();

                while (lReader.Read())
                {
                    lCurrentSubscriptionId = lReader.GetInt32(3);

                    Dormant lDormant = new Dormant();
                    lDormant.LastDeliveryDate = lReader.GetDateTime(0);
                    lDormant.DeliverableIssueName = lReader.GetString(1);
                    lDormant.DeliverableIssueId = lReader.GetInt32(2);
                    lDormant.SubscriptionId = lReader.GetInt32(3);
                    lDormant.ProductName = lReader.GetString(4);
                    lDormant.PayerId = (int)lReader.GetInt32(5);
                    lDormant.Liability = (decimal)lReader.GetDecimal(6);
                    lDormant.LastSequenceByProduct = lReader.GetInt32(7);
                    lDormant.LastSequenceBySubscription = lReader.GetInt32(8);
                    lDormant.LagByIssues = lReader.GetInt32(9);

                    lDormants.Add(lDormant);
                }
                return lDormants;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "DeliveryDataStatic", "GetDormants", "CurrentSubscriptionId = " + lCurrentSubscriptionId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
            finally
            {
                if (lConnection.State != ConnectionState.Closed)
                {
                    lConnection.Close();
                }
            }
        }

        public static bool StandarizePostCodes()
        {
            PostCodeDocTableAdapters.SAPOCodeTableAdapter lAdapter = new PostCodeDocTableAdapters.SAPOCodeTableAdapter();
            PostCodeDoc.SAPOCodeDataTable lTable = new PostCodeDoc.SAPOCodeDataTable();

            try
            {
                lAdapter.AttachConnection();
                lAdapter.Fill(lTable);

                foreach (PostCodeDoc.SAPOCodeRow lRow in lTable)
                {
                    if (!lRow.IsBoxCodeNull())
                    {
                        lRow.BoxCode = lRow.BoxCode.PadLeft(4, '0');
                    }

                    if (!lRow.IsStreetCodeNull())
                    {
                        lRow.StreetCode = lRow.StreetCode.PadLeft(4, '0');
                    }
                }

                lAdapter.Update(lTable);


                return true;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "DaliveryDataStatic", "StandardizePostCodes", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return false;
            }
        }
    }
}
