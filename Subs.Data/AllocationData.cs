using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subs.Data
{

    public struct OutstandingInvoice
    {
        public int InvoiceId;
        public decimal Balance;
    }

    public class AllocationData
    {
        private List<InvoiceAndPayment> gPaymentList;
        private List<InvoiceAndPayment> gInvoiceList;


        private Queue<InvoiceAndPayment> gPaymentQueue = new Queue<InvoiceAndPayment>();
        private Queue<InvoiceAndPayment> gInvoiceQueue = new Queue<InvoiceAndPayment>();
        private List<InvoiceAndPayment> gAllocation = new List<InvoiceAndPayment>();
        private List<InvoiceAndPayment> gFinalList = new List<InvoiceAndPayment>();
        private decimal gInvoiceBalance = 0;
        private decimal gPaymentBalance = 0;


        public AllocationData(List<InvoiceAndPayment> pPayment, List<InvoiceAndPayment> pInvoice)
        {
            gPaymentList = pPayment;
            gInvoiceList = pInvoice;



            foreach (InvoiceAndPayment item in gPaymentList)
            {
                if (item.LastRow)
                { 
                    gPaymentQueue.Enqueue(item);
                }
            }


            foreach (InvoiceAndPayment item in gInvoiceList)
            {
                if(item.LastRow)
                { 
                   gInvoiceQueue.Enqueue(item); 
                }
            }
        }


  
        public List<InvoiceAndPayment> AllocatePayments()
        {
            int lCurrentPaymentTransactionId = 0;
            InvoiceAndPayment lPaymentRow = new InvoiceAndPayment();
            InvoiceAndPayment lInvoiceRow = new InvoiceAndPayment();
            bool lAbort = false;

            try
            {
                 // Create the inital records and balances

                if (!GetNextPayment())
                {
                   lAbort = true;
                }

                if (!GetNextInvoice())
                {
                    lAbort = true; 
                }
               
                InvoiceAndPayment lAllocationRow = new InvoiceAndPayment();
                lAllocationRow.Balance = 0;

                decimal lDifference = 0.0M;
                

                while (!lAbort)  // Allocation loop
                {
                    lDifference = gPaymentBalance + gInvoiceBalance;

   
                    if (Math.Abs(lDifference) < 0.01M)
                    {
                        gInvoiceBalance = -gPaymentBalance;
                        lDifference = 0;
                    }

                    if (lDifference >= 0)
                    {
                        // Put everything into this invoice by creating an allocation entry in the invoice set

                        lAllocationRow.TransactionId = lPaymentRow.TransactionId;
                        lAllocationRow.InvoiceId = lInvoiceRow.InvoiceId;
                        lAllocationRow.OperationId = (int)Operation.AllocatePaymentToInvoice;
                        lAllocationRow.Operation = Enum.GetName(typeof(Operation), lAllocationRow.OperationId);
                        lAllocationRow.Date = lPaymentRow.Date;
                        lAllocationRow.Value = gPaymentBalance;
                        lAllocationRow.Balance = gInvoiceBalance + gPaymentBalance;
                        gAllocation.Add(lAllocationRow);

                        gInvoiceBalance = gInvoiceBalance + lAllocationRow.Value;
                        gPaymentBalance = 0; 
                        lPaymentRow.Balance = 0;
                    }
                    else
                    {
                        // Put onto this Invoice as much as you can.
                        lAllocationRow.TransactionId = lPaymentRow.TransactionId;
                        lAllocationRow.InvoiceId = lInvoiceRow.InvoiceId;
                        lAllocationRow.OperationId = (int)Operation.AllocatePaymentToInvoice;
                        lAllocationRow.Operation = Enum.GetName(typeof(Operation), lAllocationRow.OperationId);
                        lAllocationRow.Date = lPaymentRow.Date;
                        lAllocationRow.Value = -gInvoiceBalance;
                        lAllocationRow.Balance = 0;
                        gAllocation.Add(lAllocationRow);

                        gInvoiceBalance = 0;

                        lPaymentRow.Balance = lPaymentRow.Balance - lAllocationRow.Value;

                        gPaymentBalance = gPaymentBalance - lAllocationRow.Value;

                    }

                    // Create new records as necessary

                    if (gInvoiceBalance < 0.01M)
                    {
                        // Get the next invoice record
                        if (!GetNextInvoice())
                        {
                            break;
                        }
                    }


                    if (gPaymentBalance > -0.01M)
                    {
    
                        if (!GetNextPayment())
                        {
                            break;
                        }

                        lCurrentPaymentTransactionId = lPaymentRow.TransactionId;
                    }
                                      
 
                    lAllocationRow = new InvoiceAndPayment(); // Create a new Allocation row


                } // End of while loop


                // Consolidate and sort

                gInvoiceList.AddRange(gAllocation);

                List<InvoiceAndPayment> gInvoiceListSorted = (List<InvoiceAndPayment>)gInvoiceList.OrderBy(p => p.InvoiceId).ThenBy(p => p.Date).ToList();



                /////////////////////////////////////////////////////////////////// Reset the LastRow flags for Payments

                foreach (InvoiceAndPayment item in gPaymentList)
                {
                    item.LastRow = false;
                    item.StatementBalance = 0;
                }

                // Create new LastRow flags

                int lCurrentPaymentId = gPaymentList[0].TransactionId;

                for (int i = 1; i < gPaymentList.Count; i++)
                {
                    if (gPaymentList[i].TransactionId != lCurrentPaymentId)
                    {
                        gPaymentList[i - 1].LastRow = true;
                    }

                    lCurrentPaymentId = gPaymentList[i].InvoiceId;
                }

                // The last one is true as well

                gPaymentList[gPaymentList.Count - 1].LastRow = true;



                /////////////////////////////////// Reset the LastRow flags for Invoices

                foreach (InvoiceAndPayment item in gInvoiceListSorted)
                {
                    item.LastRow = false;
                    item.StatementBalance = 0;
                }

                int lCurrentInvoiceId = gInvoiceListSorted[0].InvoiceId;

                for (int i = 1; i < gInvoiceListSorted.Count; i++)
                {
                    if (gInvoiceListSorted[i].InvoiceId != lCurrentInvoiceId)
                    {
                        gInvoiceListSorted[i-1].LastRow = true;
                    }

                    lCurrentInvoiceId = gInvoiceListSorted[i].InvoiceId;
                }

                // The last one is true as well

                gInvoiceListSorted[gInvoiceListSorted.Count - 1].LastRow = true;

                //Calculate the balances for invoices

                var Query = gInvoiceListSorted.GroupBy(p => p.InvoiceId);
                foreach (IGrouping<int, InvoiceAndPayment> group in Query)
                {
                   List<InvoiceAndPayment>  myInvoices =  group.ToList<InvoiceAndPayment>();
                    myInvoices[0].Balance = myInvoices[0].Value;
                    for (int i = 1; i < myInvoices.Count; i++)
                    {
                        myInvoices[i].Balance = myInvoices[i - 1].Balance + myInvoices[i].Value;
                    }
                }
   
                ///////////////////////////////////////////////////////////////////////////////////////////

                // Update the statement totals

                gFinalList.Clear();
  
                gFinalList.AddRange(gPaymentList);
                gFinalList.AddRange(gInvoiceListSorted);

                decimal lCurrentStatementBalance = 0;

                for (int i = 0; i < gFinalList.Count; i++)
                {
                    if (gFinalList[i].LastRow)
                    {
                        gFinalList[i].StatementBalance = gFinalList[i].Balance + lCurrentStatementBalance;
                    
                        lCurrentStatementBalance = gFinalList[i].StatementBalance;
                   }
                }

                //*****************************************************************************************************************

                bool GetNextInvoice()
                {
                    try 
                    { 
                        if (gInvoiceQueue.Count == 0)
                        {
                            return false;
                        }

                        // Skip the records that do not contain totals.
                        do
                        {
                            NextInvoice:
                            lInvoiceRow = gInvoiceQueue.Dequeue();
                            if (lInvoiceRow.Balance == 0)
                            {
                                if (gInvoiceQueue.Count == 0)
                                {
                                    return false;
                                }
                                else
                                {
                                    goto NextInvoice;
                                }
                            }
                        } while (!lInvoiceRow.LastRow && gInvoiceQueue.Count != 0);

                        gInvoiceBalance = gInvoiceBalance + lInvoiceRow.Balance;
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
                            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "GetNextInvoice",
                                "PaymentTransactionId = " + lCurrentPaymentTransactionId.ToString());
                            CurrentException = CurrentException.InnerException;
                        } while (CurrentException != null);

                        throw ex;
                    }

                }

                bool GetNextPayment()
                {
                    try
                    { 
                        if (gPaymentQueue.Count == 0)
                        {
                            return false;
                        }

                        // Skip the records that do not contain totals.
                        do
                        {
                            NextPayment:
                            lPaymentRow = gPaymentQueue.Dequeue();
                            if (lPaymentRow.Balance == 0)
                            {
                                if (gPaymentQueue.Count == 0)
                                {
                                    return false;
                                }
                                else
                                { 
                                    goto NextPayment;
                                }
                            }
                        } while (!lPaymentRow.LastRow && gPaymentQueue.Count != 0);

                        gPaymentBalance = gPaymentBalance + lPaymentRow.Balance;
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
                            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "GetNextPayment",
                                "PaymentTransactionId = " + lCurrentPaymentTransactionId.ToString());
                            CurrentException = CurrentException.InnerException;
                        } while (CurrentException != null);

                        throw ex;
                    }
                }

                //*********************************************************************************************************


                return gFinalList;


           

        }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "AllocatePayments", 
                        "PaymentTransactionId = " + lCurrentPaymentTransactionId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        } 
    }
}

 