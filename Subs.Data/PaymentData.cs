using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Subs.Data
{

    public class PaymentData
    {
        private readonly SqlConnection gConnection = new SqlConnection();


        public class PaymentRecord
        {
            public int CustomerId { get; set; }
            public DateTime Date { get; set; }
            public decimal Amount { get; set; }
            public int ReferenceTypeId { get; set; }
            public string ReferenceTypeString { get; set; }
            public string Reference { get; set; }

            public int InvoiceId { get; set; }
            public string ReferenceType3 { get; set; }
            public string Reference3 { get; set; }
            public int PaymentMethod { get; set; }
            public string Explanation { get; set; }
            public string Name { get; set; }

            public PaymentRecord()
            {
                CustomerId = 0;
                Date = DateTime.Now;
                Amount = 0M;
                ReferenceTypeId = 0;
                Reference = "a";
                ReferenceTypeString = "b";
                InvoiceId = 0;
                ReferenceType3 = "c";
                Reference3 = "d";
                PaymentMethod = 3;
                Explanation = "e";
                Name = "f";
            }

            public void Clear()
            {
                CustomerId = 0;
                Amount = 0;
                ReferenceTypeId = 0;
                Reference = "";
                InvoiceId = 0;
                ReferenceType3 = "";
                Reference3 = "";
                PaymentMethod = 3;
            }
        }

        public class DebitOrderByPayer
        {
            public string RecipientName { get; set; }
            public string RecipientAccount { get; set; }

            public string RecipientAccountType { get; set;}

            public string BranchCode { get; set; }
            public decimal Amount { get; set; }
            public string OwnReference { get; set; }
            public string RecipientReference { get; set; }
            public string EmailNotify { get; set; }
            public string EmailAddress { get; set; }
            public string EmailSubject { get; set; }
        }

        public enum PaymentState
        {
            AllocatedToPayer = 1,
            Postable = 2,
            TransferBetweenBanks = 3,
            InternalTransfer = 4,
            Undecided = 5,
            ApplicableToMultiplePayers = 6,
            Debitorders = 7,
            BankCharges = 8,
            //BouncedPayment = 9,
            IncorrectlyDeposited = 10,
            Reversable = 11,
            Overridden = 12,
            PaymentForAnotherBankAccount = 13
        }


        public PaymentData()
        {
            // Set the connectionString for this object
            ;
            if (Settings.ConnectionString == "")
            {
                // This makes it possible to use the Visual studio designer.
                gConnection.ConnectionString = global::Subs.Data.Properties.Settings.Default.MIMSConnectionString;
            }
            else
            {
                gConnection.ConnectionString = Settings.ConnectionString;
            }
        }

        private readonly MIMSDataContext gMIMSDataContext = new MIMSDataContext(Settings.ConnectionString);

        public string UpdateSBStatements(PaymentDoc.SBBankStatementDataTable pTable)
        {
            // This method will update all of the rows or none of the rows.

            PaymentDocTableAdapters.SBBankStatementTableAdapter lAdapter = new Subs.Data.PaymentDocTableAdapters.SBBankStatementTableAdapter();

            // Remove entries that are in the database already


            List<PaymentDoc.SBBankStatementRow> lDuplicates = new List<PaymentDoc.SBBankStatementRow>();

            foreach (PaymentDoc.SBBankStatementRow lRow in pTable)
            {
                // See if you have the bankstatementrow  already
                MIMS_PaymentData_DuplicateSBRowResult lHits = gMIMSDataContext.MIMS_PaymentData_DuplicateSBRow(lRow.TransactionDate, lRow.Amount, lRow.Reference, lRow.StatementNo, lRow.AllocationNo).Single();
                if (lHits.Column1 > 0)
                {
                    lDuplicates.Add(lRow);
                }
            }

            foreach (PaymentDoc.SBBankStatementRow lDuplicateRow in lDuplicates)
            {
                pTable.Rows.Remove(lDuplicateRow);
            }

            // Save the rest

            SqlTransaction lTransaction;
            gConnection.Open();
            lTransaction = gConnection.BeginTransaction("SBBankStatement");

            try
            {
                lAdapter.AttachTransaction(ref lTransaction);

                try
                {
                    lAdapter.Update(pTable);
                }
                catch (System.Data.DBConcurrencyException ex)
                {
                    string myMessage = ex.Message;
                    pTable.Clear();
                    throw new System.Exception("Sorry, one of the records has been modified by another program. You will have to reload it and then redo the update.");
                }

                lTransaction.Commit();
                pTable.AcceptChanges();
                return "OK";
            }
            catch (System.Exception ex)
            {
                lTransaction.Rollback("BankStatement");
                pTable.Clear();
                ExceptionData.WriteException(1, ex.Message, this.ToString(), "UpdateSBStatements", "");
                return ex.Message;
            }
            finally
            {
                gConnection.Close();
            }
        }


        public string UpdateFNBStatements(PaymentDoc.FNBBankStatementDataTable pTable)
        {
            // This method will update all of the rows or none of the rows.
            // But what about the entries in the ledger. At this stage the ledger is not involved in terms of registering a payment.
            //

            SqlTransaction lTransaction;
            gConnection.Open();
            lTransaction = gConnection.BeginTransaction("BankStatement");

            PaymentDocTableAdapters.FNBBankStatementTableAdapter lAdapter = new Subs.Data.PaymentDocTableAdapters.FNBBankStatementTableAdapter();

            try
            {
                lAdapter.AttachTransaction(ref lTransaction);

                try
                {
                    lAdapter.Update(pTable);
                }
                catch (System.Data.DBConcurrencyException ex)
                {
                    string myMessage = ex.Message;
                    pTable.Clear();
                    throw new System.Exception("Sorry, one of the records have been modified by another program. You will have to reload it and then redo the update.");
                }

                lTransaction.Commit();
                pTable.AcceptChanges();
                return "OK";
            }
            catch (System.Exception ex)
            {
                lTransaction.Rollback("BankStatement");
                pTable.Clear();
                ExceptionData.WriteException(1, ex.Message, this.ToString(), "UpdateFNBStatements", "");
                return ex.Message;
            }
            finally
            {
                gConnection.Close();
            }
        }

        public string UpdateDebitOrderStatements(PaymentDoc.DebitOrderBankStatementDataTable pTable)
        {
            // This method will update all of the rows or none of the rows.
            // But what about the entries in the ledger. At this stage the ledger is not involved in terms of registering a payment.
            //


            PaymentDocTableAdapters.DebitOrderBankStatementTableAdapter lAdapter = new Subs.Data.PaymentDocTableAdapters.DebitOrderBankStatementTableAdapter();


            SqlTransaction lTransaction;
            gConnection.Open();
            lTransaction = gConnection.BeginTransaction("BankStatement");

            try
            {
                lAdapter.AttachTransaction(ref lTransaction);

                try
                {
                    lAdapter.Update(pTable);
                }
                catch (System.Data.DBConcurrencyException ex)
                {
                    string myMessage = ex.Message;
                    pTable.Clear();
                    throw new System.Exception("Sorry, one of the records have been modified by another program. You will have to reload it and then redo the update.");
                }

                lTransaction.Commit();
                pTable.AcceptChanges();
                return "OK";
            }
            catch (System.Exception ex)
            {
                lTransaction.Rollback("BankStatement");
                pTable.Clear();
                ExceptionData.WriteException(1, ex.Message, this.ToString(), "UpdateDebitOrderStatements", "");
                return ex.Message;
            }
            finally
            {
                gConnection.Close();
            }
        }


        public static List<DebitOrderByPayer> DebitOrder(DateTime pDeliveryMonthFirstDay)
        {
            try
            {
                List<DebitOrderByPayer> lDebitOrders = new List<DebitOrderByPayer>();
            
                SqlConnection lConnection = new SqlConnection();
                SqlCommand Command = new SqlCommand();
                SqlDataAdapter Adaptor = new SqlDataAdapter();
                lConnection.ConnectionString = Settings.ConnectionString;
                lConnection.Open();
                Command.Connection = lConnection;
                Command.CommandType = CommandType.StoredProcedure;
                Command.CommandText = "[dbo].[MIMS.PaymentData.DebitOrder]";

                SqlParameter lParameter1 = Command.CreateParameter();
                lParameter1.ParameterName = "@DeliveryMonthFirstDay";
                lParameter1.DbType  = System.Data.DbType.Date;
                lParameter1.Value = pDeliveryMonthFirstDay;
                Command.Parameters.Add(lParameter1);
 
                SqlDataReader lReader = Command.ExecuteReader();

                if (lReader.HasRows)
                {
                    while (lReader.Read())
                    {
                        DebitOrderByPayer lDebitOrder = new DebitOrderByPayer();
                        lDebitOrder.RecipientName = (string)lReader[0];
                        lDebitOrder.RecipientAccount = (string)lReader[1];
                        lDebitOrder.RecipientAccountType = (string)lReader[2];
                        lDebitOrder.BranchCode = (string)lReader[3];
                        lDebitOrder.Amount = (decimal)lReader[4];
                        lDebitOrder.OwnReference = (string)lReader[5];
                        lDebitOrder.RecipientReference = (string)lReader[6];
                        lDebitOrder.EmailNotify = (string)lReader[7];
                        lDebitOrder.EmailAddress = (string)lReader[8];
                        lDebitOrder.EmailSubject= (string)lReader[9];
                        lDebitOrders.Add(lDebitOrder);
                    }
                }
                return lDebitOrders;

            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "PaymentData", "DebitOrder", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }




    }
}
