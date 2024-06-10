using Microsoft.Win32;
using Subs.Business;
using Subs.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Subs.Presentation
{
    public partial class Deliver : Window
    {
        #region Globals

        private Subs.Presentation.IssuePicker2 frmIssuePicker;
        private int gIssueId = 0;
        private Subs.Data.DeliveryDoc gDeliveryDoc = new DeliveryDoc();
        private readonly CollectionViewSource gDeliveryRecordViewSource;
        private readonly BackgroundWorker gBackgroundWorker;
        private readonly BackgroundWorker gBackgroundWorkerPost;
        private string gCurrentProduct = "";


        [Serializable]
        public class DeliveryItem
        {
            public string Date;   //mmddyyyy
            public string FromName = "ARENA HOLDINGS (Pty) Ltd";
            public string FromAdress1 = "Hill on Empire";
            public string FromAdress2 = "16 Empire Road";
            public string FromAdress3 = "JOHANNESBURG";
            public string FromSuburb = "PARKTOWN";
            public string FromPostalCodde = "2193";
            public string Cell;
            public decimal? Length;
            public decimal? Width;
            public decimal? Height;
            public string ServiceType = "DBC";
            public decimal? Weight;
            public string AccountNumber = "J19582";
            public string WayBillReference;

            public string ToName;
            public string BuildingComplex;
            public string Company;
            public string ToAdress1;
            public string ToAdress2;
            public string ToAdress3;
            public string ToSuburb;
            public string ToPostalCodde;
            public string AccountNumber2;
            public decimal Price;
            public string City;
            public string Country;
            public string Province;
            public string EmailAddress;
            public string Product;
            public int Pieces;

            public int CustomerId;
        }
        [Serializable]
        public class MediaSupportDeliveryItem
        {
            public string Title;
            public string CompanyName;
            public string ClientName;
            public string ClientSurname;
            public string WorkPhone;
            public string HomePhone;
            public string CellNumber;
            public string Email;
            public string ComplexNumberandName;
            public string FloorNr;
            public string StreetNr;
            public string StreetName;
            public string Suburb;
            public string City;
            public string Province;
            public string PostalCode;
            public string PublicationName;
            public int Quantity;
            public int SubscriptionNumber;
            public int CustomerId;
            public string Name;
            public int DeliveryAddressId;


        }

        [Serializable]
        public class DeliveryItemOld    //  2024/04/05
        {
            public string Account_Number = "J19582";
            public string Company;
            public string ContactPerson;
            public string MobileNumber;
            public string OfficeNumber;
            public string EmailAddress;
            public string BuildingComplex;
            public string StreetAddress;
            public string Suburb;
            public string PostalCode;
            public string International = "Yes";
            public string IsSender = "Yes";
            public string IsConsignee = "No";
            public string City;
            public string Province;
            public string Country;
            public decimal? Weight;
            public decimal? Length;
            public decimal? Width;
            public decimal? Height;
            public decimal Value;
            public string WeekendDelivery = "No";
            public int Pieces;
            public string ProductName;
            public string Importer_Exporter_Code = "21340871";
            public int CustomerId;
            public string Date;   //mmddyyyy
            public int InvoiceNumber;
        }







        public class ProcessedFile
        {
            public string FileName;
            public DateTime Datum;
        }

        [Serializable]
        public class ProcessedFiles
        {
            public List<ProcessedFile> Items = new List<ProcessedFile>();
        }

        private ProcessedFiles gProcessedFiles = new ProcessedFiles();
        private string gProcessedFileName = Settings.DirectoryPath + "\\Deliveries\\Processed.xml";
        private XmlSerializer gProcessedSerializer = new XmlSerializer(typeof(ProcessedFiles));

        private List<DeliveryItem> gDeliveryItemsRaw = new List<DeliveryItem>();
        private List<DeliveryItem> gDeliveryItems = new List<DeliveryItem>();
        private List<MediaSupportDeliveryItem> gDeliveryMediaItemsRaw = new List<MediaSupportDeliveryItem>();
        private List<MediaSupportDeliveryItem> gDeliveryMediaItems = new List<MediaSupportDeliveryItem>();

        private class PackageCounter
        {
            public int CustomerId;
            public string IssueDescription;
            public int UnitsPerIssue;
        }

        private List<PackageCounter> gPackageCounters = new List<PackageCounter>();

        [Serializable]
        public class InventoryItem
        {
            public string IssueDescription;
            public int Units;
        }

        [Serializable]
        public class DeliveriesMethod
        {
            public string Name;
            public List<InventoryItem> Items = new List<InventoryItem>();
        }

        [Serializable]
        public class Inventory
        {
            public List<DeliveriesMethod> Methods = new List<DeliveriesMethod>();
        }

        Inventory gInventory = new Inventory();


        [Serializable]
        public class SelectedFiles
        {
            public List<string> Files = new List<string>();
        }


        #endregion

        #region Constructor

        public Deliver()
        {
            InitializeComponent();
            gDeliveryRecordViewSource = (CollectionViewSource)this.Resources["deliveryRecordViewSource"];
            gBackgroundWorker = ((BackgroundWorker)this.FindResource("backgroundWorker"));
            gBackgroundWorkerPost = ((BackgroundWorker)this.FindResource("backgroundWorkerPost"));

            gProcessedFiles.Items = new List<ProcessedFile>();
        }

        #endregion

        #region Form management

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            gDeliveryDoc = ((Subs.Data.DeliveryDoc)(this.FindResource("deliveryDoc")));
        }

        #endregion

        #region Propose

        private void buttonProposal(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            gCurrentProduct = "";
            // Get an Issueid
            frmIssuePicker = new Subs.Presentation.IssuePicker2();
            frmIssuePicker.ShowDialog();

            if (frmIssuePicker.IssueWasSelected)
            {
                labelProduct.Content = frmIssuePicker.ProductNaam;
                labelIssue.Content = frmIssuePicker.IssueName;
                gIssueId = frmIssuePicker.IssueId;
            }
            else
            {
                MessageBox.Show("You have not selected an issue. Please try again.");
                return;
            }

            try
            {
                // Ok if you got this far you have a valid issueid - so you can continue

                gDeliveryDoc.Clear();

                {
                    string lResult;

                    if ((lResult = DeliveryDataStatic.Load(gIssueId, ref gDeliveryDoc)) != "OK")
                    {
                        MessageBox.Show(lResult);
                        return;
                    }
                    else
                    {
                        int lUnits = gDeliveryDoc.DeliveryRecord.Sum(p => p.UnitsPerIssue);
                        MessageBox.Show("I have generated " + gDeliveryDoc.DeliveryRecord.Count.ToString() + " proposals for " + lUnits.ToString() + " units.");
                    }
                }

                //gProposalValid = false; // Enforce validation
                gCurrentProduct = frmIssuePicker.IssueName;
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonProposal", "");
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

        private void buttonProposalMedia(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            gCurrentProduct = "";
            // Get an Issueid
            frmIssuePicker = new Subs.Presentation.IssuePicker2();
            frmIssuePicker.ShowDialog();

            if (frmIssuePicker.IssueWasSelected)
            {
                labelProduct.Content = frmIssuePicker.ProductNaam;
                labelIssue.Content = frmIssuePicker.IssueName;
                gIssueId = frmIssuePicker.IssueId;
            }
            else
            {
                MessageBox.Show("You have not selected an issue. Please try again.");
                return;
            }

            try
            {
                // Ok if you got this far you have a valid issueid - so you can continue

                gDeliveryDoc.Clear();

                {
                    string lResult;

                    if ((lResult = DeliveryDataStatic.LoadMedia(gIssueId, ref gDeliveryDoc)) != "OK")
                    {
                        MessageBox.Show(lResult);
                        return;
                    }
                    else
                    {
                        int lUnits = gDeliveryDoc.DeliveryRecord.Sum(p => p.UnitsPerIssue);
                        MessageBox.Show("I have generated " + gDeliveryDoc.DeliveryRecord.Count.ToString() + " proposals for Media for " + lUnits.ToString() + " units.");
                    }
                }
                gCurrentProduct = frmIssuePicker.IssueName;
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonProposalMedia", "");
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
        

        private void buttonProposalActive(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            try
            {
                gCurrentProduct = "";
                labelProduct.Content = "All active products";
                labelIssue.Content = "";

                {
                    string lResult;

                    gDeliveryDoc.Clear();

                    if ((lResult = DeliveryDataStatic.LoadActive(ref gDeliveryDoc)) != "OK")
                    {
                        MessageBox.Show(lResult);
                        return;
                    }
                    else
                    {
                        int lUnits = gDeliveryDoc.DeliveryRecord.Count;
                        MessageBox.Show("I have generated a proposal with " + lUnits.ToString() + " delivery records.");
                    }
                }

                gCurrentProduct = "AllActive";
                //buttonPost.IsEnabled = false;
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonProposalActive", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }

            finally { this.Cursor = Cursors.Arrow; }



        }
        #endregion

        #region Validate

        private void buttonValidate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (gBackgroundWorker.IsBusy || gBackgroundWorkerPost.IsBusy)
                {
                    return;
                }

                this.Cursor = Cursors.Wait;

                if (gDeliveryDoc.DeliveryRecord.Count == 0)
                {
                    MessageBox.Show("There is nothing to validate.");
                    return;
                }

                gDeliveryDoc.DeliveryRecord.AcceptChanges();

                gBackgroundWorker.RunWorkerAsync(gDeliveryDoc);
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

                MessageBox.Show("Error in button ValidateProposal " + ex.Message.ToString());
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            DeliveryDoc lDeliveryDoc = (DeliveryDoc)e.Argument;
            e.Result = ProductBiz.ValidateProposal(lDeliveryDoc, gBackgroundWorker);
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressType lProgress = (ProgressType)e.Result;

            if (e.Error != null)
            {
                // An error was thrown by the DoWork event handler.
                MessageBox.Show(e.Error.Message, "An error occurred in the background validation");
            }

            int Rejections = 0;
            Rejections = lProgress.Counter2;

            if (Rejections == 0)
            {
                // Save the proposal in case there are problems with the post processing.

                gDeliveryDoc.DeliveryRecord.WriteXml(Settings.DirectoryPath + "\\Recovery\\OutputFromSuccessValidation" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xml");

                int lUnits = gDeliveryDoc.DeliveryRecord.Sum(p => p.UnitsPerIssue);

                MessageBox.Show("There where " + gDeliveryDoc.DeliveryRecord.Count.ToString() + " deliverable records for " + lUnits.ToString() + " units. You may proceed to post them!");
                Cursor = Cursors.Arrow;
                buttonValidate.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("There are " + Rejections.ToString() + " invalid proposals.");
                return;
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar1.Value = e.ProgressPercentage;
        }

        private void Skip(object sender, RoutedEventArgs e)
        {
            try
            {
                // Warning

                if (MessageBoxResult.No == MessageBox.Show("Are you sure that you want to skip all these entries. To reverse this operation is cumbersome and dangerous!?",
                    "Warning", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning))
                {
                    return;
                }

                ProductDoc.IssueDataTable lIssue = new ProductDoc.IssueDataTable();
                Subs.Data.ProductDocTableAdapters.IssueTableAdapter lAdaptor = new Subs.Data.ProductDocTableAdapters.IssueTableAdapter();
                lAdaptor.AttachConnection();
                lAdaptor.FillById(lIssue, gDeliveryDoc.DeliveryRecord[0].IssueId, "IssueId");

                if (lIssue[0].StartDate > DateTime.Now)
                {

                    if (MessageBoxResult.No == MessageBox.Show("This issue starts in the future" + ".\n This skip cannot be undone. Do you want to continue?",
                      "Warning", MessageBoxButton.YesNo))
                    {
                        return;
                    }
                }

                int lCount = 0;

                this.Cursor = Cursors.Wait;
                foreach (DeliveryDoc.DeliveryRecordRow lRow in gDeliveryDoc.DeliveryRecord)
                {
                    if (lRow.Skip)
                    {
                        {
                            string lResult;

                            if ((lResult = IssueBiz.Skip(lRow.SubscriptionId, lRow.IssueId)) != "OK")
                            {
                                MessageBox.Show(lResult);
                                return;
                            }
                            else
                            {
                                lCount++;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                } // End of foreach

                MessageBox.Show("Success. You have skipped " + lCount.ToString() + " issues. Please regenerate a proposal.");
            } // End of try
            finally
            {
                gDeliveryDoc.DeliveryRecord.Clear();
                Cursor = Cursors.Arrow;
            }
        }


        private void LoadValidProposal(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog lOpenFileDialog = new OpenFileDialog();

                lOpenFileDialog.InitialDirectory = Settings.DirectoryPath + "\\Recovery\\";
                lOpenFileDialog.ShowDialog();
                string FileName = lOpenFileDialog.FileName.ToString();

                if (!File.Exists(FileName))
                {
                    MessageBox.Show("You have not selected a valid source file ");
                    return;
                }

                gDeliveryDoc.DeliveryRecord.Clear();
                gDeliveryDoc.DeliveryRecord.ReadXml(FileName);



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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "LoadValidProposal", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }

        }

        #endregion

        #region Post

        private void buttonPost_Click(object sender, RoutedEventArgs e)
        {
            if (gBackgroundWorker.IsBusy || gBackgroundWorkerPost.IsBusy)
            {
                MessageBox.Show("You are too fast for me. I have not completed the previous task yet. Request will be ignored. ");
                return;
            }

            if (gDeliveryDoc.DeliveryRecord.Where(p => p.ValidationStatus != "Deliverable").Count() > 0)
            {
                MessageBox.Show("The proposal is invalid. It cannot be posted.");
                return;
            }

            // Continue with the proposal

            gDeliveryDoc.DeliveryRecord.AcceptChanges();

            this.Cursor = Cursors.Wait;

            try
            {
                this.Cursor = Cursors.Wait;

                // In case of a crash, you can rerun the delivery from the same XML. The system
                // will not redeliver an issue if is has already been done on the previous run.
                this.Cursor = Cursors.Wait;
                gBackgroundWorkerPost.RunWorkerAsync(gDeliveryDoc);
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Post", "");
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

        private void backgroundWorkerPost_DoWork(object sender, DoWorkEventArgs e)
        {
            try

            {
                DeliveryDoc lDeliveryDoc = (DeliveryDoc)e.Argument;
                e.Result = ProductBiz.PostDelivery(lDeliveryDoc, gBackgroundWorkerPost);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "An error occurred in the background post");
                ExceptionData.WriteException(1, ex.Message + " An error was displayed to the user", this.ToString(),
                    "backgroundWorkerPost_DoWork", "");
            }
        }
        private void backgroundWorkerPost_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressType lProgress = (ProgressType)e.Result;

            if (e.Error != null)
            {
                // An error was thrown by the DoWork event handler.
                MessageBox.Show(e.Error.Message, "An error occurred in the background post");
                ExceptionData.WriteException(1, e.Error.Message + " An error occurred in the background post and displayed to user", this.ToString(),
                    "backgroundWorkerPost_RunWorkerCompleted", "");
                return;
            }

            MessageBox.Show("Posting successful.");
        }

        private void backgroundWorkerPost_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar1.Value = e.ProgressPercentage;
        }

        #endregion

        #region Format
        private void ButtonSplitByDeliveryMethod(object sender, RoutedEventArgs e)
        {
            if (gBackgroundWorker.IsBusy || gBackgroundWorkerPost.IsBusy)
            {
                MessageBox.Show("You are too fast for me. I have not completed the post task yet. Request will be ignored. ");
                return;
            }

            if (gDeliveryDoc.DeliveryRecord.Where(p => p.ValidationStatus != "Deliverable").Count() > 0)
            {
                MessageBox.Show("The proposal is invalid. It cannot proceed to deliver it.");
                return;
            }

            // Check to see if all deliveries were posted

            foreach (Data.DeliveryDoc.DeliveryRecordRow item in gDeliveryDoc.DeliveryRecord)
            {
                if (IssueBiz.UnitsLeft(item.SubscriptionId, item.IssueId))
                {
                    MessageBox.Show("Subscription = " + item.SubscriptionId.ToString() + " Issue = " + item.IssueId.ToString() + " not posted as delivered.");
                    return;
                }
            }
            try
            {
                int NumberOfEntries = 0;
                Cursor = Cursors.Wait;

                //Create a file for each delivery method

                foreach (int lKey in Enum.GetValues(typeof(DeliveryMethod)))
                {
                    // Save the proposal, e.g. in order to generate labels or collectionlists or deliverylists later on
                    string FileName = Settings.DirectoryPath + "\\Deliveries\\"
                       + Enum.GetName(typeof(DeliveryMethod), lKey)
                       + "_"
                       + gCurrentProduct
                        + "_"
                        + System.DateTime.Now.ToLongDateString()
                        + ".xml";

                    if (!ProductBiz.SplitByDeliveryMethod(ref gDeliveryDoc, FileName, lKey, out int CurrentNumberOfEntries))
                    {
                        return;
                    }

                    NumberOfEntries += CurrentNumberOfEntries;

                } // End of foreach loop

                int Mismatch = gDeliveryDoc.DeliveryRecord.Count - NumberOfEntries;

                if (Mismatch != 0)
                {
                    string Message = "Your XML files do not cover all the deliveries";
                    ExceptionData.WriteException(1, Message, this.ToString(), "GenerateXML", "Mismatched by " + Mismatch.ToString());
                    MessageBox.Show(Message);
                }
                else
                {
                    //buttonGenerateXML.IsEnabled = false;
                    MessageBox.Show("Done");
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "GenerateXML", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void FormatCourierList(object sender, RoutedEventArgs e)
        {

            if (gBackgroundWorker.IsBusy || gBackgroundWorkerPost.IsBusy)
            {
                // Validation and posting is still in progress.
                return;
            }

            this.Cursor = Cursors.Wait;
            int lCurrentReceiverId = 0;
            OpenFileDialog lFileDialog = new OpenFileDialog();
            DeliveryItem lNewDeliveryItem = new DeliveryItem();
            CustomerData3 lCustomerData;
            List<DeliveryItem> International = new List<DeliveryItem>();
            List<DeliveryItem> Economy = new List<DeliveryItem>();


            try
            {
                if (checkPayers.IsChecked == false & checkNonPayers.IsChecked == false)
                {
                    MessageBox.Show("This way you are not going to get any labels.");
                    return;
                }

                SelectProposalFiles();
                if (lFileDialog.FileNames.Count() == 0)
                {
                    MessageBox.Show("You have not selected any files.");
                    return;
                }
                if (!ValidSelection()) return;

                // Combine all the filenames into a single ADO table            
                gDeliveryDoc.DeliveryRecord.Clear();
                SelectedFiles lSelectedFiles = new SelectedFiles();
                foreach (string lSelectedFileName in lFileDialog.FileNames)
                {
                    //Append the content of all the files into the DeliveryRecord table
                    gDeliveryDoc.DeliveryRecord.ReadXml(lSelectedFileName);
                    lSelectedFiles.Files.Add(lSelectedFileName);
                    gProcessedFiles.Items.Add(new ProcessedFile() { FileName = lSelectedFileName, Datum = DateTime.Now });
                }

                string lResult = ProductBiz.Filter((bool)checkPayers.IsChecked, (bool)checkNonPayers.IsChecked, ref gDeliveryDoc);
                if (lResult != "OK")
                {
                    MessageBox.Show(lResult);
                    return;
                }

                gDeliveryDoc.DeliveryRecord.DefaultView.Sort = "ReceiverId, IssueId";

                CreateRawDeliveryList();

                ConsolidateRawDeliveryList();

                SplitDeliveryListByDeliveryMethod();

                BuildInventoryForAll();

                SerialiseResults();

                if (RegisterResults())
                {
                    MessageBox.Show("Everything succeeded");
                }
                else
                {
                    MessageBox.Show("Something went wrong with the registration of listings.");
                }


                //**************************************************************************************************************************************************************

                void SelectProposalFiles()
                {
                    // Select all the delivery proposal files that you want to process.

                    lFileDialog.InitialDirectory = Settings.DirectoryPath + "\\Deliveries";
                    lFileDialog.Multiselect = true;
                    lFileDialog.ShowDialog();
                }

                bool ValidSelection()
                {
                    // Get the details of all the files that have been processed already 
                    if (File.Exists(gProcessedFileName))
                    {
                        FileStream lProcessedStream = new FileStream(gProcessedFileName, FileMode.Open);
                        gProcessedFiles = (ProcessedFiles)gProcessedSerializer.Deserialize(lProcessedStream);
                        lProcessedStream.Close();
                    }

                    foreach (string lFileName in lFileDialog.FileNames)
                    {
                        if (!lFileName.Contains("\\Courier_"))
                        {
                            MessageBox.Show("I can accept only files of which the name starts with 'Courier'");
                            return false;
                        }

                        ProcessedFiles lHits = new ProcessedFiles();

                        lHits.Items = (List<ProcessedFile>)gProcessedFiles.Items.Where(x => x.FileName == lFileName).ToList();

                        if (lHits.Items.Count > 0)
                        {
                            StringBuilder lStringBuilder = new StringBuilder();
                            foreach (ProcessedFile item in lHits.Items)
                            {
                                lStringBuilder.Append(item.Datum.ToString() + " ");
                            }

                            if (MessageBoxResult.No == MessageBox.Show("You have already precessed this file on " + lStringBuilder
                                                                     + " Do you really want to do it again?", "Warning",
                                    MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No))
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }

                //MediaSupportTemplate



                void CreateRawDeliveryList()
                {
                    int lCurrentIssue = 0;
                    try
                    {
                        gDeliveryItemsRaw.Clear();
                        gPackageCounters.Clear();
                        foreach (DataRowView lDataRowView in gDeliveryDoc.DeliveryRecord.DefaultView)
                        {
                            DeliveryDoc.DeliveryRecordRow lRow = (DeliveryDoc.DeliveryRecordRow)lDataRowView.Row;
                            lCurrentIssue = lRow.IssueId;

                            lNewDeliveryItem = new DeliveryItem();
                            gDeliveryItemsRaw.Add(lNewDeliveryItem);
                            lCurrentReceiverId = lRow.ReceiverId;

                            if (CustomerData3.Exists(lRow.ReceiverId))
                            {
                                lCustomerData = new CustomerData3(lRow.ReceiverId);
                            }
                            else
                            {
                                MessageBox.Show("It seems as though customer " + lRow.ReceiverId.ToString() + " does not exist anymore.");
                                return;
                            }

                            lNewDeliveryItem.CustomerId = lRow.ReceiverId;
                            lNewDeliveryItem.Product = lRow.IssueDescription;

                            lNewDeliveryItem.Date = DateTime.Now.ToString("ddMMyyyy");
                            lNewDeliveryItem.ToName = lRow.Title + " " + lRow.Initials + " " + lRow.Surname;

                            if (!lRow.IsCompanyNull())
                            {
                                lNewDeliveryItem.Company = lRow.Company;
                            }

                            DeliveryAddressData2 lDeliveryAddressData = new DeliveryAddressData2(lRow.DeliveryAddressId);

                            if (lDeliveryAddressData.Building != "")
                            {
                                lNewDeliveryItem.BuildingComplex = "Building: " + lDeliveryAddressData.Building;
                                if (lDeliveryAddressData.FloorNo != "")
                                {
                                    lNewDeliveryItem.BuildingComplex = lNewDeliveryItem.BuildingComplex + " Floor: " + lDeliveryAddressData.FloorNo;
                                    if (lDeliveryAddressData.Room != "")
                                    {
                                        lNewDeliveryItem.BuildingComplex = lNewDeliveryItem.BuildingComplex + " Room: " + lDeliveryAddressData.Room;
                                    }
                                };
                            }


                            lNewDeliveryItem.ToAdress1 = lDeliveryAddressData.StreetNo + " " + lDeliveryAddressData.Street + " " + lDeliveryAddressData.StreetExtension
                                                                            + " " + lDeliveryAddressData.StreetSuffix;
                            lNewDeliveryItem.ToSuburb = lDeliveryAddressData.Suburb;
                            lNewDeliveryItem.City = lDeliveryAddressData.City;
                            lNewDeliveryItem.Province = lDeliveryAddressData.Province;

                            lNewDeliveryItem.ToPostalCodde = lDeliveryAddressData.PostCode;
                            lNewDeliveryItem.Country = lDeliveryAddressData.CountryName;


                            lNewDeliveryItem.Cell = lCustomerData.CellPhoneNumber;
                                                       

                            if (!lRow.IsWeightNull())
                            {
                                lNewDeliveryItem.Weight = lRow.Weight * lRow.UnitsPerIssue;
                            }
                            else
                            {
                                lNewDeliveryItem.Weight = 0;
                            }

                            lNewDeliveryItem.Length = lRow.IsLengthNull() ? 0 : lRow.Length;
                            lNewDeliveryItem.Width = lRow.IsWidthNull() ? 0 : lRow.Width;
                            lNewDeliveryItem.Height = lRow.IsHeightNull() ? 0 : lRow.Height;

                            lNewDeliveryItem.Price = lRow.UnitPrice * lRow.UnitsPerIssue;
                            lNewDeliveryItem.Pieces = lRow.UnitsPerIssue;
                            lNewDeliveryItem.EmailAddress = lRow.EmailAddress;

                            gPackageCounters.Add(new PackageCounter() { CustomerId = lRow.ReceiverId, IssueDescription = lRow.IssueDescription, UnitsPerIssue = lRow.UnitsPerIssue });
                        } // End of foreach loop
                    }
                    catch (Exception ex)
                    {
                        //Display all the exceptions

                        Exception CurrentException = ex;
                        int ExceptionLevel = 0;
                        do
                        {
                            ExceptionLevel++;
                            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "local CreateRawDeliveryList", "CurrentReceiverId = " + lCurrentReceiverId.ToString() + " CurrentIssue= " + lCurrentIssue.ToString());
                            CurrentException = CurrentException.InnerException;
                        } while (CurrentException != null);

                        MessageBox.Show(ex.Message);
                        throw ex;
                    }
                    finally
                    {
                        this.Cursor = Cursors.Arrow;
                    }
                }


                void ConsolidateRawDeliveryList()
                {
                    try
                    {
                        // Consolidate the raw DeliveryItems by CustomerId

                        var lCustomerGroups = gDeliveryItemsRaw.GroupBy(p => p.CustomerId);

                        foreach (IGrouping<int, DeliveryItem> lCustomerGroup in lCustomerGroups)
                        {
                            DeliveryItem lNewItem = lCustomerGroup.ElementAt(0);

                            lNewItem.Weight = lCustomerGroup.Sum(p => p.Weight);
                            lNewItem.Price = lCustomerGroup.Sum(p => p.Price);

                            lNewItem.Length = lCustomerGroup.Max(p => p.Length);
                            lNewItem.Width = lCustomerGroup.Max(p => p.Width);
                            lNewItem.Height = lCustomerGroup.Max(p => p.Height);

                            int lUnitsPerIssue = 0;
                            string lProductString = "";
                            var lProductGroups = lCustomerGroup.GroupBy(p => p.Product);
                            foreach (IGrouping<string, DeliveryItem> lProductGroup in lProductGroups)
                            {
                                lUnitsPerIssue = lProductGroup.Sum(p => p.Pieces);
                                lProductString = lProductString + lProductGroup.ElementAt(0).Product + " X " + lUnitsPerIssue.ToString() + "; ";
                            }

                            lNewItem.Product = lProductString;
                            lNewItem.Pieces = lCustomerGroup.Sum(p => p.Pieces);
                            gDeliveryItems.Add(lNewItem);
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
                            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "local ConsolidateRawDeliveryLis", "CurrentReceiverId = " + lCurrentReceiverId.ToString());
                            CurrentException = CurrentException.InnerException;
                        } while (CurrentException != null);

                        MessageBox.Show(ex.Message);
                        throw ex;
                    }
                    finally
                    {
                        this.Cursor = Cursors.Arrow;
                    }
                }

                void SplitDeliveryListByDeliveryMethod()
                {
                    try
                    {
                        foreach (DeliveryItem lItem in gDeliveryItems)
                        {
                            if (lItem.Country != "RSA")
                            {
                                International.Add(lItem);
                            }
                            else
                            {
                                Economy.Add(lItem);
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
                            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "local SplitDeliveryListByDeliveryMethod", "CurrentReceiverId = " + lCurrentReceiverId.ToString());
                            CurrentException = CurrentException.InnerException;
                        } while (CurrentException != null);

                        MessageBox.Show(ex.Message);
                        throw ex;
                    }
                    finally
                    {
                        this.Cursor = Cursors.Arrow;
                    }
                }

                void BuildInventoryForAll()
                {
                    try
                    {
                        // Build the inventory for all deliverymethods
                        gInventory.Methods.Clear();
                        BuildInventory(International, "International");
                        BuildInventory(Economy, "Economy");
                    }
                    catch (Exception ex)
                    {
                        //Display all the exceptions

                        Exception CurrentException = ex;
                        int ExceptionLevel = 0;
                        do
                        {
                            ExceptionLevel++;
                            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "local BuildInventoryForAll", "CurrentReceiverId = " + lCurrentReceiverId.ToString());
                            CurrentException = CurrentException.InnerException;
                        } while (CurrentException != null);

                        MessageBox.Show(ex.Message);
                        throw ex;
                    }
                    finally
                    {
                        this.Cursor = Cursors.Arrow;
                    }
                }


                void SerialiseResults()
                {
                    try
                    {
                        SerialiseList(International, "International");
                        SerialiseList(Economy, "Economy");

                        // Write the inventory to XML
                        string lInventoryFileName = Settings.DirectoryPath + "\\Final_CourierList_ZInventory " + DateTime.Now.ToString("yyyyMMdd") + ".xml";
                        FileStream lFileStream = new FileStream(lInventoryFileName, FileMode.Create);
                        XmlSerializer lSerializer = new XmlSerializer(typeof(Inventory));
                        lSerializer.Serialize(lFileStream, gInventory);
                        MessageBox.Show(lInventoryFileName + " successfully written to " + Settings.DirectoryPath);

                        // Write the selected files to XML
                        string lSelectionFileName = Settings.DirectoryPath + "\\Final_CourierList_ZSelectedFiles " + DateTime.Now.ToString("yyyyMMdd") + ".xml";
                        lFileStream = new FileStream(lSelectionFileName, FileMode.Create);
                        lSerializer = new XmlSerializer(typeof(SelectedFiles));
                        lSerializer.Serialize(lFileStream, lSelectedFiles);
                        MessageBox.Show(lSelectionFileName + " successfully written to " + Settings.DirectoryPath);

                        FileStream lProcessedFileStream = new FileStream(gProcessedFileName, FileMode.Create);
                        gProcessedSerializer.Serialize(lProcessedFileStream, gProcessedFiles);
                        lProcessedFileStream.Flush();
                        lProcessedFileStream.Close();
                        MessageBox.Show(gProcessedFileName + " successfully written to " + Settings.DirectoryPath);
                    }
                    catch (Exception ex)
                    {
                        //Display all the exceptions

                        Exception CurrentException = ex;
                        int ExceptionLevel = 0;
                        do
                        {
                            ExceptionLevel++;
                            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "local SerialiseResult", "CurrentReceiverId = " + lCurrentReceiverId.ToString());
                            CurrentException = CurrentException.InnerException;
                        } while (CurrentException != null);

                        MessageBox.Show(ex.Message);
                        throw ex;
                    }
                    finally
                    {
                        this.Cursor = Cursors.Arrow;
                    }
                }

                bool RegisterResults()
                {
                    try
                    {
                        foreach (DeliveryDoc.DeliveryRecordRow item in gDeliveryDoc.DeliveryRecord)
                        {
                            if (!SubscriptionData3.RegisterListDelivery(item.SubscriptionId, item.IssueId))
                            {
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
                            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "local RegisterResults", "CurrentReceiverId = " + lCurrentReceiverId.ToString());
                            CurrentException = CurrentException.InnerException;
                        } while (CurrentException != null);

                        MessageBox.Show(ex.Message);
                        throw ex;
                    }
                    finally
                    {
                        this.Cursor = Cursors.Arrow;
                    }
                }

                //**************************************************************************************************************************************************************

            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "FormatCourierList", "CurrentReceiverId = " + lCurrentReceiverId.ToString());
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


        private void SerialiseList(List<DeliveryItem> pList, string pMethod)
        {
            try
            {
                //    // Remove ProductName, it could be confusing

                //    foreach (DeliveryItem item in pList)
                //    {
                //        item.ProductName = "";
                //    }

                // Serialise to XML

                if (pList.Count == 0) return;

                // Serialise to a file stream
                MemoryStream lMemoryStream = new MemoryStream();
                XmlSerializer lSerializer = new XmlSerializer(typeof(List<DeliveryItem>));

                // Edit the document

                string lFileName = Settings.DirectoryPath + "\\Final_CourierList_" + pMethod + " " + DateTime.Now.ToString("yyyyMMdd") + ".xml";
                FileStream lFileStream = new FileStream(lFileName, FileMode.Create);
                lSerializer.Serialize(lFileStream, pList);
                lFileStream.Flush();
                lFileStream.Close();

                //// Ammend the files to point to xsd.
                //string[] lLines = File.ReadAllLines(lFileName);
                //lLines[1] = lLines[1].Replace(">", " xsi:noNamespaceSchemaLocation=\"Final_CourierList.xsd\">");
                //File.WriteAllLines(lFileName, lLines);

                MessageBox.Show(pList.Count.ToString() + " Records written to " + lFileName.ToString());
            }

            catch (Exception ex)
            {
                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "SerialiseList", "pMethod = " + pMethod);
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }


        private void BuildInventory(List<DeliveryItem> pDeliveryItem, string pMethod)
        {
            try
            {
                if (pDeliveryItem.Count == 0) return;

                gInventory.Methods.Add(new DeliveriesMethod() { Name = pMethod });
                List<PackageCounter> lPackageCounters = new List<PackageCounter>();

                // Accumulate all the relevant package counters for the associated customers.
                foreach (DeliveryItem lItem in pDeliveryItem)
                {
                    if (gPackageCounters.Where(p => p.CustomerId == lItem.CustomerId).Count() > 0)
                    {
                        lPackageCounters.AddRange(gPackageCounters.Where(p => p.CustomerId == lItem.CustomerId).ToList());
                    }
                }

                // Group by IssueDescription

                var lAnswer = lPackageCounters.GroupBy(p => p.IssueDescription, (key, values) => new { IssueDescription = key, Units = values.Sum(x => x.UnitsPerIssue) });

                foreach (var lItem2 in lAnswer)
                {
                    gInventory.Methods[gInventory.Methods.Count - 1].Items.Add(new InventoryItem() { IssueDescription = lItem2.IssueDescription, Units = lItem2.Units });
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "BuildInventory", "pMethod = " + pMethod);
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }

        private void FormatCollectionList(object sender, RoutedEventArgs e)
        {
            if (gBackgroundWorker.IsBusy || gBackgroundWorkerPost.IsBusy)
            {
                return;
            }

            this.Cursor = Cursors.Wait;
            try
            {
                if (checkPayers.IsChecked == false & checkNonPayers.IsChecked == false)
                {
                    MessageBox.Show("This way you are not going to get any labels.");
                    return;
                }
                //OpenFileDialog lFileDialog = new OpenFileDialog();

                //lFileDialog.InitialDirectory = Settings.DirectoryPath;
                //lFileDialog.ShowDialog();
                //string lFileName = lFileDialog.FileName.ToString();
                //if (!File.Exists(lFileName))

                //{
                //    MessageBox.Show("You have not selected a valid source file ");
                //    return;
                //}

                //gDeliveryDoc.Clear();
                //gDeliveryDoc.DeliveryRecord.ReadXml(lFileName);

                OpenFileDialog lFileDialog = new OpenFileDialog();

                lFileDialog.InitialDirectory = Settings.DirectoryPath + "\\Deliveries";
                lFileDialog.Multiselect = true;
                lFileDialog.ShowDialog();

                if (lFileDialog.FileNames.Count() == 0)
                {
                    MessageBox.Show("You have not selected any files.");
                    return;
                }

                foreach (string lFileName in lFileDialog.FileNames)
                {
                    if (!lFileName.Contains("Collect_"))
                    {
                        MessageBox.Show("I can accept only files of which the name starts with 'Collect_'");
                        return;
                    }
                }

                gDeliveryDoc.DeliveryRecord.Clear();

                foreach (string lFileName in lFileDialog.FileNames)
                {
                    //Append all the files into the DeliveryRecord table
                    gDeliveryDoc.DeliveryRecord.ReadXml(lFileName);
                }



                {
                    string lResult;

                    if ((lResult = ProductBiz.Filter((bool)checkPayers.IsChecked, (bool)checkNonPayers.IsChecked, ref gDeliveryDoc)) != "OK")
                    {
                        MessageBox.Show(lResult);
                        return;
                    }
                }

                //Sort by customerid

                gDeliveryDoc.DeliveryRecord.DefaultView.Sort = "ReceiverId";

                {
                    string lResult;

                    if ((lResult = ProductBiz.CopyToCollectionList(ref gDeliveryDoc)) != "OK")
                    {
                        MessageBox.Show(lResult);
                        return;
                    }
                }

                string OutputFile = Settings.DirectoryPath + "\\Final_CollectionList_" + lFileDialog.SafeFileName;

                gDeliveryDoc.CollectionList.WriteXml(OutputFile);

                MessageBox.Show(gDeliveryDoc.CollectionList.Count.ToString() + " Records written to " + OutputFile.ToString());

                OutputFile = OutputFile.Replace("xml", "xsd");
                gDeliveryDoc.CollectionList.WriteXmlSchema(OutputFile);
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "FormatCollectionList", "");
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

        private void FormatRegisteredMail(object sender, RoutedEventArgs e)
        {
            if (gBackgroundWorker.IsBusy || gBackgroundWorkerPost.IsBusy)
            {
                return;
            }
            this.Cursor = Cursors.Wait;
            try
            {
                if (checkPayers.IsChecked == false & checkNonPayers.IsChecked == false)
                {
                    MessageBox.Show("This way you are not going to get any labels.");
                    return;
                }
                //OpenFileDialog lFileDialog = new OpenFileDialog();

                //lFileDialog.InitialDirectory = Settings.DirectoryPath;
                //lFileDialog.ShowDialog();
                //string lFileName = lFileDialog.FileName.ToString();
                //if (!File.Exists(lFileName))
                //{
                //    MessageBox.Show("You have not selected a valid source file ");
                //    return;
                //}

                //gDeliveryDoc.Clear();
                //gDeliveryDoc.DeliveryRecord.ReadXml(lFileName);

                OpenFileDialog lFileDialog = new OpenFileDialog();

                lFileDialog.InitialDirectory = Settings.DirectoryPath + "\\Deliveries";
                lFileDialog.Multiselect = true;
                lFileDialog.ShowDialog();

                if (lFileDialog.FileNames.Count() == 0)
                {
                    MessageBox.Show("You have not selected any files.");
                    return;
                }

                foreach (string lFileName in lFileDialog.FileNames)
                {
                    if (!lFileName.Contains("\\RegisteredMail_"))
                    {
                        MessageBox.Show("I can accept only files of which the name starts with 'RegisteredMail_'");
                        return;
                    }
                }

                gDeliveryDoc.DeliveryRecord.Clear();

                foreach (string lFileName in lFileDialog.FileNames)
                {
                    //Append all the files into the DeliveryRecord table
                    gDeliveryDoc.DeliveryRecord.ReadXml(lFileName);
                }

                {
                    string lResult;

                    if ((lResult = ProductBiz.Filter((bool)checkPayers.IsChecked, (bool)checkNonPayers.IsChecked, ref gDeliveryDoc)) != "OK")
                    {
                        MessageBox.Show(lResult);
                        return;
                    }
                }

                // Generate the registered mail list

                //Sort by customerid

                gDeliveryDoc.DeliveryRecord.DefaultView.Sort = "ReceiverId";

                if (!ProductBiz.GenerateRegisteredMail("Surname, Initials", ref gDeliveryDoc))
                {
                    return;
                }

                string OutputFile = Settings.DirectoryPath + "\\Final_RegisteredMailList_" + lFileDialog.SafeFileName;
                gDeliveryDoc.RegisteredMail.WriteXml(OutputFile);

                MessageBox.Show(gDeliveryDoc.RegisteredMail.Count.ToString() + " Records written to " + OutputFile.ToString());

                OutputFile = OutputFile.Replace("xml", "xsd");
                gDeliveryDoc.RegisteredMail.WriteXmlSchema(OutputFile);

            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "FormatRegisteredMail", "");
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

    
        private void Click_SubscriptionTransactions(object sender, RoutedEventArgs e)
        {
            System.Data.DataRowView lRowView = (System.Data.DataRowView)gDeliveryRecordViewSource.View.CurrentItem;
            if (lRowView != null)
            {
                DeliveryDoc.DeliveryRecordRow lRecord = (DeliveryDoc.DeliveryRecordRow)lRowView.Row;
                SubscriptionPicker2 lSubscriptionPicker = new SubscriptionPicker2();
                lSubscriptionPicker.SelectById(lRecord.SubscriptionId);
                lSubscriptionPicker.ShowDialog();
            }
        }
        #endregion

        private void buttonCreateXSD(object sender, RoutedEventArgs e)
        {
            try
            {

                MessageBox.Show("Under construction");
                // Create XSD for use by Excel

                //XsdDataContractExporter exporter = new XsdDataContractExporter();
                //ExportOptions lOptions = new ExportOptions();
                //lOptions.KnownTypes 


                //exporter.Options = lOptions;

                //if (exporter.CanExport(typeof(List<DeliveryItem>)))
                //{
                //    exporter.Export(typeof(List<DeliveryItem>));

                //    XmlSchemaSet mySchemas = exporter.Schemas;

                //    XmlQualifiedName XmlNameValue = exporter.GetRootElementName(typeof(List<DeliveryItem>));
                //    string lNameSpace = XmlNameValue.Namespace;

                //    string lFileName = Settings.DirectoryPath + "\\Final_CourierList.xsd";
                //    FileStream lFileStream = new FileStream(lFileName, FileMode.CreateNew);

                //    foreach (XmlSchema schema in mySchemas.Schemas(lNameSpace))
                //    {
                //        schema.Write(lFileStream);
                //    }

                //    lFileStream.Flush();
                //    lFileStream.Close();

                //    MessageBox.Show("XSD created as: " + lFileName);
                //}
                //else
                //{
                //    MessageBox.Show("Schema cannot be exported");
                //} 
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonCreateXSD", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }
        }

        private void FormatMediaList(object sender, RoutedEventArgs e)
        {
            if (gBackgroundWorker.IsBusy || gBackgroundWorkerPost.IsBusy)
            {
                // Validation and posting is still in progress.
                return;
            }

            this.Cursor = Cursors.Wait;
            int lCurrentReceiverId = 0;
            OpenFileDialog lFileDialog = new OpenFileDialog();
            MediaSupportDeliveryItem lNewDeliveryItem = new MediaSupportDeliveryItem();
            CustomerData3 lCustomerData;
            List<MediaSupportDeliveryItem> International = new List<MediaSupportDeliveryItem>();
            List<MediaSupportDeliveryItem> Economy = new List<MediaSupportDeliveryItem>();
            try
            {
                if (checkPayers.IsChecked == false & checkNonPayers.IsChecked == false)
                {
                    MessageBox.Show("This way you are not going to get any labels.");
                    return;
                }

                SelectProposalFiles();
                if (lFileDialog.FileNames.Count() == 0)
                {
                    MessageBox.Show("You have not selected any files.");
                    return;
                }
                if (!ValidSelection()) return;

                // Combine all the filenames into a single ADO table            
                gDeliveryDoc.DeliveryRecord.Clear();
                SelectedFiles lSelectedFiles = new SelectedFiles();
                foreach (string lSelectedFileName in lFileDialog.FileNames)
                {
                    //Append the content of all the files into the DeliveryRecord table
                    gDeliveryDoc.DeliveryRecord.ReadXml(lSelectedFileName);
                    lSelectedFiles.Files.Add(lSelectedFileName);
                    gProcessedFiles.Items.Add(new ProcessedFile() { FileName = lSelectedFileName, Datum = DateTime.Now });
                }

                string lResult = ProductBiz.Filter((bool)checkPayers.IsChecked, (bool)checkNonPayers.IsChecked, ref gDeliveryDoc);
                if (lResult != "OK")
                {
                    MessageBox.Show(lResult);
                    return;
                }
              
                gDeliveryDoc.DeliveryRecord.DefaultView.Sort = "ReceiverId, IssueId";

                CreateRawMediaDeliveryList();

                //ConsolidateRawMediaDeliveryList();

                //SplitDeliveryListByDeliveryMethod(); //Its for InternationalCountries

                BuildInventoryMediaForAll();

                SerialiseMediaResults();

                if (RegisterResults())
                {
                    MessageBox.Show("Everything succeeded");
                }
                else
                {
                    MessageBox.Show("Something went wrong with the registration of listings.");
                }


                //**************************************************************************************************************************************************************

                void SelectProposalFiles()
                {
                    // Select all the delivery proposal files that you want to process.

                    lFileDialog.InitialDirectory = Settings.DirectoryPath + "\\Deliveries";
                    lFileDialog.Multiselect = true;
                    lFileDialog.ShowDialog();
                }

                bool ValidSelection()
                {
                    // Get the details of all the files that have been processed already 
                    if (File.Exists(gProcessedFileName))
                    {
                        FileStream lProcessedStream = new FileStream(gProcessedFileName, FileMode.Open);
                        gProcessedFiles = (ProcessedFiles)gProcessedSerializer.Deserialize(lProcessedStream);
                        lProcessedStream.Close();
                    }

                    foreach (string lFileName in lFileDialog.FileNames)
                    {
                        if (!lFileName.Contains("\\Media_"))
                        {
                            MessageBox.Show("I can accept only files of which the name starts with 'Media'");
                            return false;
                        }

                        ProcessedFiles lHits = new ProcessedFiles();

                        lHits.Items = (List<ProcessedFile>)gProcessedFiles.Items.Where(x => x.FileName == lFileName).ToList();

                        if (lHits.Items.Count > 0)
                        {
                            StringBuilder lStringBuilder = new StringBuilder();
                            foreach (ProcessedFile item in lHits.Items)
                            {
                                lStringBuilder.Append(item.Datum.ToString() + " ");
                            }

                            if (MessageBoxResult.No == MessageBox.Show("You have already precessed this file on " + lStringBuilder
                                                                     + " Do you really want to do it again?", "Warning",
                                    MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No))
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
                void CreateRawMediaDeliveryList()
                {
                    int lCurrentIssue = 0;
                    try
                    {
                        gDeliveryItemsRaw.Clear();
                        gPackageCounters.Clear();
                        foreach (DataRowView lDataRowView in gDeliveryDoc.DeliveryRecord.DefaultView)
                        {
                            DeliveryDoc.DeliveryRecordRow lRow = (DeliveryDoc.DeliveryRecordRow)lDataRowView.Row;
                            lCurrentIssue = lRow.IssueId;

                            lNewDeliveryItem = new MediaSupportDeliveryItem();
                            gDeliveryMediaItemsRaw.Add(lNewDeliveryItem);
                            lCurrentReceiverId = lRow.ReceiverId;

                            if (CustomerData3.Exists(lRow.ReceiverId))
                            {
                                lCustomerData = new CustomerData3(lRow.ReceiverId);
                            }
                            else
                            {
                                MessageBox.Show("It seems as though customer " + lRow.ReceiverId.ToString() + " does not exist anymore.");
                                return;
                            }

                            lNewDeliveryItem.Title = lCustomerData.Title;
                            lNewDeliveryItem.CompanyName = lCustomerData.CompanyName;
     
                            lNewDeliveryItem.ClientName = lCustomerData.FirstName;
                            lNewDeliveryItem.ClientSurname = lRow.Surname;
                            lNewDeliveryItem.WorkPhone = lCustomerData.PhoneNumber;
                            lNewDeliveryItem.HomePhone = lCustomerData.CellPhoneNumber;
                            lNewDeliveryItem.CellNumber = lCustomerData.CellPhoneNumber;
                            lNewDeliveryItem.Email = lRow.EmailAddress;

                            DeliveryAddressData2 lDeliveryAddressData = new DeliveryAddressData2(lRow.DeliveryAddressId);

                            if (lDeliveryAddressData.Building != "")
                            {
                                lNewDeliveryItem.ComplexNumberandName = "Building: " + lDeliveryAddressData.Building;
                                if (lDeliveryAddressData.FloorNo != "")
                                {
                                    lNewDeliveryItem.ComplexNumberandName = lNewDeliveryItem.StreetNr + " Floor: " + lNewDeliveryItem.FloorNr;
                                    if (lDeliveryAddressData.Room != "")
                                    {
                                        lNewDeliveryItem.ComplexNumberandName = lNewDeliveryItem.ComplexNumberandName + " Room: " + lDeliveryAddressData.Room;
                                    }
                                }    
                            }
                            lNewDeliveryItem.FloorNr = lDeliveryAddressData.FloorNo;
                            lNewDeliveryItem.StreetNr = lDeliveryAddressData.StreetNo;
                            lNewDeliveryItem.StreetName = lDeliveryAddressData.Street;


                            lNewDeliveryItem.Suburb = lDeliveryAddressData.Suburb;
                            lNewDeliveryItem.City = lDeliveryAddressData.City;
                            lNewDeliveryItem.Province = lDeliveryAddressData.Province;
                            lNewDeliveryItem.PostalCode = lDeliveryAddressData.PostCode;
                            lNewDeliveryItem.PublicationName = lRow.Product;
                            lNewDeliveryItem.Quantity = lRow.UnitsPerIssue;
                            lNewDeliveryItem.SubscriptionNumber = lRow.SubscriptionId;
                            lNewDeliveryItem.CustomerId = lRow.ReceiverId;
                            lNewDeliveryItem.Name = lRow.Title + " " + lCustomerData.FirstName + " " + lRow.Surname;
                            lNewDeliveryItem.DeliveryAddressId = lRow.DeliveryAddressId;

                            //if (!lRow.IsWeightNull())
                            //{
                            //    lNewDeliveryItem.Weight = lRow.Weight * lRow.UnitsPerIssue;
                            //}
                            //else
                            //{
                            //    lNewDeliveryItem.Weight = 0;
                            //}

                            //lNewDeliveryItem.Length = lRow.IsLengthNull() ? 0 : lRow.Length;
                            //lNewDeliveryItem.Width = lRow.IsWidthNull() ? 0 : lRow.Width;
                            //lNewDeliveryItem.Height = lRow.IsHeightNull() ? 0 : lRow.Height;

                            //lNewDeliveryItem.Price = lRow.UnitPrice * lRow.UnitsPerIssue;
                            //lNewDeliveryItem.Pieces = lRow.UnitsPerIssue;


                            //gPackageCounters.Add(new PackageCounter() { CustomerId = lRow.ReceiverId, IssueDescription = lRow.IssueDescription, UnitsPerIssue = lRow.UnitsPerIssue });
                        } // End of foreach loop
                    }
                    catch (Exception ex)
                    {
                        //Display all the exceptions

                        Exception CurrentException = ex;
                        int ExceptionLevel = 0;
                        do
                        {
                            ExceptionLevel++;
                            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "local CreateRawDeliveryList", "CurrentReceiverId = " + lCurrentReceiverId.ToString() + " CurrentIssue= " + lCurrentIssue.ToString());
                            CurrentException = CurrentException.InnerException;
                        } while (CurrentException != null);

                        MessageBox.Show(ex.Message);
                        throw ex;
                    }
                    finally
                    {
                        this.Cursor = Cursors.Arrow;
                    }
                }
              
              

             
                void BuildInventoryMediaForAll()
                {
                    try
                    {
                        // Build the inventory for all deliverymethods
                        gInventory.Methods.Clear();
                        //BuildInventory(International, "International");
                        //BuildInventory(Economy, "Economy");
                    }
                    catch (Exception ex)
                    {
                        //Display all the exceptions

                        Exception CurrentException = ex;
                        int ExceptionLevel = 0;
                        do
                        {
                            ExceptionLevel++;
                            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "local BuildInventoryForAll", "CurrentReceiverId = " + lCurrentReceiverId.ToString());
                            CurrentException = CurrentException.InnerException;
                        } while (CurrentException != null);

                        MessageBox.Show(ex.Message);
                        throw ex;
                    }
                    finally
                    {
                        this.Cursor = Cursors.Arrow;
                    }
                }
                void SerialiseMediaResults()
                {
                    try
                    {
                        //SerialiseList(International, "International");
                        //SerialiseList(Economy, "Economy");

                        // Write the inventory to XML
                        string lInventoryFileName = Settings.DirectoryPath + "\\Final_CourierList_ZInventory " + DateTime.Now.ToString("yyyyMMdd") + ".xml";
                        FileStream lFileStream = new FileStream(lInventoryFileName, FileMode.Create);
                        XmlSerializer lSerializer = new XmlSerializer(typeof(Inventory));
                        lSerializer.Serialize(lFileStream, gInventory);
                        MessageBox.Show(lInventoryFileName + " successfully written to " + Settings.DirectoryPath);

                        // Write the selected files to XML
                        string lSelectionFileName = Settings.DirectoryPath + "\\Final_CourierList_ZSelectedFiles " + DateTime.Now.ToString("yyyyMMdd") + ".xml";
                        lFileStream = new FileStream(lSelectionFileName, FileMode.Create);
                        lSerializer = new XmlSerializer(typeof(SelectedFiles));
                        lSerializer.Serialize(lFileStream, lSelectedFiles);
                        MessageBox.Show(lSelectionFileName + " successfully written to " + Settings.DirectoryPath);

                        FileStream lProcessedFileStream = new FileStream(gProcessedFileName, FileMode.Create);
                        gProcessedSerializer.Serialize(lProcessedFileStream, gProcessedFiles);
                        lProcessedFileStream.Flush();
                        lProcessedFileStream.Close();
                        MessageBox.Show(gProcessedFileName + " successfully written to " + Settings.DirectoryPath);
                    }
                    catch (Exception ex)
                    {
                        //Display all the exceptions

                        Exception CurrentException = ex;
                        int ExceptionLevel = 0;
                        do
                        {
                            ExceptionLevel++;
                            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "local SerialiseResult", "CurrentReceiverId = " + lCurrentReceiverId.ToString());
                            CurrentException = CurrentException.InnerException;
                        } while (CurrentException != null);

                        MessageBox.Show(ex.Message);
                        throw ex;
                    }
                    finally
                    {
                        this.Cursor = Cursors.Arrow;
                    }
                }
                bool RegisterResults()
                {
                    try
                    {
                        foreach (DeliveryDoc.DeliveryRecordRow item in gDeliveryDoc.DeliveryRecord)
                        {
                            if (!SubscriptionData3.RegisterListDelivery(item.SubscriptionId, item.IssueId))
                            {
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
                            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "local RegisterResults", "CurrentReceiverId = " + lCurrentReceiverId.ToString());
                            CurrentException = CurrentException.InnerException;
                        } while (CurrentException != null);

                        MessageBox.Show(ex.Message);
                        throw ex;
                    }
                    finally
                    {
                        this.Cursor = Cursors.Arrow;
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "MediaFormatCourierList", "CurrentReceiverId = " + lCurrentReceiverId.ToString());
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

        private void buttonCreateXSDMedia(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Under construction");
        }
    }
}


