using Subs.Business;
using Subs.Data;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Subs.Presentation
{
    /// <summary>
    /// Interaction logic for SubscriptionCapture.xaml
    /// </summary>
    public partial class SubscriptionsCapture : Window
    {
        #region Globals 

        private CustomerData3 gPayer;
        private CustomerData3 gReceiver;
        private readonly SubscriptionData3 gTemplateSubscription;
        private int gProposedStartIssue = 0;
        private readonly ObservableCollection<BasketItem> gBasket = new ObservableCollection<BasketItem>();
        private readonly ProductData gProduct = new ProductData();
        private CollectionViewSource gProductViewSource;

        public enum Tabs
        {
            Select = 0,
            Basket = 1,
            BulkCapture = 2
        }



        #endregion

        #region Construction

        public void SubscriptionCaptureGeneral()
        {
            try
            {
                gProductViewSource = (CollectionViewSource)this.Resources["product2ViewSource"];
                gProductViewSource.Source = gProduct.DesktopProducts;

                comboSubscriptionType.ItemsSource = AdministrationData2.gSubscriptionType;
                comboSubscriptionType.SelectedIndex = 0;

                comboSubscriptionMedium.ItemsSource = AdministrationData2.gSubscriptionMedium;
                comboSubscriptionMedium.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "SubscriptionsCaptureGeneral", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Error in SubscriptionCaptureGeneral " + ex.Message);
            }
        }

        public SubscriptionsCapture()
        {
            InitializeComponent();
            SubscriptionCaptureGeneral();
            gTemplateSubscription = null;
        }

        public SubscriptionsCapture(SubscriptionData3 pSubscriptionData)
        {

            // This constructor is used when you renew subscriptions.

            InitializeComponent();
            SubscriptionCaptureGeneral();
            gTemplateSubscription = pSubscriptionData;

            try
            {
                // Populate the controls of SubscriptionCapture, using information in the gTemplateSubscription
                
                // Populate the payer and receiver and reneval indicators

                gReceiver = new CustomerData3(gTemplateSubscription.ReceiverId);
                this.textReceiverSurname.Text = gReceiver.Surname;
                this.textReceiverCompany.Text = gReceiver.CompanyName;

                gPayer = new CustomerData3(gTemplateSubscription.PayerId);
                this.textPayerSurname.Text = gPayer.Surname;
                this.textPayerCompany.Text = gPayer.CompanyName;

                this.checkAutomaticRenewal.IsChecked = gTemplateSubscription.AutomaticRenewal ? true : false;
                this.checkRenewal.IsChecked = gTemplateSubscription.RenewalNotice ? true : false;

                // Transfer the product selection

                Subs.Data.DeskTopProduct lProduct = (DeskTopProduct)gProduct.DesktopProducts.Where(p => p.ProductId == gTemplateSubscription.ProductId).Single();

                gProductViewSource.View.MoveCurrentToFirst();

                do
                {
                    Subs.Data.DeskTopProduct lCurrentProduct = (DeskTopProduct)gProductViewSource.View.CurrentItem;
                    if (lCurrentProduct.ProductId == lProduct.ProductId)
                    {
                        break;
                    }
                } while (gProductViewSource.View.MoveCurrentToNext());


            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "SubscriptionsCapture(SubscriptionData)", "ProductId = " + gTemplateSubscription.ProductId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Error in SubscriptionCapture(SubscriptionData) " + ex.Message);
            }
        }

        #endregion

        #region Event handlers

        private void buttonReceiver_Click(object sender, RoutedEventArgs e)
        {
            CustomerPicker3 lCustomerPicker = new CustomerPicker3();
            lCustomerPicker.ShowDialog();

            if (Settings.CurrentCustomerId != 0)
            {
                gReceiver = new CustomerData3(Settings.CurrentCustomerId);
                this.textReceiverSurname.Text = gReceiver.Surname;
                this.textReceiverCompany.Text = gReceiver.CompanyName;

            }
        }

        private void buttonSame_Click(object sender, RoutedEventArgs e)
        {
            gPayer = gReceiver;

            this.textPayerSurname.Text = this.textReceiverSurname.Text;
            this.textPayerCompany.Text = this.textReceiverCompany.Text;
        }

        private void buttonPayer_Click(object sender, RoutedEventArgs e)
        {
            CustomerPicker3 lCustomerPicker = new CustomerPicker3();
            lCustomerPicker.ShowDialog();

            if (Settings.CurrentCustomerId != 0)
            {
                gPayer = new CustomerData3(Settings.CurrentCustomerId);
                this.textPayerSurname.Text = gPayer.Surname;
                this.textPayerCompany.Text = gPayer.CompanyName;
            }

        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        public bool AddBasketItem(SubscriptionData3 pSubscriptionData)
        {
            try
            {
                {
                    MimsValidationResult lResult;

                    if ((lResult = SubscriptionBiz.SetInitialValues(pSubscriptionData, DateTime.Now)).Message != "OK")
                    {
                        if (lResult.Prompt)
                        {
                            if (MessageBoxResult.No == MessageBox.Show(lResult.Message, "Warning", MessageBoxButton.YesNo))
                            { 
                                return false; 
                            }
                        }
                    }
                }

                pSubscriptionData.RenewalNotice = (bool)checkRenewal.IsChecked;
                pSubscriptionData.AutomaticRenewal = (bool)checkAutomaticRenewal.IsChecked;
                pSubscriptionData.OrderNumber = textCaptureOrderNumber.Text;

                if ((bool)radioQuote.IsChecked)
                {
                    pSubscriptionData.Status = SubStatus.Proposed;
                }
                else
                {
                    pSubscriptionData.Status = SubStatus.Deliverable;
                }

                BasketItem lBasketItem = new BasketItem() { Subscription = pSubscriptionData };
                lBasketItem.ProductName = pSubscriptionData.ProductName;

                // Check if such subscription exists already

                Subs.Data.MimsValidationResult lValidationResult = SubscriptionBiz.Validate(pSubscriptionData);

                if (lValidationResult.Message.Contains("order number"))
                {
                    MessageBox.Show(lValidationResult.Message);
                    return false;
                }

                if (lValidationResult.Message != "OK")
                {
                    if (lValidationResult.Prompt)
                    {
                        if (lValidationResult.Message.Contains("overlaps"))
                        {
                            lBasketItem.Warning = lValidationResult.Message;
                        }
                    }
                    else
                    {
                        ExceptionData.WriteException(1, lValidationResult.Message, this.ToString(), "AddBasketItem", "Receiver = " + lBasketItem.Subscription.ReceiverId.ToString()
                            + "Issue =" + pSubscriptionData.StartIssue.ToString());
                        MessageBox.Show(lValidationResult.Message);
                        return false;
                    }
                }
                else
                {
                    lBasketItem.Warning = "";
                }

                gBasket.Add(lBasketItem);

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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "AddBasketItem", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return false;
            }
        }

        private void buttonCalculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (gPayer == null || gReceiver == null)
                {
                    MessageBox.Show("You did not give me a payer and a receiver.");
                    return;
                }

                // Create template subscriptions for the selected products

                gBasket.Clear();

                foreach (DeskTopProduct lProduct in product2DataGrid.SelectedItems.Cast<DeskTopProduct>())
                {
                    // In the case of a renewal, there will be only one new subscription

                    SubscriptionData3 lSubscription = new Subs.Data.SubscriptionData3();

                    lSubscription.PayerId = gPayer.CustomerId;
                    lSubscription.ReceiverId = gReceiver.CustomerId;
                    lSubscription.ProductId = lProduct.ProductId;
                 

                    if (gTemplateSubscription != null)
                    {
                        // This is a renewal subscription
                        lSubscription.DeliveryMethod = gTemplateSubscription.DeliveryMethod;
                        lSubscription.DeliveryAddressId = gTemplateSubscription.DeliveryAddressId;
                        int lStartIssue = gTemplateSubscription.NextIssue;
                        lSubscription.NumberOfIssues = gTemplateSubscription.NumberOfIssues;
                        if (lStartIssue != 0)
                        {
                            gProposedStartIssue = lStartIssue;

                            lSubscription.ProposedStartIssue = lStartIssue;
                            lSubscription.ProposedStartSequence = IssueBiz.GetSequenceNumber(lStartIssue);
                            lSubscription.ProposedLastSequence = lSubscription.ProposedStartSequence + lSubscription.NumberOfIssues;
                            lSubscription.ProposedLastIssue = IssueBiz.GetIssueId(lSubscription.ProductId, lSubscription.ProposedLastSequence);
                        }
                    }

                    if (!AddBasketItem(lSubscription))
                    {
                        return;
                    }

                } // End of foreach on SelectedItems

                if (!SubscriptionBiz.CalculateBasket(gBasket))
                {
                    return;
                }
                // Display the basket
                // Note that it contains only proposed values for the issues.
                // You have not yet calculated any unitprice of discounts.


                BasketGrid.ItemsSource = gBasket;

                TabControl.SelectedIndex = (int)Tabs.Basket;

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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonCalculate_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void comboSubscription_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboSubscriptionType.SelectedValue == null || comboSubscriptionMedium.SelectedValue == null)
            {
                // This is still initialisation - do not do anything.
                return;
            }

            gProductViewSource.View.Filter = (x => FilterProducts(x));
            gProductViewSource.View.Refresh();
        }

        private bool FilterProducts(object o)
        {
            DeskTopProduct lProduct = (DeskTopProduct)o;

            if ((int)comboSubscriptionType.SelectedValue == (int)SubscriptionType.Any && (int)comboSubscriptionMedium.SelectedValue == (int)SubscriptionMedium.PrintAndBrowser)
            {
                return true;
            }


            if ((int)comboSubscriptionType.SelectedValue == (int)SubscriptionType.Any)
            {
                if (lProduct.SubscriptionMedium == (int)comboSubscriptionMedium.SelectedValue)
                    return true;
                else return false;
            }

            if ((int)comboSubscriptionMedium.SelectedValue == (int)SubscriptionMedium.PrintAndBrowser)
            {
                if (lProduct.SubscriptionType == (int)comboSubscriptionType.SelectedValue)
                    return true;
                else return false;
            }


            if (lProduct.SubscriptionType == (int)comboSubscriptionType.SelectedValue && lProduct.SubscriptionMedium == (int)comboSubscriptionMedium.SelectedValue)
                return true;
            else return false;
        }

        private void BasketChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Initialise the form from the selected item.

                BasketItem lBasketItem = (BasketItem)BasketGrid.CurrentItem;

                SubscriptionCaptureChange lSubscriptionCapture = new SubscriptionCaptureChange(lBasketItem);
                lSubscriptionCapture.ShowDialog();

                if (!SubscriptionBiz.CalculateBasket(gBasket))
                {
                    return;
                }

                BasketGrid.ItemsSource = gBasket;
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "BasketChange_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return;
            }
        }


        public static string SubmitBasket(ObservableCollection<BasketItem> pBasket)
        {
            SqlConnection lConnection = new SqlConnection();
            SqlTransaction lTransaction;
            lConnection.ConnectionString = Settings.ConnectionString;

            if (lConnection.State != ConnectionState.Open)
            {
                lConnection.Open();
            }
            lTransaction = lConnection.BeginTransaction("Submit");
            try
            {
                StringBuilder lPositiveResult = new StringBuilder();
                lPositiveResult.Append("The following subscriptions have been created successfully: ");

                foreach (BasketItem lBasketItem in pBasket)
                {
                    // Initialise the subscription
                    string ResultOfInitialise = SubscriptionBiz.Initialise(lTransaction, lBasketItem.Subscription);
                    if (ResultOfInitialise != "OK")
                    {
                        lTransaction.Rollback("Submit");
                        throw new Exception(ResultOfInitialise);
                    }
                    else
                    {
                        lPositiveResult.AppendLine(lBasketItem.Subscription.SubscriptionId.ToString());
                    }
                }  //End of foreach
                lTransaction.Commit();
                return lPositiveResult.ToString();
            }
            catch (Exception ex)
            {
                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "static SubmitBasket", "SubmitBasket", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                lTransaction.Rollback("Submit");
                throw;
            }
        }

        private void buttonSubmit_Click(object sender, RoutedEventArgs e)
        {
            string lPositiveResult = SubmitBasket(gBasket);

            {
                string lResult;

                if ((lResult = ProductBiz.DeliverElectronic(gBasket[0].Subscription.ReceiverId)) != "OK")
                {
                    MessageBox.Show(lPositiveResult.ToString() + "but automatic electronic delivery failed: " + lResult);
                }
            }

            MessageBox.Show(lPositiveResult.ToString());
            this.Close();
        }

    }
}
