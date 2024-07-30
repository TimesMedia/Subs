using Subs.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Net.Http;
using System.Collections.Specialized;
using System.Text;
using System.Text.Json;

using static Subs.Data.CustomerData3;
using System.IO;
using System.Net.Http.Headers;
using System.Net;
using System.Net.Mail;

namespace Subs.Business
{
    public static class CustomerBiz
    {
        #region Global variables and constructor
 
        public enum PaymentValidationResult
        {
            OK = 1,
            Duplicate = 2,
            TooMuch = 3,
            PayerCancelled = 4,
            NoReference = 5,
            InvalidInvoice = 6,
            InvalidAllocationNumber = 7,
            PayerDoesNotExist = 8,
            NegativeNumber = 9,
            NoInvoicesPast3years = 10,
            PaymentBeforeBalanceInvoice
        }

        private static List<int> gMediaCities = new List<int>();


        static CustomerBiz()
        {
            gMediaCities.Add(323); // Alberton
            gMediaCities.Add(349); // BoksBurg
            gMediaCities.Add(304); //Brakpan
            gMediaCities.Add(344); // Centurion
            gMediaCities.Add(285); // Edenvale
            gMediaCities.Add(352); // Johannesburg
            gMediaCities.Add(311); // Kemptonpark
            gMediaCities.Add(375); // MIdrand
            gMediaCities.Add(360); // Pretoria
            gMediaCities.Add(347); // Randburg
            gMediaCities.Add(364); // Roodepoort
            gMediaCities.Add(879); // Capetown
            gMediaCities.Add(9);   // Gqeberha
            gMediaCities.Add(457); // Durban
            gMediaCities.Add(249); // Bloemfontein
        }
        #endregion
     
        #region Payment

        public static string ValidatePayment(ref Subs.Data.PaymentData.PaymentRecord pRecord, ref PaymentValidationResult pResult, ref string pErrorMessage)
        {
            try
            {
                //Check if the value is positive

                if (pRecord.Amount < 0)
                {
                    pErrorMessage = "If this is a bounced payment, please mark it as such. ";
                    pResult = PaymentValidationResult.NegativeNumber;
                    return "OK";
                }

                //Check on the status of the payer

                if (!CustomerData3.Exists(pRecord.CustomerId))
                {
                    pErrorMessage = "Payer " + pRecord.CustomerId.ToString() + " does not exist";
                    pResult = PaymentValidationResult.PayerDoesNotExist;
                    return "OK";
                }

                CustomerData3 myCustomerData = new CustomerData3(pRecord.CustomerId);


                // Test to ensure that this is not a duplicate entry

                if (LedgerData.DuplicatePayment(pRecord.CustomerId,
                pRecord.Reference, pRecord.Amount) > (int)0)
                {
                    pErrorMessage = "There is already a payment with this reference number!";
                    ExceptionData.WriteException(5, "Duplicate Payment detected", "CustomerBizStatic", "ValidatePayment", "Reference = " + pRecord.Reference);
                    pResult = PaymentValidationResult.Duplicate;
                    return "OK";
                }

                // Test to see if it refers to a payment before the checkpoint invoice date.
              

                if (pRecord.Date.AddDays(5) < (myCustomerData.BalanceInvoiceDate?? myCustomerData.CheckpointDateInvoice)) 
                {
                    pErrorMessage = "This payment refers too far into the past!";
                    ExceptionData.WriteException(5, "PaymentBeforeBalanceInvoice detected", "CustomerBizStatic", "ValidatePayment", "Reference = " + pRecord.Reference);
                    pResult = PaymentValidationResult.PaymentBeforeBalanceInvoice;
                    return "OK";

                }

                //********************************************************************************


                // See what is due

                decimal lDue = myCustomerData.Due;


                if ((pRecord.Amount - lDue) > 1)
                {
                    pErrorMessage = "The most this guy owes us is " + lDue.ToString("#######0.00");
                    pResult = PaymentValidationResult.TooMuch;
                    return "OK";
                }

                pResult = PaymentValidationResult.OK;
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomerBizStatic", "ValidatePayment", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return "Error in ValidatePayment: " + ex.Message;
            }
        }

        public static string Pay(ref PaymentData.PaymentRecord pRecord, out int pTransactionId)
        {
            pTransactionId = 0;

            if (LedgerData.DuplicatePayment(pRecord.CustomerId, pRecord.Reference, pRecord.Amount) > 0)
            {
                return "Duplicate payment not allowed";
            }

            SqlConnection lConnection = new SqlConnection();
            lConnection.ConnectionString = Settings.ConnectionString;
            SqlTransaction lTransaction;
            lConnection.Open();
            lTransaction = lConnection.BeginTransaction("Pay");

            try
            {
                if (pRecord.Amount < 0)
                {
                    // This is a bogus payment
                    lTransaction.Rollback("Pay");
                    return "What kind of payment is this? I accept only positive numbers.";
                }

                pTransactionId = LedgerData.Pay(ref lTransaction, pRecord);

                lTransaction.Commit();

                lConnection.Close();
                lTransaction.Dispose();

                {
                    string lResult;

                    if ((lResult = ProductBiz.DeliverElectronic(pRecord.CustomerId)) != "OK")
                    {
                        // Ignore this, you will catch it on the next general delivery run.
                    }
                }

                return "OK";

            }
            catch (Exception ex)
            {
                lTransaction.Rollback("Pay");

                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomerBizStatic", "Pay", "PayerId = " + pRecord.CustomerId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return ex.Message;
            }
            finally
            {
                if (lConnection.State == ConnectionState.Open)
                {
                    lConnection.Close(); 
                }
            }
        }
        public static string ReversePayment(CustomerData3 pCustomerData, int pPaymentTransactionId, decimal pAmount, string pExplanation, out int pReverseTransactionId)
        {
            pReverseTransactionId = 0;

            if (pCustomerData.BalanceInvoiceTransactionId >= pPaymentTransactionId)
            {
                return "Payment too old to be reversed";
            }

            SqlTransaction lTransaction;
            SqlConnection lConnection = new SqlConnection();
            lConnection.ConnectionString = Settings.ConnectionString;
            lConnection.Open();
            lTransaction = lConnection.BeginTransaction("ReversePayment");

            try
            {
                if (LedgerData.ReversePaymentCheck(pPaymentTransactionId) > 0)
                {
                    return "This payment has been reversed already.";
                }

               // Create a transaction entry

                pReverseTransactionId = LedgerData.ReversePayment(ref lTransaction, pCustomerData.CustomerId, -pAmount, pPaymentTransactionId, pExplanation);

                if (pReverseTransactionId == 0)
                {
                    lTransaction.Rollback("ReversePayment");
                    return "ReversePayment failed";
                }

                // Done

                lTransaction.Commit();

                return "OK";     //DistributeAllPayments(pCustomerData.CustomerId); But the next PopulateInvoice will not show the allocation anymore
                                 // since it has been deleted.

            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomerBizStatic", "ReversePayment", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                lTransaction.Rollback("ReversePayment");
                return "ReversePayment failed due to a technical error";
            }
            finally
            {
                lConnection.Close();
            }
        }

        public static string Refund(int pPaymentTransactionId, CustomerData3 pCustomerData, decimal pRefundAmount, DateTime pEffectiveDate)
        {
            // Start the transaction
            SqlTransaction lSqlTransaction;
            SqlConnection lConnection = new SqlConnection();
            lConnection.ConnectionString = Settings.ConnectionString;
            lConnection.Open();
            lSqlTransaction = lConnection.BeginTransaction("Refund");
            try
            {
                if (!LedgerData.Refund(ref lSqlTransaction, pPaymentTransactionId, pCustomerData.CustomerId, pRefundAmount, pEffectiveDate))
                {
                    lSqlTransaction.Rollback("Refund");
                    return "Error in Refund ";
                }

                lSqlTransaction.Commit();

                return "OK";    //DistributeAllPayments(pPayerId);

            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomerBizStatic", "Refund", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return "Error in Refund " + ex.Message;
            }
            finally
            {
                if (lConnection.State == System.Data.ConnectionState.Open)
                {
                    lConnection.Close();
                }
            }
        }

        public static bool WriteOffMoney(ref PaymentData.PaymentRecord pRecord, out int pTransactionId)
        {
            // Start the transaction

            SqlTransaction myTransaction;
            SqlConnection lConnection = new SqlConnection();
            lConnection.ConnectionString = Settings.ConnectionString;
            lConnection.Open();
            myTransaction = lConnection.BeginTransaction("WriteOffMoney");
            pTransactionId = 0;
            try
            {
                if (!LedgerData.WriteOffMoney(ref myTransaction, pRecord))
                {
                    myTransaction.Rollback("WriteOffMoney");
                    return false;
                }

                myTransaction.Commit();
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomerBizStatic", "WriteOffMoney", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return false;
            }
            finally
            {
                if (lConnection.State == System.Data.ConnectionState.Open)
                {
                    lConnection.Close();
                }
            }
        }

        public static string ReverseWriteOffMoney(int pTransactionId, int pInvoiceId, int pPayerId, decimal pAmount, string pExplanation)
        {
            Subs.Data.LedgerDoc2TableAdapters.TransactionsTableAdapter lTransactionAdapter = new Subs.Data.LedgerDoc2TableAdapters.TransactionsTableAdapter();
            lTransactionAdapter.AttachConnection();

            // See if the reversal has not occured already in the past

            int lNumberOfReversals = (int)lTransactionAdapter.CheckReference(pExplanation, (int)Operation.ReverseWriteOffMoney, pPayerId);

            if (lNumberOfReversals > 0)
            {
                return "This writeoff reversal has already been done in the past";
            }

            SqlConnection lConnection = new SqlConnection();
            lConnection.ConnectionString = Settings.ConnectionString;

            // Start the transaction
            SqlTransaction lSqlTransaction;
            lConnection.Open();
            lSqlTransaction = lConnection.BeginTransaction("ReverseWriteOffMoney");

            try
            {
                if (!LedgerData.ReverseWriteOffMoney(ref lSqlTransaction, pTransactionId, pInvoiceId, pPayerId, pAmount, pExplanation))
                {
                    lSqlTransaction.Rollback("ReverseWriteOffMoney");
                    return "Error in posting to the ledger. " + pPayerId.ToString();
                }


                // Done

                lSqlTransaction.Commit();
                return "OK";

            }

            catch (Exception ex)
            {
                lSqlTransaction.Rollback("ReverseWriteOffMoney");

                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomerBizStatic", "ReverseWriteOffMoney", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return ex.Message;
            }
            finally
            {
                lConnection.Close();
            }
        }


        #endregion

        #region Customer related

        public static string ValidateDuplicate(CustomerData3 pCustomerData)
        {
            // Do I have this customer already

            Subs.Data.CustomerDoc2TableAdapters.CustomerTableAdapter lCustomerAdapter = new Subs.Data.CustomerDoc2TableAdapters.CustomerTableAdapter();
            CustomerDoc2.CustomerDataTable lCustomerTable = new CustomerDoc2.CustomerDataTable();
            lCustomerAdapter.AttachConnection();

            try
            {
                if (string.IsNullOrWhiteSpace(pCustomerData.Initials)) return "Initials are compulsory";
                if (string.IsNullOrWhiteSpace(pCustomerData.Surname)) return "Surname is compulsory";
                if (string.IsNullOrWhiteSpace(pCustomerData.CellPhoneNumber)) return "Cellphone is compulsory";
                if (string.IsNullOrWhiteSpace(pCustomerData.EmailAddress)) return "EmailAddress is compulsory";

                lCustomerAdapter.Like(lCustomerTable,
                pCustomerData.Initials,
                pCustomerData.Surname,
                pCustomerData.CellPhoneNumber,
                pCustomerData.EmailAddress,
                pCustomerData.CompanyId);

                bool lKeep = false;


                foreach (Subs.Data.CustomerDoc2.CustomerRow lRow in lCustomerTable)
                {
                    char[] lInitials = pCustomerData.Initials.ToCharArray();

                    lKeep = false;

                    foreach (char lChar in lInitials)
                    {
                        if (lRow.Initials.Contains(lChar))
                        {
                            // If there is at least one overlapping initial character, this could be a duplicate
                            lKeep = true;
                        }
                    }

                    if (lKeep == false)
                    {
                        lRow.Delete();
                    }
                }

                lCustomerTable.AcceptChanges();

                if (lCustomerTable.Rows.Count > 0)
                {
                    return "You seem to be an existing customer. Please contact MIMS to clarify the issue.";
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "static CustomerBiz", "ValidateDuplicate", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return "Failed in ValidateDuplicate: " + ex.Message;
            }
        }

        public static string DestroyCustomer(ref Subs.Data.CustomerData3 pCustomerData, string pReason)
        {
            SqlConnection lConnection = new SqlConnection();
            int lCustomerId = pCustomerData.CustomerId;

            try
            {
                if (pCustomerData.Liability > 1)
                {
                    return "You cannot cancel a customer while we owe him money!";
                }

                if (pCustomerData.Liability < -1)
                {
                    return "You cannot cancel a customer while he owes us money!";
                }

                // Active subscriptions 
                if (pCustomerData.NumberOfActiveSubscriptions > 0)
                {
                    return "You cannot cancel a customer while he has active subscriptions!";
                }

                {
                    string lResult;

                    if ((lResult = pCustomerData.Destroy()) != "OK")
                    {
                        return lResult;
                    }

                    ExceptionData.WriteException(3, "Destroyed customer = " + lCustomerId.ToString(),"static CustomerBiz", "DestroyCustomer", pReason);
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomerBizStatic", "Destroy", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return "Failed due to technical error";
            }
            finally
            {
                lConnection.Close();
            }
        }


        public static string SetMediaDeliveryFlag(DeliveryAddressData2 pDeliveryAddressData)
        {
            int lCityId = DeliveryAddressStatic.GetCityId((int)pDeliveryAddressData.StreetId);

            if (gMediaCities.Contains(lCityId))
            {
                if (pDeliveryAddressData.MediaDelivery == null)
                {
                    pDeliveryAddressData.MediaDelivery = true;
                }
                // else leave it as is
            }
            else
            {
                pDeliveryAddressData.MediaDelivery = false;
            }

            string lResult;
            if ((lResult = pDeliveryAddressData.Update()) != "OK")
            {
                return lResult;
               
            }
            return "OK";
        }




        #endregion

        #region Email
        public static string SendEmail(string pFileName, string pDestination, string pSubject, string pBody)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var lClient = new HttpClient();

            try
            {
                MailRequesttype lRequest = new MailRequesttype();
                lRequest.headers = new OrderedDictionary();
                lRequest.body = new OrderedDictionary();
                lRequest.options = new OrderedDictionary();

                lRequest.headers.Add("from", "vandermerwer@mims.co.za");
                lRequest.headers.Add("to", pDestination);

                lRequest.headers.Add("subject", pSubject);
                lRequest.headers.Add("reply_to", "vandermerwer@mims.co.za");

                lRequest.body.Add("text", pBody);
                
                if (!String.IsNullOrEmpty(pFileName))
                {
                      var attachments = new OrderedDictionary();
                    attachments.Add("filename", pFileName);
                    Byte[] bytes = File.ReadAllBytes(pFileName);
                    String file = Convert.ToBase64String(bytes);
                    attachments.Add("data", file);

                    lRequest.attachments = new OrderedDictionary[1];
                    lRequest.attachments[0] = attachments;
                }

                // Setting content type.                   
                lClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var byteArray = Encoding.ASCII.GetBytes("vanderMerweR@mims.co.za:VTzyC0BG7PXZLB2mROmdQ1y27jTM9cdM_8");

                // Setting Authorization.  
                lClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));


                // Process HTTP GET/POST REST Web API  

                string jsonString = JsonSerializer.Serialize(lRequest);
                // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                string UriString = "https://arena.everlytic.net/api/2.0/trans_mails";
                var lResponse = lClient.PostAsync(UriString, content);

                if (lResponse.Result.ReasonPhrase == "Created")
                {
                    return "OK";
                }
                else
                {
                    ExceptionData.WriteException(1, "1" + " " + lResponse.Result.ReasonPhrase, "CustomerBiz static", "SendEmail", pDestination);
                    return lResponse.Result.ReasonPhrase;
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomerBiz static", "SendEmail", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return ex.Message;
            }
        }

        public static string SendSMTP(string pFileName, string pDestination, string pSubject, string pBody)
        {
            try
            {

                var lClient = new SmtpClient("smtp.everlytic.net", 25);
                lClient.UseDefaultCredentials = false;
                lClient.Credentials = new System.Net.NetworkCredential("8.vanderMerw67353", "VTzyC0BG7PXZLB2mROmdQ1y27jTM9cdM_8");

                MailAddress lFrom = new MailAddress("vanderMerweR@mims.co.za", "Mims");
                MailAddress lTo = new MailAddress(pDestination);
                MailMessage lMessage = new MailMessage(lFrom, lTo);
    
                lMessage.Subject = pSubject;
                lMessage.SubjectEncoding = System.Text.Encoding.UTF8;
                lMessage.Body = pBody;
                lMessage.BodyEncoding = System.Text.Encoding.UTF8;
                
                lClient.Send(lMessage);

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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomerBiz static", "SendSMTP", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);
                
                return ex.Message;
            }
        }

        #endregion

    }
}
