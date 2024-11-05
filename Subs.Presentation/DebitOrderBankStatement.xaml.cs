using Subs.Data;
using Subs.Business;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using static Subs.Data.PaymentData;

namespace Subs.Presentation
{
    public partial class DebitOrderBankStatement : Window
    {
        #region Globals
            
        private Subs.Data.PaymentDoc gPaymentDoc;
        private readonly Subs.Data.PaymentDocTableAdapters.DebitOrderBankStatementTableAdapter gBankStatementAdapter = new Subs.Data.PaymentDocTableAdapters.DebitOrderBankStatementTableAdapter();
        private readonly CollectionViewSource gDebitOrderBankStatementViewSource;
        private readonly List<string> NonPayLines = new List<string>();
        private readonly List<string> PayLines = new List<string>();

        private readonly Subs.Data.SBDebitOrderDocTableAdapters.SBDebitOrderTableAdapter gDebitOrderUserAdapter = new Subs.Data.SBDebitOrderDocTableAdapters.SBDebitOrderTableAdapter();
        private Subs.Data.SBDebitOrderDoc gSBDebitOrderDoc;
        private Subs.Data.SBDebitOrderDocTableAdapters.DebitOrderHistoryTableAdapter gDebitOrderHistoryAdapter = new Subs.Data.SBDebitOrderDocTableAdapters.DebitOrderHistoryTableAdapter();

        private List<DebitOrderProposal> gDebitOrderProposals = new List<DebitOrderProposal>();
        private List<DebitOrderByPayer> gDebitOrderByPayer = new List<DebitOrderByPayer>();
        

        private readonly Subs.Data.LedgerDoc2 gLedgerDoc = new LedgerDoc2();
        private readonly Regex gRegEx1 = new Regex(@"INV[\d]{3,8}"); // 3 to 8  digits too long.

        #endregion

        #region Constructor

        public DebitOrderBankStatement()
        {
            InitializeComponent();
            gBankStatementAdapter.AttachConnection();
            gDebitOrderUserAdapter.AttachConnection();
            gDebitOrderHistoryAdapter.AttachConnection();


            gPaymentDoc = (PaymentDoc)this.Resources["paymentDoc"];
            gDebitOrderBankStatementViewSource = (CollectionViewSource)this.Resources["DebitOrderBankStatementViewSource"];

           //pickerMonth.SelectedDate = DateTime.Now.AddMonths(-1);
           
        }

        #endregion

        #region Window Management

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            gPaymentDoc = ((Subs.Data.PaymentDoc)(this.FindResource("paymentDoc")));
            gSBDebitOrderDoc = ((Subs.Data.SBDebitOrderDoc)(this.FindResource("sBDebitOrderDoc")));
        }

        private bool LoadBankStatement(string FileName)
        {
            string lRunDate = "";
            StreamReader s = File.OpenText(FileName);
            try
            {
                // Process the file

                string Buffer = null;
                this.Cursor = Cursors.Wait;

                PaymentDoc.DebitOrderBankStatementRow StatementRow;

                int CurrentAllocationNo = 0;
                //myLedgerAdapter.AttachConnection();

                gPaymentDoc.DebitOrderBankStatement.Clear(); //This will prevent you from loading the data twice. It forces you to do only one at a time.

                // Read the first record to get the Run date

                Buffer = s.ReadLine();
                if (Buffer.Substring(0, 2) != "SB")
                {
                    MessageBox.Show("First record does not start with SB ");
                    return false;
                }
                else
                {
                    lRunDate = Buffer.Substring(2, 8);
                }

                // Read the rest of the file

                while ((Buffer = s.ReadLine()) != null)
                {
                    string lFirst2 = Buffer.Substring(0, 2);
                    if (lFirst2 != "SD")
                    {
                        break;  // Do notprocess the first line again.
                    }

                    //        // Capture the values of the line 

                    StatementRow = gPaymentDoc.DebitOrderBankStatement.NewDebitOrderBankStatementRow();

                    string lCustomerIdString = Buffer.Substring(138, 10).Trim();

                    // Ensure that it is a number

                    if (!int.TryParse(lCustomerIdString, out int lCustomerId))
                    {
                        MessageBox.Show("CustomerId " + lCustomerIdString + " is invalid. Check your input file!");
                        return false;
                    }
                    else
                    {
                        if (CustomerData3.Exists(lCustomerId))
                        {
                            StatementRow.CustomerId = lCustomerId;
                        }
                        else
                        {
                            MessageBox.Show("CustomerId " + lCustomerIdString + " does not exist. Check your input file!");
                            return false;
                        }

                    }
                    StatementRow.StatementNo = 0;
                    StatementRow.AllocationNo = ++CurrentAllocationNo;
                    StatementRow.BankTransactionType = "DebitOrder";
                    StatementRow.BankPaymentMethod = "DebitOrder";
                    StatementRow.Posted = false;
                    StatementRow.TransactionDate = new DateTime(Convert.ToInt32(lRunDate.Substring(0, 4)),
                                                    Convert.ToInt32(lRunDate.Substring(4, 2)),
                                                    Convert.ToInt32(lRunDate.Substring(6, 2)));
                    double WorkAmount = System.Convert.ToDouble(Buffer.Substring(71, 15)) * 0.01;
                    StatementRow.Amount = System.Convert.ToDecimal(WorkAmount);
                    StatementRow.Reference = "";
                    StatementRow.ModifiedBy = Environment.UserDomainName.ToString() + "\\" + Environment.UserName.ToString();
                    StatementRow.ModifiedOn = DateTime.Now;

                    gPaymentDoc.DebitOrderBankStatement.AddDebitOrderBankStatementRow(StatementRow);


                }   // End of while loop

                // Write the stuff to disk

                PaymentData lPaymentData = new PaymentData();

                {
                    string lResult;

                    if ((lResult = lPaymentData.UpdateDebitOrderStatements(gPaymentDoc.DebitOrderBankStatement)) != "OK")
                    {
                        MessageBox.Show(lResult);
                        return false;
                    }
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "LoadBankStatement", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Error in LoadBankStatement: " + ex.Message);
                return false;
            }

            finally
            {
                this.Cursor = Cursors.Arrow;
                s.Close();
            }
        }

        #endregion

        #region Edit Debit order user

        private void buttonLoadDOUsers(object sender, RoutedEventArgs e)
        {
            try
            {
                gDebitOrderUserAdapter.Fill(gSBDebitOrderDoc.SBDebitOrder);
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonLoadDOUsers", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Error in buttonLoadDOUsers: " + ex.Message);
            }
        }
        private void buttonSaveDOUser(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (SBDebitOrderDoc.SBDebitOrderRow lRow in gSBDebitOrderDoc.SBDebitOrder)
                {
                    if (lRow.RowState == DataRowState.Modified || lRow.RowState == DataRowState.Added)
                    {
                        lRow.ModifiedBy = Environment.UserName;
                        lRow.ModifiedOn = DateTime.Now;
                    }
                }

                gDebitOrderUserAdapter.Update(gSBDebitOrderDoc.SBDebitOrder);
                MessageBox.Show("Done");
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonSaveDOUser", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Error in buttonSaveDOUser: " + ex.Message);
            }

        }
        private void buttonExitDOUser(object sender, RoutedEventArgs e)
        {
            gSBDebitOrderDoc.SBDebitOrder.Clear();
        }
        private void buttonLoadSpecificDOUser(object sender, RoutedEventArgs e)
        {
            try
            {
                CustomerPicker3 lPicker = new CustomerPicker3();
                lPicker.ShowDialog();

                gSBDebitOrderDoc.SBDebitOrder.Clear();
                gDebitOrderUserAdapter.FillBy(gSBDebitOrderDoc.SBDebitOrder, Settings.CurrentCustomerId);

                if (gSBDebitOrderDoc.SBDebitOrder.Count() != 1)
                {
                    MessageBox.Show("The selected customer does not use our debitorders.");
                    return;
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonLoadSpecificUser", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Error in buttonLoadSpecificUser: " + ex.Message);
            }
        }
        private void buttonAddDOUser(object sender, RoutedEventArgs e)
        {
            try
            {
                CustomerPicker3 lPicker = new CustomerPicker3();
                lPicker.ShowDialog();

                gSBDebitOrderDoc.SBDebitOrder.Clear();
                SBDebitOrderDoc.SBDebitOrderRow lNewRow = gSBDebitOrderDoc.SBDebitOrder.NewSBDebitOrderRow();
                lNewRow.CustomerId = Settings.CurrentCustomerId;
                lNewRow.ModifiedBy = System.Environment.UserName;
                lNewRow.ModifiedOn = DateTime.Now;
                gSBDebitOrderDoc.SBDebitOrder.AddSBDebitOrderRow(lNewRow);
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonAddDOUser", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Error in buttonAddDOUser: " + ex.Message);
            }
        }

        #endregion

        #region ProposeDebitOrders

        private void ButtonProposeDebitOrder(object sender, RoutedEventArgs e)
        {
            DateTime lDeliveryMonth;
            try
            {
                if (!calenderDeliver.SelectedDate.HasValue)
                {
                    MessageBox.Show("You have not selected a month.");
                    return;
                }
                else
                {
                    lDeliveryMonth = (DateTime)calenderDeliver.SelectedDate;
                    if (lDeliveryMonth.Day != 1)
                    {
                        MessageBox.Show("I can work only with the first day of the month.");
                        return;
                    }
                }

                Cursor = Cursors.Wait;

                MIMSDataContext lContext = new MIMSDataContext(Settings.ConnectionString);
                gDebitOrderProposals.Clear();
                gDebitOrderProposals = lContext.MIMS_DataContext_DebitOrder_Proposal(lDeliveryMonth).ToList();
                ProposalDataGrid.ItemsSource = gDebitOrderProposals;
                MessageBox.Show("I have generated " + gDebitOrderProposals.Count.ToString() + " proposals.");

            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonProposeDebitOrder_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void buttonWriteToXML_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Save a list to the database to prevent duplicates in future.
                int lCount = 0;

                foreach (DebitOrderProposal item in gDebitOrderProposals)
                {
                    if (item.IssueId != 0)
                    {
                        // Persist in database, except when they are debt entries. So the debt entries cannot be tied to a debtor.
                        gDebitOrderHistoryAdapter.Insert(item.SubscriptionId, item.IssueId, DateTime.Now, Environment.UserName);
                        lCount++;
                    }
                }

                // OK, if this succeeded, write stuff to XML.


                string XMLFile = "c:\\Subs\\DebitOrder_" + calenderDeliver.SelectedDate.Value.Year.ToString()
                          + calenderDeliver.SelectedDate.Value.Month.ToString("0#")
                          + ".xml";
                System.IO.FileStream lOutputFile = System.IO.File.Create(XMLFile);

                System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(List<DebitOrderProposal>));

                writer.Serialize(lOutputFile, gDebitOrderProposals);
                MessageBox.Show("XML written to " + XMLFile);
                lOutputFile.Close();

            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonWriteToXML_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Error in buttonWriteToXML_Click " + ex.Message);
            }
        }

        public Stream GetResourceFileStream(string fileName)
        {
            try
            {
                Assembly currentAssembly = Assembly.GetExecutingAssembly();
                // Get all embedded resources
                string[] arrResources = currentAssembly.GetManifestResourceNames();

                foreach (string resourceName in arrResources)
                {
                    if (resourceName.Contains(fileName))
                    {
                        return currentAssembly.GetManifestResourceStream(resourceName);
                    }
                }
                return null;
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "GetResourceFileStream", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return null;
            }
        }

        private void ButtonWriteToExcel(object sender, RoutedEventArgs e)
        {
            if (calenderDeliver.SelectedDate == null)
            {
                MessageBox.Show("You have not selected a data yet.");
                return;
            }

            int lCounter = 1;
            string ExcelFile = "c:\\Subs\\FNB" + lCounter.ToString().PadLeft(4, '0') + "_" + calenderDeliver.SelectedDate.Value.Year.ToString()
            + calenderDeliver.SelectedDate.Value.Month.ToString("0#")
            + ".xlsx";
           
            this.Cursor = Cursors.Wait;

            if (File.Exists(ExcelFile))
            {
                File.Delete(ExcelFile);
            }

            var lExcelStream = GetResourceFileStream("DOTemplate");

            if (lExcelStream == null)
            {
                return;
            }

            var lFileStream = File.Create(ExcelFile);
           
            lExcelStream.Seek(0, SeekOrigin.Begin);
            lExcelStream.CopyTo(lFileStream);
          
            lExcelStream.Close();  // Close the original template
            lFileStream.Flush();
            lFileStream.Close();

            ExcelIO lExcelIO = new ExcelIO(ExcelFile);
            
            try
            {
                gDebitOrderByPayer.Clear();
                gDebitOrderByPayer = PaymentData.DebitOrder(calenderDeliver.SelectedDate.Value);

                // Allocate the rows

                //int lNumberOfRows = 0;
                //foreach (DebitOrderByPayer lProposal in gDebitOrderByPayer)
                //{
                  
                //    lNumberOfRows++;
                //}

                //// Note that you add NumberOfRows - 1
                //for (int j = 1; j < lNumberOfRows; j++)
                //{
                //    lExcelIO.AddRow("Items");
                //}


                // Initialise
                lExcelIO.PutCellDate("Datum", 1, 1, DateTime.Now);
                int i = 0;

                foreach (DebitOrderByPayer lProposal in gDebitOrderByPayer)
                {

                    lExcelIO.PutCellString("Items", i + 1, 1, lProposal.RecipientName);
                    lExcelIO.PutCellString("Items", i + 1, 2, lProposal.RecipientAccount);
                    lExcelIO.PutCellString("Items", i + 1, 3, lProposal.RecipientAccountType);
                    lExcelIO.PutCellString("Items", i + 1, 4, lProposal.BranchCode.PadLeft(6, '0'));
                    lExcelIO.PutCellString("Items", i + 1, 5, lProposal.Amount.ToString("#######0.00"));
                    lExcelIO.PutCellString("Items", i + 1, 6, lProposal.OwnReference.ToString());
                    lExcelIO.PutCellString("Items", i + 1, 7, lProposal.RecipientReference.ToString());
                    lExcelIO.PutCellString("Items", i + 1, 8, lProposal.EmailNotify);
                    lExcelIO.PutCellString("Items", i + 1, 9, lProposal.EmailAddress);
                    lExcelIO.PutCellString("Items", i + 1, 10, lProposal.EmailSubject);
                    i++;
                }


                {
                    string lResult;
                    if ((lResult = lExcelIO.Save()) != "OK")
                    {
                        throw new Exception(lResult);
                    }
                }

                MessageBox.Show("Excel written to " + ExcelFile);
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonWriteToCSV_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Error in buttonWriteTExcel " + ex.Message);
            }
            finally
            {
                lExcelIO.Close();
                this.Cursor = Cursors.Arrow;
            }
        }
        #endregion
     
 











    private bool LoadLinkServ(string FileName)
    {
        string lRunDate = "";
        StreamReader s = File.OpenText(FileName);
        try
        {
            // Process the file

            string Buffer = null;
            this.Cursor = Cursors.Wait;

            PaymentDoc.DebitOrderBankStatementRow StatementRow;

            int CurrentAllocationNo = 0;
            //myLedgerAdapter.AttachConnection();

            gPaymentDoc.DebitOrderBankStatement.Clear(); //This will prevent you from loading the data twice. It forces you to do only one at a time.

            // Read the first record to get the Run date

            Buffer = s.ReadLine();
            if (Buffer.Substring(0, 2) != "SB")
            {
                MessageBox.Show("First record does not start with SB ");
                return false;
            }
            else
            {
                lRunDate = Buffer.Substring(2, 8);
            }

            // Read the rest of the file

            while ((Buffer = s.ReadLine()) != null)
            {
                string lFirst2 = Buffer.Substring(0, 2);
                if (lFirst2 != "SD")
                {
                    break;  // Do notprocess the first line again.
                }

                //        // Capture the values of the line 

                StatementRow = gPaymentDoc.DebitOrderBankStatement.NewDebitOrderBankStatementRow();

                string lCustomerIdString = Buffer.Substring(131, 10).Trim();


                // Ensure that it is a number

                if (!int.TryParse(lCustomerIdString, out int lCustomerId))
                {
                    MessageBox.Show("CustomerId " + lCustomerIdString + " is invalid. Check your input file!");
                    return false;
                }
                else
                {
                    StatementRow.CustomerId = lCustomerId;
                }
                StatementRow.StatementNo = 0;
                StatementRow.AllocationNo = ++CurrentAllocationNo;
                StatementRow.BankTransactionType = "DebitOrder";
                StatementRow.BankPaymentMethod = "DebitOrder";
                StatementRow.TransactionDate = new DateTime(Convert.ToInt32(lRunDate.Substring(0, 4)),
                                                Convert.ToInt32(lRunDate.Substring(4, 2)),
                                                Convert.ToInt32(lRunDate.Substring(6, 2)));
                double WorkAmount = System.Convert.ToDouble(Buffer.Substring(71, 15)) * 0.01;
                StatementRow.Amount = System.Convert.ToDecimal(WorkAmount);
                StatementRow.Reference = "";
                StatementRow.ModifiedBy = Environment.UserDomainName.ToString() + "\\" + Environment.UserName.ToString();
                StatementRow.ModifiedOn = DateTime.Now;

                gPaymentDoc.DebitOrderBankStatement.AddDebitOrderBankStatementRow(StatementRow);


            }   // End of while loop

            // Write the stuff to disk

            PaymentData lPaymentData = new PaymentData();

            {
                string lResult;

                if ((lResult = lPaymentData.UpdateDebitOrderStatements(gPaymentDoc.DebitOrderBankStatement)) != "OK")
                {
                    MessageBox.Show(lResult);
                    return false;
                }
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
                ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "LoadBankStatement", "");
                CurrentException = CurrentException.InnerException;
            } while (CurrentException != null);

            MessageBox.Show("Error in LoadBankStatement: " + ex.Message);
            return false;
        }

        finally
        {
            this.Cursor = Cursors.Arrow;
            s.Close();
        }
    }

    private void buttonLoad_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Get the name of the path to the bank statement.

            System.Windows.Forms.OpenFileDialog lOpenFileDialog = new System.Windows.Forms.OpenFileDialog();

            lOpenFileDialog.InitialDirectory = "c:\\SUBS";
            lOpenFileDialog.ShowDialog();
            string FileName = lOpenFileDialog.FileName.ToString();


            if (!File.Exists(FileName))
            {
                MessageBox.Show("You have not selected a valid source file ");
                return;
            }

            if (!LoadBankStatement(FileName))
            {
                return;
            }

            MessageBox.Show("Done");
        }

        catch (Exception ex)
        {
            //Display all the exceptions

            Exception CurrentException = ex;
            int ExceptionLevel = 0;
            do
            {
                ExceptionLevel++;
                ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonLoad_Click", "");
                CurrentException = CurrentException.InnerException;
            } while (CurrentException != null);

            return;
        }
    }

    private bool DateCheck()
    {
        if (pickerMonth.SelectedDate > DateTime.Now.Date)
        {
            MessageBox.Show("I do not cater for payments in the future!");
            return false;
        }
        return true;
    }

    private void buttonSelectRange_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!DateCheck())
            {
                return;
            }

            gBankStatementAdapter.FillBy(gPaymentDoc.DebitOrderBankStatement, "All", pickerMonth.SelectedDate);
            textBalanceOverPeriod.Text = gPaymentDoc.DebitOrderBankStatement.Where(b => b.Posted == true).Sum(a => a.Amount).ToString("#########0.00");
        }

        catch (Exception ex)
        {
            //Display all the exceptions

            Exception CurrentException = ex;
            int ExceptionLevel = 0;
            do
            {
                ExceptionLevel++;
                ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonSelectRange_Click", "");
                CurrentException = CurrentException.InnerException;
            } while (CurrentException != null);

            MessageBox.Show("Error in buttonSelectRange_Click: " + ex.Message);
        }

    }

    private void buttonNotPosted_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!DateCheck())
            {
                return;
            }
            gBankStatementAdapter.FillBy(gPaymentDoc.DebitOrderBankStatement, "Outstanding", pickerMonth.SelectedDate);

            if (gPaymentDoc.DebitOrderBankStatement.Count() == 0)
            {
                MessageBox.Show("There is nothing that is not posted.");
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
                ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonNotPosted_Click", "");
                CurrentException = CurrentException.InnerException;
            } while (CurrentException != null);

            MessageBox.Show("Error in buttonNotPosted_Click: " + ex.Message);
        }
    }

    private bool ValidatePayment(ref PaymentData.PaymentRecord PaymentRecord, out string ErrorMessage)
    {

        ErrorMessage = "OK";

        try
        {
            // Insist on a CustomerId
            if (PaymentRecord.CustomerId == 0)
            {
                ErrorMessage = "No CustomerId has been supplied.";
                return true;
            }

            // Validate the rest of the stuff

            CustomerBiz.PaymentValidationResult myResult = new CustomerBiz.PaymentValidationResult();


            {
                string lResult;

                if ((lResult = CustomerBiz.ValidatePayment(ref PaymentRecord, ref myResult, ref ErrorMessage)) != "OK")
                {
                    MessageBox.Show(lResult);
                    return false;
                }
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
                ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ValidatePayment", "");
                CurrentException = CurrentException.InnerException;
            } while (CurrentException != null);

            return false;
        }
    }

    private void buttonValidate_Click(object sender, RoutedEventArgs e)
    {
        // Ensure that you validate only on the last batch of debitorders.

        DateTime lLastBatchDate = (DateTime)gBankStatementAdapter.GetLastBatchDate();

        if (gPaymentDoc.DebitOrderBankStatement[0].TransactionDate != lLastBatchDate)
        {
            MessageBox.Show("Sorry, I can validate only the last debit order batch.");
            return;
        }

        gBankStatementAdapter.Update(gPaymentDoc.DebitOrderBankStatement);
        gPaymentDoc.DebitOrderBankStatement.AcceptChanges();
        this.Cursor = Cursors.Wait;
        try
        {
            PaymentData.PaymentRecord myRecord = new PaymentData.PaymentRecord();
            string ErrorMessage = "";
            CustomerBiz.PaymentValidationResult myResult = new CustomerBiz.PaymentValidationResult();

            for (int i = 0; i < gPaymentDoc.DebitOrderBankStatement.Count; i++)
            {
                Subs.Data.PaymentDoc.DebitOrderBankStatementRow lRow = gPaymentDoc.DebitOrderBankStatement[i];

                // skip the ones that has already been validated. 
                if (!lRow.IsErrorMessageNull())
                {
                    if (lRow.ErrorMessage == "OK" || lRow.ErrorMessage == "Incorrectly deposited" || lRow.ErrorMessage == "Internal transfer" || lRow.ErrorMessage == "OK_Overridden" || lRow.Posted)
                    {
                        //If the row was validated OK once, automatically or manually, it is good enough for me. 
                        continue;
                    }
                }

                // Populate the payment record
                myRecord.Clear();
                if (lRow.IsCustomerIdNull() || lRow.CustomerId == 0)
                {
                    lRow.ErrorMessage = "I cannot do anything without a CustomerId";
                    continue;
                }

                string customer = lRow.CustomerId.ToString();
                myRecord.CustomerId = lRow.CustomerId;
                myRecord.Amount = lRow.Amount;
                myRecord.Date = lRow.TransactionDate;
                myRecord.PaymentMethod = (int)PaymentMethod.DirectDeposit;
                myRecord.ReferenceTypeId = 5; // Allocation number
                myRecord.ReferenceTypeString = "Allocation number";
                myRecord.Reference = lRow.TransactionDate.Year.ToString() + "/" + lRow.StatementNo.ToString() + "/" + lRow.AllocationNo.ToString();


                ErrorMessage = "OK";

                // Insist on a CustomerId
                if (myRecord.CustomerId == 0)
                {
                    ErrorMessage = "No CustomerId has been supplied.";

                }

                // Validate the rest of the stuff


                {
                    string lResult;

                    if ((lResult = CustomerBiz.ValidatePayment(ref myRecord, ref myResult, ref ErrorMessage)) != "OK")
                    {
                        MessageBox.Show(lResult);
                        return;
                    }
                }


                lRow.ErrorMessage = ErrorMessage;

            } // End of for loop

            // Write the stuff to disk

            gBankStatementAdapter.Update(gPaymentDoc.DebitOrderBankStatement);

            this.buttonPost.IsEnabled = true;
            MessageBox.Show("Done");
        }

        catch (Exception ex)
        {
            //Display all the exceptions

            Exception CurrentException = ex;
            int ExceptionLevel = 0;
            do
            {
                ExceptionLevel++;
                ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonValidate_Click", "");
                CurrentException = CurrentException.InnerException;
            } while (CurrentException != null);

            MessageBox.Show(ex.Message);
            return;
        }
        finally
        {
            this.Cursor = Cursors.Arrow;
        }



    }

    private void buttonPost_Click(object sender, RoutedEventArgs e)
    {
        buttonPost.IsEnabled = false;  // Prevent this button from being hit twice. Can be reset only via a new validate. 

        this.Cursor = Cursors.Wait;
        try
        {
            PaymentData.PaymentRecord lPaymentRecord = new PaymentData.PaymentRecord();
            int Submitted = 0;


            foreach (PaymentDoc.DebitOrderBankStatementRow lBankStatementRow in gPaymentDoc.DebitOrderBankStatement.Rows)
            {
                // See what is eligible for posting

                if (lBankStatementRow.Posted)
                {
                    // This one has already been done
                    continue;
                }

                if (!(lBankStatementRow.ErrorMessage == "OK" || lBankStatementRow.ErrorMessage.EndsWith("Overridden")))
                {
                    continue; // Skip this one, do not post it.
                }

                //Construct an OverallPayment object


                lPaymentRecord.CustomerId = lBankStatementRow.CustomerId;
                lPaymentRecord.Amount = lBankStatementRow.Amount; ;
                lPaymentRecord.PaymentMethod = (int)PaymentMethod.Debitorder;
                lPaymentRecord.ReferenceTypeId = 5;
                lPaymentRecord.Reference = lBankStatementRow.TransactionDate.Year.ToString() + "/"
                    + lBankStatementRow.TransactionDate.Day.ToString().PadLeft(2)
                    + lBankStatementRow.TransactionDate.Month.ToString().PadLeft(2)
                    + "/" + lBankStatementRow.AllocationNo.ToString();
                lPaymentRecord.Date = lBankStatementRow.TransactionDate;


                // Do the overall payment

                int lPaymentTransactionId = 0;

                {
                    string lResult;

                    if ((lResult = CustomerBiz.Pay(ref lPaymentRecord, out lPaymentTransactionId)) != "OK")
                    {
                        string lMessage = lResult + " CustomerId = " + lBankStatementRow.CustomerId.ToString();
                        MessageBox.Show(lMessage);
                        ExceptionData.WriteException(1, lMessage, this.ToString(), "buttonPost_Click", "");
                        return;
                    }
                    lBankStatementRow.PaymentTransactionId = lPaymentTransactionId;
                }

                lBankStatementRow.Posted = true;
                gBankStatementAdapter.Update(lBankStatementRow);

                Submitted++;


            } // End of foreach loop

            MessageBox.Show("I have submitted " + Submitted.ToString() + " payments");
        }

        catch (Exception ex)
        {
            //Display all the exceptions

            Exception CurrentException = ex;
            int ExceptionLevel = 0;
            do
            {
                ExceptionLevel++;
                ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonPost_Click", "");
                CurrentException = CurrentException.InnerException;
            } while (CurrentException != null);

            return;
        }
        finally
        {
            this.Cursor = Cursors.Arrow;
        }
    }

    private void DebitOrderBankStatementDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {

        // Force users to invoke contextmenu only from Header, becasue in this way the row is also selected. 
        Type lType = e.OriginalSource.GetType();
        if (lType.Name == "DataGridHeaderBorder")
        {
            // Invoke context menu only when mouse is in Header border. 
            e.Handled = false;
        }
        else
        {
            e.Handled = true;
        }

    }

    private void FindCustomer_Click(object sender, RoutedEventArgs e)
    {
        CustomerPicker3 lCustomerPicker = new CustomerPicker3();
        lCustomerPicker.ShowDialog();

        if (Settings.CurrentCustomerId == 0)
        {
            // The user did not select a customer
            return;
        }

        DataRowView lRowView = (DataRowView)gDebitOrderBankStatementViewSource.View.CurrentItem;
        if (lRowView != null)
        {
            PaymentDoc.DebitOrderBankStatementRow lRow = (PaymentDoc.DebitOrderBankStatementRow)lRowView.Row;
            lRow.CustomerId = Settings.CurrentCustomerId;
        }
        else
        {
            MessageBox.Show("No row has been selected");
        }
    }

    private void GoToCustomer_Click(object sender, RoutedEventArgs e)
    {
        DataRowView lRowView = (DataRowView)gDebitOrderBankStatementViewSource.View.CurrentItem;
        PaymentDoc.DebitOrderBankStatementRow lRow = (PaymentDoc.DebitOrderBankStatementRow)lRowView.Row;

        if (lRow.IsCustomerIdNull())
        {
            MessageBox.Show("Sorry, there is not customerid to go to.");
            return;
        }

        CustomerPicker3 lCustomerPicker = new CustomerPicker3();
        lCustomerPicker.SetCurrentCustomer(lRow.CustomerId);
        lCustomerPicker.ShowDialog();

    }
    private void FindCustomerAndAllocatePayment_Click(object sender, RoutedEventArgs e)
    {
        DataRowView lRowView = (DataRowView)gDebitOrderBankStatementViewSource.View.CurrentItem;
        PaymentDoc.DebitOrderBankStatementRow lRow = (PaymentDoc.DebitOrderBankStatementRow)lRowView.Row;

        CustomerPicker3 lCustomerPicker = new CustomerPicker3();
        lCustomerPicker.gCustomerPickerViewModel.PaymentAmount = lRow.Amount;
        lCustomerPicker.gCustomerPickerViewModel.PaymentMethod = (int)PaymentMethod.DirectDeposit;
        lCustomerPicker.gCustomerPickerViewModel.ReferenceTypeId = 5;
        lCustomerPicker.gCustomerPickerViewModel.PaymentReference = lRow.TransactionDate.Year.ToString() + "/"
                    + lRow.TransactionDate.Day.ToString().PadLeft(2)
                    + lRow.TransactionDate.Month.ToString().PadLeft(2)
                    + "/" + lRow.AllocationNo.ToString(); ;

        lCustomerPicker.ShowDialog();

        if (Settings.CurrentCustomerId == 0)
        {
            // The user did not select a customer
            return;
        }

        // Set the customerid and the paymentTransactionId
        lRow.CustomerId = Settings.CurrentCustomerId;
    }

    private void AcceptPayment_Click(object sender, RoutedEventArgs e)
    {
        if (Settings.Authority >= 2)
        {
            // Manually validate the row.

            DataRowView lRowView = (DataRowView)gDebitOrderBankStatementViewSource.View.CurrentItem;
            PaymentDoc.DebitOrderBankStatementRow lRow = (PaymentDoc.DebitOrderBankStatementRow)lRowView.Row;

            if (lRow.IsCustomerIdNull())
            {
                lRow.ErrorMessage = ("I cannot do anything without a CustomerId");
                return;
            }

            if (lRow.ErrorMessage != "OK")
            {
                lRow.ErrorMessage = "OK_Overridden";
            }
        }
    }

    private void MarkAsIncorrectlyDeposited_Click(object sender, RoutedEventArgs e)
    {
        if (Settings.Authority >= 2)
        {
            DataRowView lRowView = (DataRowView)gDebitOrderBankStatementViewSource.View.CurrentItem;
            PaymentDoc.DebitOrderBankStatementRow lRow = (PaymentDoc.DebitOrderBankStatementRow)lRowView.Row;
            lRow.ErrorMessage = "ncorrectly deposited";
        }
    }

    private void MarkAsInternalTransfer_Click(object sender, RoutedEventArgs e)
    {
        if (Settings.Authority >= 2)
        {
            DataRowView lRowView = (DataRowView)gDebitOrderBankStatementViewSource.View.CurrentItem;
            PaymentDoc.DebitOrderBankStatementRow lRow = (PaymentDoc.DebitOrderBankStatementRow)lRowView.Row;
            lRow.ErrorMessage = "Internal transfer";
        }
    }
      
    }

}
