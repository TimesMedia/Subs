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
    /// <summary>
    /// Interaction logic for Deliver2.xaml
    /// </summary>
    public partial class Deliver2 : Window
    {
        #region Globals

        private Subs.Presentation.IssuePicker2 frmIssuePicker;
        private int gIssueId = 0;
        private Subs.Data.DeliveryDoc gDeliveryDoc = new DeliveryDoc();
        private DeliveryData gDeliveryData = new DeliveryData();
        private readonly CollectionViewSource gDeliveryProposalViewSource;
        private readonly BackgroundWorker gBackgroundWorker;
        private readonly BackgroundWorker gBackgroundWorkerPost;
        private string gCurrentProduct = "";


            [Serializable]
        public class MediaDeliveryItem
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
            public int UnitsPerissue;
            public string DeliveryMethod;
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

        //private List<SkynetDeliveryItem> gSkynetDeliveryItemsRaw = new List<SkynetDeliveryItem>();
        //private List<SkynetDeliveryItem> gSkynetDeliveryItems = new List<SkynetDeliveryItem>();
        private List<MediaDeliveryItem> gMediaDeliveryItemsRaw = new List<MediaDeliveryItem>();
        private List<MediaDeliveryItem> gDeliveryMediaItems = new List<MediaDeliveryItem>();

        //private class PackageCounter
        //{
        //    public int CustomerId { get; set;}
        //    public string IssueDescription;
        //    public int UnitsPerIssue;
        //}

        //private List<PackageCounter> gPackageCounters = new List<PackageCounter>();

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

        public Deliver2()
        {
            InitializeComponent();
            gDeliveryProposalViewSource = (CollectionViewSource)this.Resources["deliveryProposalViewSource"];
            gDeliveryProposalViewSource.Source = gDeliveryData.gDeliveryProposal;
            gBackgroundWorker = ((BackgroundWorker)this.FindResource("backgroundWorker"));
            gBackgroundWorkerPost = ((BackgroundWorker)this.FindResource("backgroundWorkerPost"));
            gProcessedFiles.Items = new List<ProcessedFile>();
            pickerStartDate.SelectedDate = DateTime.Today;
            DateTime lDate = DateTime.Today;
            pickerEndDate.SelectedDate = lDate.AddDays(1);
        }

        #endregion

        #region Proposals

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
                gDeliveryData.Propose(gIssueId, "Media");

                gDeliveryData.SaveProposal();

                int lUnits = gDeliveryData.gDeliveryProposal.Sum(p => p.UnitsPerIssue);
                MessageBox.Show("I have generated " + gDeliveryData.gDeliveryProposal.Count.ToString() + " proposals for Media for " + lUnits.ToString() + " units.");
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonProposeMedia", "");
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

        //private void buttonProposal(object sender, RoutedEventArgs e)
        //{
        //    Cursor = Cursors.Wait;
        //    gCurrentProduct = "";
        //    // Get an Issueid
        //    frmIssuePicker = new Subs.Presentation.IssuePicker2();
        //    frmIssuePicker.ShowDialog();

        //    if (frmIssuePicker.IssueWasSelected)
        //    {
        //        labelProduct.Content = frmIssuePicker.ProductNaam;
        //        labelIssue.Content = frmIssuePicker.IssueName;
        //        gIssueId = frmIssuePicker.IssueId;
        //    }
        //    else
        //    {
        //        MessageBox.Show("You have not selected an issue. Please try again.");
        //        return;
        //    }

        //    try
        //    {
        //        gDeliveryData.Propose(gIssueId);

        //        int lUnits = gDeliveryData.gDeliveryProposal.Sum(p => p.UnitsPerIssue);
        //        MessageBox.Show("I have generated " + gDeliveryData.gDeliveryProposal.Count.ToString() + " proposals for Skynet for " + lUnits.ToString() + " units.");
        //        gCurrentProduct = frmIssuePicker.IssueName;
        //    }

        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonProposeSkynet", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        MessageBox.Show(ex.Message);
        //        return;
        //    }

        //    finally
        //    {
        //        this.Cursor = Cursors.Arrow;
        //    }
        //}

        //private void buttonProposalActive(object sender, RoutedEventArgs e)
        //{
        //    this.Cursor = Cursors.Wait;
        //    try
        //    {
        //        gCurrentProduct = "";
        //        labelProduct.Content = "All active products";
        //        labelIssue.Content = "";

        //        foreach(int lIssueId in ProductDataStatic.CurrentIssues())
        //        { 
                
                
        //        }





        //        {
        //            string lResult;

        //            gDeliveryDoc.Clear();

        //            if ((lResult = DeliveryDataStatic.LoadActive(ref gDeliveryDoc)) != "OK")
        //            {
        //                MessageBox.Show(lResult);
        //                return;
        //            }
        //            else
        //            {
        //                int lUnits = gDeliveryData.gDeliveryProposal.Count;
        //                MessageBox.Show("I have generated a proposal with " + lUnits.ToString() + " delivery records.");
        //            }
        //        }

        //        gCurrentProduct = "AllActive";
        //        //buttonPost.IsEnabled = false;
        //    }

        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonProposalActive", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        MessageBox.Show(ex.Message);
        //    }

        //    finally { this.Cursor = Cursors.Arrow; }

        //}
        #endregion


        #region Manage proposals


        private void buttonLoadProposal(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
           
            try
            {
                if (!pickerStartDate.SelectedDate.HasValue || !pickerEndDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("You have not selected a valid date range yet.");
                }

                gDeliveryData.LoadProposal((DateTime)pickerStartDate.SelectedDate, (DateTime)pickerEndDate.SelectedDate);

                int lUnits = gDeliveryData.gDeliveryProposal.Sum(p => p.UnitsPerIssue);
                MessageBox.Show("I have loaded " + gDeliveryData.gDeliveryProposal.Count.ToString() + " proposals for " + lUnits.ToString() + " units.");
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonLoadProposal", "");
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

        //private void LoadProposal(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        OpenFileDialog lOpenFileDialog = new OpenFileDialog();

        //        lOpenFileDialog.InitialDirectory = Settings.DirectoryPath + "\\Recovery\\";
        //        lOpenFileDialog.ShowDialog();
        //        string FileName = lOpenFileDialog.FileName.ToString();

        //        if (!File.Exists(FileName))
        //        {
        //            MessageBox.Show("You have not selected a valid source file ");
        //            return;
        //        }

        //        gDeliveryDoc.DeliveryProposal.Clear();
        //        gDeliveryDoc.DeliveryProposal.ReadXml(FileName);

        //        MessageBox.Show("Done");
        //    }
        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "LoadProposal", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        MessageBox.Show(ex.Message);
        //    }

        //}

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            gCurrentProduct = "";

            try
            {
                gDeliveryData.SaveProposal();
                MessageBox.Show("DeliveryProposal saved successfully");
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonSaveProposal", "");
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
            if (gBackgroundWorker.IsBusy || gBackgroundWorkerPost.IsBusy)
            {
                MessageBox.Show("You are too fast for me. I have not completed the previous task yet. Request will be ignored. ");
                return;
            }

            if (gDeliveryData.gDeliveryProposal.Where(p => p.ValidationStatus != "Deliverable").Count() > 0)
            {
                MessageBox.Show("The proposal is invalid. It cannot be posted.");
                return;
            }

            // Continue with the proposal

            gDeliveryData.gDeliveryProposal.AcceptChanges();

            this.Cursor = Cursors.Wait;

            try
            {
                this.Cursor = Cursors.Wait;

                // In case of a crash, you can rerun the delivery from the same XML. The system
                // will not redeliver an issue if is has already been done on the previous run.
                this.Cursor = Cursors.Wait;
                gBackgroundWorkerPost.RunWorkerAsync(gDeliveryData.gDeliveryProposal);
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
                //DeliveryDoc lDeliveryDoc = (DeliveryDoc)e.Argument;
                e.Result = ProductBiz.PostDeliveryMedia(gDeliveryData, gBackgroundWorkerPost);
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


        #region Validate proposals

        private void buttonValidate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (gBackgroundWorker.IsBusy || gBackgroundWorkerPost.IsBusy)
                {
                    return;
                }

                this.Cursor = Cursors.Wait;

                if (gDeliveryData.gDeliveryProposal.Count == 0)
                {
                    MessageBox.Show("There is nothing to validate.");
                    return;
                }

                gDeliveryData.gDeliveryProposal.AcceptChanges();

                gBackgroundWorker.RunWorkerAsync();
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
            e.Result = ProductBiz.ValidateProposal(gDeliveryData, gBackgroundWorker);
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

                gDeliveryData.gDeliveryProposal.WriteXml(Settings.DirectoryPath + "\\Recovery\\OutputFromSuccessValidation" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xml");

                int lUnits = gDeliveryData.gDeliveryProposal.Sum(p => p.UnitsPerIssue);

                MessageBox.Show("There where " + gDeliveryData.gDeliveryProposal.Count.ToString() + " deliverable records for " + lUnits.ToString() + " units. You may proceed to post them!");
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
                lAdaptor.FillById(lIssue, gDeliveryData.gDeliveryProposal[0].IssueId, "IssueId");

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
                foreach (DeliveryDoc.DeliveryProposalRow lRow in gDeliveryData.gDeliveryProposal)
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
                gDeliveryData.gDeliveryProposal.Clear();
                Cursor = Cursors.Arrow;
            }
        }

        #endregion


        #region Format delivery lists
      
        private void SerialiseMediaList(List<MediaDeliveryItem> pList)
        {
            try
            {
                // Serialise to XML

                if (pList.Count == 0) return;

                // Serialise to a file stream
                MemoryStream lMemoryStream = new MemoryStream();
                XmlSerializer lSerializer = new XmlSerializer(typeof(List<MediaDeliveryItem>));

                // Edit the document

                string lFileName = Settings.DirectoryPath + "\\Final_MediaList_" + DateTime.Now.ToString("yyyyMMdd") + ".xml";
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "SerialiseList", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }


        //private void BuildInventory(List<SkynetDeliveryItem> pDeliveryItem, string pMethod)
        //{
        //    try
        //    {
        //        if (pDeliveryItem.Count == 0) return;

        //        gInventory.Methods.Add(new DeliveriesMethod() { Name = pMethod });
        //        List<PackageCounter> lPackageCounters = new List<PackageCounter>();

        //        // Accumulate all the relevant package counters for the associated customers.
        //        foreach (SkynetDeliveryItem lItem in pDeliveryItem)
        //        {
        //            if (gPackageCounters.Where(p => p.CustomerId == lItem.CustomerId).Count() > 0)
        //            {
        //                lPackageCounters.AddRange(gPackageCounters.Where(p => p.CustomerId == lItem.CustomerId).ToList());
        //            }
        //        }

        //        // Group by IssueDescription

        //        var lAnswer = lPackageCounters.GroupBy(p => p.IssueDescription, (key, values) => new { IssueDescription = key, Units = values.Sum(x => x.UnitsPerIssue) });

        //        foreach (var lItem2 in lAnswer)
        //        {
        //            gInventory.Methods[gInventory.Methods.Count - 1].Items.Add(new InventoryItem() { IssueDescription = lItem2.IssueDescription, Units = lItem2.Units });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "BuildInventory", "pMethod = " + pMethod);
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        throw ex;
        //    }
        //}

        //private void FormatCollectionList(object sender, RoutedEventArgs e)
        //{
        //    if (gBackgroundWorker.IsBusy || gBackgroundWorkerPost.IsBusy)
        //    {
        //        return;
        //    }

        //    this.Cursor = Cursors.Wait;
        //    try
        //    {
        //        if (checkPayers.IsChecked == false & checkNonPayers.IsChecked == false)
        //        {
        //            MessageBox.Show("This way you are not going to get any labels.");
        //            return;
        //        }
        //        OpenFileDialog lFileDialog = new OpenFileDialog();

        //        lFileDialog.InitialDirectory = Settings.DirectoryPath + "\\Deliveries";
        //        lFileDialog.Multiselect = true;
        //        lFileDialog.ShowDialog();

        //        if (lFileDialog.FileNames.Count() == 0)
        //        {
        //            MessageBox.Show("You have not selected any files.");
        //            return;
        //        }

        //        foreach (string lFileName in lFileDialog.FileNames)
        //        {
        //            if (!lFileName.Contains("Collect_"))
        //            {
        //                MessageBox.Show("I can accept only files of which the name starts with 'Collect_'");
        //                return;
        //            }
        //        }

        //        gDeliveryData.gDeliveryProposal.Clear();

        //        foreach (string lFileName in lFileDialog.FileNames)
        //        {
        //            //Append all the files into the DeliveryRecord table
        //            gDeliveryData.gDeliveryProposal.ReadXml(lFileName);
        //        }



        //        {
        //            string lResult;

        //            if ((lResult = ProductBiz.Filter((bool)checkPayers.IsChecked, (bool)checkNonPayers.IsChecked, gDeliveryData)) != "OK")
        //            {
        //                MessageBox.Show(lResult);
        //                return;
        //            }
        //        }

        //        //Sort by customerid

        //        gDeliveryData.gDeliveryProposal.DefaultView.Sort = "ReceiverId";

        //        {
        //            string lResult;

        //            if ((lResult = ProductBiz.CopyToCollectionList(gDeliveryData)) != "OK")
        //            {
        //                MessageBox.Show(lResult);
        //                return;
        //            }
        //        }

        //        string OutputFile = Settings.DirectoryPath + "\\Final_CollectionList_" + lFileDialog.SafeFileName;

        //        gDeliveryData.gCollectionList.WriteXml(OutputFile);

        //        MessageBox.Show(gDeliveryData.gCollectionList.Count.ToString() + " Records written to " + OutputFile.ToString());

        //        OutputFile = OutputFile.Replace("xml", "xsd");
        //        gDeliveryData.gCollectionList.WriteXmlSchema(OutputFile);
        //    }
        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "FormatCollectionList", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        MessageBox.Show(ex.Message);
        //        return;
        //    }
        //    finally
        //    {
        //        this.Cursor = Cursors.Arrow;
        //    }

        //}

        //private void FormatRegisteredMail(object sender, RoutedEventArgs e)
        //{
        //    if (gBackgroundWorker.IsBusy || gBackgroundWorkerPost.IsBusy)
        //    {
        //        return;
        //    }
        //    this.Cursor = Cursors.Wait;
        //    try
        //    {
        //        if (checkPayers.IsChecked == false & checkNonPayers.IsChecked == false)
        //        {
        //            MessageBox.Show("This way you are not going to get any labels.");
        //            return;
        //        }
        //        //OpenFileDialog lFileDialog = new OpenFileDialog();

        //        //lFileDialog.InitialDirectory = Settings.DirectoryPath;
        //        //lFileDialog.ShowDialog();
        //        //string lFileName = lFileDialog.FileName.ToString();
        //        //if (!File.Exists(lFileName))
        //        //{
        //        //    MessageBox.Show("You have not selected a valid source file ");
        //        //    return;
        //        //}

        //        //gDeliveryDoc.Clear();
        //        //gDeliveryData.gDeliveryProposal.ReadXml(lFileName);

        //        OpenFileDialog lFileDialog = new OpenFileDialog();

        //        lFileDialog.InitialDirectory = Settings.DirectoryPath + "\\Deliveries";
        //        lFileDialog.Multiselect = true;
        //        lFileDialog.ShowDialog();

        //        if (lFileDialog.FileNames.Count() == 0)
        //        {
        //            MessageBox.Show("You have not selected any files.");
        //            return;
        //        }

        //        foreach (string lFileName in lFileDialog.FileNames)
        //        {
        //            if (!lFileName.Contains("\\RegisteredMail_"))
        //            {
        //                MessageBox.Show("I can accept only files of which the name starts with 'RegisteredMail_'");
        //                return;
        //            }
        //        }

        //        gDeliveryData.gDeliveryProposal.Clear();

        //        foreach (string lFileName in lFileDialog.FileNames)
        //        {
        //            //Append all the files into the DeliveryRecord table
        //            gDeliveryData.gDeliveryProposal.ReadXml(lFileName);
        //        }

        //        {
        //            string lResult;

        //            if ((lResult = ProductBiz.Filter((bool)checkPayers.IsChecked, (bool)checkNonPayers.IsChecked, gDeliveryData)) != "OK")
        //            {
        //                MessageBox.Show(lResult);
        //                return;
        //            }
        //        }

        //        // Generate the registered mail list

        //        //Sort by customerid

        //        gDeliveryData.gDeliveryProposal.DefaultView.Sort = "ReceiverId";

        //        if (!ProductBiz.GenerateRegisteredMail("Surname, Initials", ref gDeliveryDoc))
        //        {
        //            return;
        //        }

        //        string OutputFile = Settings.DirectoryPath + "\\Final_RegisteredMailList_" + lFileDialog.SafeFileName;
        //        gDeliveryDoc.RegisteredMail.WriteXml(OutputFile);

        //        MessageBox.Show(gDeliveryDoc.RegisteredMail.Count.ToString() + " Records written to " + OutputFile.ToString());

        //        OutputFile = OutputFile.Replace("xml", "xsd");
        //        gDeliveryDoc.RegisteredMail.WriteXmlSchema(OutputFile);

        //    }
        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "FormatRegisteredMail", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        MessageBox.Show(ex.Message);
        //        return;
        //    }

        //    finally
        //    {
        //        this.Cursor = Cursors.Arrow;
        //    }
        //}

        private void Click_SubscriptionTransactions(object sender, RoutedEventArgs e)
        {
            System.Data.DataRowView lRowView = (System.Data.DataRowView)gDeliveryProposalViewSource.View.CurrentItem;
            if (lRowView != null)
            {
                DeliveryDoc.DeliveryRecordRow lRecord = (DeliveryDoc.DeliveryRecordRow)lRowView.Row;
                SubscriptionPicker2 lSubscriptionPicker = new SubscriptionPicker2();
                lSubscriptionPicker.SelectById(lRecord.SubscriptionId);
                lSubscriptionPicker.ShowDialog();
            }
        }
   
        private void buttonFormatMediaList(object sender, RoutedEventArgs e)
        {
            if (gBackgroundWorker.IsBusy || gBackgroundWorkerPost.IsBusy)
            {
                // Validation and posting is still in progress.
                return;
            }

            this.Cursor = Cursors.Wait;
            int lCurrentReceiverId = 0;
            OpenFileDialog lFileDialog = new OpenFileDialog();
            MediaDeliveryItem lMediaDeliveryItem = new MediaDeliveryItem();
            CustomerData3 lCustomerData;
            //List<MediaSupportDeliveryItem> International = new List<MediaSupportDeliveryItem>();
            //List<MediaSupportDeliveryItem> Economy = new List<MediaSupportDeliveryItem>();
            try
            {
                if (checkPayers.IsChecked == false & checkNonPayers.IsChecked == false)
                {
                    MessageBox.Show("This way you are not going to get any data.");
                    return;
                }

                //SelectProposalFiles();
                //if (lFileDialog.FileNames.Count() == 0)
                //{
                //    MessageBox.Show("You have not selected any files.");
                //    return;
                //}
                //if (!ValidSelection()) return;

                //// Combine all the filenames into a single ADO table            
                //gDeliveryData.gDeliveryProposal.Clear();
                //SelectedFiles lSelectedFiles = new SelectedFiles();
                //foreach (string lSelectedFileName in lFileDialog.FileNames)
                //{
                //    //Append the content of all the files into the DeliveryRecord table
                //    gDeliveryData.gDeliveryProposal.ReadXml(lSelectedFileName);
                //    lSelectedFiles.Files.Add(lSelectedFileName);
                //    gProcessedFiles.Items.Add(new ProcessedFile() { FileName = lSelectedFileName, Datum = DateTime.Now });
                //}

                string lResult = ProductBiz.Filter((bool)checkPayers.IsChecked, (bool)checkNonPayers.IsChecked, gDeliveryData);
                if (lResult != "OK")
                {
                    MessageBox.Show(lResult);
                    return;
                }

                gDeliveryData.gDeliveryProposal.DefaultView.Sort = "ReceiverId, IssueId";

                CreateRawMediaDeliveryList();

                SerialiseMediaList(gMediaDeliveryItemsRaw);

                //ConsolidateRawMediaDeliveryList();

                //SplitDeliveryListByDeliveryMethod(); //Its for InternationalCountries

                //BuildInventoryMediaForAll();

                //SerialiseMediaResults();

                if (RegisterResults())
                {
                    MessageBox.Show("Everything succeeded");
                }
                else
                {
                    MessageBox.Show("Something went wrong with the registration of listings.");
                }


                //**************************************************************************************************************************************************************

                //void SelectProposalFiles()
                //{
                //    // Select all the delivery proposal files that you want to process.

                //    lFileDialog.InitialDirectory = Settings.DirectoryPath + "\\Deliveries";
                //    lFileDialog.Multiselect = true;
                //    lFileDialog.ShowDialog();
                //}

                //bool ValidSelection()
                //{
                //    // Get the details of all the files that have been processed already 
                //    if (File.Exists(gProcessedFileName))
                //    {
                //        FileStream lProcessedStream = new FileStream(gProcessedFileName, FileMode.Open);
                //        gProcessedFiles = (ProcessedFiles)gProcessedSerializer.Deserialize(lProcessedStream);
                //        lProcessedStream.Close();
                //    }

                //    foreach (string lFileName in lFileDialog.FileNames)
                //    {
                //        if (!lFileName.Contains("\\Media_"))
                //        {
                //            MessageBox.Show("I can accept only files of which the name starts with 'Media'");
                //            return false;
                //        }

                //        ProcessedFiles lHits = new ProcessedFiles();

                //        lHits.Items = (List<ProcessedFile>)gProcessedFiles.Items.Where(x => x.FileName == lFileName).ToList();

                //        if (lHits.Items.Count > 0)
                //        {
                //            StringBuilder lStringBuilder = new StringBuilder();
                //            foreach (ProcessedFile item in lHits.Items)
                //            {
                //                lStringBuilder.Append(item.Datum.ToString() + " ");
                //            }

                //            if (MessageBoxResult.No == MessageBox.Show("You have already precessed this file on " + lStringBuilder
                //                                                     + " Do you really want to do it again?", "Warning",
                //                    MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No))
                //            {
                //                return false;
                //            }
                //        }
                //    }
                //    return true;
                //}
                void CreateRawMediaDeliveryList()
                {
                    int lCurrentIssue = 0;
                    try
                    {
                        gMediaDeliveryItemsRaw.Clear();
                        //gPackageCounters.Clear();
                        foreach (DataRowView lDataRowView in gDeliveryData.gDeliveryProposal.DefaultView)
                        {
                            DeliveryDoc.DeliveryProposalRow lRow = (DeliveryDoc.DeliveryProposalRow)lDataRowView.Row;

                            if (!lRow.Post)
                            {
                                MessageBox.Show("I cannot format unposted deliveries");
                                return;
                            }
                           
                            lCurrentIssue = lRow.IssueId;

                            lMediaDeliveryItem = new MediaDeliveryItem();
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

                            lMediaDeliveryItem.Title = lCustomerData.Title;
                            lMediaDeliveryItem.CompanyName = lCustomerData.CompanyName;

                            lMediaDeliveryItem.ClientName = lCustomerData.FirstName;
                            lMediaDeliveryItem.ClientSurname = lCustomerData.Surname;
                            lMediaDeliveryItem.WorkPhone = lCustomerData.PhoneNumber;
                            lMediaDeliveryItem.HomePhone = lCustomerData.CellPhoneNumber;
                            lMediaDeliveryItem.CellNumber = lCustomerData.CellPhoneNumber;
                            lMediaDeliveryItem.Email = lCustomerData.EmailAddress;

                            DeliveryAddressData2 lDeliveryAddressData = new DeliveryAddressData2(lRow.DeliveryAddressId);

                            if (lDeliveryAddressData.Building != "")
                            {
                                lMediaDeliveryItem.ComplexNumberandName = "Building: " + lDeliveryAddressData.Building;
                                if (lDeliveryAddressData.FloorNo != "")
                                {
                                    lMediaDeliveryItem.ComplexNumberandName = lMediaDeliveryItem.StreetNr + " Floor: " + lMediaDeliveryItem.FloorNr;
                                    if (lDeliveryAddressData.Room != "")
                                    {
                                        lMediaDeliveryItem.ComplexNumberandName = lMediaDeliveryItem.ComplexNumberandName + " Room: " + lDeliveryAddressData.Room;
                                    }
                                }
                            }
                            lMediaDeliveryItem.FloorNr = lDeliveryAddressData.FloorNo;
                            lMediaDeliveryItem.StreetNr = lDeliveryAddressData.StreetNo;
                            lMediaDeliveryItem.StreetName = lDeliveryAddressData.Street;
                            lMediaDeliveryItem.Suburb = lDeliveryAddressData.Suburb;
                            lMediaDeliveryItem.City = lDeliveryAddressData.City;
                            lMediaDeliveryItem.Province = lDeliveryAddressData.Province;
                            lMediaDeliveryItem.PostalCode = lDeliveryAddressData.PostCode;
                            lMediaDeliveryItem.PublicationName = lRow.IssueDescription;
                            lMediaDeliveryItem.Quantity = lRow.UnitsPerIssue;
                            lMediaDeliveryItem.SubscriptionNumber = lRow.SubscriptionId;
                            lMediaDeliveryItem.CustomerId = lRow.ReceiverId;
                            lMediaDeliveryItem.Name = lCustomerData.Title + " " + lCustomerData.FirstName + " " + lCustomerData.Surname;
                            lMediaDeliveryItem.DeliveryAddressId = lRow.DeliveryAddressId;
                            lMediaDeliveryItem.DeliveryMethod = lRow.DeliveryMethodString;


                            gMediaDeliveryItemsRaw.Add(lMediaDeliveryItem);

                           
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




                //void BuildInventoryMediaForAll()
                //{
                //    try
                //    {
                //        // Build the inventory for all deliverymethods
                //        gInventory.Methods.Clear();
                //        //BuildInventory(International, "International");
                //        //BuildInventory(Economy, "Economy");
                //    }
                //    catch (Exception ex)
                //    {
                //        //Display all the exceptions

                //        Exception CurrentException = ex;
                //        int ExceptionLevel = 0;
                //        do
                //        {
                //            ExceptionLevel++;
                //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "local BuildInventoryForAll", "CurrentReceiverId = " + lCurrentReceiverId.ToString());
                //            CurrentException = CurrentException.InnerException;
                //        } while (CurrentException != null);

                //        MessageBox.Show(ex.Message);
                //        throw ex;
                //    }
                //    finally
                //    {
                //        this.Cursor = Cursors.Arrow;
                //    }
                //}


                //void SerialiseMediaResults()
                //{
                //    try
                //    {
                //        //SerialiseList(International, "International");
                //        //SerialiseList(Economy, "Economy");

                //        // Write the inventory to XML
                //        string lInventoryFileName = Settings.DirectoryPath + "\\Final_MediaList_ZInventory " + DateTime.Now.ToString("yyyyMMdd") + ".xml";
                //        FileStream lFileStream = new FileStream(lInventoryFileName, FileMode.Create);
                //        XmlSerializer lSerializer = new XmlSerializer(typeof(Inventory));
                //        lSerializer.Serialize(lFileStream, gInventory);
                //        MessageBox.Show(lInventoryFileName + " successfully written to " + Settings.DirectoryPath);

                //        //// Write the selected files to XML
                //        //string lSelectionFileName = Settings.DirectoryPath + "\\Final_CourierList_ZSelectedFiles " + DateTime.Now.ToString("yyyyMMdd") + ".xml";
                //        //lFileStream = new FileStream(lSelectionFileName, FileMode.Create);
                //        //lSerializer = new XmlSerializer(typeof(SelectedFiles));
                //        //lSerializer.Serialize(lFileStream, lSelectedFiles);
                //        //MessageBox.Show(lSelectionFileName + " successfully written to " + Settings.DirectoryPath);

                //        //FileStream lProcessedFileStream = new FileStream(gProcessedFileName, FileMode.Create);
                //        //gProcessedSerializer.Serialize(lProcessedFileStream, gProcessedFiles);
                //        //lProcessedFileStream.Flush();
                //        //lProcessedFileStream.Close();
                //        //MessageBox.Show(gProcessedFileName + " successfully written to " + Settings.DirectoryPath);
                //    }
                //    catch (Exception ex)
                //    {
                //        //Display all the exceptions

                //        Exception CurrentException = ex;
                //        int ExceptionLevel = 0;
                //        do
                //        {
                //            ExceptionLevel++;
                //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "local SerialiseResult", "CurrentReceiverId = " + lCurrentReceiverId.ToString());
                //            CurrentException = CurrentException.InnerException;
                //        } while (CurrentException != null);

                //        MessageBox.Show(ex.Message);
                //        throw ex;
                //    }
                //    finally
                //    {
                //        this.Cursor = Cursors.Arrow;
                //    }
                //}


                bool RegisterResults()
                {
                    try
                    {
                        foreach (DeliveryDoc.DeliveryProposalRow item in gDeliveryData.gDeliveryProposal)
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
 
        #endregion
      
    }
}
