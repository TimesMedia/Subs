using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Subs.Data
{
    public class Discrepancy
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public decimal Due { get; set; }
        public decimal DeliverableMinusDue { get; set; }
        public DateTime LastActivity { get; set; }
    }

    public class DeliverableDetail
    {
        public string IssueDescription { get; set; }
        public DateTime StartDate { get; set; }
        public int Units { get; set; }
        public decimal Amount  { get; set; }
        public decimal Accumulated { get; set; }
    }






    public class InvoiceAndPayment
    {
        public int TransactionId {get; set;}
        public int InvoiceId {get; set;}
        public System.DateTime Date {get; set;}
        public System.DateTime CaptureDate { get; set; }
        public string Operation { get; set; }
        public int OperationId { get; set; }
        public decimal Amount { get; set; }
        public decimal DueValue { get; set; }

        // Up to here is produced by Due2

        public int Reference1 { get; set; }
        public string Reference2 { get; set; }

        public decimal InvoiceBalance { get; set; }
        public decimal StatementBalance { get; set; }
        public bool FirstRow { get; set; }
        public bool LastRow { get; set; }
        public decimal Value { get; set; }
        public decimal Balance { get; set; }
        public System.Nullable<int> OriginalTransactionId { get; set; }
    }



    public class CustomerData3 : BaseModel
    {
        #region Globals - private

        private readonly Subs.Data.CustomerDoc2.CustomerDataTable gTable = new CustomerDoc2.CustomerDataTable();
        private readonly SqlConnection gConnection = new SqlConnection();
        private readonly Subs.Data.CustomerDoc2TableAdapters.CustomerTableAdapter gCustomerAdapter = new CustomerDoc2TableAdapters.CustomerTableAdapter();
        private readonly Subs.Data.SBDebitOrderDocTableAdapters.SBDebitOrderTableAdapter gSBDebitOrderAdapter = new SBDebitOrderDocTableAdapters.SBDebitOrderTableAdapter();
        private readonly Subs.Data.ClassificationDoc2.CustomerClassificationDataTable gCustomerClassificationAdapter = new ClassificationDoc2.CustomerClassificationDataTable();
        int _ClassificationId1 = 9; // Unknown
        int?  _ClassificationId2 = null;

        private string gPassword = "Koos";
        private byte[] gSalt = { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x5, 0x4, 0x3, 0x2, 0x1, 0x0 };

        private List<InvoiceAndPayment> gInvoiceAllocations = new List<InvoiceAndPayment>();
        private List<InvoiceAndPayment> gAllInvoiceAndPayment = new List<InvoiceAndPayment>();
  
        #endregion

        #region Constructors

        public CustomerData3()
        {
            try
            { 
            //if (!SetConnection()) { return; }
            gTable.Clear(); //Start with a clean slate
            gTable.AcceptChanges(); // Do not attempt to reconcile with the database

            Subs.Data.CustomerDoc2.CustomerRow myRow = gTable.NewCustomerRow();
            //Initialise the compulsory fields
            
            myRow.Initials = "X";
            myRow.Surname = "XX";
            myRow.TitleId = 1;
            myRow.CompanyId = 1;
            myRow.CountryId = 61;
            myRow.AddressType = 1;
            myRow.PostAddressId = 1;
            myRow.CellPhoneNumber = "000000000";
            myRow.Reserved = 0;
            myRow.Correspondence2 = 1;
            myRow.CheckpointDateInvoice = DateTime.Now;  //DateTime.Parse("2017/06/01");
            myRow.BalanceInvoiceId = 0;
            myRow.AutomaticPaymentAllocation = true;
            myRow.Marketing = true;
            myRow.SetPhysicalAddressIdNull();
            
            // myRow.LoginEmail = DateTime.Now.ToString();  // Provide an initial value that is unique.
            myRow.ModifiedBy = Environment.UserDomainName;
            myRow.ModifiedOn = DateTime.Now;
            gTable.AddCustomerRow(myRow);
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "CustomerData3 Default Constructor", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return;
            }
        }

   
        public CustomerData3(int CustomerId)
        {
            try
            {
                if (!SetConnection())
                {
                    throw new Exception("Error in CustomerData3 constructor: SetConnection ");
                }
                if (!Load(CustomerId, out string Message))
                {
                    if (!Message.Contains("There is no such customer"))
                    {
                        throw new Exception("Error in CustomerData3 constructor: Load - " + Message);
                    }
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "CustomerData3Constructor", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return;
            }
        }

        #endregion

        #region Housekeeping methods

        private bool SetConnection()
        {
            try
            {
                // Set the connectionString for this object
                if (Settings.ConnectionString != "")
                {
                    gConnection.ConnectionString = Settings.ConnectionString;
                    gCustomerAdapter.AttachConnection();
                    gConnection.Open();
                    return true;
                }
                else return false;
            }
            catch (Exception Ex)
            {
                string lDummy = Ex.Message;
                return false;
            }
        }


        private bool Load(int CustomerId, out string Message)
        {

            //SqlConnection Connection = new SqlConnection(Settings.ConnectionString);
            try
            {
                // Cleanup before you start a new one
                gTable.Clear();
                Message = "";
                // Get new data

                SqlCommand Command = new SqlCommand();
                SqlDataAdapter Adaptor = new SqlDataAdapter();
              
               // Connection.Open();
                Command.Connection = gConnection;
                Command.CommandType = CommandType.StoredProcedure;
                Command.CommandText = "dbo.[MIMS.CustomerDoc.Customer.FillById]";
                SqlCommandBuilder.DeriveParameters(Command);
                Adaptor.SelectCommand = Command;
                Command.Parameters["@IntegerId"].Value = CustomerId;
                Command.Parameters["@Type"].Value = "CustomerId";
                Adaptor.Fill(gTable);

                if (gTable.Rows.Count == 0)
                {
                    Message = " There is no such customer: " + CustomerId.ToString();
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Load - CustomerId", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                Message = ex.Message + "on " + CustomerId.ToString();
                return false;
            }
            finally
            {
                gConnection.Close();
            }
        }


        private string Load(string pLoginEmail)
        {
            try
            {
                // Cleanup before you start a new one
                gTable.Clear();

                // Get new data

                gCustomerAdapter.FillById(gTable, "LoginEmail", 0, pLoginEmail);



                if (gTable.Rows.Count == 0)
                {
                    // We do not raise an event, because this is called by the constructor, and at this time
                    // no event handler can conceivably be listening to the event.
                    return "There is no customer with LoginEmail of " + pLoginEmail.ToString();
                }



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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Load - LoginEmail", pLoginEmail);
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return ex.Message + " loading " + pLoginEmail.ToString();
            }
            finally
            {
                //gConnection.Close();
            }
        }


        private string ValidateOverAll()
        {
            try
            {
                if (TitleId == 1)
                {
                    return "A Title is compulsory";
                }


                if (string.IsNullOrWhiteSpace(CellPhoneNumber))
                {
                    return "Cellphone number is a compulsory field";
                }

                // EmailAddress

                if (string.IsNullOrWhiteSpace(EmailAddress))
                {
                    return "EMail address is compulsory";
                }

                // Correspondence

                BitVector32 lVector = new BitVector32(this.Correspondence2);

                // Check for a particular bit

                if (lVector[(int)Correspondence.EMail])
                {
                    if (this.EmailAddress.Length < 5)
                    {
                        return "You cannot specify Email for correspondence if you do not supply me with an EMail Address. Please try again.";
                    }
                }

                if (lVector[(int)Correspondence.SMS] || lVector[(int)Correspondence.WhatsUp] || lVector[(int)Correspondence.Phone])
                {
                    if (this.PhoneNumber.Length < 5)
                    {
                        return "You cannot specify SMS or WhatsUp or Phone for correspondence if you do not supply me with a phone number. Please try again.";
                    }
                }

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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ValidateOverAll", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return "Failed due to technical error";
            }
        }

        public (int InvoiceId, decimal Due) DueOnDate(DateTime lDate)
        {
            List<InvoiceAndPayment> lInvoiceAndPayments = GetInvoiceAndPayment();

            for (int i = 1; i < lInvoiceAndPayments.Count(); i++)       // Calculate the rest
            {
                if (lInvoiceAndPayments[i].Date >= lDate)
                {
                    return (lInvoiceAndPayments[i].InvoiceId, lInvoiceAndPayments[i].DueValue);
                }
            }
            return (0, 0.0M);
        }


        public List<InvoiceAndPayment> GetInvoiceAndPayment()
        {
            int lCurrentTransactionId = 0;
            SqlConnection lConnection = new SqlConnection();
            try
            {
                List<InvoiceAndPayment> lInvoiceAndPayments = new List<InvoiceAndPayment>();
                gAllInvoiceAndPayment.Clear();

                SqlCommand Command = new SqlCommand();
                SqlDataAdapter Adaptor = new SqlDataAdapter();
                lConnection.ConnectionString = Settings.ConnectionString;
                lConnection.Open();
                Command.Connection = lConnection;
                Command.CommandType = CommandType.StoredProcedure;
                Command.CommandText = "[dbo].[MIMS.CustomerDoc.Due3]";

                Command.Parameters.Add("@PayerId", SqlDbType.Int);
                Command.Parameters["@PayerId"].Value = CustomerId;

                //Command.Parameters.Add("@InvoiceId", SqlDbType.Int);
                //Command.Parameters["@InvoiceId"].Value = pInvoiceId;

                //Command.Parameters.Add("@Balance", SqlDbType.Decimal);
                //Command.Parameters["@Balance"].Value = pBalance;

                SqlDataReader lReader = Command.ExecuteReader();

                while (lReader.Read())
                {
                    lCurrentTransactionId = lReader.GetInt32(0);

                    InvoiceAndPayment lInvoiceAndPayment = new InvoiceAndPayment();
                    lInvoiceAndPayment.TransactionId = lReader.GetInt32(0);
                    lInvoiceAndPayment.InvoiceId = lReader.GetInt32(1);
                    lInvoiceAndPayment.Date = lReader.GetDateTime(2);
                    lInvoiceAndPayment.CaptureDate = lReader.GetDateTime(3);
                    lInvoiceAndPayment.OperationId = lReader.GetInt32(4);
                    lInvoiceAndPayment.Operation = lReader.GetString(5);
                    lInvoiceAndPayment.Value = lReader.GetDecimal(6);
                    lInvoiceAndPayment.DueValue = lReader.GetDecimal(7);
                    lInvoiceAndPayment.Reference2 = lReader.GetString(8);
                    lInvoiceAndPayments.Add(lInvoiceAndPayment);
                }

                if (lInvoiceAndPayments.Count == 0)
                {
                    return lInvoiceAndPayments;
                }


                //Calculate the balance values

                lInvoiceAndPayments[0].DueValue = lInvoiceAndPayments[0].Value;

                for (int i = 1; i < lInvoiceAndPayments.Count(); i++)       // Calculate the rest
                {
                    lInvoiceAndPayments[i].DueValue = lInvoiceAndPayments[i - 1].DueValue + lInvoiceAndPayments[i].Value;
                }
    
                return lInvoiceAndPayments;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomersData", "GetInvoiceAndPayment", "CurrentTransactionId = " + lCurrentTransactionId.ToString());
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

        public string PopulateInvoice2()
        {
            List<InvoiceAndPayment> lPayment = null;
            List<InvoiceAndPayment> lInvoice = null;
            try

            {
                gAllInvoiceAndPayment = GetInvoiceAndPayment();

                if (gAllInvoiceAndPayment.Count == 0)
                {
                    return "Nothing found. There were no new invoices since the last checkpoints.";
                }

                decimal lInvoiceBalance = 0;
                decimal lStatementBalance = 0;
                int lCurrentInvoiceId = 0;

                // Change some transactionids to the referenced transactionid in order to be able to group payments together.

                for (int i = 0; i < gAllInvoiceAndPayment.Count; i++)
                {
                    if (gAllInvoiceAndPayment[i].OperationId == (int)Operation.ReversePayment || gAllInvoiceAndPayment[i].OperationId == (int)Operation.Refund)
                    {
                        gAllInvoiceAndPayment[i].OriginalTransactionId = gAllInvoiceAndPayment[i].TransactionId;
                        gAllInvoiceAndPayment[i].TransactionId = Int32.Parse(gAllInvoiceAndPayment[i].Reference2);
                    }
                }

                // Create a list of all the invoiceids in this report.

                IEnumerable<int> lInvoicesInReport = gAllInvoiceAndPayment.ToList().Where(q => q.OperationId == 19).Select(p => p.InvoiceId).Distinct();

                if (lInvoicesInReport.Count() == 0)
                {
                    return "Nothing found. There were no new invoices since the last checkpoints.";
                }


                if (lInvoicesInReport.Contains(0))
                {
                    return "Nothing: I cannot do anything with a subscription that is not invoiced! ";
                }



                // Set Balance to LastRow **********************************************************************************************

                //IEnumerable<InvoiceAndPayment> lElement = gAllInvoiceAndPayment.Where(q => q.OperationId == (int)Operation.Balance).ToList();
                //if (lElement.Count() == 1)
                //{
                //    lElement.First().LastRow = true;
                //}

                // Process the payments by grouping the relevant stuff together *********************************************************

                lPayment = gAllInvoiceAndPayment.Where(p => p.OperationId == (int)Operation.Pay
                                                       || p.OperationId == (int)Operation.Refund
                                                       || p.OperationId == (int)Operation.ReversePayment).OrderBy(q => q.TransactionId).ThenBy(r => r.Date).ToList();

                InvoiceAndPayment lPreviousPaymentRow = null;
                InvoiceAndPayment lPreviousInvoiceRow = null;
                int lCurrentTransactionId = 0;
                decimal lPaymentBalance = 0M;

                foreach (InvoiceAndPayment lRow in lPayment)
                {
                    lRow.LastRow = false;

                    if (lCurrentTransactionId == lRow.TransactionId)
                    {
                        lRow.FirstRow = false;
                        lPaymentBalance = lPaymentBalance + (decimal)lRow.Value;
                    }
                    else
                    {
                        // Complete the previous group on the same transactionid.

                        if (lPreviousPaymentRow != null)
                        {
                            lPreviousPaymentRow.LastRow = true;
                            lPreviousPaymentRow.Balance = lPaymentBalance;
                        }

                        // Work with the new invoice
                        lRow.FirstRow = true;
                        lCurrentTransactionId = lRow.TransactionId;
                        lPaymentBalance = lRow.Value;
                    }
                    lPreviousPaymentRow = lRow;
                }

                // Do the last row
                if (lPreviousPaymentRow != null)
                {
                    lPreviousPaymentRow.LastRow = true;
                    lPreviousPaymentRow.Balance = lPaymentBalance;
                }


                // Calculate the invoices ******************************************************************************************

                lInvoice = gAllInvoiceAndPayment.Where(p => !(p.OperationId == (int)Operation.Pay
                                                         || p.OperationId == (int)Operation.Refund
                                                         || p.OperationId == (int)Operation.ReversePayment)).OrderBy(q => q.InvoiceId).ThenBy(r => r.Date).ToList();

                foreach (InvoiceAndPayment lRow in lInvoice)
                {
                    lRow.LastRow = false;

                    if (lRow.InvoiceId == lCurrentInvoiceId)
                    {
                        //This is a subsequent row
                        lRow.FirstRow = false;
                        lInvoiceBalance = lInvoiceBalance + (decimal)lRow.Value;
                    }
                    else
                    {
                        // Complete the old invoice

                        if (lPreviousInvoiceRow != null)
                        {
                            lPreviousInvoiceRow.LastRow = true;
                            lPreviousInvoiceRow.Balance = lInvoiceBalance;
                            lInvoiceBalance = 0;
                        }

                        // Work with the new invoice
                        lRow.FirstRow = true;
                        lCurrentInvoiceId = lRow.InvoiceId;
                        lInvoiceBalance = lRow.Value;
                    }

                    lPreviousInvoiceRow = lRow;
                }

                // Do the last row.
                if (lPreviousInvoiceRow != null)
                {
                    lPreviousInvoiceRow.LastRow = true;
                    lPreviousInvoiceRow.Balance = lInvoiceBalance;
                }

                // Combine the two collections into one collection


                gAllInvoiceAndPayment.Clear();
                gAllInvoiceAndPayment.AddRange(lPayment);
                gAllInvoiceAndPayment.AddRange(lInvoice);
                //gAllInvoiceAndPayment.AddRange(gInvoiceAllocations);

                lStatementBalance = 0;

                foreach (InvoiceAndPayment lRow2 in gAllInvoiceAndPayment)
                {
                    lStatementBalance = lStatementBalance + lRow2.Balance;

                    if (lRow2.LastRow)
                    {
                        lRow2.StatementBalance = lStatementBalance;
                    }
                }

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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message,this.ToString(), "PopulateInvoice2",
                        "PayerId = " + CustomerId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return "Error in Populate Invoices: " + ex.Message + " PayerId = " + CustomerId.ToString();
            }
        }

        
        public static DateTime GetCheckpointDate(int pInvoiceId)
        {
            try
            {
                SqlConnection lConnection = new SqlConnection();
                SqlCommand Command = new SqlCommand();
                SqlDataAdapter Adaptor = new SqlDataAdapter();
                lConnection.ConnectionString = Settings.ConnectionString;
                lConnection.Open();
                Command.Connection = lConnection;
                Command.CommandType = CommandType.StoredProcedure;
                Command.CommandText = "[dbo].[MIMS.CustomerData.InvoiceToCheckpointDate]";
              
                Command.Parameters.Add("@InvoiceId", SqlDbType.Int);
                Command.Parameters["@InvoiceId"].Value = pInvoiceId;
               
                SqlDataReader lReader = Command.ExecuteReader();

                lReader.Read();

                return lReader.GetDateTime(0);
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomersData", "GetCheckpointDate", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }


        public DateTime GetFirstInvoiceDate(int pInvoiceId)
        {
            
            try
            {
                SqlConnection lConnection = new SqlConnection();
                SqlCommand Command = new SqlCommand();
                SqlDataAdapter Adaptor = new SqlDataAdapter();
                lConnection.ConnectionString = Settings.ConnectionString;
                lConnection.Open();
                Command.Connection = lConnection;
                Command.CommandType = CommandType.StoredProcedure;
                Command.CommandText = "[dbo].[MIMS.CustomerData.GetFirstInvoiceDate]";

                Command.Parameters.Add("@InvoiceId", SqlDbType.Int);
                Command.Parameters["@InvoiceId"].Value = pInvoiceId;

                SqlDataReader lReader = Command.ExecuteReader();

                lReader.Read();

                return lReader.GetDateTime(0);
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomersData", "GetFirstInvoiceDate", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }

        public decimal CalculateBalanceByInvoice(int pInvoiceId)
        {
            int lIndex = 0;
  
            try
            {
                // Get a working base

                List<InvoiceAndPayment> lInvoiceAndPayment = GetInvoiceAndPayment();

                if( lInvoiceAndPayment.Count == 0)
                {
                    throw new Exception("No balance records found");
                }

                // Load the relevant Balance records 

                if (pInvoiceId > 0 )
                {
                    lIndex = lInvoiceAndPayment.FindIndex(p => p.InvoiceId == pInvoiceId);
                    return lInvoiceAndPayment[lIndex - 1].DueValue;
                }
                else
                {   
                    return 0.000000M;  // The original checkpoint dates suggests a starting balance of 0
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomersData", "CalculateBalance", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }

        public string Update()
        {
            try
            {
                if (gTable.Count == 0)
                {
                    return "There is nothing to update";
                }


                // Overall validation
                {
                    string lResult;

                    if ((lResult = ValidateOverAll()) != "OK")
                    {
                        return lResult;
                    }
                }



                if (gTable[0].RowState != DataRowState.Added && gTable[0].RowState != DataRowState.Modified)
                {
                    return "OK";
                }

                // Fill out the last two fields to trace modifications 

                if (gTable[0].RowState == DataRowState.Added | gTable[0].RowState == DataRowState.Modified)
                {
                    gTable[0].ModifiedBy = Environment.UserDomainName.ToString() + "\\" + Environment.UserName.ToString();
                    gTable[0].ModifiedOn = DateTime.Now;
                }

                // Create the appropriate adaptor and commands

                CustomerDoc2TableAdapters.CustomerTableAdapter myAdaptor = new Subs.Data.CustomerDoc2TableAdapters.CustomerTableAdapter();
                myAdaptor.AttachConnection();

                try
                {
                    myAdaptor.Update(gTable);
                    gTable.AcceptChanges();

                }
                catch (System.Data.DBConcurrencyException ex)
                {
                    string lMessage = ex.Message;
                    return "Sorry, this record has been modified by another program. You will have to reload it and then redo the update.";
                }

                gTable.AcceptChanges();
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Update", "CustomerId = " + gTable[0].CustomerId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return "UpdateCustomer failed: " + ex.Message;

            }
        }


        public string Destroy()
        {
            try
            {

                // Create the appropriate adaptor and commands

                CustomerDoc2TableAdapters.CustomerTableAdapter myAdaptor = new Subs.Data.CustomerDoc2TableAdapters.CustomerTableAdapter();
                myAdaptor.AttachConnection();
              
                myAdaptor.Destroy(gTable[0].CustomerId); // Remove from Database
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Destroy", "CustomerId = " + gTable[0].CustomerId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return "DestroyCustomer failed: " + ex.Message;

            }
        }
     

        private void CalculateClassification()
        {
            Subs.Data.ClassificationDoc2.CustomerClassificationDataTable lCustomerClassification = new ClassificationDoc2.CustomerClassificationDataTable();
            Subs.Data.ClassificationDoc2TableAdapters.CustomerClassificationTableAdapter lCustomerClassificationAdapter = new ClassificationDoc2TableAdapters.CustomerClassificationTableAdapter();
            lCustomerClassificationAdapter.AttachConnection();
            lCustomerClassificationAdapter.FillBy(lCustomerClassification, (int)CustomerId);

            int lClassificationId = 0;


            if (lCustomerClassification.Count > 0)
            {
                lClassificationId = lCustomerClassification.Max(p => p.ClassificationId); // Largest, most specific classificationId
            }
            else
            {
                _ClassificationId1 = 9; // Unknown
                _ClassificationId2 = null;
                return;
            }

            Subs.Data.ClassificationDoc2TableAdapters.ClassificationTableAdapter lClassificationAdapter = new Subs.Data.ClassificationDoc2TableAdapters.ClassificationTableAdapter();
            Subs.Data.ClassificationDoc2.ClassificationDataTable lClassification = new ClassificationDoc2.ClassificationDataTable();
            lClassificationAdapter.AttachConnection();
            lClassificationAdapter.Fill(lClassification);

            int lParentClassificationId = 0;

            while (lClassificationId != 1)
            {
                lParentClassificationId = lClassification.Where(p => p.ClassificationId == lClassificationId).Select(q => q.ParentId).Single();
                if (lParentClassificationId == 1)
                {
                    // Now you are are at the top
                    _ClassificationId1 = lClassificationId;
                }
                else
                {
                    _ClassificationId2 = lClassificationId;
                }
                lClassificationId = lParentClassificationId;
            }
        }


        private byte[] EncryptPassword(string pPassword)
        {
            try 
            { 
                byte[] cipherText;
  
                var key = new Rfc2898DeriveBytes(gPassword, gSalt);

                // Encrypt the data

                var algorithm = new RijndaelManaged();
                algorithm.Key = key.GetBytes(16);
                algorithm.IV = key.GetBytes(16);

                var sourceBytes = new System.Text.UnicodeEncoding().GetBytes(pPassword);
                using( var SourceStream = new MemoryStream(sourceBytes))
                using(var destinationStream =  new MemoryStream())
                using( var crypto = new CryptoStream(SourceStream, algorithm.CreateEncryptor(), CryptoStreamMode.Read))
                { 
                moveBytes(crypto, destinationStream);
                cipherText = destinationStream.ToArray();
                }
                return cipherText;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "EncryptPassword","" );
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);
                return new byte[]{ 0x00};
            }
        }

        private string DecryptPassword(System.Byte[] cipherText)
        {
            try
            {
                string lResult;
                byte[] salt = { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x5, 0x4, 0x3, 0x2, 0x1, 0x0 };
                var key = new Rfc2898DeriveBytes(gPassword, gSalt);

                // Encrypt the data

                var algorithm = new RijndaelManaged();
                algorithm.Key = key.GetBytes(16);
                algorithm.IV = key.GetBytes(16);

                //var sourceBytes = 
                using (var SourceStream = new MemoryStream(cipherText))
                using (var destinationStream = new MemoryStream())
                using (var crypto = new CryptoStream(SourceStream, algorithm.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    moveBytes(crypto, destinationStream);
                    var decryptedBytes = destinationStream.ToArray();
                    var decryptedMessage = new System.Text.UnicodeEncoding().GetString(decryptedBytes);
                    lResult = decryptedMessage;
                }
                return lResult;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "DecryptPassword", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return "Error";
            }
        }

        private void moveBytes(Stream source, Stream dest)
        {
            byte[] bytes = new byte[2048];
            var count  = source.Read(bytes, 0, bytes.Length);

            while( 0 != count)
            {
                dest.Write(bytes, 0, count);
                count = source.Read(bytes, 0, bytes.Length);
            }
        }

        #endregion

        #region Properties - public machine readable

        public List<InvoiceAndPayment> InvoiceAndPayment
        {
            get
            {
                return (List<InvoiceAndPayment>)gAllInvoiceAndPayment;
            }
        }

        public int CustomerId
        {
            get
            {
                if (gTable.Rows.Count == 0)
                {
                    return 0;
                }
                return gTable[0].CustomerId;
            }

            set
            {
                try
                {
                    if (value != 0)
                    {
                        gTable[0].CustomerId = value;
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
                        ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "CustomerId set", value.ToString());
                        CurrentException = CurrentException.InnerException;
                    } while (CurrentException != null);
                }
            }
        }

        public string Password1
        {
            get
            {
                if (gTable[0].IsPassword1Null())
                {
                    return "";
                }
                else
                    return DecryptPassword(gTable[0].Password1);
            }

            set
            {
                if (value == null)
                {
                    gTable[0].SetPassword1Null();
                }
                else
                {
                    if (value.Contains(" "))
                    {
                        throw new Exception("No blanks allowed in a password");
                    }
                    gTable[0].Password1 = EncryptPassword(value);
                }
            }
        }

        public int TitleId
        {
            get
            { return gTable[0].TitleId; }

            set
            {
                if (value < 2)
                {
                    throw new Exception("The Title is invalid! You have to supply a Title.");
                }
                gTable[0].TitleId = value;
                NotifyPropertyChanged("Title");
            }
        }

        public string Initials
        {
            get
            {
                if (gTable[0].IsInitialsNull())
                {
                    return "";
                }
                else
                {
                    return gTable[0].Initials;
                }
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    gTable[0].SetInitialsNull();
                    return;
                }

                Regex Test1 = new Regex(@"^[A-Z]+$");

                if (!Test1.IsMatch(value))
                {
                    throw new Exception("Initials should be only capital letters, no blanks or fullstops. Please try again.");
                }

                gTable[0].Initials = value;
                NotifyPropertyChanged("Initials");
            }
        }

        public string FirstName
        {
            get
            {
                if (gTable[0].IsFirstNameNull())
                {
                    return "";
                }
                else
                {
                    return gTable[0].FirstName;
                }
            }

            set
            {
                if (value == null || value.Trim() == "")
                {
                    gTable[0].SetFirstNameNull();
                }
                else
                {
                    gTable[0].FirstName = value;
                }
                NotifyPropertyChanged("FirstName");
            }
        }

        public string Surname
        {
            get
            {
                if (gTable[0].IsSurnameNull())
                {
                    return "";
                }
                else
                {
                    return gTable[0].Surname;
                }
            }
            set
            {
                if (value == null || value.Trim() == "")
                {
                    gTable[0].SetSurnameNull();
                }
                else
                {
                    gTable[0].Surname = value;
                }

                NotifyPropertyChanged("Surname");
            }
        }

        public string FullName {

            get 
            {
                StringBuilder lStringBuilder = new StringBuilder();

                if (Title != "")
                {
                    lStringBuilder.Append(Title + " ");
                }


                if (Initials != "")
                {
                    lStringBuilder.Append(Initials + " ");
                }


                if (Surname != "")
                {
                    lStringBuilder.Append(Surname);
                }

                return lStringBuilder.ToString();
            }
        }

        public string NationalId1
        {
            get
            {
                if (gTable[0].IsNationalId1Null())
                {
                    return "";
                }
                else
                {
                    return gTable[0].NationalId1.Trim();
                }
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    gTable[0].SetNationalId1Null();
                    return;
                }

                if (!Functions.IsInteger(value))
                {
                    throw new Exception("National id is not a proper number. Only digits are allowed, no spaces or dashes. Please try again.");
                }

                gTable[0].NationalId1 = value;
                NotifyPropertyChanged("NationalId1");
            }
        }

        public string NationalId2
        {
            get
            {
                if (gTable[0].IsNationalId2Null())
                {
                    return "";
                }
                else
                {
                    return gTable[0].NationalId2;
                }
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    gTable[0].SetNationalId2Null();
                    return;
                }

                if (!Functions.IsInteger(value))
                {
                    throw new Exception("National id is not a proper number. Only digits are allowed, no spaces or dashes. Please try again.");
                }

                gTable[0].NationalId2 = value;
                NotifyPropertyChanged("NationalId2");

            }
        }

        public string NationalId3
        {
            get
            {
                if (gTable[0].IsNationalId3Null())
                {
                    return "";
                }
                else
                {
                    return gTable[0].NationalId3;
                }
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    gTable[0].SetNationalId3Null();
                    return;
                }

                if (!Functions.IsInteger(value))
                {
                    throw new Exception("National id is not a proper number. Only digits are allowed, no spaces or dashes. Please try again.");
                }

                gTable[0].NationalId3 = value;
                NotifyPropertyChanged("NationalId1");
            }
        }

        public int CompanyId
        {
            get
            { return gTable[0].CompanyId; }

            set
            {
                if (value < 1)
                {
                    throw new Exception("The CompanyId is invalid!");
                }

                gTable[0].CompanyId = value;
                NotifyPropertyChanged("CompanyId");
                NotifyPropertyChanged("CompanyName");
                NotifyPropertyChanged("VatRegistration");
                NotifyPropertyChanged("CompanyRegistrationNumber");
                NotifyPropertyChanged("VendorNumber");
            }
        }

        public string CompanyName
        {
            get
            {
                if (CompanyId == 1)
                {
                    return "";
                }
                else return AdministrationData2.Company.FindByCompanyId(this.CompanyId).CompanyName;
            }
        }

        public string CompanyNameUnverified
        {
            get
            {
                if (gTable[0].IsCompanyNameUnverifiedNull())
                {
                    return "";
                }
                else
                {

                    return gTable[0].CompanyNameUnverified;
                }
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    gTable[0].SetCompanyNameUnverifiedNull();
                }
                else
                {
                    gTable[0].CompanyNameUnverified = value;
                }
            }
        }

        public string Department
        {
            get
            {
                if (gTable[0].IsDepartmentNull())
                {
                    return "";
                }
                else
                {

                    return gTable[0].Department;
                }
            }

            set
            { 
                gTable[0].Department = value;
                NotifyPropertyChanged("Department");
            }
        }
        

        public string Address1
        {
            get
            {
                if (gTable[0].IsAddress1Null())
                {
                    return "";
                }
                else
                {
                    return gTable[0].Address1;
                }
            }

            set
            {
                if (value == null || value.Trim() == "")
                {
                    throw new Exception("AddressLine1 is compulsory");
                }
                else
                {
                    gTable[0].Address1 = value;
                    NotifyPropertyChanged("Address1");
                }
            }
        }

        public string Address2
        {
            get
            {
                if (gTable[0].IsAddress2Null())
                {
                    return "";
                }
                else
                {
                    return gTable[0].Address2;
                }
            }

            set
            {
                if (value == null || value.Trim() == "")
                {
                    gTable[0].SetAddress2Null();
                }
                else
                {
                    gTable[0].Address2 = value;
                }
                NotifyPropertyChanged("Address2");
            }
        }

        public string Address3
        {
            get
            {
                if (gTable[0].IsAddress3Null())
                {
                    return "";
                }
                else
                {
                    return gTable[0].Address3;
                }
            }

            set
            {
                if (value == null || value.Trim() == "")
                {
                    gTable[0].SetAddress3Null();
                }
                else
                {
                    gTable[0].Address3 = value;
                }
                NotifyPropertyChanged("Address3");
            }
        }

        public string Address4
        {
            get
            {
                if (gTable[0].IsAddress4Null())
                {
                    return "";
                }
                else
                {
                    return gTable[0].Address4;
                }
            }

            set
            {
                if (value == null || value.Trim() == "")
                {
                    gTable[0].SetAddress4Null();
                }
                else
                {
                    gTable[0].Address4 = value;
                }
                NotifyPropertyChanged("Address4");
            }
        }

        public string Address5
        {
            get
            {
                if (gTable[0].IsAddress5Null())
                {
                    return "";
                }
                else
                {
                    return gTable[0].Address5;
                }
            }

            set
            {
                if (value == null || value.Trim() == "")
                {
                    gTable[0].SetAddress5Null();
                }
                else
                {
                    gTable[0].Address5 = value;
                }
                NotifyPropertyChanged("Address5");
            }
        }

        public AddressType AddressType
        {
            get
            {
                return (AddressType)Enum.Parse(typeof(AddressType), Enum.GetName(typeof(AddressType), gTable[0].AddressType));
            }

            set
            {
                gTable[0].AddressType = (int)value;
                NotifyPropertyChanged("AddressType");
            }
        }

        public int PostAddressId
        {
            get
            {
                return gTable[0].PostAddressId;
            }

            set
            {
                gTable[0].PostAddressId = (int)value;
                NotifyPropertyChanged("PostAddressId");
            }
        }

        public int? PhysicalAddressId
        {
            get
            {
                if (gTable[0].IsPhysicalAddressIdNull())
                {
                    return null;
                }
                else
                { 
                    return gTable[0].PhysicalAddressId;
                }
            }

            set
            { gTable[0].PhysicalAddressId = (int)value; NotifyPropertyChanged("PhysicalAddressId");
            }
        }

        public int CountryId
        {
            get
            { return gTable[0].CountryId; }

            set
            {
                if (value <= 1)
                {
                    throw new Exception("The Country is invalid");
                }

                if (gTable[0].CountryId.ToString() != value.ToString())
                {
                    ExceptionData.WriteException(3, "CountryId changed from " + gTable[0].CountryId.ToString()
                            + " to " + value.ToString(), this.ToString(), "CountryId", "");
                }

                gTable[0].CountryId = value;
                NotifyPropertyChanged("CountryNaam");
            }
        }

        public string PhoneNumber
        {
            get
            {
                if (gTable[0].IsPhoneNumberNull())
                {
                    return "";
                }
                else
                {
                    return gTable[0].PhoneNumber;
                }
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    gTable[0].SetPhoneNumberNull();
                    return;
                }

                if (!Functions.IsInteger(value))
                {
                    throw new Exception("Phone number is not a proper number. Only digits are allowed, no spaces or dashes. Please try again.");
                }
                
                gTable[0].PhoneNumber = value;
                NotifyPropertyChanged("PhoneNumber");
            }
        }

        public string CellPhoneNumber
        {
            get
            {
                  return gTable[0].CellPhoneNumber;
            }
            set
            {
                if (!Functions.IsInteger(value))
                {
                    throw new Exception("Cell phone number is not a proper number. Only digits are allowed, no spaces or dashes or '+'. Please try again.");
                }

                if (!value.StartsWith("0"))
                {
                    value = "0" + value;
                }

                gTable[0].CellPhoneNumber = value;
                NotifyPropertyChanged("CellPhoneNumber");

            }
        }

        public string EmailAddress
        {
            get
            {
                if (gTable[0].IsEmailAddressNull())
                {
                    return "";
                }
                else
                {
                    return gTable[0].EmailAddress;
                }
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    gTable[0].SetEmailAddressNull();
                    return;
                }
                else
                {
                    if (Regex.IsMatch(value, @"\.@|@\."))
                    {
                        throw new Exception("This is not a valid Email address");
                    }

                    if (!Regex.IsMatch(value, @"^([&\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,20}|[0-9]{1,3})(\]?)$"))
                                             
                    {
                        throw new Exception("This is not a valid Email address");
                    }
                    gTable[0].EmailAddress = value;
                    NotifyPropertyChanged("InvoiceEmail");  
                    NotifyPropertyChanged("StatementEmail");
                }
            }
        }

        public string InvoiceEmail
        {
            get
            {
                if (gTable[0].IsInvoiceEmailNull())
                {
                    if (!gTable[0].IsEmailAddressNull())
                    {
                        return gTable[0].EmailAddress;
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return gTable[0].InvoiceEmail;
                }
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    gTable[0].SetInvoiceEmailNull();
                }
                else
                {

                    if (Regex.IsMatch(value, @"\.@|@\."))
                    {
                        throw new Exception("This is not a valid Invoice Email address");
                    }

                    if (!Regex.IsMatch(value, @"^([&\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,20}|[0-9]{1,3})(\]?)$"))
                    {
                        throw new Exception("This is not a valid Invoice Email address");
                    }

                    gTable[0].InvoiceEmail = value;
                    NotifyPropertyChanged("NationalId1");

                }
            }
        }

        public string StatementEmail
        {
            get
            {
                if (gTable[0].IsStatementEmailNull())
                {
                    if (!gTable[0].IsEmailAddressNull())
                    {
                        return gTable[0].EmailAddress;
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return gTable[0].StatementEmail;
                }
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    gTable[0].SetStatementEmailNull();
                }
                else
                {
                    if (Regex.IsMatch(value, @"\.@|@\."))
                    {
                        throw new Exception("This is not a valid Statement Email address");
                    }

                    if (!Regex.IsMatch(value, @"^([&\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,20}|[0-9]{1,3})(\]?)$"))
                    {
                        throw new Exception("This is not a valid Statement Email address");
                    }

                    gTable[0].StatementEmail = value;
                    NotifyPropertyChanged("NationalId1");
                }
            }
        }

        public int Correspondence2
        {
            get
            { return gTable[0].Correspondence2; }

            set
            { gTable[0].Correspondence2 = value; NotifyPropertyChanged("Correspondence2");
            }
        }

        public decimal DeliverableMinusDue
        {
            get
            {
                  return Deliverable - Due;
            }
        }

        public decimal Deliverable
        {
            get
            {
                try
                {
                    return (decimal)gCustomerAdapter.Deliverable(CustomerId);
                }
                catch (Exception Ex)
                {
                    //Display all the exceptions

                    Exception CurrentException = Ex;
                    int ExceptionLevel = 0;
                    do
                    {
                        ExceptionLevel++;
                        ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Deliverable", "CustomerId = " + CustomerId.ToString());
                        CurrentException = CurrentException.InnerException;
                    } while (CurrentException != null);

                    throw Ex;
                }

            }
        }

        public decimal Due
        {
            get
            {
                try
                {
                    decimal lDue = (decimal)gCustomerAdapter.DueSum(CustomerId);
                    return lDue;
                }
                catch (Exception Ex)
                {
                    //Display all the exceptions

                    Exception CurrentException = Ex;
                    int ExceptionLevel = 0;
                    do
                    {
                        ExceptionLevel++;
                        ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Due", "CustomerId = " + CustomerId.ToString());
                        CurrentException = CurrentException.InnerException;
                    } while (CurrentException != null);

                    throw Ex;
                }

            }
            set
            {
                    gTable[0].Due = value;
            }

        }

        public int PreviousCheckpoint
        {
            get
            {
                try
                {
                    return (int)gCustomerAdapter.GetPreviousCheckpoint(CustomerId);
                }
                catch (Exception Ex)
                {
                    //Display all the exceptions

                    Exception CurrentException = Ex;
                    int ExceptionLevel = 0;
                    do
                    {
                        ExceptionLevel++;
                        ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "PreviousCheckpoint", "CustomerId = " + CustomerId.ToString());
                        CurrentException = CurrentException.InnerException;
                    } while (CurrentException != null);

                    throw Ex;
                }

            }
        }








        public DateTime? VerificationDate
        {
            get
            {
                if (gTable[0].IsVerificationDateNull())
                {
                    return DateTime.Now.Date;
                }
                else
                {
                    return gTable[0].VerificationDate;
                }
            }

            set
            {
                if (value.HasValue)
                {
                    gTable[0].VerificationDate = (DateTime)value;
                    NotifyPropertyChanged("VerificationDate");

                }
            }
        }

        public string CouncilNumber
        {
            get
            {
                if (gTable[0].IsCouncilNumberNull())
                {
                    return "";
                }
                else
                {
                    return gTable[0].CouncilNumber;
                }
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    gTable[0].SetCouncilNumberNull();
                }
                else
                {
                    gTable[0].CouncilNumber = value;
                    NotifyPropertyChanged("CouncilNumber");
                }
            }
        }


        public string PracticeNumber1
        {
            get
            {
                if (gTable[0].IsPracticeNumber1Null())
                {
                    return "";
                }
                else
                {
                    return gTable[0].PracticeNumber1;
                }
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    gTable[0].SetPracticeNumber1Null();
                }
                else
                {
                    gTable[0].PracticeNumber1 = value;
                    NotifyPropertyChanged("PracticeNumber1");
                }
            }
        }

        public string PracticeNumber2
        {
            get
            {
                if (gTable[0].IsPracticeNumber2Null())
                {
                    return "";
                }
                else
                {
                    return gTable[0].PracticeNumber2;
                }
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    gTable[0].SetPracticeNumber2Null();
                }
                else
                {
                    gTable[0].PracticeNumber2 = value;
                    NotifyPropertyChanged("PracticeNumber2");
                }
            }
        }

        public string PracticeNumber3
        {
            get
            {
                if (gTable[0].IsPracticeNumber3Null())
                {
                    return "";
                }
                else
                {
                    return gTable[0].PracticeNumber3;
                }
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    gTable[0].SetPracticeNumber3Null();
                }
                else
                {
                    gTable[0].PracticeNumber3 = value;
                    NotifyPropertyChanged("PracticeNumber3");
                }
            }
        }

        public bool AutomaticPaymentAllocation
        {
            get
            {
                return gTable[0].AutomaticPaymentAllocation;
            }

            set 
            {
                gTable[0].AutomaticPaymentAllocation = value;
                NotifyPropertyChanged("AutomaticPaymentAllocation");
            }
        }

        public bool Marketing
        {
            get
            {
                return gTable[0].Marketing;
            }

            set 
            {
                gTable[0].Marketing = value;
                NotifyPropertyChanged("Marketing");

            }
        }

        public int ClassificationId1 
        { 
            get
            {
                if (_ClassificationId1 == 9)
                {
                    CalculateClassification();
                }
               
                return _ClassificationId1;
            }
        }

        public int? ClassificationId2
        {
            get
            {
                if (_ClassificationId2 == 9)
                {
                    CalculateClassification();
                }

                return _ClassificationId2;
            }

            set
            {
                try
                { 

                if (value == null)
                {
                    return;
                }

                if ((int)value == ClassificationId2)
                {
                    // I have this one already
                    return;
                }

                // I am assuming that this comes in from the web. So, I am going to allow only one classification.
                Subs.Data.ClassificationDoc2.CustomerClassificationDataTable lCustomerClassification = new ClassificationDoc2.CustomerClassificationDataTable();
                Subs.Data.ClassificationDoc2TableAdapters.CustomerClassificationTableAdapter lCustomerClassificationAdapter = new ClassificationDoc2TableAdapters.CustomerClassificationTableAdapter();
                lCustomerClassificationAdapter.AttachConnection();
                lCustomerClassificationAdapter.FillBy(lCustomerClassification, (int)CustomerId);

                if (lCustomerClassification.Count != 1)
                {
                    foreach (ClassificationDoc2.CustomerClassificationRow item in lCustomerClassification)
                    {
                        item.Delete();
                    }
                    lCustomerClassificationAdapter.Update(lCustomerClassification);
                    lCustomerClassification.AcceptChanges();

                    Subs.Data.ClassificationDoc2.CustomerClassificationRow lNewRow = lCustomerClassification.NewCustomerClassificationRow();
                    lNewRow.CustomerId = CustomerId;
                    lNewRow.ClassificationId = (int)value;
                    lNewRow.ModifiedBy = System.Environment.UserDomainName;
                    lNewRow.ModifiedOn = DateTime.Now;
                    lCustomerClassification.AddCustomerClassificationRow(lNewRow);
                }
                else
                {
                    // Modify the existing one
                    lCustomerClassification[0].ClassificationId = (int)value;
                }

                lCustomerClassificationAdapter.Update(lCustomerClassification);
                }
                catch (Exception ex)
                {
                    //Display all the exceptions

                    Exception CurrentException = ex;
                    int ExceptionLevel = 0;
                    do
                    {
                        ExceptionLevel++;
                        ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomerData", "ClassificationId2 Set", "ClassificationId2 = " + value.ToString());
                        CurrentException = CurrentException.InnerException;
                    } while (CurrentException != null);
                }
            }
        }

        //public int CheckpointDate
        //{
        //    // This is  old one
        //    get
        //    { return gTable[0].CheckpointPaymentTransactionId; }

        //    set
        //    {
        //        gTable[0].CheckpointPaymentTransactionId = value;
        //        NotifyPropertyChanged("CheckpointPaymentTransactionId");
        //    }
        //}



        //public decimal CheckpointPaymentValue
        //{
        //    get
        //    { return gTable[0].CheckpointValue; }

        //    set
        //    {
        //        gTable[0].CheckpointValue = value;
        //        NotifyPropertyChanged("CheckpointPaymentValue");
        //    }
        //}

        public DateTime CheckpointDateInvoice
        {
            get
            { return gTable[0].CheckpointDateInvoice; }

            set
            {
                gTable[0].CheckpointDateInvoice = value;
                NotifyPropertyChanged("CheckpointDateInvoice");
            }
        }

        public bool Modified
        {
            get 
            {
                if (gTable[0].RowState == DataRowState.Unchanged)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            set 
            {
                if (!(bool)value)
                {
                    gTable[0].RejectChanges();
                }
            }
        }


        public int? BalanceInvoiceId
        {
            get
            {
                if (gTable[0].IsBalanceInvoiceIdNull())
                {
                    // This is a dormant customer
                    return null;
                }
                else
                { 
                    return gTable[0].BalanceInvoiceId;
                }
            }

            set
            {
                if (!value.HasValue)
                {
                    gTable[0].SetBalanceInvoiceIdNull();
                }
                else
                { 
                    gTable[0].BalanceInvoiceId = (int)value;
                }
                NotifyPropertyChanged("BalanceInvoiceId");
            }
        }


        public DateTime? BalanceInvoiceDate
        {
            get
            {
                try
                {
                    if (BalanceInvoiceId == 0)
                    {
                        return null;
                    }
                    else
                    { 
                        return (DateTime)gCustomerAdapter.BalanceInvoiceDate(BalanceInvoiceId);
                    }
                }
                catch (Exception Ex)
                {
                    //Display all the exceptions

                    Exception CurrentException = Ex;
                    int ExceptionLevel = 0;
                    do
                    {
                        ExceptionLevel++;
                        ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "BalanceInvoiceDate", "CustomerId = " + CustomerId.ToString());
                        CurrentException = CurrentException.InnerException;
                    } while (CurrentException != null);

                    throw Ex;
                }
            }
        }



        public int? BalanceInvoiceTransactionId
        {
            get
            {
                try
                {
                    return (int)gCustomerAdapter.BalanceInvoiceTransactionId(BalanceInvoiceId);
                }
                catch (Exception Ex)
                {
                    //Display all the exceptions

                    Exception CurrentException = Ex;
                    int ExceptionLevel = 0;
                    do
                    {
                        ExceptionLevel++;
                        ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "BalanceInvoiceTransactionId", "CustomerId = " + CustomerId.ToString());
                        CurrentException = CurrentException.InnerException;
                    } while (CurrentException != null);

                    throw Ex;
                }
            }
        }



        public Decimal Balance
        {
            get
            {
                if (gTable[0].IsBalanceNull())
                {
                    return 0M;
                }
                else
                {
                    return gTable[0].Balance; 
                }
            }

            set
            { 
                gTable[0].Balance = value;
                NotifyPropertyChanged("Balance");
            }
        }

   
        public string ModifiedBy
        {
            get
            { return gTable[0].ModifiedBy; }

            set
            { gTable[0].ModifiedBy = value; }
        }

        public DateTime ModifiedOn
        {
            get
            { return gTable[0].ModifiedOn; }

            set
            { gTable[0].ModifiedOn = value; }
        }

        #endregion

        #region Properties, derived, human readable

        public string CompanyRegistrationNumber
        {
            get
            {
                if (AdministrationData2.Company.FindByCompanyId(this.CompanyId).IsCompanyRegistrationNumberNull())
                { return ""; }
                else
                { return AdministrationData2.Company.FindByCompanyId(this.CompanyId).CompanyRegistrationNumber; }

                //return AdministrationData2.Company.Where(p => p.CompanyId == this.CompanyId).Select(q => q.CompanyRegistrationNumber).Single() ?? "";
            }
        }

        public string VATRegistration
        {
            get
            {
                if (AdministrationData2.Company.FindByCompanyId(this.CompanyId).IsVatRegistrationNull())
                { return ""; }
                else
                { return AdministrationData2.Company.FindByCompanyId(this.CompanyId).VatRegistration; }
            }
        }

        public string VendorNumber
        {
            get
            {
                if (AdministrationData2.Company.FindByCompanyId(this.CompanyId).IsVendorNumberNull())
                { return ""; }
                else
                { return AdministrationData2.Company.FindByCompanyId(this.CompanyId).VendorNumber; }

            }
        }

        public string CountryName
        {
            get
            {
                return AdministrationData2.Country.FindByCountryId(this.CountryId).CountryName;
            }
        }

        public string Title
        {
            get
            {
                if (TitleId == 1)
                {
                    return "";
                }
                else
                { 
                    return (Enum.GetName(typeof(Data.Title), this.TitleId));
                }
            }
        }

        public int NumberOfActiveSubscriptions
        {
            get
            {
                try
                {
                    SqlConnection Connection = new SqlConnection(Settings.ConnectionString);
                    SqlCommand Command = new SqlCommand();
                    SqlDataAdapter Adaptor = new SqlDataAdapter();
                    Connection.Open();
                    Command.Connection = Connection;
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.CommandText = "[MIMS.CustomerData.NumberOfActiveSubscriptions]";
                    SqlCommandBuilder.DeriveParameters(Command);
                    Adaptor.SelectCommand = Command;
                    Command.Parameters["@CustomerId"].Value = gTable[0].CustomerId;

                    int? Result = (int?)Command.ExecuteScalar();
                    if (Result != null && Result.HasValue)
                    {
                        return (int)Result;
                    }
                    else
                    {
                        throw new Exception("The query did not return any scalar");
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
                        ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "NumberOfActiveSubscriptions", "");
                        CurrentException = CurrentException.InnerException;
                    } while (CurrentException != null);

                    throw ex;
                }

                finally
                {
                    //gConnection.Close();
                }
            }
        }

        public bool DebitorderUser
        {
            get
            {
                try
                {
                    gSBDebitOrderAdapter.AttachConnection();

                    int lResult = (int)gSBDebitOrderAdapter.Valid(CustomerId);

                    if (lResult == 1)
                    {
                        return true;
                    }
                    else
                    {

                        return false;
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
                        ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "DebitorderUser", "");
                        CurrentException = CurrentException.InnerException;
                    } while (CurrentException != null);

                    return false;
                }
            }
        }

        #endregion

        #region Direct static queries on the data

        public static List<Discrepancy> GetDiscrepancy(int pAbsoluteValue)
        {
            SqlConnection lConnection = new SqlConnection();
            int lCurrentCustomerId = 0;
            try
            {
                List<Discrepancy> lDiscrepancies = new List<Discrepancy>();
                
                SqlCommand Command = new SqlCommand();
                SqlDataAdapter Adaptor = new SqlDataAdapter();
                lConnection.ConnectionString = Settings.ConnectionString;
                lConnection.Open();
                Command.Connection = lConnection;
                Command.CommandType = CommandType.StoredProcedure;
                Command.CommandText = "[dbo].[MIMS.CustomerData.GetDiscrepancy]";

                Command.Parameters.Add("@AbsoluteValue", SqlDbType.Int);
                Command.Parameters["@AbsoluteValue"].Value = pAbsoluteValue;

                SqlDataReader lReader = Command.ExecuteReader();

                while (lReader.Read())
                {
                    Discrepancy lDiscrepancy = new Discrepancy();
                    lDiscrepancy.CustomerId = lReader.GetInt32(0);
                    lCurrentCustomerId = lDiscrepancy.CustomerId;
                    lDiscrepancy.FirstName = lReader.GetString(1);
                    lDiscrepancy.Surname= lReader.GetString(2);
                    lDiscrepancy.Due = lReader.GetDecimal(3);
                    lDiscrepancy.DeliverableMinusDue = lReader.GetDecimal(4);
                    lDiscrepancy.LastActivity = lReader.GetDateTime(5);
                    lDiscrepancies.Add(lDiscrepancy);
                }

                return lDiscrepancies;
                   
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomersData", "GetDiscrepancy", "Customer = " + lCurrentCustomerId.ToString());
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

        public static List<DeliverableDetail> GetDeliverable(int pPayerId)
        {
            SqlConnection lConnection = new SqlConnection();
            int lCurrentCustomerId = 0;
            try
            {
                List<DeliverableDetail> lDeliverableDetails = new List<DeliverableDetail>();

                SqlCommand Command = new SqlCommand();
                SqlDataAdapter Adaptor = new SqlDataAdapter();
                lConnection.ConnectionString = Settings.ConnectionString;
                lConnection.Open();
                Command.Connection = lConnection;
                Command.CommandType = CommandType.StoredProcedure;
                Command.CommandText = "[dbo].[MIMS.CustomerData.DeliverableDetail]";

                Command.Parameters.Add("@PayerId", SqlDbType.Int);
                Command.Parameters["@PayerId"].Value = pPayerId;

                SqlDataReader lReader = Command.ExecuteReader();

                while (lReader.Read())
                {
                    DeliverableDetail lDeliveryableDetail = new DeliverableDetail();
                    lDeliveryableDetail.IssueDescription = lReader.GetString(0);
                    lDeliveryableDetail.StartDate = lReader.GetDateTime(1);
                    lDeliveryableDetail.Units = lReader.GetInt32(2);
                    lDeliveryableDetail.Amount = lReader.GetDecimal(3);
                    lDeliverableDetails.Add(lDeliveryableDetail);
                }

                return lDeliverableDetails;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomersData", "GetDeliverable", "Customer = " + lCurrentCustomerId.ToString());
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




        public string CalculateLiability2(ref List<LiabilityRecord> pLiabilityList, ref decimal pLiability)
        {
            try
            {
                {
                    string lResult;

                    if ((lResult = PopulateInvoice2()) != "OK")
                    {
                        return lResult;
                    }
                }


                List<LiabilityRecord> lLiabilityList = gAllInvoiceAndPayment.Where(p => p.OperationId == (int)Operation.Pay
                                                                                      || p.OperationId == (int)Operation.ReversePayment
                                                                                      || p.OperationId == (int)Operation.Refund)
                                                  .Select(v => new LiabilityRecord()
                                                  {
                                                      TransactionId = v.TransactionId,
                                                      OriginalTransactionId = v.OriginalTransactionId,
                                                      CaptureDate = v.CaptureDate,
                                                      EffectiveDate = v.Date,
                                                      Operation = v.Operation,
                                                      OperationId = v.OperationId,
                                                      InvoiceId = 0,
                                                      SubscriptionId = 0,
                                                      Value = v.Value
                                                  }).ToList();

                // Seperate out the payment related stuff

                List<LiabilityRecord> lPaymentList = lLiabilityList.Where(p => p.OperationId == (int)Operation.Pay
                                                                                      || p.OperationId == (int)Operation.ReversePayment
                                                                                      || p.OperationId == (int)Operation.Refund).ToList();

                //SumOfPayments = -lPaymentList.Sum(p => p.Value);
                gCustomerAdapter.Update(gTable[0]);



                IEnumerable<int> lInvoices = (IEnumerable<int>)gAllInvoiceAndPayment.ToList<InvoiceAndPayment>().ToList().Select(p => p.InvoiceId).Distinct();

                // Get deliveries

                MIMSDataContext lContext = new MIMSDataContext(Settings.ConnectionString);
                List<LiabilityRecord> lDeliveryList = (List<LiabilityRecord>)lContext.MIMS_DataContext_Deliveries(CustomerId).ToList();

                foreach (LiabilityRecord lDelivery in lDeliveryList)
                {
                    // I do not work with deliveries that are not related to invoiced payments
                    if (lInvoices.Contains(lDelivery.InvoiceId))
                    {
                        lLiabilityList.Add(lDelivery);
                    }
                }

                //SumOfDeliveries = -lDeliveryList.Sum(p => p.Value);
                gCustomerAdapter.Update(gTable[0]);

                // From here on switch the sign to represent Liability from our perspective

                pLiability = -lLiabilityList.Sum(r => r.Value);


                // I want to order by date, and then, within date, by transactionId. 

                // Convert to days without time, i.e. time = 00.00.00.000

                //Put the original transactionid back, so that you do not get duplicate keys in LiabilityRecord.

                foreach (LiabilityRecord item in lLiabilityList)
                {
                    item.EffectiveDate = new DateTime(item.EffectiveDate.Year, item.EffectiveDate.Month, item.EffectiveDate.Day);

                    if (item.OperationId == (int)Operation.ReversePayment || item.OperationId == (int)Operation.Refund)
                    {
                        item.PaymentTransactionId = item.TransactionId;
                        item.TransactionId = (int)item.OriginalTransactionId;
                    }
                }


                List<DateTime> lDates = (List<DateTime>)lLiabilityList.Select(o => o.EffectiveDate).Distinct().OrderBy(o => o.Date).ToList();
                List<LiabilityRecord> lGroup = new List<LiabilityRecord>();
                decimal lBalance = 0M;
                
                foreach (DateTime lDate in lDates)
                {
                    lGroup = (List<LiabilityRecord>)lLiabilityList.Where(o => o.EffectiveDate == lDate).OrderBy(o => o.TransactionId).ToList();
                    foreach (LiabilityRecord lRecord in lGroup)
                    {
                        lRecord.Value = -lRecord.Value;
                        lBalance = lBalance + lRecord.Value;
                        lRecord.Balance = lBalance;
                    }

                    foreach (LiabilityRecord lRecord in lGroup)
                    {
                        pLiabilityList.Add(lRecord);
                    }
                }
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "static CustomerData", "CalculateLiability2", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return "Error in CalculateLiability: " + ex.Message;
            }
        }

        public static bool Exists(int CustomerId)
        {
            CustomerDoc2TableAdapters.CustomerTableAdapter lAdapter = new CustomerDoc2TableAdapters.CustomerTableAdapter();

            try
            {
                lAdapter.AttachConnection();
                if ((int)lAdapter.Exists(CustomerId) == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                ExceptionData.WriteException(1, ex.Message, "CustomerData", "Exists", "");
                throw new Exception(ex.Message);
            }
            finally
            {
                if (lAdapter.Connection.State == ConnectionState.Open)
                {
                    lAdapter.Connection.Close();
                }
            }
        }
         
        public static bool FindCustomerId(string pEMail, out int pCustomerId)
        {
            CustomerDoc2TableAdapters.CustomerTableAdapter lAdapter = new CustomerDoc2TableAdapters.CustomerTableAdapter();
            CustomerDoc2.CustomerDataTable lCustomerDataTable = new CustomerDoc2.CustomerDataTable();
           
            try
            {
                lAdapter.AttachConnection();
                lAdapter.FillById(lCustomerDataTable, "Email", 0, pEMail);

                if (lCustomerDataTable.Count == 1)
                {
                    pCustomerId = lCustomerDataTable[0].CustomerId;
                }
                else
                {   
                    pCustomerId = 0;
                }
                return true;

            }
            catch (Exception ex)
            {
                ExceptionData.WriteException(1, ex.Message, "CustomerData", "FindCustomerId", "Email = " + pEMail);
                pCustomerId = 0;
                return false;
            }
            finally
            {
                if (lAdapter.Connection.State == ConnectionState.Open)
                {
                    lAdapter.Connection.Close();
                }
            }
        }

       public static List<int> GetCustomerIds(string pType, int pIntegerId, string pStringId)
        {
            List<int> lCustomerIds = new List<int>();
            try
            {
                SqlConnection lConnection = new SqlConnection();
                SqlCommand Command = new SqlCommand();
                SqlDataAdapter Adaptor = new SqlDataAdapter();
                lConnection.ConnectionString = Settings.ConnectionString;
                lConnection.Open();
                Command.Connection = lConnection;
                Command.CommandType = CommandType.StoredProcedure;
                Command.CommandText = "[dbo].[MIMS.CustomerData.Select]";
           
                SqlParameter lParameter1 = Command.CreateParameter();
                lParameter1.ParameterName = "@Type";
                lParameter1.DbType = DbType.String;
                lParameter1.Value = pType;
                Command.Parameters.Add(lParameter1);

                SqlParameter lParameter2 = Command.CreateParameter();
                lParameter2.ParameterName = "@IntegerId";
                lParameter2.DbType = DbType.Int32;
                lParameter2.Value = pIntegerId;
                Command.Parameters.Add(lParameter2);

                SqlParameter lParameter3 = Command.CreateParameter();
                lParameter3.ParameterName = "@StringId";
                lParameter3.DbType = DbType.String;
                lParameter3.Value = pStringId;
                Command.Parameters.Add(lParameter3);

                SqlDataReader lReader = Command.ExecuteReader();


                while (lReader.Read())
                {
                    lCustomerIds.Add(lReader.GetInt32(0));
                }

                return lCustomerIds;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomersData", "GetCustomerIds", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }

    
        public static bool FindCustomerIdByNationalId(string pNationalId, out int pCustomerId)
        {
            CustomerDoc2TableAdapters.CustomerTableAdapter lAdapter = new CustomerDoc2TableAdapters.CustomerTableAdapter();
            CustomerDoc2.CustomerDataTable lCustomerDataTable = new CustomerDoc2.CustomerDataTable();

            try
            {
                lAdapter.AttachConnection();
                lAdapter.FillById(lCustomerDataTable, "NationalId", 0, pNationalId);
                if (lCustomerDataTable.Count == 1)
                {
                    pCustomerId = lCustomerDataTable[0].CustomerId;
                    return true;
                }
                else
                {
                    pCustomerId = 0;
                    return false;
                }
            }
            catch (Exception ex)
            {
                ExceptionData.WriteException(1, ex.Message, "CustomerData", "FindCustomerId", "");
                throw new Exception(ex.Message);
            }
            finally
            {
                if (lAdapter.Connection.State == ConnectionState.Open)
                {
                    lAdapter.Connection.Close();
                }
            }
        }

        public static int ExistsByEmail(string pEmail)
        {
            CustomerDoc2.CustomerDataTable lTable = new CustomerDoc2.CustomerDataTable();
            CustomerDoc2TableAdapters.CustomerTableAdapter lAdapter = new CustomerDoc2TableAdapters.CustomerTableAdapter();
            try
            {
                lAdapter.AttachConnection();
                lAdapter.FillById(lTable, "Email", 0, pEmail);

                if (lTable.Count == 1)
                {
                    return lTable[0].CustomerId;
                }
                else return 0;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "static CustomerData3", "ExistsByLoginEmail", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);
                return 0;
            }
        }

        public static bool UpdateConsolidation(int Source, int Target)
        {
            SqlConnection lConnection = new SqlConnection();
            try
            {
                lConnection.ConnectionString = Settings.ConnectionString;
                SqlCommand Command = new SqlCommand();
                lConnection.Open();
                Command.Connection = lConnection;
                Command.CommandType = CommandType.StoredProcedure;
                Command.CommandText = "[MIMS.CustomerData.UpdateConsolidation]";
                SqlCommandBuilder.DeriveParameters(Command);

                Command.Parameters["@Source"].Value = Source;
                Command.Parameters["@Target"].Value = Target;
                Command.ExecuteNonQuery();

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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomerData3", "UpdateConsolidation", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return false;
            }
            finally
            {
                lConnection.Close();
            }
        }
          
        public static List<CustomerData3> CustomersWithUnverifiedCompany()
        {
            try
            {
                List<CustomerData3> lCustomers = new List<CustomerData3>();
                List<int> lCustomerIds = new List<int>();

                SqlConnection lConnection = new SqlConnection();
                SqlCommand Command = new SqlCommand();
                SqlDataAdapter Adaptor = new SqlDataAdapter();
                lConnection.ConnectionString = Settings.ConnectionString;
                lConnection.Open();
                Command.Connection = lConnection;
                Command.CommandType = CommandType.Text;
                Command.CommandText = "select CustomerId from Customer where CompanyNameUnverified is not null";
                SqlDataReader lReader = Command.ExecuteReader();

                while (lReader.Read())
                {
                    lCustomerIds.Add(lReader.GetInt32(0));
                }

                foreach (int item in lCustomerIds)
                {
                    lCustomers.Add(new CustomerData3(item));
                }
                return lCustomers;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomersData", "CompanyNameUnverified", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }
        public static (int InvoiceId, decimal OutstandingAmount) GetOutstandingOnLastInvoice(int pPayerId)
        {
            try 
            { 
            SqlConnection lConnection = new SqlConnection();
            SqlCommand Command = new SqlCommand();
            SqlDataAdapter Adaptor = new SqlDataAdapter();
            lConnection.ConnectionString = Settings.ConnectionString;
            lConnection.Open();
            Command.Connection = lConnection;
            Command.CommandType = CommandType.StoredProcedure;
            Command.CommandText = "[dbo].[MIMS.CustomerData.GetOutstandingOnLastInvoice]";

            SqlParameter lParameter2 = Command.CreateParameter();
            lParameter2.ParameterName = "@PayerId";
            lParameter2.DbType = DbType.Int32;
            lParameter2.Value = pPayerId;
            Command.Parameters.Add(lParameter2);

            SqlDataReader lReader = Command.ExecuteReader();
            lReader.Read();
            return (lReader.GetInt32(0) , lReader.GetDecimal(1));
            }
            catch(Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomersData", "GetOutstandingOnLastInvoice", pPayerId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }

        public static (string Reference, decimal CreditValue) GetLastPayment(int pPayerId)
        {
            try
            {
                SqlConnection lConnection = new SqlConnection();
                SqlCommand Command = new SqlCommand();
                SqlDataAdapter Adaptor = new SqlDataAdapter();
                lConnection.ConnectionString = Settings.ConnectionString;
                lConnection.Open();
                Command.Connection = lConnection;
                Command.CommandType = CommandType.StoredProcedure;
                Command.CommandText = "[dbo].[MIMS.CustomerData.GetLastPayment]";

                SqlParameter lParameter2 = Command.CreateParameter();
                lParameter2.ParameterName = "@PayerId";
                lParameter2.DbType = DbType.Int32;
                lParameter2.Value = pPayerId;
                Command.Parameters.Add(lParameter2);

                SqlDataReader lReader = Command.ExecuteReader();
                if (!lReader.HasRows)
                {
                    return ("Dummy", 0.00M);
                }

                lReader.Read();
                return (lReader.GetString(0), lReader.GetDecimal(1));
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomersData", "GetLastPayment", pPayerId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }

        public static int GetLastInvoiceId(int pPayerId)
        {
            try
            {
                SqlConnection lConnection = new SqlConnection();
                SqlCommand Command = new SqlCommand();
                SqlDataAdapter Adaptor = new SqlDataAdapter();
                lConnection.ConnectionString = Settings.ConnectionString;
                lConnection.Open();
                Command.Connection = lConnection;
                Command.CommandType = CommandType.StoredProcedure;
                Command.CommandText = "[dbo].[MIMS.CustomerData.GetLastInvoiceId]";

                SqlParameter lParameter2 = Command.CreateParameter();
                lParameter2.ParameterName = "@PayerId";
                lParameter2.DbType = DbType.Int32;
                lParameter2.Value = pPayerId;
                Command.Parameters.Add(lParameter2);

                SqlDataReader lReader = Command.ExecuteReader();
                if (!lReader.HasRows)
                {
                    return 0;
                }

                lReader.Read();
                return (lReader.GetInt32(0));
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomersData", "GetLastPayment", pPayerId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }

        public static (DateTime CheckpointDatePayment, DateTime CheckpointDateInvoice) CalculateCheckpoint(int pInvoiceId)
        {
            try
            {
                SqlCommand Command = new SqlCommand();
                SqlConnection lConnection = new SqlConnection(Settings.ConnectionString);
                lConnection.Open();
                Command.CommandType = CommandType.StoredProcedure;
                Command.Connection = lConnection;

                Command.CommandText = "[dbo].[MIMS.CustomerData.CalculateCheckpoint]";

                Command.Parameters.Add("@InvoiceId", SqlDbType.Int);
                Command.Parameters["@InvoiceId"].Value = pInvoiceId;

                SqlDataReader lReader = Command.ExecuteReader();

                lReader.Read();

                (DateTime, DateTime) lResult = (lReader.GetDateTime(0), lReader.GetDateTime(1));

                return lResult;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "static CustomerData", "CalculateCheckpoint",
                       "InvoiceId = " + pInvoiceId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }

        #endregion
    }
}


