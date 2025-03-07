﻿using Subs.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Input;


namespace Subs.Presentation
{
    public partial class MainWindow : Window
    {
        #region Globals

        //public Subs.Data.AdministrationData gAdministrationData;

        private readonly BackgroundWorker gBackgroundWorker;
        public static string gVersion;
        #endregion

        #region Constructor

        public MainWindow()
        {
            //string lProductFilter = "";
            string lConnectionString = "";

            try
            {
                lConnectionString = global::Subs.Presentation.Properties.Settings.Default.ConnectionString;

                if (lConnectionString == "")
                {
                    throw new Exception("No connection string has been set.");
                }
                else
                {
                    Settings.ConnectionString = lConnectionString;
                    Settings.DirectoryPath = global::Subs.Presentation.Properties.Settings.Default.DirectoryPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Setting " + ex.Message);
            }

            try
            {
                InitializeComponent();

                gBackgroundWorker = new BackgroundWorker();
                gBackgroundWorker.DoWork += new DoWorkEventHandler(workerThread_DoWork);
                gBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerThread_RunWorkerCompleted);
                gBackgroundWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("InitializeComponent " + ex.Message);
            }


            try
            {
                // Initialise data objects
                ProductDataStatic.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Initialize data objects " + ex.Message);
            }



            try
            {

                // Set the Status-strip
                string[] myStatusMessages;
                char[] charSeparators = new char[] { ';' };
                myStatusMessages = lConnectionString.Split(charSeparators, 10, StringSplitOptions.RemoveEmptyEntries);
                string myServer = "";
                string myDataBase = "";
                //string myVersion = "";
                foreach (string myMember in myStatusMessages)
                {
                    if (myMember.StartsWith("Data Source"))
                    {
                        myServer = myMember.Substring(12);
                    }

                    if (myMember.StartsWith("Initial Catalog"))
                    {
                        myDataBase = myMember.Substring(16);
                    }
                }

                if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                {

                    Settings.Version = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                }

                this.Title = "MIMS on " + myServer + " on database " + myDataBase + " Version " + Settings.Version;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Title " + ex.Message);
            }


            try
            {
                //*****
                string lUserName = (string)Environment.UserName.ToString().ToUpper();

                System.Collections.Specialized.StringCollection Authority4Users
                  = (System.Collections.Specialized.StringCollection)global::Subs.Presentation.Properties.Settings.Default.Authority4;

                System.Collections.Specialized.StringCollection Authority3Users
                    = (System.Collections.Specialized.StringCollection)global::Subs.Presentation.Properties.Settings.Default.Authority3;

                System.Collections.Specialized.StringCollection Authority2Users
                    = (System.Collections.Specialized.StringCollection)global::Subs.Presentation.Properties.Settings.Default.Authority2;


                if (Authority2Users.Contains(lUserName))
                {
                    Settings.Authority = 2;
                }

                if (Authority3Users.Contains(lUserName))
                {
                    Settings.Authority = 3;
                }

                if (Authority4Users.Contains(lUserName))
                {
                    Settings.Authority = 4;
                }



            }
            catch (Exception ex)
            {
                MessageBox.Show("Special priviledges " + ex.Message);
            }
        }

        private void workerThread_DoWork(object sender, DoWorkEventArgs e)
        {
            // Prime the loading of the delivery addresses.
            DeliveryAddressDoc lPrompt = DeliveryAddressStatic.DeliveryAddresses;
        }

        private void workerThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DeliveryAddressStatic.Loaded = true;
        }

        private void SetVisibility(object sender, RoutedEventArgs e)
        {
            FrameworkElement lFrameworkElement = (FrameworkElement)sender;

            if (string.IsNullOrWhiteSpace((string)lFrameworkElement.Tag))
            {
                // This event handler is dependent on the Tag property
                return;
            }

            if (Settings.Authority == 4 && ((string)lFrameworkElement.Tag == "AuthorityHighest"
                                         || (string)lFrameworkElement.Tag == "AuthorityHigh"
                                         || (string)lFrameworkElement.Tag == "AuthorityMedium"))
            {
                lFrameworkElement.Visibility = Visibility.Visible;
            }
            else
            {
                if (Settings.Authority == 3 && ((string)lFrameworkElement.Tag == "AuthorityHigh"
                                             || (string)lFrameworkElement.Tag == "AuthorityMedium"))
                {
                    lFrameworkElement.Visibility = Visibility.Visible;
                }
                else
                {
                    if (Settings.Authority == 2 && (string)lFrameworkElement.Tag == "AuthorityMedium")
                    {
                        lFrameworkElement.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        lFrameworkElement.Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        #endregion

        #region Administration

        private void Country_Click(object sender, RoutedEventArgs e)
        {
            Subs.Presentation.AdministrationCountry lAdministration = new Subs.Presentation.AdministrationCountry();
            lAdministration.Show();
        }

        private void Click_AdministrationProduct(object sender, RoutedEventArgs e)
        {
            Subs.Presentation.AdministrationProduct frmProductAdministration = new Subs.Presentation.AdministrationProduct();
            //frmProductAdministration.SelectTab(Subs.Presentation.AdministrationProduct.Tabs.Product);
            frmProductAdministration.ShowDialog();
        }

        //private void Click_CustomerTitle(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        AdministrationCustomer lWindow = new AdministrationCustomer();
        //        lWindow.SelectTab(AdministrationCustomer.Tabs.Title);
        //        lWindow.ShowDialog();
        //        AdministrationData2.Refresh(); // It seems important to do this here, becasue this triggers the refresh of the control as well.
        //    }
        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Click_CompanyConsolidation", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        MessageBox.Show(ex.Message);
        //    }
        //}

        private void Click_CustomerClassification(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                Subs.Presentation.ClassificationPicker Classification = new Subs.Presentation.ClassificationPicker();
                Classification.ShowDialog();
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }

        }

        private void Click_Company(object sender, RoutedEventArgs e)
        {
            try
            {
                AdministrationCompany lWindow = new AdministrationCompany();
                lWindow.ShowDialog();
                AdministrationData2.RefreshCompany(); // It seems important to do this here, because this triggers the refresh of the control as well.
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Click_CompanyConsolidation", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }
        }

        private void Click_DeliveryCost(object sender, RoutedEventArgs e)
        {
            Subs.Presentation.AdministrationCountry lAdministration = new Subs.Presentation.AdministrationCountry();
            lAdministration.Show();
        }

        //private void Click_PostCode(object sender, RoutedEventArgs e)
        //{
        //    AdministrationPostCode lWindow = new AdministrationPostCode();
        //    lWindow.ShowDialog();
        //}

        private void Click_Promotion(object sender, RoutedEventArgs e)
        {
            AdministrationPromotion lPromotion = new AdministrationPromotion();
            lPromotion.ShowDialog();
        }

        private void Click_DeliveryAddress(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                AdministrationDeliveryAddress lDeliveryAddress = new AdministrationDeliveryAddress();
                lDeliveryAddress.ShowDialog();
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void Click_Refresh(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                AdministrationData2.Refresh();
                ProductDataStatic.Refresh();
                DeliveryAddressStatic.Refresh();
                MessageBox.Show("DataTemplates have been refreshed successfully");
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (Settings.CurrentCustomerId > 0)
                {
                    CustomerData3 lCustomer = new CustomerData3(Settings.CurrentCustomerId);
                    labelCurrentCustomer.Content = lCustomer.Surname + "\n" + Settings.CurrentCustomerId.ToString();
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Window_MouseEnter", "CustopmerId = " + Settings.CurrentCustomerId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return;
            }
        }

        #endregion

        #region Product

        private void Click_Deliver(object sender, RoutedEventArgs e)
        {
            Subs.Presentation.Deliver frmDeliver = new Deliver();
            frmDeliver.ShowDialog();
        }

        private void DeliveryReversal(object sender, RoutedEventArgs e)
        {
            Subs.Presentation.Maintenance lMaintenance = new Subs.Presentation.Maintenance();
            //lMaintenance.SelectTab(Subs.Presentation.Maintenance.Tabs.Reversal);
            lMaintenance.ShowDialog();
        }

        #endregion

        #region Subscription

        private void Click_SubscriptionPicker(object sender, RoutedEventArgs e)
        {
            try
            {
                //MessageBox.Show("Trying to create SubscriptionPicker2");
                SubscriptionPicker2 lSubscriptionPicker = new SubscriptionPicker2();
                //MessageBox.Show("Created SubscriptionPicker2");
                lSubscriptionPicker.ShowDialog();
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Click_SubscriptionPicker", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }
        }

        private void Click_Quote(object sender, RoutedEventArgs e)
        {
            ProForma2 lProForma = new ProForma2();
            lProForma.ShowDialog();
        }

        private void Click_GlobalSkip(object sender, RoutedEventArgs e)
        {
            Subs.Presentation.Maintenance lMaintenance = new Subs.Presentation.Maintenance();
            // frmMaintenance.SelectTab(Subs.Presentation.Maintenance2.MaintenanceTabs.Skip);
            lMaintenance.ShowDialog();
        }

        #endregion

        #region Customer

        private void CustomerMenu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Subs.Presentation.CustomerPicker3 lCustomerPicker = new Subs.Presentation.CustomerPicker3();
                lCustomerPicker.ShowDialog();
            }
        }

        private void Click_CustomerGoTo(object sender, RoutedEventArgs e)
        {
            Subs.Presentation.CustomerPicker3 lCustomerPicker = new Subs.Presentation.CustomerPicker3();
            lCustomerPicker.ShowDialog();
        }

        private void Click_CommunicationInitiate(object sender, RoutedEventArgs e)
        {
            try
            {
                Subs.Presentation.Maintenance lMaintenance = new Subs.Presentation.Maintenance();
                lMaintenance.ShowDialog();
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Click_CommunicationInitiate", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Failed on Click_CommunicationInitiate " + ex.Message);
            }
        }

        private void Click_FNBBankStatement(object sender, RoutedEventArgs e)
        {
            Subs.Presentation.FNBBankStatement lBankstatement = new FNBBankStatement();
            lBankstatement.ShowDialog();
        }

        private void Click_SBBankStatement(object sender, RoutedEventArgs e)
        {
            Subs.Presentation.SBBankStatement lBankStatement = new Subs.Presentation.SBBankStatement();
            lBankStatement.ShowDialog();
        }


        private void Click_DebitOrderBankStatement(object sender, RoutedEventArgs e)
        {
            Subs.Presentation.DebitOrderBankStatement lBankStatement = new Subs.Presentation.DebitOrderBankStatement();
            lBankStatement.ShowDialog();
        }
      

        #endregion
   
        #region Maintenance

        private void Click_PostCodeStandardisation(object sender, RoutedEventArgs e)
        {
            try
            {

                Cursor = Cursors.Wait;

                if (!DeliveryDataStatic.StandarizePostCodes())
                {
                    MessageBox.Show("Failed due to an error");
                }
                else
                {
                    MessageBox.Show("Done!");
                }
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }

        }

        private void Click_RefreshEnums(object sender, RoutedEventArgs e)
        {
            try
            {
                //RefreshEnums
                AdministrationDoc lAdministrationDoc = new AdministrationDoc();
                MessageBox.Show(lAdministrationDoc.RefreshEnums());
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "RefreshEnums", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }
        }

        private void Click_PostCodeAddSapoCompliment(object sender, RoutedEventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;
                if (PostCodeData.AddSapoCompliment())
                {
                    MessageBox.Show("Sapo compliment successfully added");
                }
                else
                {
                    MessageBox.Show("Sapo compliment addition failed");
                }
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void ClickInvalidPassword(object sender, RoutedEventArgs e)
        {
            ElicitInteger lElicitInteger = new ElicitInteger("Please enter the customerid");
            lElicitInteger.ShowDialog();
            CustomerData3 lCustomerData = new CustomerData3(lElicitInteger.Answer);
            MessageBox.Show(lCustomerData.Password1);
        }

        #endregion



        private void Click_Test(object sender, RoutedEventArgs e)
        {
            try
            {
                int Content = 100;

                int Token = Content * DateTime.Now.Hour;

                int Result = Token / DateTime.Now.Hour;
            

                MessageBox.Show(Content.ToString() 
                    + " " +Token.ToString()
                    + " " + Result.ToString());
                  

            }
            catch (Exception ex)
            {
                int lExceptionId = 0;
                lExceptionId = ExceptionData.WriteException(1, ex.Message, this.ToString(), ex.TargetSite.ToString(), ex.Source);
   
                ExceptionData.WriteExceptionTrace(lExceptionId, ex.StackTrace, "Click_Test");


                MessageBox.Show("Exception throwed");
            }
        }

    
    }
}

