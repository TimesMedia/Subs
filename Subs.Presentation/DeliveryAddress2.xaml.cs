using Subs.Business;
using Subs.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Subs.Presentation
{
    /// <summary>
    /// Interaction logic for DeliveryAddress.xaml
    /// </summary>
    public partial class DeliveryAddress2 : Window
    {
        #region Globals

        private DeliveryAddressDoc gDeliveryAddressDoc;
        private DeliveryAddressDoc.DeliveryAddressDataTable gDeliveryAddressTable;
        private readonly Subs.Data.DeliveryAddressDocTableAdapters.DeliveryAddressTableAdapter gDeliveryAddressAdapter = new Subs.Data.DeliveryAddressDocTableAdapters.DeliveryAddressTableAdapter();
        private CollectionViewSource gDeliveryAddressViewSource;
        private DeliveryAddressData2 gCurrentDeliveryAddress;
        private ObservableCollection<DeliveryAddressData2> gDeliveryAddresses = new ObservableCollection<DeliveryAddressData2>();

        private CollectionViewSource gCountryViewSource;
        private CollectionViewSource gProvinceViewSource;
        private CollectionViewSource gCityViewSource;
        private CollectionViewSource gSuburbViewSource;
        private CollectionViewSource gStreetViewSource;

        public struct TemplateRows
        {
            public DeliveryAddressDoc.StreetRow StreetRow;
            public DeliveryAddressDoc.SuburbRow SuburbRow;
            public DeliveryAddressDoc.CityRow CityRow;
            public DeliveryAddressDoc.ProvinceRow ProvinceRow;
            public DeliveryAddressDoc.CountryRow CountryRow;
        }

        private bool gPhysicalAddressChecked = false;
              
        private readonly CustomerData3 gCustomerData;

        #endregion

        #region Housekeeping

        private bool GeneralConstructor()
        {
            try
            {
                gDeliveryAddressDoc = ((DeliveryAddressDoc)(this.FindResource("deliveryAddressDoc")));
                gDeliveryAddressTable = gDeliveryAddressDoc.DeliveryAddress;
                gDeliveryAddressViewSource = (CollectionViewSource)(this.FindResource("deliveryAddressViewSource"));
                gDeliveryAddressAdapter.AttachConnection();

                gCountryViewSource = (CollectionViewSource)(this.FindResource("countryViewSource"));
                gCountryViewSource.Source = DeliveryAddressStatic.DeliveryAddresses.Country;

                gProvinceViewSource = (CollectionViewSource)(this.FindResource("provinceViewSource"));
                gCityViewSource = (CollectionViewSource)(this.FindResource("provinceCityViewSource"));
                gSuburbViewSource = (CollectionViewSource)(this.FindResource("provinceCitySuburbViewSource"));
                gStreetViewSource = (CollectionViewSource)(this.FindResource("provinceCitySuburbStreetViewSource"));

                // Prime with country = RSA

                foreach (System.Data.DataRowView lViewRow in gCountryViewSource.View)
                {
                    DeliveryAddressDoc.CountryRow lCountryRow = (DeliveryAddressDoc.CountryRow)lViewRow.Row;

                    if (lCountryRow.CountryId == 61)
                    {
                        gCountryViewSource.View.MoveCurrentTo(lViewRow);
                    }
                }

                gCountryViewSource.View.SortDescriptions.Add(new System.ComponentModel.SortDescription("CountryName", System.ComponentModel.ListSortDirection.Ascending));
                gProvinceViewSource.View.SortDescriptions.Add(new System.ComponentModel.SortDescription("ProvinceName", System.ComponentModel.ListSortDirection.Ascending));
                gCityViewSource.View.SortDescriptions.Add(new System.ComponentModel.SortDescription("CityName", System.ComponentModel.ListSortDirection.Ascending));
                gSuburbViewSource.View.SortDescriptions.Add(new System.ComponentModel.SortDescription("SuburbName", System.ComponentModel.ListSortDirection.Ascending));
                gStreetViewSource.View.SortDescriptions.Add(new System.ComponentModel.SortDescription("StreetName", System.ComponentModel.ListSortDirection.Ascending));

         

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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "GeneralConstructor", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
                return false;

            }
        }

        public DeliveryAddress2()
        {
            InitializeComponent();
            GeneralConstructor();
        }

        public DeliveryAddress2(int pCustomerId)
        {
            InitializeComponent();

            try
            {
                textCustomerId.Text = pCustomerId.ToString();
                gCustomerData = new CustomerData3(pCustomerId);

                textPhysicalAddressId.Text = gCustomerData.PhysicalAddressId.ToString();
                gPhysicalAddressChecked = true;

                if (!GeneralConstructor()) return;

                gDeliveryAddressTable.Clear();
                gDeliveryAddressAdapter.FillBy(gDeliveryAddressTable, pCustomerId, "ByCustomer");

                foreach (DeliveryAddressDoc.DeliveryAddressRow item in gDeliveryAddressTable)
                {
                    gDeliveryAddresses.Add(new DeliveryAddressData2(item.DeliveryAddressId));
                }

                gDeliveryAddressViewSource.Source = gDeliveryAddresses;
                gTabControl.DataContext = gDeliveryAddressViewSource;

                // Set the default country

                foreach (System.Data.DataRowView lViewRow in gCountryViewSource.View)
                {
                    DeliveryAddressDoc.CountryRow lCountryRow = (DeliveryAddressDoc.CountryRow)lViewRow.Row;

                    if (lCountryRow.CountryId == gCustomerData.CountryId)
                    {
                        gCountryViewSource.View.MoveCurrentTo(lViewRow);
                        return;

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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "DeliveryAddress2(int pCustomerId)", "CustomerId = " + pCustomerId.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (gPhysicalAddressChecked)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
                MessageBox.Show("Please ensure that we have a physical address for the invoices");
                gTabControl.SelectedIndex = 0;
            }

        }

        #endregion

        #region Select tab

        private void SelectRow()
        {
            DeliveryAddressData2 lView = (DeliveryAddressData2)gDeliveryAddressViewSource.View.CurrentItem;
            if (lView != null)
            {
                this.Hide();
            }
        }

        private void Click_ContextSelect(object sender, RoutedEventArgs e)
        {
            SelectRow();
        }

        private void deliveryAddressDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectRow();
        }

        private void ValidationError(object sender, ValidationErrorEventArgs e)
        {
            try
            {
                if (e.Action == ValidationErrorEventAction.Added)
                {

                    MessageBox.Show(e.Error.ErrorContent.ToString());
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ValidationError", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Error in ValidationError " + ex.Message);
            }
        }


        private void EditDeliveryAddress()
        {
            int lProgress = 0;
            try
            {
                gTabControl.SelectedIndex = 1;
                TabEdit.Visibility = Visibility.Visible;
                gCurrentDeliveryAddress = (DeliveryAddressData2)gDeliveryAddressViewSource.View.CurrentItem;
                                
                TabEdit.DataContext = gCurrentDeliveryAddress;

                lProgress = 1;

                if (gCurrentDeliveryAddress.StreetId.HasValue)
                {
                    // This adress has been validated yet, so, initialisation of the template is required.
                    InitialiseTemplate();
 
                    string lResult;
                    if ((lResult = CustomerBiz.SetMediaDeliveryFlag(gCurrentDeliveryAddress)) != "OK")
                    {
                        MessageBox.Show(lResult);

                    }
                    return;
                }
                gTabControl.SelectedIndex = 1;
                TabEdit.Visibility = Visibility.Visible;

                //********************************************************************************************************************************************
                              

                void InitialiseTemplate()
                {
                    // Initialise the Template datagrid

                    TemplateRows lTemplateRows = GetTemplateRows((int)gCurrentDeliveryAddress.StreetId);

                    foreach (System.Data.DataRowView lViewRow in gCountryViewSource.View)
                    {
                        if ((int)lViewRow["CountryId"] == lTemplateRows.CountryRow.CountryId)
                        {
                            gCountryViewSource.View.MoveCurrentTo(lViewRow);
                            countryDataGrid.ScrollIntoView(countryDataGrid.SelectedItem);
                            break;
                        }
                    }

                    lProgress = 2;

                    foreach (System.Data.DataRowView lViewRow in gProvinceViewSource.View)
                    {
                        if ((int)lViewRow["ProvinceId"] == lTemplateRows.ProvinceRow.ProvinceId)
                        {
                            gProvinceViewSource.View.MoveCurrentTo(lViewRow);
                            Province_DataGrid.ScrollIntoView(Province_DataGrid.SelectedItem);
                            break;
                        }
                    }

                    lProgress = 3;

                    foreach (System.Data.DataRowView lViewRow in gCityViewSource.View)
                    {
                        if ((int)lViewRow["CityId"] == lTemplateRows.CityRow.CityId)
                        {
                            gCityViewSource.View.MoveCurrentTo(lViewRow);
                            City_DataGrid.ScrollIntoView(City_DataGrid.SelectedItem);
                            break;
                        }
                    }

                    lProgress = 4;

                    foreach (System.Data.DataRowView lViewRow in gSuburbViewSource.View)
                    {
                        if ((int)lViewRow["SuburbId"] == lTemplateRows.SuburbRow.SuburbId)
                        {
                            gSuburbViewSource.View.MoveCurrentTo(lViewRow);
                            Suburb_DataGrid.ScrollIntoView(Suburb_DataGrid.SelectedItem);
                            break;
                        }
                    }

                    lProgress = 5;

                    foreach (System.Data.DataRowView lViewRow in gStreetViewSource.View)
                    {
                        if ((int)lViewRow["StreetId"] == lTemplateRows.StreetRow.StreetId)
                        {
                            gStreetViewSource.View.MoveCurrentTo(lViewRow);
                            Street_DataGrid.ScrollIntoView(Street_DataGrid.SelectedItem);
                            break;
                        }
                    }
                }

                //*********************************************************************************************************************************************************
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "EditDeliveryAddress", "Progress = " + lProgress.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void buttonExitEdit_Click(object sender, RoutedEventArgs e)
        {
            gTabControl.SelectedIndex = 0;
        }

        private void Click_ContextEdit(object sender, RoutedEventArgs e)
        {
            EditDeliveryAddress();
        }

        private void buttonRefreshTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("This might take 7 seconds");
                this.Cursor = Cursors.Wait;
                if (DeliveryAddressStatic.Refresh())
                {
                    MessageBox.Show("Refresh was successful");
                }
                else
                {
                    MessageBox.Show("There was a problem with refreshing the deliveryaddress template.");
                }
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void Button_Done_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonDeliveryAddressRecordAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Here I do not add something to the template. I create a new DeliveryAddress and then
                // select from the template, and to that I add streetno etc. 

                DeliveryAddressData2 lDeliveryAddress = new DeliveryAddressData2();
                gDeliveryAddresses.Add(lDeliveryAddress);


                //DeliveryAddressDoc.DeliveryAddressRow lNewRow = gDeliveryAddressTable.NewDeliveryAddressRow();

                //lNewRow.ModifiedBy = Environment.UserName;
                //lNewRow.ModifiedOn = DateTime.Now.Date;
                //lNewRow.CountryId = gCustomerData.CountryId;
                //gDeliveryAddressTable.AddDeliveryAddressRow(lNewRow);
                //gDeliveryAddressAdapter.Update(gDeliveryAddressTable);
                //gDeliveryAddressTable.AcceptChanges();
                ////gDeliveryAddressCurrentRow = lNewRow;

                // Select the new added row.
                gDeliveryAddressViewSource.View.MoveCurrentToLast();

                EditDeliveryAddress();
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonDeliveryAddressRecordAdd_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);

                return;
            }
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (gDeliveryAddressViewSource.View.CurrentItem != null)
            {
                DeliveryAddressData2 lDeliveryAddress = (DeliveryAddressData2)gDeliveryAddressViewSource.View.CurrentItem;
                textDelete.Text = lDeliveryAddress.DeliveryAddressId.ToString();

                if (textPhysicalAddressId.Text == textDelete.Text)
                {
                    MessageBox.Show("You cannot delete the physical address id. You will first have to assign an alternate physical address id.");
                    return;
                }
            }
            else
            {
                MessageBox.Show("You have not selected an address to be deleted. Please try again.");
            }
        }

        private void buttonRetain_Click(object sender, RoutedEventArgs e)
        {
            if (gDeliveryAddressViewSource.View.CurrentItem != null)
            {
                DeliveryAddressData2 lDeliveryAddress = (DeliveryAddressData2)gDeliveryAddressViewSource.View.CurrentItem;
                textRetain.Text = lDeliveryAddress.DeliveryAddressId.ToString();
            }
            else
            {
                MessageBox.Show("You have not selected a address to retain. Please try again.");
            }
        }

        private void buttonConsolidate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (textDelete.Text == textRetain.Text)
                {
                    MessageBox.Show("The source and the target cannot be the same address. Please try again.");
                    return;
                }

                if (!int.TryParse(textDelete.Text, out int Source))
                {
                    MessageBox.Show("No proper address is selected as the address to be deleted.");
                    return;
                }

                if (!int.TryParse(textRetain.Text, out int Target))
                {
                    MessageBox.Show("No proper address is selected as the address to be retained.");
                    return;
                }

                gDeliveryAddressAdapter.Consolidate(Source, Target);

               
                ExceptionData.WriteException(5, "DeliveryAddress " + Source.ToString() + " consolidated into " + Target.ToString(), this.ToString(), " buttonConsolidateClick ", "");

                gDeliveryAddresses.Clear();
                gDeliveryAddressAdapter.FillBy(gDeliveryAddressTable, int.Parse(textCustomerId.Text), "ByCustomer");
                foreach (DeliveryAddressDoc.DeliveryAddressRow item in gDeliveryAddressTable)
                {
                    gDeliveryAddresses.Add(new DeliveryAddressData2(item.DeliveryAddressId));
                }

                gDeliveryAddressViewSource.Source = gDeliveryAddresses;
                gTabControl.DataContext = gDeliveryAddressViewSource;
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonConsolidate_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return;
            }

        }

        #endregion

        #region Edit tab

        private void buttonRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (gDeliveryAddresses.Count == 0 || gDeliveryAddressViewSource.View.CurrentItem == null)
                {
                    MessageBox.Show("No deliveryaddress has been selected.");
                    return;
                }

                gCurrentDeliveryAddress = (DeliveryAddressData2)gDeliveryAddressViewSource.View.CurrentItem;

                gCustomerData.PhysicalAddressId = gCurrentDeliveryAddress.DeliveryAddressId;

                string lResult;
                if ((lResult = gCustomerData.Update()) != "OK")
                {
                    MessageBox.Show(lResult);
                    return;
                }

                gCustomerData.Update();
                textPhysicalAddressId.Text = gCustomerData.PhysicalAddressId.ToString();
                gPhysicalAddressChecked = true;
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonRegister_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);

                return;
            }
        }

        private TemplateRows GetTemplateRows(int pStreetId)
        {
            TemplateRows lTemplateRows = new TemplateRows();
            try
            {


                // Find the primary keys for the hierarchy

                lTemplateRows.StreetRow = DeliveryAddressStatic.DeliveryAddresses.Street.FindByStreetId(pStreetId);
                lTemplateRows.SuburbRow = (DeliveryAddressDoc.SuburbRow)lTemplateRows.StreetRow.GetParentRow("FK_Street_Suburb");
                lTemplateRows.CityRow = (DeliveryAddressDoc.CityRow)lTemplateRows.SuburbRow.GetParentRow("FK_Suburb_City");
                lTemplateRows.ProvinceRow = (DeliveryAddressDoc.ProvinceRow)lTemplateRows.CityRow.GetParentRow("FK_City_Province");
                lTemplateRows.CountryRow = (DeliveryAddressDoc.CountryRow)lTemplateRows.ProvinceRow.GetParentRow("FK_Province_Country");
                return lTemplateRows;
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "GetTemplateRows", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return lTemplateRows;
            }

        }

        private void buttonUpdateDeliveryAddress_Click(object sender, RoutedEventArgs e)
        {
            string lProgress = "Start";
            try
            {
                DataRowView lView = (DataRowView)gStreetViewSource.View.CurrentItem;
                DeliveryAddressDoc.StreetRow lStreetRow = (DeliveryAddressDoc.StreetRow)lView.Row;

                // Validate all the controls

                if (lStreetRow == null)
                {
                    MessageBox.Show("You have not selected a street yet.");
                    return;
                }

                TemplateRows lTemplate;

                lTemplate = GetTemplateRows(lStreetRow.StreetId);

                lProgress = "GetTemplateRows";

                gCurrentDeliveryAddress.CountryId = lTemplate.CountryRow.CountryId;
                gCurrentDeliveryAddress.Province = lTemplate.ProvinceRow.ProvinceName;
                gCurrentDeliveryAddress.City = lTemplate.CityRow.CityName;

                lProgress = "CityName";

                gCurrentDeliveryAddress.Suburb = lTemplate.SuburbRow.SuburbName;
                gCurrentDeliveryAddress.Street = lTemplate.StreetRow.StreetName;

                lProgress = "StreetName";

                if (lTemplate.StreetRow.IsStreetSuffixNull())
                {
                    gCurrentDeliveryAddress.StreetSuffix = null;
                }
                else
                {
                    gCurrentDeliveryAddress.StreetSuffix = lTemplate.StreetRow.StreetSuffix;
                }


                if (lTemplate.StreetRow.IsStreetExtensionNull())
                {
                    gCurrentDeliveryAddress.StreetExtension = null;
                }
                else
                {
                    gCurrentDeliveryAddress.StreetExtension = lTemplate.StreetRow.StreetExtension;
                }
               

                lProgress = "StreetId";

                gCurrentDeliveryAddress.StreetId = lTemplate.StreetRow.StreetId;
                
                lProgress = "StreetId";
                gCurrentDeliveryAddress.Update();

                // Also update the many to many mapping 

                if (gCustomerData != null)
                {

                    {
                        string lResult;

                        if ((lResult = DeliveryAddressData2.Link(gCurrentDeliveryAddress.DeliveryAddressId, gCustomerData.CustomerId)) != "OK")
                        {
                            MessageBox.Show(lResult);
                            return;
                        }
                    }
                }


                MessageBox.Show("DeliveryAddress successfully updated.");

                gTabControl.SelectedIndex = 0;
                TabEdit.Visibility = Visibility.Hidden;
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "buttonUpdateDeliveryRecord", "Progress = " + lProgress);
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region Properties

        public int? SelectedDeliveryAddressId
        {
            get
            {
                DeliveryAddressData2 lView = (DeliveryAddressData2)gDeliveryAddressViewSource.View.CurrentItem;
                if (lView == null)
                {
                    return null;
                }
                else
                {
                    return lView.DeliveryAddressId;
                }
            }
        }

        #endregion

    }
}
