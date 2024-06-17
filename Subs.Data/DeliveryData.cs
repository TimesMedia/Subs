using System;
using System.Data;
using System.Data.SqlClient;


namespace Subs.Data
{
    public class DeliveryData
    {
        private  Subs.Data.DeliveryDocTableAdapters.DeliveryProposalTableAdapter gDeliveryProposalAdapter = new DeliveryDocTableAdapters.DeliveryProposalTableAdapter();
 
        public DeliveryDoc gDoc = new DeliveryDoc();
        
        public Subs.Data.DeliveryDoc.DeliveryProposalDataTable gDeliveryProposal;
        public Subs.Data.DeliveryDoc.CollectionListDataTable gCollectionList = new DeliveryDoc.CollectionListDataTable();

        public DeliveryData()
        {
            gDeliveryProposal = gDoc.DeliveryProposal;
            gCollectionList = gDoc.CollectionList;
            gDeliveryProposalAdapter.AttachConnection();
        }

        public void Propose(int pIssueId, string pType)
        {
            try
            {
                gDeliveryProposal.Clear();

                // Do the query with a DataReader



                SqlConnection lConnection = new SqlConnection();
                SqlCommand Command = new SqlCommand();
                SqlDataAdapter Adaptor = new SqlDataAdapter();
                lConnection.ConnectionString = Settings.ConnectionString;
                lConnection.Open();
                Command.Connection = lConnection;
                Command.CommandType = CommandType.StoredProcedure;

                Command.CommandText = "[dbo].[MIMS.DeliveryData.ProposeMedia]";

                SqlParameter lParameter1 = Command.CreateParameter();
                lParameter1.ParameterName = "@IssueId";
                lParameter1.DbType = System.Data.DbType.Int32;
                lParameter1.Value = pIssueId;
                Command.Parameters.Add(lParameter1);

                SqlDataReader lReader = Command.ExecuteReader();

                if (lReader.HasRows)
                {
                    while (lReader.Read())
                    {
                        DeliveryDoc.DeliveryProposalRow lRow = gDeliveryProposal.NewDeliveryProposalRow();
                        lRow.ProposalDate = lReader.GetDateTime(0);
                        lRow.SubscriptionId = lReader.GetInt32(1);
                        lRow.IssueId = lReader.GetInt32(2);
                        lRow.IssueDescription = lReader.GetString(3);
                        lRow.DeliveryAddressId = lReader.GetInt32(4);
                        lRow.DeliveryMethod = lReader.GetInt32(5);
                        lRow.DeliveryMethodString = lReader.GetString(6);
                        lRow.ValidationStatus = lReader.GetString(7);
                        lRow.ModifiedBy = lReader.GetString(8);
                        lRow.ModifiedOn = lReader.GetDateTime(9);
                        lRow.PayerId = lReader.GetInt32(10);
                        lRow.ReceiverId = lReader.GetInt32(11);
                        lRow.UnitsPerIssue = lReader.GetInt32(12);
                        lRow.UnitPrice = (decimal)lReader.GetDecimal(13);
                        lRow.DebitOrder = (bool)lReader.GetBoolean(14);
                        lRow.ExpirationDate = lReader.GetDateTime(15);

                        if (!lReader.IsDBNull(16))
                        {
                            lRow.InvoiceId = lReader.GetInt32(16);
                        }

                        lRow.Weight = (decimal)lReader.GetDecimal(17);
                        lRow.Length = lReader.GetInt32(18);
                        lRow.Width = lReader.GetInt32(19);
                        lRow.Height = lReader.GetInt32(20);
                        lRow.MediaDelivery = lReader.GetBoolean(21);

                        gDeliveryProposal.AddDeliveryProposalRow(lRow);
                    }
                    gDeliveryProposalAdapter.Update(gDeliveryProposal);
                    gDeliveryProposal.AcceptChanges();

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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ProposeMedia", pIssueId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }

        }

        //public void ProposeSkynet(int pIssueId)
        //{
        //    gDeliveryProposal.Clear();
        //    gDeliveryProposalAdapter.Propose(gDeliveryProposal, pIssueId, "Skynet");
        //}

        //public string ProposeActiveActive(ref DeliveryDoc pDeliveryDoc)
        //{
        //    foreach (int lIssueId in ProductDataStatic.CurrentIssues())
        //    {

        //        {
        //            string lResult;

        //            if ((lResult = Propose(lIssueId)) != "OK")
        //            {
        //                return lResult;
        //            }
        //        }
        //    }
        //    return "OK";
        //}

        public void LoadProposal(DateTime pStartDate, DateTime pEndDate)
        {
            try
            {
                gDeliveryProposalAdapter.AttachConnection();
                gDeliveryProposalAdapter.FillBy(gDeliveryProposal, pStartDate, pEndDate);

            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "DeliveryDataStatic", "LoadProposal", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }

        public void SaveProposal()
        {
            try
            {
                  gDeliveryProposalAdapter.Update(gDeliveryProposal);
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "DeliveryDataStatic", "SaveProposal", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }
    }
}
