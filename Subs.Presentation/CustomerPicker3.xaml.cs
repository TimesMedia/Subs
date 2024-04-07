using Microsoft.Win32;
using Subs.Business;
using Subs.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Subs.Presentation
{
    public class CustomerPickerViewModel : BaseModel
    {
        private decimal _PaymentAmount = 1000;
        private int _PaymentMethod;
        private int _ReferenceTypeId;
        private string _PaymentReference;

        public decimal PaymentAmount
        {
            get
            {
                return _PaymentAmount;
            }

            set
            {
                _PaymentAmount = value;
                NotifyPropertyChanged("PaymentAmount");
            }
        }

        public int PaymentMethod
        {
            get
            {
                return _PaymentMethod;
            }

            set
            {
                _PaymentMethod = value;
                NotifyPropertyChanged("PaymentMethod");
            }
        }
        public int ReferenceTypeId
        {
            get
            {
                return _ReferenceTypeId;
            }

            set
            {
                _ReferenceTypeId = value;
                NotifyPropertyChanged("ReferenceTypeId");
            }
        }

        public string PaymentReference
        {
            get
            {
                return _PaymentReference;
            }

            set
            {
                _PaymentReference = value;
                NotifyPropertyChanged("PaymentReference");
            }
        }
    }

    public partial class CustomerPicker3 : Window
    {
        #region Globals
        private readonly Subs.Data.CustomerDoc2TableAdapters.CustomerTableAdapter gCustomerAdapter = new Subs.Data.CustomerDoc2TableAdapters.CustomerTableAdapter();
        private readonly CustomerDoc2.CustomerDataTable gCustomer = new CustomerDoc2.CustomerDataTable();
        private readonly MIMSDataContext gDataContext = new MIMSDataContext(Settings.ConnectionString);

        private readonly System.Windows.Data.CollectionViewSource gCollectionViewSourceCustomer;
        private readonly System.Windows.Data.CollectionViewSource gInvoiceAndPaymentViewSource;
        private System.Windows.Data.CollectionViewSource gPaymentViewSource = new CollectionViewSource();
        private System.Windows.Data.CollectionViewSource gInvoiceViewSource = new CollectionViewSource();
        private readonly System.Windows.Data.CollectionViewSource gTooMuchTooLittleViewSource;
        private readonly System.Windows.Data.CollectionViewSource gLiabilityRecordsViewSource;
        private readonly System.Windows.Data.CollectionViewSource gDueViewSource;

        private readonly System.Windows.Data.CollectionViewSource gSubscriptionsViewSource;

        private int gConsolidateCustomerSource = 0;
        private int gHits = 0;
        public CustomerPickerViewModel gCustomerPickerViewModel = new CustomerPickerViewModel();

        private readonly Dictionary<int, string> gPaymentMethodDictionary = new Dictionary<int, string>();
        private readonly Dictionary<int, string> gReferenceTypeDictionary = new Dictionary<int, string>();
        private readonly Subs.Data.LedgerDoc2 gLedgerDoc = new LedgerDoc2();
        private readonly ObservableCollection<CustomerData3> gCustomers = new ObservableCollection<CustomerData3>();
        private CustomerData3 gCurrentCustomer;
        //private Subs.Data.InvoiceAndPayment gSelectedPayment = null; 
        //private decimal gCalculatedLiability = 0M;
        private List<LiabilityRecord> gLiabilityRecords = new List<LiabilityRecord>();

        //private List<Subs.Data.InvoicesAndPayments> gPaymentAndInvoice = new List<InvoicesAndPayments>();
        //private List<Subs.Data.InvoicesAndPayments> gInvoice = new List<InvoicesAndPayments>();
        //private List<Subs.Data.InvoicesAndPayments> gPayment = new List<InvoicesAndPayments>();


        private List<Subs.Data.InvoiceAndPayment> gInvoiceAndPaymentCopy = new List<InvoiceAndPayment>();
        private List<Subs.Data.InvoiceAndPayment> gInvoiceAndPayment2 = new List<InvoiceAndPayment>();
        private List<Subs.Data.InvoiceAndPayment> gInvoice2 = new List<InvoiceAndPayment>();
        private List<Subs.Data.InvoiceAndPayment> gPayment2 = new List<InvoiceAndPayment>();
        private List<Subs.Data.InvoiceAndPayment> gDue = new List<InvoiceAndPayment>();


        private enum ContextMenuOperations
        {
            Select = 1,
            Report = 2
        }
        public enum PickerTabs
        {
            Select = 0,
            Invoice = 1,
            Subscription = 2,
            Discrepancies = 3,
            Liability = 4,
            Due = 5,
            XPS = 6
 
        }

        private Subs.Presentation.CustomerUpdateExists2 gCustomerExistsForm;

        #endregion

        #region Constructors
        public CustomerPicker3()
        {

            InitializeComponent();
            gCustomerAdapter.AttachConnection();
            //gMIMSCustomerTableAdapter.AttachConnection();
            gCollectionViewSourceCustomer = (System.Windows.Data.CollectionViewSource)this.Resources["customerViewSource"];
            gCollectionViewSourceCustomer.Source = gCustomers;
            gInvoiceAndPaymentViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["InvoiceAndPaymentViewSource"];
            //gPaymentViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["PaymentViewSource"];
            //gInvoiceViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["InvoiceViewSource"];

            System.Windows.Data.CollectionViewSource gPaymentViewSource = new CollectionViewSource();
            System.Windows.Data.CollectionViewSource gInvoiceViewSource = new CollectionViewSource();   



            gSubscriptionsViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["SubscriptionsViewSource"];
            gTooMuchTooLittleViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["TooMuchTooLittleViewSource"];
            gLiabilityRecordsViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["LiabilityRecordsViewSource"];
            gDueViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["DueViewSource"];

            PaymentDatePicker.SelectedDate = DateTime.Now;

            //Initialise combo boxes

            foreach (int i in Enum.GetValues(typeof(PaymentMethod)))
            {
                ComboItem lNewItem = new ComboItem();
                lNewItem.Key = i;
                lNewItem.Value = Enum.GetName(typeof(PaymentMethod), i);
                comboPaymentMethod.Items.Add(lNewItem);
            }

            foreach (int i in Enum.GetValues(typeof(ReferenceType)))
            {
                ComboItem lNewItem = new ComboItem();
                lNewItem.Key = i;
                lNewItem.Value = Enum.GetName(typeof(ReferenceType), i);
                comboReferenceType.Items.Add(lNewItem);
            }

            PaymentPanel.DataContext = gCustomerPickerViewModel;

            int lCurrentCustomer = 0;
            try
            {
                //Load the currently selected Customer.

                if (Settings.CurrentCustomerId != 0)
                {
                    SetCurrentCustomer(Settings.CurrentCustomerId);
                }

                if (Settings.Authority == 3)
                {
                    Profile.IsEnabled = true;
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "CustomerPicker3 constructor", "Current Customer = " + lCurrentCustomer.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return;
            }
        }

        #endregion

        #region Window management

        private void SetVisibility(object sender, RoutedEventArgs e)
        {
            Utilities.SetVisibility(sender);
        }

        private bool SelectTab(PickerTabs pTab)
        {
            tabControl1.SelectedIndex = (int)pTab;
            return true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            this.comboCompany.DataSource = AdministrationData2.Company; // this.bindingSourceCompany;
            this.comboCompany.DisplayMember = "CompanyName";
            this.comboCompany.ValueMember = "CompanyId";
            this.comboCompany.SelectedIndex = 1;
        }

        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl1.SelectedIndex == (int)PickerTabs.Select)
            {
                // Prevent referencing old data.
                Tab_Invoice.Visibility = Visibility.Hidden;
            }
        }

        #endregion

        #region Search tab

        private void CustomerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectCurrentCustomer();
        }

        public bool SelectCurrentCustomer([CallerMemberName] string pCaller = null)
        {
            try
            {
                if (!gCollectionViewSourceCustomer.View.IsEmpty)
                {
                    if (gCollectionViewSourceCustomer.View.CurrentItem != null)
                    {
                        gCurrentCustomer = (CustomerData3)gCollectionViewSourceCustomer.View.CurrentItem;
                        Settings.CurrentCustomerId = gCurrentCustomer.CustomerId;
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "SelectCurrentCustomer", "Caller= " + pCaller);
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return false;
            }
        }

        public bool SetCurrentCustomer(int pCustomerId)
        {
            try
            {
                gCurrentCustomer = new CustomerData3(pCustomerId);
                if (gCurrentCustomer.CustomerId == 0)
                {
                    return false;
                }
                gCustomers.Clear();
                gCustomers.Add(gCurrentCustomer);
                gCollectionViewSourceCustomer.Source = gCustomers;
                gCollectionViewSourceCustomer.View.MoveCurrentToFirst();
                Settings.CurrentCustomerId = pCustomerId;

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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "SetCurrentCustomer(CustomerId)", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return false;
            }
        }

        private void buttonSearchCompany_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.comboCompany.SelectedIndex == 1)
                {
                    MessageBox.Show("I cannot search on company [None]. What answer did you expect?");
                    return;
                }


                int.TryParse(comboCompany.SelectedValue.ToString(), out int CompanyId);

                if (CompanyId == 0)
                {
                    MessageBox.Show("CompanyId could not be parsed");
                    return;
                }

                this.Cursor = Cursors.Wait;

                IEnumerable<Customer> lSelectedCustomers = gDataContext.MIMS_DataContext_Customer_Select("Company", CompanyId, "").ToList();

                if (lSelectedCustomers.Count() == 0)
                {
                    MessageBox.Show("No customer found");
                    return;
                }

                gCustomers.Clear();
                foreach (Customer lSelection in lSelectedCustomers)
                {
                    CustomerData3 lCustomer = new CustomerData3(lSelection.CustomerId);
                    gCustomers.Add(lCustomer);
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonSearchCompany_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return;
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void buttonSearchCustomerId_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Initialise

                ElicitInteger lElicitNumber = new ElicitInteger("What is the CustomerId?");
                lElicitNumber.ShowDialog();
                if (lElicitNumber.Answer == 0)
                {
                    return;
                }

                if (!SetCurrentCustomer(lElicitNumber.Answer))
                {
                    MessageBox.Show("No customer found");
                    return;
                }

                switch (lElicitNumber.gShortcut)
                {
                    case ElicitOptions.Select:
                        Close();
                        break;
                    case ElicitOptions.Invoice:
                        GoToStatement();
                        break;
                    case ElicitOptions.Update:
                        CustomerUpdate();
                        break;
                    default:
                        break;
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonSearchCustomerId_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return;
            }
        }

        private void buttonSearchSurname_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Initialise

                ElicitString lElicit = new ElicitString("What is the Surname?");
                lElicit.ShowDialog();
                if (string.IsNullOrWhiteSpace(lElicit.Answer))
                {
                    return;
                }

                this.Cursor = Cursors.Wait;

                // Select the parameter to search on, and then load that data

                IEnumerable<int> lSelectedCustomers = CustomerData3.GetCustomerIds("Surname", 0, "%" + lElicit.Answer + "%");

                if (lSelectedCustomers.Count() == 0)
                {
                    MessageBox.Show("No customer found");
                    return;
                }

                gCustomers.Clear();
                foreach (int lSelection in lSelectedCustomers)
                {
                    CustomerData3 lCustomer = new CustomerData3(lSelection);
                    gCustomers.Add(lCustomer);
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonSearchSurname_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return;
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void buttonSearchEmail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Initialise

                ElicitString lElicit = new ElicitString("What is the Email address?");
                lElicit.ShowDialog();
                if (string.IsNullOrWhiteSpace(lElicit.Answer))
                {
                    return;
                }

                this.Cursor = Cursors.Wait;

                IEnumerable<Customer> lSelectedCustomers = gDataContext.MIMS_DataContext_Customer_Select("Email", 0, "%" + lElicit.Answer + "%").ToList();

                if (lSelectedCustomers.Count() == 0)
                {
                    MessageBox.Show("No customer found");
                    return;
                }

                gCustomers.Clear();
                foreach (Customer lSelection in lSelectedCustomers)
                {
                    CustomerData3 lCustomer = new CustomerData3(lSelection.CustomerId);
                    gCustomers.Add(lCustomer);
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonSearchEmail_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return;
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void buttonSearchPostalCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Initialise

                ElicitString lElicit = new ElicitString("What is the postal code?");
                lElicit.ShowDialog();
                if (string.IsNullOrWhiteSpace(lElicit.Answer))
                {
                    return;
                }

                this.Cursor = Cursors.Wait;

                IEnumerable<Customer> lSelectedCustomers = gDataContext.MIMS_DataContext_Customer_Select("PostalCode", 0, "%" + lElicit.Answer + "%").ToList();

                if (lSelectedCustomers.Count() == 0)
                {
                    MessageBox.Show("No customer found");
                    return;
                }

                gCustomers.Clear();
                foreach (Customer lSelection in lSelectedCustomers)
                {
                    CustomerData3 lCustomer = new CustomerData3(lSelection.CustomerId);
                    gCustomers.Add(lCustomer);
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonSearchPostalCode_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return;
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void buttonSearchInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Initialise

                ElicitInteger lElicit = new ElicitInteger("What is the invoice number?");
                lElicit.ShowDialog();
                if (lElicit.Answer == 0)
                {
                    return;
                }

                this.Cursor = Cursors.Wait;


                IEnumerable<Customer> lSelectedCustomers = gDataContext.MIMS_DataContext_Customer_Select("Invoice", lElicit.Answer, "").ToList();

                if (lSelectedCustomers.Count() == 0)
                {
                    MessageBox.Show("No customer found");
                    return;
                }

                gCustomers.Clear();
                foreach (Customer lSelection in lSelectedCustomers)
                {
                    CustomerData3 lCustomer = new CustomerData3(lSelection.CustomerId);
                    gCustomers.Add(lCustomer);
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonSearchInvoice_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return;
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

  

        private void buttonLoad_Click(object sender, RoutedEventArgs e)
        {
            int lCurrentCustomer = 0;
            try
            {
                List<int> lCustomers = new List<int>();
                XmlSerializer lSerializer = new XmlSerializer(typeof(List<int>));

                OpenFileDialog lFileDialog = new OpenFileDialog();

                lFileDialog.InitialDirectory = @"c:\Subs";
                lFileDialog.Filter = "Xml files (*.xml)|*.xml";
                lFileDialog.Multiselect = false;
                lFileDialog.ShowDialog();

                if (lFileDialog.FileNames.Count() == 0)
                {
                    MessageBox.Show("You have not selected any files.");
                    return;
                }

                FileStream lStream = new FileStream(lFileDialog.FileName, FileMode.Open);
                lCustomers = (List<int>)lSerializer.Deserialize(lStream);
                lStream.Close();

                foreach (int item in lCustomers)
                {
                    lCurrentCustomer = item;
                    CustomerData3 lCustomer = new CustomerData3(item);
                    gCustomers.Add(lCustomer);
                }

                MessageBox.Show("Done");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + lCurrentCustomer.ToString());
            }
        }




        private void Click_NewCustomer(object sender, RoutedEventArgs e)
        {
            try
            {

                gCustomerExistsForm = new Subs.Presentation.CustomerUpdateExists2();
                gCustomerExistsForm.ShowDialog();

                if (gCustomerExistsForm.gCustomerData == null)
                {
                    return;
                }

                //****************************************************************************************************************


                {
                    string lResult;

                    if ((lResult = CustomerBiz.ValidateDuplicate(gCustomerExistsForm.gCustomerData)) != "OK")
                    {
                        MessageBox.Show(lResult);
                        return;
                    }
                }

                Subs.Presentation.CustomerUpdate2 lCustomerUpdate = new Subs.Presentation.CustomerUpdate2(gCustomerExistsForm.gCustomerData);
                lCustomerUpdate.ShowDialog();

                if (!lCustomerUpdate.Cancelled)
                {
                    if (!SetCurrentCustomer(gCustomerExistsForm.gCustomerData.CustomerId)) return;
                    PostProcessCustomer();
                }

                if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                {
                    gCollectionViewSourceCustomer.Source = gCustomer;
                }

                // Make it possible to resume, once you have displayed other possibilities. 
                buttonNewResume.IsEnabled = true;

            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Click_NewCustomer", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (gCustomerExistsForm != null)
                {
                    gCustomerExistsForm.Close();
                }
            }
        }

        private void PostProcessCustomer()
        {
            try
            {
                // Prevent creating a new customer at this point in time.
                buttonNewResume.IsEnabled = false;

                // Do the classification

                Subs.Presentation.CustomerClassification lWindow = new CustomerClassification();
                lWindow.ShowDialog();

                // Check the delivery addresses

                //SetCurrentCustomer();

                if (!DeliveryAddressStatic.Loaded)
                {
                    MessageBox.Show("DeliveryAddresses not loaded yet. Try again in 3 seconds");
                    return;
                }

                Subs.Presentation.DeliveryAddress2 lDeliveryAddress = new Subs.Presentation.DeliveryAddress2(gCurrentCustomer.CustomerId);
                lDeliveryAddress.ShowDialog();

                // Do the MIMS specific stuff

                //CustomerSpecific lCustomerSpecific = new CustomerSpecific();
                //lCustomerSpecific.ShowDialog();

                // Record it so that the operator can get recognition for her work

                LedgerData.UpdateCustomer(gCurrentCustomer);

                buttonNewResume.IsEnabled = false;

                return;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "PostProcessCustomer", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }
        }

        private void Click_NewCustomerResume(object sender, RoutedEventArgs e)
        {
            Subs.Presentation.CustomerUpdate2 lCustomerUpdate = new Subs.Presentation.CustomerUpdate2(gCustomerExistsForm.gCustomerData);
            lCustomerUpdate.ShowDialog();
            if (!lCustomerUpdate.Cancelled)
            {
                PostProcessCustomer();
            }
        }


        #endregion

        #region Select tab - context menu

        private void CustomerDataGrid_KeyUp(object sender, KeyEventArgs e)
        {

            if (e.OriginalSource.GetType() == typeof(DataGridCell))
            {
                if (e.Key == Key.Enter)
                {
                    CustomerUpdate();
                }
            }
        }

        private void Click_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
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

        private void Click_CustomerSelect(object sender, RoutedEventArgs e)
        {
            SelectCurrentCustomer();
            Close();
        }

        private void Click_CustomerSelect(object sender, MouseButtonEventArgs e)
        {
            SelectCurrentCustomer();
            Close();
        }

        public void GoToStatement()
        {
            try 
            {
                // Original  ***
                //CustomerId.Text = gCurrentCustomer.CustomerId.ToString();
                //if (PopulatePaymentAndInvoice())
                //{
                //    Tab_Invoice.Visibility = Visibility.Visible;
                //    SelectTab(PickerTabs.Invoice);
                //}
                // Original ***


                CustomerId.Text = gCurrentCustomer.CustomerId.ToString();

                this.Cursor = Cursors.Wait;

                {
                    string lResult;

                    if ((lResult = gCurrentCustomer.PopulateInvoice2()) != "OK")
                    {
                        MessageBox.Show(lResult);
                        return;
                    }
                }


                //At this point, no payments have been allocated yet, but the net payments and invoices are all on the status of LastRow.

                gInvoiceAndPayment2 = gCurrentCustomer.InvoiceAndPayment;

                AssignStatementResources();

                //***
                AllocationData lAllocationData = new AllocationData(gPayment2, gInvoice2);

                gInvoiceAndPaymentCopy = lAllocationData.AllocatePayments();

                gInvoiceAndPayment2.Clear();
                gInvoiceAndPayment2.AddRange(gInvoiceAndPaymentCopy);    // I do this to get a copy of the data, rather than a pointer to a foreign object.

                AssignStatementResources();
                //***
                Tab_Invoice.Visibility = Visibility.Visible;
                SelectTab(PickerTabs.Invoice);
                return;
                }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "GoToStatement", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }

            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }


        private void Click_ShowDue(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            try
            {
                // Get the report data

                SelectCurrentCustomer();

                gDue = gCurrentCustomer.GetInvoiceAndPayment();
                gDueViewSource.Source = gDue;
                DueDataGrid.ItemsSource = gDueViewSource.View;
                //textCurrentLiability.Text = gCurrentCustomer.Liability.ToString("#########0.00");

                SelectTab(PickerTabs.Due);
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Click_Due", "CustomerId = " + gCurrentCustomer.CustomerId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;

            }
        }




        //private void Click_ShowLiabilities(object sender, RoutedEventArgs e)
        //{
        //    this.Cursor = Cursors.Wait;
        //    try
        //    {
        //        // Get the report data

        //        SelectCurrentCustomer();

        //        gCalculatedLiability = 0M;
        //        gLiabilityRecords.Clear();
                
        //        {
        //            string lResult;

        //            if ((lResult = gCurrentCustomer.CalculateLiability2(ref gLiabilityRecords, ref gCalculatedLiability)) != "OK")
        //            {
        //                MessageBox.Show(lResult);
        //                return;
        //            }
        //        }

        //        textSumOfPayments.Text = gCurrentCustomer.SumOfPayments.ToString("#########0.00");
        //        textSumOfDeliveries.Text = gCurrentCustomer.SumOfDeliveries.ToString("#########0.00");

        //        gLiabilityRecordsViewSource.Source = gLiabilityRecords;
        //        LiabilityDataGrid.ItemsSource = gLiabilityRecordsViewSource.View;
        //        textCurrentLiability.Text = gCurrentCustomer.Liability.ToString("#########0.00");

        //        SelectTab(PickerTabs.Liability);

        //    }
        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Click_ShowLiabilities", "CustomerId = " + gCurrentCustomer.CustomerId.ToString());
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        MessageBox.Show(ex.Message);
        //    }
        //    finally
        //    {
        //        this.Cursor = Cursors.Arrow;

        //    }
        //}

        private void Click_Statement(object sender, RoutedEventArgs e)
        {
            SelectCurrentCustomer();
            GoToStatement();
        }

        private void CustomerUpdate()
        {
            try
            {
                SelectCurrentCustomer();

                //if (gCurrentCustomer.Status == CustomerStatus.Cancelled)
                //{
                //    MessageBox.Show("You cannot update a cancelled customer");
                //    return;
                //}

                Subs.Presentation.CustomerUpdate2 lCustomerUpdate = new Subs.Presentation.CustomerUpdate2(gCurrentCustomer);
                lCustomerUpdate.ShowDialog();
                if (!lCustomerUpdate.Cancelled)
                {
                    PostProcessCustomer();
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "CustomerUpdate", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }
        }

        private void Click_SetPassword(object sender, RoutedEventArgs e)
        {
            SelectCurrentCustomer();
            ElicitString lElicitString = new ElicitString("What do you want the password to be?");
            lElicitString.ShowDialog();
            if (lElicitString.Answer.Length < 5)
            {
                MessageBox.Show("You password should be at least 5 characters. Please try again.");
                return;
            }

            gCurrentCustomer.Password1 = lElicitString.Answer;



            string lResult;
            if ((lResult = gCurrentCustomer.Update()) != "OK")
            {
                MessageBox.Show(lResult);
            }

            MessageBox.Show("Done!");
        }


        private void Click_CustomerUpdate(object sender, RoutedEventArgs e)
        {
            CustomerUpdate();
        }

        private void Click_DestroyCustomer(object sender, RoutedEventArgs e)
        {
            try
            {
                SelectCurrentCustomer();
                ElicitString lElicitString = new ElicitString("Please supply a reason.");
                lElicitString.ShowDialog();
                if (lElicitString.Answer.Length < 3)
                {
                    MessageBox.Show("You have to supply a reason");
                    return;
                }

                int lCustomerId = gCurrentCustomer.CustomerId;

                {
                    string lResult;
                    if ((lResult = CustomerBiz.DestroyCustomer(ref gCurrentCustomer, lElicitString.Answer)) != "OK") // Delete from Database
                    {
                        MessageBox.Show(lResult);
                        return;
                    }

                    // Remove from memory

                    gCustomers.Remove(gCurrentCustomer);
                    gCollectionViewSourceCustomer.Source = gCustomers;

                    MessageBox.Show("Customer " + lCustomerId.ToString() + " successfully destroyed.");

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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Click_DestroyCustomer", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Click_Destroy customer failed due to technical error");
            }

        }





        private void Click_DeliveryAddress(object sender, RoutedEventArgs e)
        {
            SelectCurrentCustomer();

            if (!DeliveryAddressStatic.Loaded)
            {
                MessageBox.Show("DeliveryAddresses not loaded yet. Try again in 3 seconds");
                return;
            }

            Subs.Presentation.DeliveryAddress2 lDeliveryAddress = new Subs.Presentation.DeliveryAddress2(gCurrentCustomer.CustomerId);
            lDeliveryAddress.ShowDialog();


            //this.Cursor = Cursors.Wait;
            //DeliveryAddress lDeliveryAddress = DeliveryAddress.GetSingleton();
            //this.Cursor = Cursors.Arrow;
            //lDeliveryAddress.ShowDeliveryAddressDialog();
        }

        private void Click_Profile(object sender, RoutedEventArgs e)
        {
            try
            {
                SelectCurrentCustomer();
                this.Cursor = Cursors.Wait;
                Subs.Presentation.CustomerClassification lWindow = new CustomerClassification();
                lWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Click_Profile", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);


            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }


        private void Click_ConsolidateSource(object sender, RoutedEventArgs e)
        {
            SelectCurrentCustomer();
            gConsolidateCustomerSource = 0;
            gConsolidateCustomerSource = gCurrentCustomer.CustomerId;
        }

        private void Click_ConsolidateTarget(object sender, RoutedEventArgs e)
        {
            int lConsolidateCustomerTarget = 0;
            try
            {
                SelectCurrentCustomer();
                lConsolidateCustomerTarget = gCurrentCustomer.CustomerId;

                // Check for deliverable subscriptions

                if (lConsolidateCustomerTarget == 2)
                {
                    SubscriptionDoc3.SubscriptionDataTable lSubscriptionTable = new SubscriptionDoc3.SubscriptionDataTable();
                    Subs.Data.SubscriptionDoc3TableAdapters.SubscriptionTableAdapter lSubscriptionAdapter = new Subs.Data.SubscriptionDoc3TableAdapters.SubscriptionTableAdapter();
                    lSubscriptionAdapter.AttachConnection();
                    lSubscriptionAdapter.FillById(lSubscriptionTable, "ByConsolidateCustomerTo2", gConsolidateCustomerSource, 0);
                    if (lSubscriptionTable.Count > 0)
                    {
                        MessageBox.Show("You cannot consolidate into customer 2 if your source customer is linked to active subscriptions");
                        return;
                    }
                }

                // Double check

                if (MessageBoxResult.No == MessageBox.Show("Customer " + gConsolidateCustomerSource.ToString() +
                    " to be consolidated into customer " + lConsolidateCustomerTarget.ToString() + ".\n This cannot be undone. Do you want to continue?",
                    "Warning", MessageBoxButton.YesNo))
                {
                    return;
                }


                // Reusable part
                if (!CustomerData3.UpdateConsolidation(gConsolidateCustomerSource, lConsolidateCustomerTarget))
                {
                    MessageBox.Show("Consolidation failed");
                    return;
                }
                else
                {
                    string Message = "Customer " + gConsolidateCustomerSource.ToString() + " consolidated into customer " + lConsolidateCustomerTarget.ToString();
                    // Record the event in the Exception table
                    ExceptionData.WriteException(5, Message, this.ToString(), "Click_ConsolidateTarget", "");
                    MessageBox.Show(Message);
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Click_ConsolidateTarget", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return;
            }
        }

        #endregion

        #region Invoice tab payments

        //private bool PopulatePaymentAndInvoice([CallerMemberName] string pCaller = null)
        //{
        //    // This assigns the necessary data sources for display.

        //    this.Cursor = Cursors.Wait;
        //    string lStage = "Start";
        //    try
        //    {

        //        gPaymentAndInvoice = gCurrentCustomer.GetInvoiceAndPayment(gCurrentCustomer.BalanceInvoiceId, gCurrentCustomer.Balance);
        //        {
        //            string lResult;

        //            if ((lResult =  != "OK")
        //            {
        //                MessageBox.Show(lResult);
        //                return false;
        //            }
        //        }

        //        // Assign data sources
        //        lStage = "Payment";

        //        gPayment.Clear();
        //        gPayment = gPaymentAndInvoice.Where(p => p.OperationId == (int)Operation.Balance
        //                                              || p.OperationId == (int)Operation.Pay
        //                                              || p.OperationId == (int)Operation.Refund
        //                                              || p.OperationId == (int)Operation.ReversePayment).OrderBy(q => q.TransactionId).ThenBy(r => r.Date).ToList();
        //          if (gPayment != null)
        //        {
        //            gPaymentViewSource.Source = gPayment;
        //            PaymentDataGrid.ItemsSource = gPaymentViewSource.View;
        //            PaymentDataGrid.Refresh();
        //        }

        //        lStage = "Invoice";

        //        gInvoice.Clear();
        //        gInvoice = gPaymentAndInvoice.Where(p => !(p.OperationId == (int)Operation.Balance
        //                                                || p.OperationId == (int)Operation.Pay
        //                                                || p.OperationId == (int)Operation.Refund
        //                                                || p.OperationId == (int)Operation.ReversePayment)).OrderBy(q => q.InvoiceId).ThenBy(r => r.Date).ToList();
               
        //        gInvoiceViewSource.Source = gInvoice;
        //        InvoiceDataGrid.ItemsSource = gInvoiceViewSource.View;
        //        gInvoiceViewSource.View.GroupDescriptions.Add(new PropertyGroupDescription("InvoiceId"));
        //        InvoiceDataGrid.Refresh();


        //        return true;
        //    }

        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        int lExceptionId = 0;

        //        do
        //        {
        //            ExceptionLevel++;
        //            lExceptionId = ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "PopulatePaymentAndInvoice", "Stage = " + lStage
        //                + " Customer= " + gCurrentCustomer.CustomerId.ToString() + " Caller = " + pCaller);
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        ExceptionData.WriteExceptionTrace(lExceptionId, ex.StackTrace, "PopulatePaymentAndInvoice");

        //        return false;
        //    }
        //    finally
        //    {
        //        this.Cursor = Cursors.Arrow;
        //    }
        //}


        private bool PopulatePaymentAndInvoice2([CallerMemberName] string pCaller = null)
        {
            // This assigns the necessary data sources for display.

            this.Cursor = Cursors.Wait;
            string lStage = "Start";
            try
            {

                gInvoiceAndPayment2 = gCurrentCustomer.GetInvoiceAndPayment();
               
                // Assign data sources
                lStage = "Payment";

                gPayment2.Clear();
                gPayment2 = gInvoiceAndPayment2.Where(p => p.OperationId == (int)Operation.Balance
                                                      || p.OperationId == (int)Operation.Pay
                                                      || p.OperationId == (int)Operation.Refund
                                                      || p.OperationId == (int)Operation.ReversePayment).OrderBy(q => q.TransactionId).ThenBy(r => r.Date).ToList();
                if (gPayment2 != null)
                {
                    gPaymentViewSource.Source = gPayment2;
                    PaymentDataGrid.ItemsSource = gPaymentViewSource.View;
                    PaymentDataGrid.Refresh();
                }

                lStage = "Invoice";

                gInvoice2.Clear();
                gInvoice2 = gInvoiceAndPayment2.Where(p => !(p.OperationId == (int)Operation.Balance
                                                        || p.OperationId == (int)Operation.Pay
                                                        || p.OperationId == (int)Operation.Refund
                                                        || p.OperationId == (int)Operation.ReversePayment)).OrderBy(q => q.InvoiceId).ThenBy(r => r.Date).ToList();

                gInvoiceViewSource.Source = gInvoice2;
                InvoiceDataGrid.ItemsSource = gInvoiceViewSource.View;
                gInvoiceViewSource.View.GroupDescriptions.Add(new PropertyGroupDescription("InvoiceId"));
                InvoiceDataGrid.Refresh();


                return true;
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                int lExceptionId = 0;

                do
                {
                    ExceptionLevel++;
                    lExceptionId = ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "PopulatePaymentAndInvoice", "Stage = " + lStage
                        + " Customer= " + gCurrentCustomer.CustomerId.ToString() + " Caller = " + pCaller);
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                ExceptionData.WriteExceptionTrace(lExceptionId, ex.StackTrace, "PopulatePaymentAndInvoice");

                return false;
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }





        private void AssignStatementResources([CallerMemberName] string pCaller = null)
        {
            // This assigns the necessary data sources for display.
            string lStage = "";
            
            try
            {
 
                

                // Assign data sources
                lStage = "Payment";


                gPayment2.Clear();
                gPayment2 = gInvoiceAndPayment2.Where(p => p.OperationId == (int)Operation.Balance
                                                      || p.OperationId == (int)Operation.Pay
                                                      || p.OperationId == (int)Operation.Refund
                                                      || p.OperationId == (int)Operation.ReversePayment).OrderBy(q => q.TransactionId).ThenBy(r => r.Date).ToList();
              
                gPaymentViewSource.Source = gPayment2;
                PaymentDataGrid.ItemsSource = gPaymentViewSource.View;
                PaymentDataGrid.Refresh();


                lStage = "Invoice";

                gInvoice2 = gInvoiceAndPayment2.Where(p => !(p.OperationId == (int)Operation.Balance  // Take the rest, i.e. the compliment
                                                        || p.OperationId == (int)Operation.Pay
                                                        || p.OperationId == (int)Operation.Refund
                                                        || p.OperationId == (int)Operation.ReversePayment)).OrderBy(q => q.InvoiceId).ThenBy(r => r.Date).ToList();
              
                gInvoiceViewSource.Source = gInvoice2;
                InvoiceDataGrid.ItemsSource = gInvoiceViewSource.View;
                InvoiceDataGrid.Refresh();
                gInvoiceViewSource.View.GroupDescriptions.Add(new PropertyGroupDescription("InvoiceId"));
             
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                int lExceptionId = 0;

                do
                {
                    ExceptionLevel++;
                    lExceptionId = ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "PopulatePaymentAndInvoice2", "Stage = " + lStage
                        + " Customer= " + gCurrentCustomer.CustomerId.ToString() + " Caller = " + pCaller);
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                ExceptionData.WriteExceptionTrace(lExceptionId, ex.StackTrace, "PopulatePaymentAndInvoice2");

                throw ex;
            }
           
        }





        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            try
            {
                T child = default(T);
                int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < numVisuals; i++)
                {
                    var v = (Visual)VisualTreeHelper.GetChild(parent, i);
                    child = v as T ?? GetVisualChild<T>(v);
                    if (child != null)
                    {
                        break;
                    }
                }
                return child;
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomerPicker", "GetVisualChild", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }


        public DataGridCell GetCell(DataGrid host, DataGridRow row, int columnIndex)
        {
            try
            {
                if (row == null) return null;

                var presenter = GetVisualChild<DataGridCellsPresenter>(row);
                if (presenter == null) return null;

                // try to get the cell but it may possibly be virtualized
                var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
                if (cell == null)
                {
                    // now try to bring into view and retreive the cell
                    host.ScrollIntoView(row, host.Columns[columnIndex]);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
                }
                return cell;
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "GetCell", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                throw ex;
            }
        }


        private void InvoiceDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                if (e.Row.DataContext.GetType() != typeof(InvoiceAndPayment))
                {
                    return;
                }

                InvoiceAndPayment lRow = (InvoiceAndPayment)e.Row.DataContext;
                if (lRow.LastRow)
                {
                    e.Row.Background = new SolidColorBrush(Colors.Yellow);
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "InvoiceDataGrid_LoadingRow", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Error in InvoiceDataGrid_LoadingRow: " + ex.Message);
            }
        }


        private void Click_buttonPayManually(object sender, RoutedEventArgs e)
        {

            // Validate the payment


            if ((int)comboPaymentMethod.SelectedValue == 0)
            {
                MessageBox.Show("You did not select a PaymentMethod");
                return;
            }

            if (decimal.Parse(textPaymentLeft.Text) == 0)
            {
                MessageBox.Show("You have to enter an amount.");
                return;
            }

            if (decimal.Parse(textPaymentLeft.Text) < 0)
            {
                MessageBox.Show("I do not accept negative numbers.");
                return;
            }

            if ((int)comboReferenceType.SelectedValue == 0)
            {
                MessageBox.Show("You have not entered a ReferenceType.");
                return;
            }

            if (string.IsNullOrWhiteSpace(textPaymentReference.Text))
            {
                MessageBox.Show("You have to enter a reference");
                return;
            }

            // Construct a PaymentRecord for more validation
            PaymentData.PaymentRecord lRecord = new PaymentData.PaymentRecord();
            lRecord.CustomerId = gCurrentCustomer.CustomerId;
            lRecord.Amount = decimal.Parse(textPaymentLeft.Text);
            lRecord.Date = (DateTime)PaymentDatePicker.SelectedDate;
            lRecord.PaymentMethod = (int)comboPaymentMethod.SelectedValue;
            lRecord.ReferenceTypeId = (int)comboReferenceType.SelectedValue;
            lRecord.ReferenceTypeString = Enum.GetName(typeof(ReferenceType), (int)comboReferenceType.SelectedValue);
            lRecord.Reference = textPaymentReference.Text;

            Subs.Business.CustomerBiz.PaymentValidationResult lResult = new Subs.Business.CustomerBiz.PaymentValidationResult();

            string lErrorMessage = "";

            {
                string lResult2;

                if ((lResult2 = CustomerBiz.ValidatePayment(ref lRecord, ref lResult, ref lErrorMessage)) != "OK")
                {
                    MessageBox.Show(lResult2);
                    return;
                }
            }

            switch (lResult)
            {
                case Subs.Business.CustomerBiz.PaymentValidationResult.PayerCancelled:
                    {
                        MessageBox.Show(lErrorMessage);
                        return;
                    }
                case Subs.Business.CustomerBiz.PaymentValidationResult.Duplicate:
                    {
                        if (Settings.Authority >= 2)
                        {
                            // Query
                            if (MessageBoxResult.No == MessageBox.Show(lErrorMessage + ". Do you want to continue anyway?", "Warning", MessageBoxButton.YesNo))
                            {
                                return;
                            }
                            else
                            {
                                ExceptionData.WriteException(3, "An apparenlty duplicate payment has been accepted.", this.ToString(), "buttonPostpayment", "CustomerId = " + lRecord.CustomerId.ToString());
                                break;
                            }
                        }
                        else
                        {
                            // This is not an authorised user. Do not allow the capture
                            MessageBox.Show(lErrorMessage);
                            return;
                        }
                    }
                case Subs.Business.CustomerBiz.PaymentValidationResult.InvalidAllocationNumber:
                    {
                        MessageBox.Show(lErrorMessage);
                        return;
                    }
                case Subs.Business.CustomerBiz.PaymentValidationResult.NegativeNumber:
                    {
                        MessageBox.Show(lErrorMessage);
                        return;
                    }


                case Subs.Business.CustomerBiz.PaymentValidationResult.TooMuch:
                    {
                        if (Settings.Authority >= 2)
                        {
                            // Promp the superuser for a response
                            if (MessageBoxResult.No == MessageBox.Show(lErrorMessage + ". Do you want to continue anyway?", "Warning", MessageBoxButton.YesNo))
                            {
                                return;
                            }
                            else
                            {
                                ExceptionData.WriteException(3, "A payment has been excepted for more than the Customer ows.", this.ToString(), "buttonPostpayment", "");
                                break;
                            }
                        }
                        else
                        {
                            // This is not an authorised user. Do not allow the capture
                            MessageBox.Show(lErrorMessage);
                            return;
                        }
                    }

                case Subs.Business.CustomerBiz.PaymentValidationResult.InvalidInvoice:
                    break; // Accept it in this context
                case Subs.Business.CustomerBiz.PaymentValidationResult.OK:
                    {
                        break;
                    }
                default:
                    {
                        MessageBox.Show("Unknown validation condition");
                        return;
                    }
            }  // end of switch

            // Do the overall payment

            int lPaymentTransactionId = 0;

            {
                string lResult2;

                if ((lResult2 = CustomerBiz.Pay(ref lRecord, out lPaymentTransactionId)) != "OK")
                {
                    MessageBox.Show(lResult2);
                    return;
                }
            }

            PopulatePaymentAndInvoice2();

            string lResult3;

            if ((lResult3 = StatementControl.SendEmail(CreateAStatement(), gCurrentCustomer.CustomerId, gCurrentCustomer.StatementEmail)) != "OK")
            {
                MessageBox.Show(lResult3);
                return;
            }

            MessageBox.Show("Payment completed and Statement successfully Emailed to " + gCurrentCustomer.CustomerId.ToString());
        }

        //private void AutomaticAllocate()
        //{
        //    try
        //    {
        //        this.Cursor = Cursors.Wait;
        //        {
        //            string lResult;

        //            if ((lResult = CustomerBiz.DistributeAllPayments(gCurrentCustomer.CustomerId)) != "OK")
        //            {
        //                MessageBox.Show(lResult);
        //                return;
        //            }
        //        }
        //        PopulatePaymentAndInvoice();
        //    }
        //    finally
        //    {
        //        this.Cursor = Cursors.Arrow;
        //    }
        //}

        //private void ButtonAllocate_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!gCurrentCustomer.AutomaticPaymentAllocation)
        //    {
        //        MessageBox.Show("I cannot do automatic payment allocation for this customer. You will have to do it manually.");
        //        return;
        //    }

        //    AutomaticAllocate();
        //    MessageBox.Show("Done!");
        //}

        private void InvoiceDataGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            foreach (InvoiceAndPayment lRow in InvoiceDataGrid.Items)
            {
                if (lRow.OperationId == (int)Operation.Init_Sub)
                {
                    DataGridRow lDataGridRow = (DataGridRow)InvoiceDataGrid.ItemContainerGenerator.ContainerFromItem(lRow);
                    GetCell(InvoiceDataGrid, lDataGridRow, 3).Background = new SolidColorBrush(Colors.OrangeRed);
                }
            }
        }


        #endregion

        #region Invoice tab payment contextmenu


        private void PaymentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string lStage = "Start";

            try
            {

                if (InvoiceDataGrid.Items == null)
                {
                    return;
                }

                lStage = "Before foreach";

                foreach (InvoiceAndPayment lRow in InvoiceDataGrid.Items)
                {
                    lStage = "Within foreach";
                    if (lRow.OperationId == (int)Operation.AllocatePaymentToInvoice)
                    {
                        if (InvoiceDataGrid.ItemContainerGenerator.ContainerFromItem(lRow) == null)
                        {
                            continue;
                        }
                        DataGridRow lDataGridRow = (DataGridRow)InvoiceDataGrid.ItemContainerGenerator.ContainerFromItem(lRow);
                        GetCell(InvoiceDataGrid, lDataGridRow, 1).Background = lDataGridRow.Background;
                        GetCell(InvoiceDataGrid, lDataGridRow, 5).Background = lDataGridRow.Background;
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "PaymentDataGrid_SelectionChanged", "Stage = " + lStage);
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);
            }

        }


        private void Click_HighlightAllocations(object sender, RoutedEventArgs e)
        {
            InvoiceAndPayment lPayment = (InvoiceAndPayment)gPaymentViewSource.View.CurrentItem;
            if (lPayment.OperationId != (int)Operation.Pay && lPayment.OperationId != (int)Operation.Balance)
            {
                MessageBox.Show("Sorry, I respond only to payment lines.");
                return;
            }

            foreach (InvoiceAndPayment lRow in InvoiceDataGrid.Items)
            {
                if (lRow.OperationId == (int)Operation.AllocatePaymentToInvoice)
                {
                    DataGridRow lDataGridRow = (DataGridRow)InvoiceDataGrid.ItemContainerGenerator.ContainerFromItem(lRow);

                    if (lRow.TransactionId == lPayment.TransactionId)
                    {
                        GetCell(InvoiceDataGrid, lDataGridRow, 1).Background = new SolidColorBrush(Colors.LightSeaGreen);
                        GetCell(InvoiceDataGrid, lDataGridRow, 5).Background = new SolidColorBrush(Colors.LightSeaGreen);
                    }
                }
            }
        }


        //private void Click_SelectPaymentForAllocation(object sender, RoutedEventArgs e)
        //{
        //    if (gCurrentCustomer.AutomaticPaymentAllocation)
        //    {
        //        MessageBox.Show("This feature is not availible for this customer.");
        //        return;
        //    }

        //    InvoicesAndPayments lInvoice = (InvoicesAndPayments)gPaymentViewSource.View.CurrentItem;
        //    if (lInvoice.Operation != "Payment")
        //    {
        //        MessageBox.Show("Sorry, I respond only to payment lines.");
        //        return;
        //    }


        //    if (lInvoice.Balance >= 0)
        //    {
        //        MessageBox.Show("This payment has nothing left to allocate");
        //        gSelectedPayment = null;
        //        return;
        //    }

        //    gSelectedPayment = lInvoice;
        //    MessageBox.Show("OK, you have select the payment with transaction id = " + lInvoice.TransactionId.ToString());
        //}


        //private void Button_SetPaymentCheckpoint(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {

        //        InvoicesAndPayments lInvoiceAndPayment = (InvoicesAndPayments)gPaymentViewSource.View.CurrentItem;
        //        if (lInvoiceAndPayment.Operation != "Payment")
        //        {
        //            MessageBox.Show("Sorry, I respond only to payment lines.");
        //            return;
        //        }

        //        DateTime lPreviousValue = gCurrentCustomer.CheckpointDatePayment;

        //        ElicitDate lElicitDate = new ElicitDate("Please supply a date before the payments that you want to include");
        //        lElicitDate.ShowDialog();
        //        gCurrentCustomer.CheckpointDatePayment = lElicitDate.Answer;

        //        {
        //            string lResult;

        //            if ((lResult = gCurrentCustomer.Update()) != "OK")
        //            {
        //                MessageBox.Show(lResult);
        //                return;
        //            }
        //        }

        //        Cursor = Cursors.Wait;
        //        AutomaticAllocate();

        //        {
        //            string lResult;

        //            if ((lResult = CustomerBiz.SynchronizeLiability(gCurrentCustomer)) != "OK")
        //            {
        //                MessageBox.Show(lResult);
        //                return;
        //            }
        //        }

        //        ExceptionData.WriteException(5, "PreviousValue = " + lPreviousValue.ToString() + "NewValue = " + gCurrentCustomer.CheckpointDatePayment, this.ToString(),
        //            "ButtonSetPaymentCheckpoint", "");

        //        MessageBox.Show("Done");

        //        return;
        //    }
        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Button_SetPaymentCheckpoint", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        MessageBox.Show("Error in Click_SetAsPaymentCheckpoint: " + ex.Message);
        //    }
        //    finally
        //    {
        //        Cursor = Cursors.Arrow;
        //    }
        //}


        //private void Click_SetAsPaymentCheckpoint1(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        InvoicesAndPayments lInvoiceAndPayment = (InvoicesAndPayments)gPaymentViewSource.View.CurrentItem;
        //        if (lInvoiceAndPayment.Operation != "Payment")
        //        {
        //            MessageBox.Show("Sorry, I respond only to payment lines.");
        //            return;
        //        }

        //        // Ensure that the checkpoint payment can cover all previously active subscriptions

        //        decimal lPastSubscriptionValue = InvoiceData.ActiveSubscriptionsBefore(lInvoiceAndPayment.Date, gCurrentCustomer.CustomerId);

        //        if (lPastSubscriptionValue > -lInvoiceAndPayment.Value)
        //        {
        //            MessageBox.Show("No can do! This payment does not cover all previous subscriptions.");
        //            return;
        //        }

        //        gCurrentCustomer.CheckpointPaymentDate = lInvoiceAndPayment.Date.AddDays(1);
        //        gCurrentCustomer.CheckpointPaymentValue = 0M;

        //        Cursor = Cursors.Wait;


        //        string lResult;
        //        if ((lResult = gCurrentCustomer.Update()) != "OK")
        //        {
        //            return;
        //        }

        //        //PopulatePaymentAndInvoice();
        //        AutomaticAllocate();

        //        {
        //            string lResult2;

        //            if ((lResult2 = CustomerBiz.SynchronizeLiability(gCurrentCustomer)) != "OK")
        //            {
        //                MessageBox.Show(lResult2);
        //                return;
        //            }
        //        }
        //        MessageBox.Show("Done");

        //        return;
        //    }
        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Button_SetAsPaymentCheckpoint1", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        MessageBox.Show("Error in Click_SetAsPaymentCheckpoint: " + ex.Message);
        //    }
        //    finally
        //    {
        //        Cursor = Cursors.Arrow;
        //    }
        //}

        //private void Click_ResetAllCheckpoints(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        gCurrentCustomer.CheckpointDatePayment = DateTime.Parse("2017/06/01");
        //        //gCurrentCustomer.CheckpointPaymentValue = 0M;
        //        gCurrentCustomer.CheckpointDateInvoice = DateTime.Parse("2017/06/01");
        //        //gCurrentCustomer.CheckpointPaymentTransactionId = 0;
        //        gCurrentCustomer.Update();

        //        //PopulatePaymentAndInvoice();
        //        this.AutomaticAllocate();

        //        {
        //            string lResult;

        //            if ((lResult = CustomerBiz.SynchronizeLiability(gCurrentCustomer)) != "OK")
        //            {
        //                MessageBox.Show(lResult);
        //                return;
        //            }
        //        }

        //        MessageBox.Show("Done");

        //        return;
        //    }
        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Click_ResetAllCheckpoints", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        MessageBox.Show("Error in Button_SetCheckpointInvoice: " + ex.Message);
        //    }
        //}

        //private void Click_SetAsReducedCheckpoint(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        InvoicesAndPayments lInvoiceAndPayment = (InvoicesAndPayments)gPaymentViewSource.View.CurrentItem;
        //        if (lInvoiceAndPayment.OperationId != (int)Operation.Pay && lInvoiceAndPayment.OperationId != (int)Operation.Balance)
        //        {
        //            MessageBox.Show("Sorry, I respond only to the payment that you plan to reduce.");
        //            return;
        //        }

        //        ElicitDecimal lElicitDecimal = new ElicitDecimal("Please supply the balance pertaining to the future in terms of a positive number.");
        //        lElicitDecimal.ShowDialog();

        //        gCurrentCustomer.CheckpointPaymentTransactionId = lInvoiceAndPayment.TransactionId;
        //        gCurrentCustomer.CheckpointPaymentDate = lInvoiceAndPayment.Date.AddDays(1);
        //        gCurrentCustomer.CheckpointPaymentValue = -lElicitDecimal.Answer;

        //        string lResult;
        //        if ((lResult = gCurrentCustomer.Update()) != "OK")
        //        {
        //            return;
        //        }

        //        Cursor = Cursors.Wait;

        //        PopulatePaymentAndInvoice();
        //        this.AutomaticAllocate();

        //        {
        //            string lResult2;

        //            if ((lResult2 = CustomerBiz.SynchronizeLiability(gCurrentCustomer)) != "OK")
        //            {
        //                MessageBox.Show(lResult2);
        //                return;
        //            }
        //        }

        //        //gCustomerAdapter.FillById(gCustomer, "CustomerId", gCurrentCustomer.CustomerId, "");

        //        MessageBox.Show("Done");

        //        return;
        //    }
        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Button_SetCheckpointPayment", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        MessageBox.Show("Error in Button_SetCheckpointPayment: " + ex.Message);
        //    }
        //    finally
        //    {
        //        Cursor = Cursors.Arrow;
        //    }
        //}


        private void Click_ReversePayment(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            try
            {

                InvoiceAndPayment lInvoice = (InvoiceAndPayment)gPaymentViewSource.View.CurrentItem;
                if (lInvoice.OperationId != (int)Operation.Pay)
                {
                    MessageBox.Show("Sorry, I respond only to payment lines with a valid invoice.");
                    return;
                }

                ElicitString lElicitString = new ElicitString("Please supply a reason.");
                lElicitString.ShowDialog();
                if (lElicitString.Answer.Length < 3)
                {
                    MessageBox.Show("You have to supply a reason");
                    return;
                }

                int lReverseTransactionId = 0;

                {
                    string lResult;

                    if ((lResult = CustomerBiz.ReversePayment(gCurrentCustomer, lInvoice.TransactionId,
                        lInvoice.Value, lElicitString.Answer, out lReverseTransactionId)) != "OK")
                    {
                        MessageBox.Show(lResult);
                        return;
                    }
                }

                //PopulatePaymentAndInvoice2(); // You also have to do the reallocation

                GoToStatement();
                //AutomaticAllocate();

                // Propagate the new liability to the current view
                //CustomerData3 lModifiedCustomer = new CustomerData3(gCurrentCustomer.CustomerId);
                //gCurrentCustomer.Liability = lModifiedCustomer.Liability;

                MessageBox.Show("Done");

                return;


            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Click_ReversePayment", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Failed due to technical error");
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void Click_Refund(object sender, RoutedEventArgs e)
        {

            try
            {
                // Check that you are standing on a payment line.
                InvoiceAndPayment lInvoice = (InvoiceAndPayment)gPaymentViewSource.View.CurrentItem;
                if (lInvoice.Operation != "Payment")
                {
                    MessageBox.Show("Sorry, I respond only to payment lines");
                    return;
                }

                ElicitDate lElicitDate = new ElicitDate("Please supply the effective date for the refund.");
                lElicitDate.ShowDialog();

                ElicitDecimal lElicitDecimal = new ElicitDecimal("Please provide the amount to be refunded.");
                lElicitDecimal.ShowDialog();

                if (lElicitDecimal.Answer == 0)
                {
                    MessageBox.Show("You did not supply me with a refund amount.");
                    return;
                }

                if (lElicitDecimal.Answer > -lInvoice.Value)
                {
                    MessageBox.Show("Sorry, I cannot refund an amount larger than the payment value.");
                    return;
                }


                {
                    string lResult;

                    if ((lResult = CustomerBiz.Refund(lInvoice.TransactionId, gCurrentCustomer.CustomerId, lElicitDecimal.Answer, lElicitDate.Answer)) != "OK")
                    {
                        MessageBox.Show(lResult);
                        return;
                    }
                }

                //PopulatePaymentAndInvoice2();

                GoToStatement();

                MessageBox.Show("Done");
                return;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Click_Refund", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Click_Refund failed: " + ex.Message);
            }
        }


        private void Click_AssignRefund(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check that you are standing on a refund line.
                InvoiceAndPayment lInvoice = (InvoiceAndPayment)gPaymentViewSource.View.CurrentItem;
                if (lInvoice.Operation != "Refund")
                {
                    MessageBox.Show("Sorry, I respond only to refund lines");
                    return;
                }


                ElicitInteger lElicit = new ElicitInteger("Please provide the transactionid of the payment.");
                lElicit.ShowDialog();

                if (lElicit.Answer < 1)
                {
                    MessageBox.Show("Sorry, that is not a valid payment transactionid");
                    return;
                }

                if (LedgerData.AssignRefund(int.Parse(lInvoice.Reference2), gCurrentCustomer.CustomerId, lElicit.Answer))
                {
                    //AutomaticAllocate();
                    MessageBox.Show("Done");
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Click_AssignRefund", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Click_AssignRefund failed: " + ex.Message);
            }
        }

        #endregion

        #region Invoice tab invoices

        private void Click_Checkpoint(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                InvoiceAndPayment lInvoice = (InvoiceAndPayment)gDueViewSource.View.CurrentItem;
                if (lInvoice.OperationId != (int)Operation.VATInvoice)
                {
                    MessageBox.Show("Sorry, I respond only to invoice lines");
                    return;
                }
                
                //Try to set a new checkpoint
                // Save originals
                int lOriginalBalanceInvoiceId = gCurrentCustomer.BalanceInvoiceId;
                decimal lOriginalBalance = gCurrentCustomer.Balance;
                decimal lOriginalDue = gCurrentCustomer.Due;
                //decimal lNewBalance = gCurrentCustomer.CalculateBalanceByInvoice(lInvoice.InvoiceId);
               
                try
                {
                   // if (MessageBoxResult.No == MessageBox.Show("Do you want to provide your own starting balance?",
                   //"Warning", MessageBoxButton.YesNo))
                   // {
                  
                   //     if (Math.Abs(lNewBalance) < 1M)
                   //     {
                   //         lNewBalance = 0;   // Get rid of decimal point variations
                   //     }

                   //     if (lNewBalance > 0)
                   //     {
                   //         MessageBox.Show("No can do, you first have to write off " + lNewBalance.ToString("######0.######") + " from some of the previous invoices.");
                   //         return;
                   //     }

                   //     if (lNewBalance < 0)
                   //     {
                   //         MessageBox.Show("No can do, you first have to refund " + lNewBalance.ToString("######0.######") +
                   //             " or get the client to do a reduced payment.");
                   //         return;
                   //     }

                   //     gCurrentCustomer.Balance = lNewBalance;
                   // }

                   // else
                   // {
                   //     // Override the past by providing your own balance value
                   //     ElicitDecimal lElicitDecimal = new ElicitDecimal("What should the new starting balance be?");
                   //     lElicitDecimal.ShowDialog();
                   //     gCurrentCustomer.Balance = lElicitDecimal.Answer;  
                   // }

                    gCurrentCustomer.BalanceInvoiceId = lInvoice.InvoiceId;  // The sequence is important
                    gCurrentCustomer.Update();
                    LedgerData.ChangeCheckpoint(gCurrentCustomer.CustomerId, lInvoice.InvoiceId, lOriginalBalanceInvoiceId, lOriginalBalance);
                    MessageBox.Show("Checkpoints successfully changed by force.");
                }
                catch (Exception InnerException)
                {
                    gCurrentCustomer.BalanceInvoiceId = lOriginalBalanceInvoiceId;
                    gCurrentCustomer.Balance = lOriginalBalance;
                    gCurrentCustomer.Update();
                    throw InnerException;
                }

                //if (Math.Abs(gCurrentCustomer.Due - lOriginalDue) < 1M)
                //{
                //    GoToStatement();  //Refresh the displayed statement
                //    MessageBox.Show("Checkpoints successfully changed.");

                //}
                //else
                //{
                //    gCurrentCustomer.BalanceInvoiceId = lOriginalBalanceInvoiceId;
                //    gCurrentCustomer.Balance = lOriginalBalance;
                //    gCurrentCustomer.Update();
                //    ExceptionData.WriteException(1, "Difference in due values, this.ToString()", this.ToString(), "ClickCheckpoint", "CustomerId = " + gCurrentCustomer.CustomerId.ToString()
                //        + "New = " + gCurrentCustomer.Due.ToString("#0.000000")
                //        + " Original = " + lOriginalDue.ToString("#0.000000"));

                //    MessageBox.Show("Checkpoints change failed. No changes made to database.");
                //}
                return;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Click_Checkpoint", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Error in Click_Checkpoint " + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }

        }
        //private void ButtonSetCheckpointInvoice(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        ElicitDate lElicitDate = new ElicitDate("Please supply a date to start reporting on invoices");
        //        lElicitDate.ShowDialog();

        //        decimal lPastSubscriptionValue = InvoiceData.ActiveSubscriptionsBefore(lElicitDate.Answer, gCurrentCustomer.CustomerId);

        //        if (lPastSubscriptionValue > 0)
        //        {
        //            MessageBox.Show("No can do! There are active subscriptions valued at " + lPastSubscriptionValue.ToString() + " before this date.");
        //            return;
        //        }

        //        DateTime lPreviousValue = gCurrentCustomer.CheckpointDateInvoice;

        //        gCurrentCustomer.CheckpointDateInvoice = lElicitDate.Answer;
        //        gCurrentCustomer.Update();

        //        Cursor = Cursors.Wait;

        //        PopulatePaymentAndInvoice();
        //        //this.AutomaticAllocate();

        //        {
        //            string lResult;

        //            if ((lResult = CustomerBiz.SynchronizeLiability(gCurrentCustomer)) != "OK")
        //            {
        //                MessageBox.Show(lResult);
        //                return;
        //            }
        //        }

        //        ExceptionData.WriteException(5, "PreviousValue = " + lPreviousValue.ToString() + "NewValue = " + gCurrentCustomer.CheckpointDateInvoice, this.ToString(),
        //        "ButtonSetInvoiceCheckpoint", "");

        //        MessageBox.Show("Done");

        //        return;
        //    }
        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Button_SetCheckpointInvoice", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        MessageBox.Show("Error in Button_SetCheckpointINvoice: " + ex.Message);
        //    }
        //    finally
        //    {
        //        Cursor = Cursors.Arrow;
        //    }
        //}

        private void ButtonCreateStatement_Click(object sender, RoutedEventArgs e)
        {

            this.Cursor = Cursors.Wait;

            try
            {
                CreateAStatement();
                MessageBox.Show("Statement created on M: drive"); //in "SelectTab(PickerTabs.ByInvoiceAndPayment);
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonCreateStatement_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private int CreateAStatement()
        {
            try
            {
                int lStatementId = 0;
                CounterData.GetUniqueNumber("Statement", ref lStatementId);
                StatementControl lStatementControl = new StatementControl(gCurrentCustomer.CustomerId, lStatementId);
                LedgerData.Statement(gCurrentCustomer.CustomerId, lStatementId, lStatementControl.StatementValue);
                return lStatementId;
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "CreateAStatement", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return 0;
            }
        }

        //private void Click_AllocatePayment(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (gCurrentCustomer.AutomaticPaymentAllocation)
        //        {
        //            MessageBox.Show("This feature is not availible for this customer.");
        //            return;
        //        }

        //        if (gSelectedPayment == null)
        //        {
        //            MessageBox.Show("You have not selected a payment to allocate");
        //            return;
        //        }

        //        InvoicesAndPayments lInvoice = (InvoicesAndPayments)gInvoiceViewSource.View.CurrentItem;
        //        if (!lInvoice.LastRow)
        //        {
        //            MessageBox.Show("Sorry, I respond only to the last row on the invoice.");
        //            return;
        //        }

        //        CustomerData3.OutstandingInvoice lOutstandingInvoice = new CustomerData3.OutstandingInvoice();
        //        lOutstandingInvoice.InvoiceId = lInvoice.InvoiceId;
        //        lOutstandingInvoice.Balance = lInvoice.Balance;

        //        CustomerBiz.DistributePaymentToInvoice(gSelectedPayment.TransactionId, gSelectedPayment.Balance, lOutstandingInvoice);

        //        gSelectedPayment = null;

        //        PopulatePaymentAndInvoice();
        //        MessageBox.Show("Done");
        //        return;
        //    }
        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Click_AllocatePayment", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        MessageBox.Show("Failed due to technical error");
        //    }
        //}
        //private void Click_DeallocatePayment(object sender, RoutedEventArgs e)
        //{
        //    if (gCurrentCustomer.AutomaticPaymentAllocation)
        //    {
        //        MessageBox.Show("This feature is not availible for this customer.");
        //        return;
        //    }

        //    InvoicesAndPayments lInvoice = (InvoicesAndPayments)gInvoiceViewSource.View.CurrentItem;
        //    if (lInvoice.Operation != "PaymentAllocation")
        //    {
        //        MessageBox.Show("Sorry, I respond only to payment lines with a valid invoice.");
        //        return;
        //    }

        //    CustomerBiz.DeallocateByInvoiceId(lInvoice.InvoiceId);
        //    PopulatePaymentAndInvoice();
        //    MessageBox.Show("Done");
        //}

        private void Click_WriteOffMoney(object sender, RoutedEventArgs e)
        {

            try
            {
                InvoiceAndPayment lInvoice = (InvoiceAndPayment)gInvoiceViewSource.View.CurrentItem;
                if (lInvoice.OperationId != (int)Operation.VATInvoice)
                {
                    MessageBox.Show("Sorry, I respond only to invoice lines");
                    return;
                }

                ElicitDecimal lElicitDecimal = new ElicitDecimal("How much money should be written off?");
                lElicitDecimal.ShowDialog();


                if (lElicitDecimal.Answer > lInvoice.Value)
                {
                    // Prevent writing off more than the value of the invoice
                    lElicitDecimal.Answer = lInvoice.Value;
                }

                ElicitString lElicitString = new ElicitString("Please supply a reason.");
                lElicitString.ShowDialog();
                if (lElicitString.Answer.Length < 3)
                {
                    MessageBox.Show("You have to supply a reason");
                    return;
                }

                // Build a Payment record

                Subs.Data.PaymentData.PaymentRecord lPaymentRecord = new Subs.Data.PaymentData.PaymentRecord();
                lPaymentRecord.CustomerId = gCurrentCustomer.CustomerId;
                lPaymentRecord.InvoiceId = lInvoice.InvoiceId;
                lPaymentRecord.Amount = lElicitDecimal.Answer;
                lPaymentRecord.Explanation = lElicitString.Answer;

                if (CustomerBiz.WriteOffMoney(ref lPaymentRecord, out int lPaymentTransactionId))
                {

                    //AutomaticAllocate();
                    MessageBox.Show("Done");
                    GoToStatement();
                }
                else
                {
                    MessageBox.Show("There was a problem in excuting the write-off");
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "WriteOffMoney", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("WriteOffMOney failed due to technical error" + ex.Message);
            }
        }

        private void Click_RecreateInvoice(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                InvoiceAndPayment lInvoice = (InvoiceAndPayment)gInvoiceViewSource.View.CurrentItem;
                if (lInvoice.OperationId != (int)Operation.VATInvoice)
                {
                    MessageBox.Show("Sorry, I respond only to invoice lines");
                    return;
                }

                InvoiceControl lInvoiceControl = new InvoiceControl();
                string lResult;
                if ((lResult = lInvoiceControl.LoadAndRenderInvoice(lInvoice.InvoiceId)) != "OK")
                {
                    MessageBox.Show(lResult);
                    return;
                }
                else
                {
                    MessageBox.Show("Done! Invoice created on M:drive");
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Click_RecreateInvoice", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Click_RecreateInvoice failed due to technical error" + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }

        }

        private void Click_RecreateCreditNotes(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                InvoiceAndPayment lInvoice = (InvoiceAndPayment)gInvoiceViewSource.View.CurrentItem;
                if (lInvoice.OperationId != (int)Operation.CreditNote)
                {
                    MessageBox.Show("Sorry, I respond only to creditnote lines");
                    return;
                }

                CreditNote lCreditNote = new CreditNote(lInvoice.InvoiceId);
                lCreditNote.ShowDialog();
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Click_RecreateCreditNotes", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Click_RecreateCreditNotes failed due to technical error" + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }




        private void Click_ReverseWriteOffMoney(object sender, RoutedEventArgs e)
        {
            try
            {
                InvoiceAndPayment lInvoice = (InvoiceAndPayment)gInvoiceViewSource.View.CurrentItem;
                if (lInvoice.OperationId != (int)Operation.WriteOffMoney)
                {
                    MessageBox.Show("Sorry, I respond only to write off lines");
                    return;
                }

                ElicitString lElicitString = new ElicitString("Please supply a reason.");
                lElicitString.ShowDialog();
                if (lElicitString.Answer.Length < 3)
                {
                    MessageBox.Show("You have to supply a reason");
                    return;
                }

                {
                    string lResult;

                    if ((lResult = CustomerBiz.ReverseWriteOffMoney(lInvoice.TransactionId, lInvoice.InvoiceId, gCurrentCustomer.CustomerId, -lInvoice.Value, lElicitString.Answer)) != "OK")
                    {
                        MessageBox.Show(lResult);
                        return;
                    }
                }
                //AutomaticAllocate();
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "WriteOffMoney", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("WriteOffMOney failed due to technical error" + ex.Message);
            }
        }

        private void Click_ViewSubscriptions(object sender, RoutedEventArgs e)
        {
            ViewSubscriptions();
        }

        private void ViewSubscriptions()
        {

            gSubscriptionStatusDisplayControl.Clear();

            InvoiceAndPayment lInvoice = (InvoiceAndPayment)gInvoiceViewSource.View.CurrentItem;
            if (!lInvoice.LastRow)
            {
                MessageBox.Show("Sorry, I respond only to totals lines.");
                return;
            }

            if (lInvoice.InvoiceId == 0)
            {
                // This means that the invoice run has NOT been run against this new subscription yet.
                if (PopulateSubscriptionsByPayer(lInvoice.Date, gCurrentCustomer.CustomerId))
                {
                    SelectTab(PickerTabs.Subscription);
                }
            }
            else
            {
                if (PopulateSubscriptions(lInvoice.InvoiceId))
                {
                    SelectTab(PickerTabs.Subscription);
                }
            }
        }

        private void InvoiceDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewSubscriptions();
        }

        //private void InvoiceItem_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    Tab_Invoice.Visibility = Visibility.Collapsed;
        //    CustomerId.Text = "";
        //    gPaymentViewSource.Source = null;
        //    gInvoiceViewSource.Source = null;
        //}

        #endregion

        #region Subscriptions tab

        private bool PopulateSubscriptions(int pInvoiceId)
        {
            try
            {
                MIMSDataContext lContext = new MIMSDataContext(Settings.ConnectionString);
                List<SelectedSubscription> lSubscriptions = lContext.MIMS_DataContext_SubscriptionsByInvoice(pInvoiceId).ToList();

                gSubscriptionsViewSource.Source = lSubscriptions;

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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "PopulateSubscriptions", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Error in Populate Subscriptions: " + ex.Message);

                return false;
            }

        }

        private void DisplayStatusAndHistory(object sender, MouseButtonEventArgs e)
        {
            DisplayStatusAndHistory();
            e.Handled = false;
        }

        private void DisplayStatusAndHistory()
        {
            int lCurrentSubscriptionId = 0;

            try
            {
                SelectedSubscription lSubscription = (SelectedSubscription)gSubscriptionsViewSource.View.CurrentItem;
                lCurrentSubscriptionId = lSubscription.SubscriptionId;


                // Invoke a report of status and history for a specific subscription.
                this.Cursor = Cursors.Wait;
                gSubscriptionStatusDisplayControl.SubscriptionId = lCurrentSubscriptionId;
                return;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "DisplayStatusAndHistory", "SubscriptionId = " + lCurrentSubscriptionId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void Click_StatusAndHistory(object sender, RoutedEventArgs e)
        {
            DisplayStatusAndHistory();
        }

        private void TabSubscription_LostFocus(object sender, RoutedEventArgs e)
        {
            gSubscriptionStatusDisplayControl.ClearValue(SubscriptionStatusDisplayControl2.SubscriptionIdProperty);
        }

        private bool PopulateSubscriptionsByPayer(DateTime pDate, int pPayerId)
        {
            try
            {
                MIMSDataContext lContext = new MIMSDataContext(Settings.ConnectionString);
                List<SelectedSubscription> lSubscriptions = lContext.MIMS_DataContext_SubscriptionsByDate(pDate, pPayerId).ToList();

                gSubscriptionsViewSource.Source = lSubscriptions;

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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "PopulateSubscriptions", "PayerId" + pPayerId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Error in Populate Subscriptions: " + ex.Message);

                return false;
            }

        }

        private void Click_DeliverOnCredit(object sender, RoutedEventArgs e)
        {
            SelectedSubscription lSubscription = (SelectedSubscription)gSubscriptionsViewSource.View.CurrentItem;
            Subs.Presentation.PaymentAllocation lPaymentAllocation = new Subs.Presentation.PaymentAllocation(lSubscription.SubscriptionId);
            lPaymentAllocation.ShowDialog();
        }

        #endregion

        #region Liability calculation tab
        //private void ButtonSynchroniseLiability_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        gCurrentCustomer.Liability = gCalculatedLiability;
        //        gCurrentCustomer.Update();
        //        ExceptionData.WriteException(2, "Synchronise liability manually", this.ToString(), "ButtonSynchroniseLiability_Click", "CustomerId = "
        //              + gCurrentCustomer.CustomerId.ToString() + " Calculated = " + gCalculatedLiability.ToString());

        //        textCurrentLiability.Text = gCalculatedLiability.ToString("########0.00");
        //        MessageBox.Show("Done");
        //        PopulatePaymentAndInvoice2();
        //        tabControl1.SelectedIndex = (int)PickerTabs.Select;
        //    }

        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ButtonSynchroniseLiability_Click", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        MessageBox.Show("Error in ButtonSynchroniseLiability_Click: " + ex.Message);
        //    }
        //}

        private void ButtonRunningToXML_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string lOutputFile = "c:\\SUBS" + "\\" + gCurrentCustomer.CustomerId.ToString() + "RunningBalances.xml";
                FileStream lFileStream = new FileStream(lOutputFile, FileMode.Create);
                XmlSerializer lSerializer = new XmlSerializer(typeof(List<LiabilityRecord>));
                lSerializer.Serialize(lFileStream, gLiabilityRecords);
                MessageBox.Show(gLiabilityRecords.Count.ToString() + " Records written to " + lOutputFile.ToString());
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ButtonRunningToXML_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
                return;
            }
        }

        #endregion

        #region Over paid delivered tab

        private void buttonPaidTooMuch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Functions.IsDecimal(this.textOverPaid.Text))
                {
                    MessageBox.Show("Sorry, this has to be a decimal number.");
                    return;
                }

                decimal lAbsoluteValue = Convert.ToDecimal(this.textOverPaid.Text);

                if (lAbsoluteValue < 0)
                {
                    MessageBox.Show("Sorry, this has to be a positive number.");
                    return;
                }


                // OK do the job

                CustomerDoc2 lCustomerDoc = new CustomerDoc2();

                Subs.Data.CustomerDoc2TableAdapters.DiscrepanciesTableAdapter lDiscrepancyAdapter = new Subs.Data.CustomerDoc2TableAdapters.DiscrepanciesTableAdapter();
                lDiscrepancyAdapter.AttachConnection();

                lDiscrepancyAdapter.FillBy(lCustomerDoc.Discrepancies, lAbsoluteValue, "Creditor");

                OverDeliveredDataGrid.Visibility = Visibility.Hidden;
                PaidTooMuchDataGrid.Visibility = Visibility.Visible;

                gTooMuchTooLittleViewSource.Source = lCustomerDoc.Discrepancies;

                //MessageBox.Show("Written to " + FileName.ToString());

            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonPaidTooMuch_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }


        }

        private void buttonOverDelivered_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (!Functions.IsDecimal(this.textOverDelivered.Text))
                {
                    MessageBox.Show("Sorry, this has to be a decimal number.");
                    return;
                }

                decimal lAbsoluteValue = Convert.ToDecimal(this.textOverDelivered.Text);

                if (lAbsoluteValue < 0)
                {
                    MessageBox.Show("Sorry, this has to be a positive number.");
                    return;
                }

                // OK do the job

                CustomerDoc2 lCustomerDoc = new CustomerDoc2();

                Subs.Data.CustomerDoc2TableAdapters.DiscrepanciesTableAdapter lDiscrepancyAdapter = new Subs.Data.CustomerDoc2TableAdapters.DiscrepanciesTableAdapter();
                lDiscrepancyAdapter.AttachConnection();

                lDiscrepancyAdapter.FillBy(lCustomerDoc.Discrepancies, lAbsoluteValue, "Debtor");

                PaidTooMuchDataGrid.Visibility = Visibility.Hidden;
                OverDeliveredDataGrid.Visibility = Visibility.Visible;


                foreach (CustomerDoc2.DiscrepanciesRow lRow in lCustomerDoc.Discrepancies)
                {
                    lRow.Liability = -lRow.Liability;
                }


                gTooMuchTooLittleViewSource.Source = lCustomerDoc.Discrepancies;
                // MessageBox.Show("Written to " + FileName.ToString());

            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonOverDelivered_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }


        }

        private void OverPaidDelivered_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OverPaidDeliveredSelect();
            SelectCurrentCustomer();
            GoToStatement();
        }

        private void OverPaidDeliveredSelect()
        {
            try
            {
                DataRowView lRowView = (DataRowView)gTooMuchTooLittleViewSource.View.CurrentItem;
                CustomerDoc2.DiscrepanciesRow lRow = (CustomerDoc2.DiscrepanciesRow)lRowView.Row;
                if (lRow == null) return;

                gHits = gCustomerAdapter.FillById(gCustomer, "CustomerId", (int)lRow.CustomerId, "");

                // At this point, one search should have been done.

                if (gHits == 0)
                {
                    MessageBox.Show("No customer found");
                    return;
                }

                gCollectionViewSourceCustomer.Source = gCustomer;
                gCollectionViewSourceCustomer.View.MoveCurrentToFirst();
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "DebtorCreditorSelect", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Error in DebtorCreditorSelect " + ex.Message);
            }

        }

        private void Click_TooMuchTooLittleInvoice(object sender, RoutedEventArgs e)
        {
            OverPaidDeliveredSelect();
            SelectCurrentCustomer();
            GoToStatement();
        }



        #endregion

     
    }
}


