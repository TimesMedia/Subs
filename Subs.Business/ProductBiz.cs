﻿using Subs.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Subs.Business
{
    public static class ProductBiz
    {
        #region Global

        //private const int gMaxExponent = 8;

        #endregion

        public static ProgressType ValidateProposal(DeliveryDoc pDeliveryDoc, BackgroundWorker pBackGroundWorker)
        {
            int CurrentSubscription = 0; // for debugging purposes
            try
            {
                ProgressType lProgress = new ProgressType();
                lProgress.Counter = 0;
                lProgress.Counter2 = 0;
                lProgress.StockShortCount = 0;

                Dictionary<int, int> lStockDictionary = new Dictionary<int, int>();

                int lStockBalance = 0;

                // Validate the proposal 
                // Initialise
                decimal CurrentLiability = 0;
                int CurrentPayer = 0;

                CustomerData3 localCustomerData; //Just to initialise the process

                foreach (Data.DeliveryDoc.DeliveryRecordRow lRow in pDeliveryDoc.DeliveryRecord.OrderBy(p => p.Payer).ToList())
                {
                    CurrentSubscription = lRow.SubscriptionId;

                    // Count
                    lProgress.Counter++;


                    // Populate the remaining stock balance if you do not have an initial value already.

                    if (!lStockDictionary.ContainsKey(lRow.IssueId))
                    {
                        if (!ProductDataStatic.CheckStockBalance(lRow.IssueId, ref lStockBalance))
                        {
                            throw new Exception("Error in Productdata.CheckStockBalance");
                        }
                        lStockDictionary.Add(lRow.IssueId, lStockBalance);
                    }


                    //Load a new payer if there is a new one

                    if (CurrentPayer != lRow.Payer)
                    {
                        localCustomerData = null; // Remove the previous customer
                        localCustomerData = new CustomerData3(lRow.Payer);

                        CurrentPayer = lRow.Payer;
                        CurrentLiability = localCustomerData.DeliverableMinusDue;
                    }

                    // Check to see if there is an InvoiceNumber
                    if (lRow.IsInvoiceIdNull() && lRow.UnitPrice > 0)
                    {
                        lProgress.Counter2++;
                        lRow.ValidationStatus = "A - missing invoice ";
                        continue;
                    }


                    // Check to see if there is a deliveryadress
                    if (lRow.DeliveryMethod == (int)DeliveryMethod.Courier)
                    {
                        if (lRow.DeliveryAddressId == 0)
                        {
                            lProgress.Counter2++;
                            lRow.ValidationStatus = "A - missing delivery address ";
                            continue;
                        }
                        else
                        {
                            // Is this a valid deliveryaddress
                            if (!DeliveryAddressStatic.Exists(lRow.DeliveryAddressId))
                            {
                                lProgress.Counter2++;
                                lRow.ValidationStatus = "A - invalid delivery address ";
                                continue;
                            }
                        }
                    }

                    // Check to see if there is enough money

                    if (lRow.UnitPrice > 0)
                    {
                        if ((CurrentLiability + 1) < (lRow.UnitPrice * lRow.UnitsPerIssue))
                        {
                            // There is not enough money to pay for the delivery
                            if (!IssueBiz.DeliverOnCredit(lRow.SubscriptionId, lRow.IssueId))
                            {
                                lProgress.Counter2++;
                                lRow.ValidationStatus = "A - Not paid and no credit.";
                                lRow.Skip = true;
                                continue;
                            }
                        }

                        //
                        CurrentLiability = CurrentLiability - (lRow.UnitsPerIssue * lRow.UnitPrice);
                        // This is only for the proposal - do not update!
                    }

                    //Check to see if there is enough stock

                    if (lStockDictionary[lRow.IssueId] < 1)
                    {
                        // Count the number of rejections
                        lProgress.Counter2++;
                        lRow.ValidationStatus = "A - There is not enough stock";

                        lProgress.StockShortCount = lProgress.StockShortCount +
                            System.Convert.ToInt32(lRow.UnitsPerIssue.ToString());
                        continue;
                    }
                    else
                    {
                        lStockDictionary[lRow.IssueId] = lStockDictionary[lRow.IssueId] - System.Convert.ToInt32(lRow.UnitsPerIssue);
                    }

                    // See if any intermediate deliveries where skipped.

                    if (IssueBiz.GetSkipped(lRow.SubscriptionId, lRow.IssueId))
                    {
                        // Count the number of rejections
                        lProgress.Counter2++;
                        lRow.ValidationStatus = "A - Some intermediate issues skipped";
                        continue;
                    }

                    // Check if it has not been delivered already
                    if (!IssueBiz.UnitsLeft(lRow.SubscriptionId, lRow.IssueId))
                    {
                        lProgress.Counter2++;
                        lRow.ValidationStatus = "A - delivered already ";
                        continue;
                    }

                    // if you got this far, you are deliverable
                    lRow.ValidationStatus = "Deliverable";

                    if (pBackGroundWorker.WorkerReportsProgress && lProgress.Counter % 5 == 0)
                    {
                        pBackGroundWorker.ReportProgress(100 * lProgress.Counter / pDeliveryDoc.DeliveryRecord.Count());
                    }
                } // End of For each loop

                pBackGroundWorker.ReportProgress(100);

                return lProgress;

            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "ProductBizStatic", "ValidateProposal", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }



        public static ProgressType PostDelivery(DeliveryDoc pDeliveryDoc, BackgroundWorker pBackGroundWorkerPost)
        {
            ProgressType lProgress = new ProgressType();
            lProgress.Counter = 0;
            lProgress.Counter2 = 0;
            lProgress.StockShortCount = 0;

            int lCurrentSubscriptionId = 0;


            try
            {
                lProgress.Counter = 0;
                lProgress.Counter2 = 0;
                lProgress.StockShortCount = 0;

                // Save the input data to XML

                pDeliveryDoc.DeliveryRecord.WriteXml(Settings.DirectoryPath + "\\Recovery\\InputToPost_ " + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xml");

                // Post each record individually

                pDeliveryDoc.DeliveryRecord.AcceptChanges(); // Work only with the remaining records.

                foreach (Data.DeliveryDoc.DeliveryRecordRow lRow in pDeliveryDoc.DeliveryRecord)
                {
                    lCurrentSubscriptionId = lRow.SubscriptionId;

                    if (lRow.IsValidationStatusNull())
                    {
                        throw new Exception("How the hell could this have happened? ValidationStatus is null");
                    }

                    if (lRow.ValidationStatus != "Deliverable")
                    {
                        throw new Exception("How the hell could this have happened? Status= " + lRow.ValidationStatus);
                    }

                    // Advance the counter
                    lProgress.Counter++;

                    if (!Deliver(lRow))
                    {
                        throw new Exception("Error in posting delivery.");
                    }

                    if (pBackGroundWorkerPost.WorkerReportsProgress && lProgress.Counter % 5 == 0)
                    {
                        pBackGroundWorkerPost.ReportProgress(100 * lProgress.Counter / pDeliveryDoc.DeliveryRecord.Count());
                    }
                }

                pBackGroundWorkerPost.ReportProgress(100);
                return lProgress;
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "ProductBizStatic", "PostDelivery",
                        "CurrentSubscriptionId = " + lCurrentSubscriptionId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }


        public static ProgressType PostDeliveryMedia(DeliveryData pDeliveryData, BackgroundWorker pBackGroundWorkerPost)
        {
            ProgressType lProgress = new ProgressType();
            lProgress.Counter = 0;
            lProgress.Counter2 = 0;
            lProgress.StockShortCount = 0;

            int lCurrentSubscriptionId = 0;


            try
            {
                lProgress.Counter = 0;
                lProgress.Counter2 = 0;
                lProgress.StockShortCount = 0;

                //// Save the input data to XML

                //pDeliveryData.DeliveryRecord.WriteXml(Settings.DirectoryPath + "\\Recovery\\InputToPost_ " + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xml");

                //// Post each record individually

                //pDeliveryData.DeliveryRecord.AcceptChanges(); // Work only with the remaining records.

                foreach (Data.DeliveryDoc.DeliveryProposalRow lRow in pDeliveryData.gDeliveryProposal)
                {
                    lCurrentSubscriptionId = lRow.SubscriptionId;

                    if (lRow.IsValidationStatusNull())
                    {
                        throw new Exception("How the hell could this have happened? ValidationStatus is null");
                    }

                    if (lRow.ValidationStatus != "Deliverable")
                    {
                        throw new Exception("How the hell could this have happened? Status= " + lRow.ValidationStatus);
                    }

                    // Advance the counter
                    lProgress.Counter++;

                    if (!DeliverMedia(lRow))
                    {
                        throw new Exception("Error in posting delivery.");
                    }

                    lRow.Post = true;

                    if (pBackGroundWorkerPost.WorkerReportsProgress && lProgress.Counter % 5 == 0)
                    {
                        pBackGroundWorkerPost.ReportProgress(100 * lProgress.Counter / pDeliveryData.gDeliveryProposal.Count());
                    }
                }

                pDeliveryData.SaveProposal();

                pBackGroundWorkerPost.ReportProgress(100);
                return lProgress;
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "ProductBizStatic", "PostDelivery",
                        "CurrentSubscriptionId = " + lCurrentSubscriptionId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }



        public static ProgressType ValidateProposal(DeliveryData pDeliveryData, BackgroundWorker pBackGroundWorker)
        {
            int CurrentSubscription = 0; // for debugging purposes
            try
            {
                ProgressType lProgress = new ProgressType();
                lProgress.Counter = 0;
                lProgress.Counter2 = 0;
                lProgress.StockShortCount = 0;

                Dictionary<int, int> lStockDictionary = new Dictionary<int, int>();

                int lStockBalance = 0;

                // Validate the proposal 
                // Initialise
                decimal CurrentLiability = 0;
                int CurrentPayer = 0;

                CustomerData3 localCustomerData; //Just to initialise the process

                foreach (Data.DeliveryDoc.DeliveryProposalRow lRow in pDeliveryData.gDeliveryProposal.OrderBy(p => p.PayerId).ToList())
                {
                    CurrentSubscription = lRow.SubscriptionId;

                    // Count
                    lProgress.Counter++;

               
                    // Populate the remaining stock balance if you do not have an initial value already.

                    if (!lStockDictionary.ContainsKey(lRow.IssueId))
                    {
                        if (!ProductDataStatic.CheckStockBalance(lRow.IssueId, ref lStockBalance))
                        {
                            throw new Exception("Error in Productdata.CheckStockBalance");
                        }
                        lStockDictionary.Add(lRow.IssueId, lStockBalance);
                    }


                    //Load a new payer if there is a new one

                    if (CurrentPayer != lRow.PayerId)
                    {
                        localCustomerData = null; // Remove the previous customer
                        localCustomerData = new CustomerData3(lRow.PayerId);

                        CurrentPayer = lRow.PayerId;
                        CurrentLiability = localCustomerData.DeliverableMinusDue;
                    }

                    // Check to see if there is an InvoiceNumber
                    if (lRow.IsInvoiceIdNull() && lRow.UnitPrice > 0)
                    {
                        lProgress.Counter2++;
                        lRow.ValidationStatus = "A - missing invoice ";
                        continue;
                    }


                    // Check to see if there is a deliveryadress
                    if (lRow.DeliveryMethod == (int)DeliveryMethod.Courier)
                    {
                        if (lRow.DeliveryAddressId == 0)
                        {
                            lProgress.Counter2++;
                            lRow.ValidationStatus = "A - missing delivery address ";
                            continue;
                        }
                        else
                        {
                            // Is this a valid deliveryaddress
                            if (!DeliveryAddressStatic.Exists(lRow.DeliveryAddressId))
                            {
                                lProgress.Counter2++;
                                lRow.ValidationStatus = "A - invalid delivery address ";
                                continue;
                            }
                        }
                    }

                    // Check to see if there is enough money

                    if (lRow.UnitPrice > 0)
                    {
                        if ((CurrentLiability + 1) < (lRow.UnitPrice * lRow.UnitsPerIssue))
                        {
                            // There is not enough money to pay for the delivery
                            if (!IssueBiz.DeliverOnCredit(lRow.SubscriptionId, lRow.IssueId))
                            {
                                lProgress.Counter2++;
                                lRow.ValidationStatus = "A - Not paid and no credit.";
                                lRow.Skip = true;
                                continue;
                            }
                        }

                        //
                        CurrentLiability = CurrentLiability - (lRow.UnitsPerIssue * lRow.UnitPrice);
                        // This is only for the proposal - do not update!
                    }

                    //Check to see if there is enough stock

                    if (lStockDictionary[lRow.IssueId] < 1)
                    {
                        // Count the number of rejections
                        lProgress.Counter2++;
                        lRow.ValidationStatus = "A - There is not enough stock";

                        lProgress.StockShortCount = lProgress.StockShortCount +
                            System.Convert.ToInt32(lRow.UnitsPerIssue.ToString());
                        continue;
                    }
                    else
                    {
                        lStockDictionary[lRow.IssueId] = lStockDictionary[lRow.IssueId] - System.Convert.ToInt32(lRow.UnitsPerIssue);
                    }

                    // See if any intermediate deliveries where skipped.

                    if (IssueBiz.GetSkipped(lRow.SubscriptionId, lRow.IssueId))
                    {
                        // Count the number of rejections
                        lProgress.Counter2++;
                        lRow.ValidationStatus = "A - Some intermediate issues skipped";
                        continue;
                    }

                    // Check if it has not been delivered already
                    if (!IssueBiz.UnitsLeft(lRow.SubscriptionId, lRow.IssueId))
                    {
                        lProgress.Counter2++;
                        lRow.ValidationStatus = "A - delivered already ";
                        continue;
                    }

                    // if you got this far, you are deliverable
                    lRow.ValidationStatus = "Deliverable";
 
                    if (pBackGroundWorker.WorkerReportsProgress && lProgress.Counter % 5 == 0)
                    {
                        pBackGroundWorker.ReportProgress(100 * lProgress.Counter / pDeliveryData.gDeliveryProposal.Count());
                    }
                } // End of For each loop

                pBackGroundWorker.ReportProgress(100);

                return lProgress;

            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "ProductBizStatic", "ValidateProposal", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }
    

        public static ProgressType PostDelivery(DeliveryDoc.DeliveryRecordDataTable pTable, BackgroundWorker pBackGroundWorkerPost)
        {
            ProgressType lProgress = new ProgressType();
            lProgress.Counter = 0;
            lProgress.Counter2 = 0;
            lProgress.StockShortCount = 0;

            int lCurrentSubscriptionId = 0;


            try
            {
                lProgress.Counter = 0;
                lProgress.Counter2 = 0;
                lProgress.StockShortCount = 0;

                // Save the input data to XML

                //pDeliveryData.gDeliveryProposal.WriteXml(Settings.DirectoryPath + "\\Recovery\\InputToPost_ " + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xml");

                // Post each record individually

                //pDeliveryData.gDeliveryProposal.AcceptChanges(); // Work only with the remaining records.
         
                foreach (Data.DeliveryDoc.DeliveryRecordRow lRow in pTable)
                {
                    lCurrentSubscriptionId = lRow.SubscriptionId;

                    if (lRow.IsValidationStatusNull())
                    {
                        throw new Exception("How the hell could this have happened? ValidationStatus is null");
                    }

                    if (lRow.ValidationStatus != "Deliverable")
                    {
                        throw new Exception("How the hell could this have happened? Status= " + lRow.ValidationStatus);
                    }

                    // Advance the counter
                    lProgress.Counter++;

                    if (!Deliver(lRow))
                    {
                        throw new Exception("Error in posting delivery.");
                    }

                    if (pBackGroundWorkerPost.WorkerReportsProgress && lProgress.Counter % 5 == 0)
                    {
                        pBackGroundWorkerPost.ReportProgress(100 * lProgress.Counter / pTable.Count());
                    }
                }

                pBackGroundWorkerPost.ReportProgress(100);
                return lProgress;
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "ProductBizStatic", "PostDelivery", 
                        "CurrentSubscriptionId = " + lCurrentSubscriptionId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }

        public static string DeliverElectronic(int pReceiverId)
        {
            try
            {
                List<MIMS_DeliveryDataStatic_DeliverableElectronicResult> lData;

                CustomerData3 lCustomer = new CustomerData3(pReceiverId);


                {
                    string lResult;

                    if ((lResult = DeliveryDataStatic.GetDeliverableElectronic(pReceiverId, out lData)) != "OK")
                    {
                        return lResult;
                    }
                }

                if (lData.Count == 0)
                {
                    return "OK";
                }

                decimal lRemainingLiability = lCustomer.DeliverableMinusDue;
  
                foreach (var lItem in lData)
                {
                    if (lRemainingLiability > lItem.UnitPrice * lItem.UnitsPerIssue)
                    {
                        // It has been paid for
                        // See if any intermediate deliveries where skipped.

                        if (IssueBiz.GetSkipped(lItem.SubscriptionId, lItem.IssueId))
                        {
                            return "Some intermediate issues skipped";
                        }

                        // Check if it has not been delivered already
                        if (!IssueBiz.UnitsLeft(lItem.SubscriptionId, lItem.IssueId))
                        {
                            continue;
                        }


                        // Deliver
                                             

                        if (!Deliver(lItem))
                        {
                            return "There has been an error with delivering electronic products to " + pReceiverId;
                        }

                        //lReport = lReport + "Delivered IssueId = " + lItem.IssueId.ToString() + " for customer =" + pReceiverId.ToString() + "\n";

                        lRemainingLiability = lRemainingLiability - (lItem.UnitPrice * lItem.UnitsPerIssue);
                    }
                    else
                    {
                        break;
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "static ProductBiz", "DeliverElectronic", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return "Failed in DeliverElectronic:  " + ex.Message;
            }

        }

        private static bool Deliver(DeliveryDoc.DeliveryRecordRow pData)
        {
            // Start the transaction

            SqlConnection lConnection = new SqlConnection(Settings.ConnectionString);
            SqlTransaction lTransaction;
            lConnection.Open();
            lTransaction = lConnection.BeginTransaction("Deliver");

            try
            {
                // Update the SubscriptionIssue rows

                if (!IssueBiz.Deliver(ref lTransaction, pData.SubscriptionId, pData.IssueId, out bool Expired, out bool Deliverable))
                {
                    lTransaction.Rollback("Deliver");
                    return false;
                }

                if (!Deliverable)
                {
                    // Return true, because it has been delivered already. 
                    // This makes the process restartable.
                    lTransaction.Rollback("Deliver");
                    return true;
                }

                // Update the ProductData

                if (!ProductDataStatic.PostDelivery(ref lTransaction, pData.IssueId, pData.UnitsPerIssue))
                {

                    lTransaction.Rollback("Deliver");
                    return false;
                }


                // Update the LedgerData
                if (!LedgerData.Deliver(ref lTransaction, pData.SubscriptionId, pData.IssueId, pData.UnitPrice, pData.UnitsPerIssue))
                {
                    lTransaction.Rollback("Deliver");
                    return false;
                }

                lTransaction.Commit();

                if (Expired)
                {
                    SubscriptionData3 lSubscriptionData = new SubscriptionData3(pData.SubscriptionId);
                    if (!SubscriptionBiz.Expire(lSubscriptionData)) { return false; }
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "ProductBizStatic", "Deliver(pData)",
                        "SubscriptionId = " + pData.SubscriptionId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                lTransaction.Rollback("Deliver");

                return false;
            }
            finally
            {
                lConnection.Close();
            }
        }

        private static bool DeliverMedia(Data.DeliveryDoc.DeliveryProposalRow pProposalRow)
        {
            // Start the transaction

            SqlConnection lConnection = new SqlConnection(Settings.ConnectionString);
            SqlTransaction lTransaction;
            lConnection.Open();
            lTransaction = lConnection.BeginTransaction("Deliver");

            try
            {
                // Update the SubscriptionIssue rows

                if (!IssueBiz.Deliver(ref lTransaction, pProposalRow.SubscriptionId, pProposalRow.IssueId, out bool Expired, out bool Deliverable))
                {
                    lTransaction.Rollback("Deliver");
                    return false;
                }

                if (!Deliverable)
                {
                    // Return true, because it has been delivered already. 
                    // This makes the process restartable.
                    lTransaction.Rollback("Deliver");
                    return true;
                }

                // Update the ProductData

                if (!ProductDataStatic.PostDelivery(ref lTransaction, pProposalRow.IssueId, pProposalRow.UnitsPerIssue))
                {

                    lTransaction.Rollback("Deliver");
                    return false;
                }


                // Update the LedgerData
                if (!LedgerData.Deliver(ref lTransaction, pProposalRow.SubscriptionId, pProposalRow.IssueId, pProposalRow.UnitPrice, pProposalRow.UnitsPerIssue))
                {
                    lTransaction.Rollback("Deliver");
                    return false;
                }

                lTransaction.Commit();

                if (Expired)
                {
                    SubscriptionData3 lSubscriptionData = new SubscriptionData3(pProposalRow.SubscriptionId);
                    if (!SubscriptionBiz.Expire(lSubscriptionData)) { return false; }
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "ProductBizStatic", "Deliver(pData)",
                        "SubscriptionId = " + pProposalRow.SubscriptionId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                lTransaction.Rollback("Deliver");

                return false;
            }
            finally
            {
                lConnection.Close();
            }
        }

        private static bool Deliver(MIMS_DeliveryDataStatic_DeliverableElectronicResult pData)
        {
            // Start the transaction

            SqlConnection lConnection = new SqlConnection(Settings.ConnectionString);
            SqlTransaction lTransaction;
            lConnection.Open();
            lTransaction = lConnection.BeginTransaction("Deliver");

            try
            {
                // Update the SubscriptionIssue rows

                if (!IssueBiz.Deliver(ref lTransaction, pData.SubscriptionId, pData.IssueId, out bool Expired, out bool Deliverable))
                {
                    lTransaction.Rollback("Deliver");
                    return false;
                }

                if (!Deliverable)
                {
                    // Return true, because it has been delivered already. 
                    // This makes the process restartable.
                    lTransaction.Rollback("Deliver");
                    return true;
                }

                // Update the ProductData

                if (!ProductDataStatic.PostDelivery(ref lTransaction, pData.IssueId, pData.UnitsPerIssue))
                {

                    lTransaction.Rollback("Deliver");
                    return false;
                }


                // Update the LedgerData
                if (!LedgerData.Deliver(ref lTransaction, pData.SubscriptionId, pData.IssueId, pData.UnitPrice, pData.UnitsPerIssue))
                {
                    lTransaction.Rollback("Deliver");
                    return false;
                }

                lTransaction.Commit();

                if (Expired)
                {
                    SubscriptionData3 lSubscriptionData = new SubscriptionData3(pData.SubscriptionId);
                    if (!SubscriptionBiz.Expire(lSubscriptionData)) { return false; }
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "ProductBizStatic", "Deliver(pData)",
                        "SubscriptionId = " + pData.SubscriptionId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                lTransaction.Rollback("Deliver");

                return false;
            }
            finally
            {
                lConnection.Close();
            }
        }


        //private static bool DeliverMedia(Data.DeliveryDoc.DeliveryProposalRow pRow)
        //{
        //    MIMS_DeliveryDataStatic_DeliverableElectronicResult lData = new MIMS_DeliveryDataStatic_DeliverableElectronicResult();
        //    lData.PayerId = pRow.PayerId;
        //    lData.SubscriptionId = pRow.SubscriptionId;
        //    lData.IssueId = pRow.IssueId;
        //    lData.UnitsPerIssue = pRow.UnitsPerIssue;
        //    lData.UnitPrice = pRow.UnitPrice;

        //    return DeliverMedia((lData); 
        //}

        public static bool SplitByDeliveryMethod(ref DeliveryData pDeliveryData, string pFile, int pDeliveryMethod, out int pNumberOfEntries)
        {
            pNumberOfEntries = 0;

            try
            {
                // Accept changes
                pDeliveryData.gDeliveryProposal.AcceptChanges();

                //Filter the data by the delivery method
                DataTable lDeliveryTable = new DataTable();
                DataView lDeliveryView = new DataView(pDeliveryData.gDeliveryProposal);

                lDeliveryView.RowFilter = "DeliveryMethod = " + pDeliveryMethod;
                lDeliveryTable = lDeliveryView.ToTable();

                //Check if there are any rows with the specified delivery method
                if (lDeliveryTable.Rows.Count > 0)
                {
                    // Save the proposal
                    lDeliveryTable.WriteXml(pFile);

                    pNumberOfEntries = lDeliveryTable.Rows.Count;
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "ProductBizStatic", "SplitByDeliveryMethod", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return false;
            }
        }

        public static bool SplitByDeliveryMethod(ref DeliveryDoc pDeliveryDoc, string pFile, int pDeliveryMethod, out int pNumberOfEntries)
        {
            pNumberOfEntries = 0;

            try
            {
                // Accept changes
                pDeliveryDoc.AcceptChanges();

                //Filter the data by the delivery method
                DataTable lDeliveryTable = new DataTable();
                DataView lDeliveryView = new DataView(pDeliveryDoc.DeliveryRecord);

                lDeliveryView.RowFilter = "DeliveryMethod = " + pDeliveryMethod;
                lDeliveryTable = lDeliveryView.ToTable();

                //Check if there are any rows with the specified delivery method
                if (lDeliveryTable.Rows.Count > 0)
                {
                    // Save the proposal
                    lDeliveryTable.WriteXml(pFile);

                    pNumberOfEntries = lDeliveryTable.Rows.Count;
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "ProductBizStatic", "SplitByDeliveryMethod", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return false;
            }
        }


        public static string Filter(bool pPayers, bool pNonPayers, DeliveryData pDeliveryData)
        {
            try
            {
                // Before you even start, check that this has been posted.

                Data.DeliveryDoc.DeliveryProposalRow lFirstRow = pDeliveryData.gDeliveryProposal[0];

                Data.SubscriptionDoc3TableAdapters.SubscriptionIssueTableAdapter lSubscriptionIssueAdapter = new Data.SubscriptionDoc3TableAdapters.SubscriptionIssueTableAdapter();
                lSubscriptionIssueAdapter.AttachConnection();

                int lUnitsLeft = (int)lSubscriptionIssueAdapter.UnitsLeft(lFirstRow.SubscriptionId, lFirstRow.IssueId);

                //if (lUnitsLeft > 0)
                //{
                //    throw new Exception("This issue has not been posted for delivery yet. Do this before generating labels.");
                //}


                // Remove inappropriate records

                if (!pPayers & !pNonPayers)
                {
                    // Leave all records
                    return "OK";
                }


                foreach (Data.DeliveryDoc.DeliveryProposalRow lRow in pDeliveryData.gDeliveryProposal)
                {
                    if (lRow.UnitPrice == 0 & pNonPayers)
                    {
                        // Leave this record to be processed
                        continue;
                    }

                    if (lRow.UnitPrice > 0 & pPayers)
                    {
                        // Leave this record to be processed
                        continue;
                    }

                    // If you get here, the record has to be disregarded as far as 
                    // label printing goes. I do not delete it here, because that disrupts the enumeration

                    lRow.ValidationStatus = "NOLABEL";

                }

                //Now delete those records
                Data.DeliveryDoc.DeliveryProposalRow[] myRows;
                myRows = (Data.DeliveryDoc.DeliveryProposalRow[])pDeliveryData.gDeliveryProposal.Select("ValidationStatus = 'NOLABEL'");

                for (int i = 0; i <= myRows.Length - 1; i++)
                {
                    myRows[i].Delete();
                }

                pDeliveryData.gDeliveryProposal.AcceptChanges();
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "ProductBizStatic", "Filter", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return ex.Message;
            }
        }


        public static string FilterOnPayment(bool pPayers, bool pNonPayers, ref DeliveryDoc pDeliveryDoc)
        {
            try
            {
                // Before you even start, check that this has been posted.

                Data.DeliveryDoc.DeliveryRecordRow lFirstRow = pDeliveryDoc.DeliveryRecord[0];

                Data.SubscriptionDoc3TableAdapters.SubscriptionIssueTableAdapter lSubscriptionIssueAdapter = new Data.SubscriptionDoc3TableAdapters.SubscriptionIssueTableAdapter();
                lSubscriptionIssueAdapter.AttachConnection();

                int lUnitsLeft = (int)lSubscriptionIssueAdapter.UnitsLeft(lFirstRow.SubscriptionId, lFirstRow.IssueId);

                if (lUnitsLeft > 0)
                {
                    throw new Exception("This issue has not been posted for delivery yet. Do this before generating labels.");
                }


                // Remove inappropriate records

                if (!pPayers & !pNonPayers)
                {
                    // Leave all records
                    return "OK";
                }


                foreach (Data.DeliveryDoc.DeliveryRecordRow lRow in pDeliveryDoc.DeliveryRecord)
                {
                    if (lRow.UnitPrice == 0 & pNonPayers)
                    {
                        // Leave this record to be processed
                        continue;
                    }

                    if (lRow.UnitPrice > 0 & pPayers)
                    {
                        // Leave this record to be processed
                        continue;
                    }

                    // If you get here, the record has to be disregarded as far as 
                    // label printing goes. I do not delete it here, because that disrupts the enumeration

                    lRow.ValidationStatus = "NOLABEL";

                }

                // Now delete those records
                Data.DeliveryDoc.DeliveryRecordRow[] myRows;
                myRows = (Data.DeliveryDoc.DeliveryRecordRow[])pDeliveryDoc.DeliveryRecord.Select("ValidationStatus = 'NOLABEL'");

                for (int i = 0; i <= myRows.Length - 1; i++)
                {
                    myRows[i].Delete();
                }

                pDeliveryDoc.DeliveryRecord.AcceptChanges();
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "ProductBizStatic", "FilterOnPayment", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return ex.Message;
            }
        }


        public static string CopyToCollectionList(DeliveryData pDeliveryData)
        {
            try
            {
                // Open the proposal
                pDeliveryData.gCollectionList.Clear();

                foreach (Data.DeliveryDoc.DeliveryProposalRow lRow in pDeliveryData.gDeliveryProposal)
                {
                    Data.DeliveryDoc.CollectionListRow lNewRow = pDeliveryData.gCollectionList.NewCollectionListRow();
                    CustomerData3 lCustomerData = new CustomerData3(lRow.ReceiverId);

                    lNewRow.SubscriptionId = lRow.SubscriptionId;
                    lNewRow.ReceiverId = lRow.ReceiverId;
                    lNewRow.Title = lCustomerData.Title;
                    lNewRow.Initials = lCustomerData.Initials;
                    lNewRow.Surname = lCustomerData.Surname;
                    lNewRow.PhoneNumber = lCustomerData.PhoneNumber;
                    lNewRow.Company = lCustomerData.CompanyName;
                     lNewRow.Description = "";
                    lNewRow.Product = lRow.IssueDescription;
                    lNewRow.UnitPrice = lRow.UnitPrice;
                    lNewRow.UnitsPerIssue = lRow.UnitsPerIssue;
                    lNewRow.Payer = lRow.PayerId;
                    lNewRow.EmailAddress = lCustomerData.EmailAddress;
                    if (!lRow.IsExpirationDateNull()) lNewRow.ExpirationDate = lRow.ExpirationDate;
                    pDeliveryData.gCollectionList.AddCollectionListRow(lNewRow);
                }

                    pDeliveryData.gCollectionList.AcceptChanges();
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "ProductBizStatic", "CopyToCollectionList", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return "Error in CopyToCollectionList";
            }
        }

        public static string CopyToCollectionList(ref DeliveryDoc pDeliveryDoc)
        {
            try
            {
                // Open the proposal
                pDeliveryDoc.CollectionList.Clear();

                foreach (Data.DeliveryDoc.DeliveryRecordRow lRow in pDeliveryDoc.DeliveryRecord)
                {
                    Data.DeliveryDoc.CollectionListRow lNewRow = pDeliveryDoc.CollectionList.NewCollectionListRow();
                    lNewRow.SubscriptionId = lRow.SubscriptionId;
                    if (!lRow.IsReceiverIdNull()) lNewRow.ReceiverId = lRow.ReceiverId;
                    if (!lRow.IsTitleNull()) lNewRow.Title = lRow.Title;
                    if (!lRow.IsInitialsNull()) lNewRow.Initials = lRow.Initials;
                    if (!lRow.IsSurnameNull()) lNewRow.Surname = lRow.Surname;
                    if (!lRow.IsPhoneNumberNull()) lNewRow.PhoneNumber = lRow.PhoneNumber;
                    if (!lRow.IsCompanyNull()) lNewRow.Company = lRow.Company;
                    if (!lRow.IsDescriptionNull()) lNewRow.Description = lRow.Description;
                    if (!lRow.IsProductNull()) lNewRow.Product = lRow.IssueDescription;
                    if (!lRow.IsUnitPriceNull()) lNewRow.UnitPrice = lRow.UnitPrice;
                    if (!lRow.IsUnitsPerIssueNull()) lNewRow.UnitsPerIssue = lRow.UnitsPerIssue;
                    if (!lRow.IsPayerNull()) lNewRow.Payer = lRow.Payer;
                    if (!lRow.IsEmailAddressNull()) lNewRow.EmailAddress = lRow.EmailAddress;
                    if (!lRow.IsExpirationDateNull()) lNewRow.ExpirationDate = lRow.ExpirationDate;
                    pDeliveryDoc.CollectionList.AddCollectionListRow(lNewRow);
                }

                pDeliveryDoc.CollectionList.AcceptChanges();
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "ProductBizStatic", "CopyToCollectionList", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return "Error in CopyToCollectionList";
            }
        }


        //public static bool GenerateRegisteredMail(string SortString, ref DeliveryDoc pDeliveryDoc)
        //{
        //    int CurrentSubscriptionId = 0;
        //    DeliveryDoc.gDeliveryProposalRow lgDeliveryProposalRow;
        //    try
        //    {
        //        // Sort the data according to Surname and Initials of the receiver.

        //        DataView lView = new DataView(pDeliveryDoc.gDeliveryProposal);
        //        lView.Sort = SortString;

        //        pDeliveryDoc.RegisteredMail.Clear();

        //        foreach (DataRowView lRow in lView)
        //        {
        //            // Count

        //            lgDeliveryProposalRow = (DeliveryDoc.gDeliveryProposalRow)lRow.Row;

        //            // For debugging purposes, remember where we are

        //            CurrentSubscriptionId = lgDeliveryProposalRow.SubscriptionId;

        //            // Create data into a single Label Row

        //            if (!DeliveryDataStatic.LoadLabelBySubscription(lgDeliveryProposalRow.SubscriptionId, ref pDeliveryDoc))
        //            { return false; }


        //            DeliveryDoc.RegisteredMailRow NewRow = pDeliveryDoc.RegisteredMail.NewRegisteredMailRow();
        //            NewRow.SubscriptionId = pDeliveryDoc.Label[0].SubscriptionID;
        //            NewRow.CustomerId = lgDeliveryProposalRow.ReceiverId;
        //            NewRow.UnitsPerIssue = lgDeliveryProposalRow.UnitsPerIssue;

        //            string Addressee = pDeliveryDoc.Label[0].IsTitleNull() ? "" : pDeliveryDoc.Label[0].Title + " ";
        //            Addressee += pDeliveryDoc.Label[0].Initials + " ";
        //            Addressee += pDeliveryDoc.Label[0].Surname;
        //            NewRow.Name = Addressee;

        //            if (!pDeliveryDoc.Label[0].IsCompanyNull())
        //            { NewRow.Company = pDeliveryDoc.Label[0].Company; }

        //            if (!pDeliveryDoc.Label[0].IsDepartmentNull())
        //            { NewRow.Department = pDeliveryDoc.Label[0].Department; }

        //            if (!pDeliveryDoc.Label[0].IsAddress1Null())
        //            { NewRow.Address1 = pDeliveryDoc.Label[0].Address1; }

        //            if (!pDeliveryDoc.Label[0].IsAddress2Null())
        //            { NewRow.Address2 = pDeliveryDoc.Label[0].Address2; }

        //            if (!pDeliveryDoc.Label[0].IsAddress3Null())
        //            { NewRow.Address3 = pDeliveryDoc.Label[0].Address3; }
        //            else
        //            {
        //                NewRow.Address3 = "-";  // It is required
        //            }

        //            if (!pDeliveryDoc.Label[0].IsAddress4Null())
        //            { NewRow.Address4 = pDeliveryDoc.Label[0].Address4; }

        //            if (!pDeliveryDoc.Label[0].IsCountryNull())
        //            { NewRow.Country = pDeliveryDoc.Label[0].Country; }

        //            if (!pDeliveryDoc.Label[0].IsAddress5Null())
        //            {
        //                NewRow.PostalCode = pDeliveryDoc.Label[0].Address5;
        //            }

        //            if (!pDeliveryDoc.Label[0].IsEMailAddressNull())
        //                NewRow.EMailAddress = pDeliveryDoc.Label[0].EMailAddress;

        //            if (!pDeliveryDoc.Label[0].IsCellPhoneNumberNull())
        //            { NewRow.CellPhoneNumber = pDeliveryDoc.Label[0].CellPhoneNumber; }

        //            if (!pDeliveryDoc.Label[0].IsPhoneNumberNull())
        //            {
        //                NewRow.PhoneNumber = pDeliveryDoc.Label[0].PhoneNumber;
        //            }

        //            if (!lgDeliveryProposalRow.IsExpirationDateNull())
        //            {
        //                NewRow.ExpirationDate = lgDeliveryProposalRow.ExpirationDate;
        //            }

        //            NewRow.IssueDescription = ProductDataStatic.GetIssueDescription(lgDeliveryProposalRow.IssueId);

        //            pDeliveryDoc.RegisteredMail.AddRegisteredMailRow(NewRow);

        //        }

        //        // Remember the results

        //        pDeliveryDoc.RegisteredMail.AcceptChanges();

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "ProductBizStatic", "GenerateRegisteredMail", "SubscriptionId = " + CurrentSubscriptionId.ToString());
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);
        //        return false;
        //    }
        //}

        public static Dictionary<int, string> GetDeliveryOptions(int pProductId)
        {
            try
            {
                Dictionary<int, string> lDictionary = new Dictionary<int, string>();

                int lBitArray = ProductDataStatic.GetDeliveryOptions(pProductId);
                int lDigits = 15;

                if (lBitArray > (int)Math.Pow(2, lDigits) - 1)
                {
                    throw new Exception("I cannot handle a number with more than " + lDigits.ToString() + " digits.");
                }

                int lKeyValue = 0;

                while (lBitArray > 0)
                {
                    lKeyValue = (int)Math.Pow(2, lDigits);

                    if (lBitArray >= lKeyValue)
                    {
                        lDictionary.Add(lKeyValue, Enum.GetName(typeof(DeliveryMethod), lKeyValue));
                        lBitArray = lBitArray - lKeyValue;
                    }
                    lDigits--;
                }

                return lDictionary;
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "static ProductBiz", "GetDeliveryOptions", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return new Dictionary<int, string>();
            }

        }

    }
}
