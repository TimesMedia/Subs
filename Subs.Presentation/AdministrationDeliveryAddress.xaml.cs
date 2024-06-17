using Subs.Business;
using Subs.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Subs.Presentation
{

    public partial class AdministrationDeliveryAddress : Window
    {
        #region Globals
        MIMSDataContext gContext = new MIMSDataContext(Settings.ConnectionString);
        private readonly Subs.Data.DeliveryAddressDocTableAdapters.DeliveryAddressTableAdapter gDeliveryAddressAdapter = new Subs.Data.DeliveryAddressDocTableAdapters.DeliveryAddressTableAdapter();
        private readonly Subs.Data.DeliveryAddressDocTableAdapters.CountryTableAdapter gCountryTableAdapter = new Subs.Data.DeliveryAddressDocTableAdapters.CountryTableAdapter();
        private readonly Subs.Data.DeliveryAddressDocTableAdapters.ProvinceTableAdapter gProvinceTableAdapter = new Subs.Data.DeliveryAddressDocTableAdapters.ProvinceTableAdapter();
        private readonly Subs.Data.DeliveryAddressDocTableAdapters.CityTableAdapter gCityTableAdapter = new Subs.Data.DeliveryAddressDocTableAdapters.CityTableAdapter();
        private readonly Subs.Data.DeliveryAddressDocTableAdapters.SuburbTableAdapter gSuburbTableAdapter = new Subs.Data.DeliveryAddressDocTableAdapters.SuburbTableAdapter();
        private readonly Subs.Data.DeliveryAddressDocTableAdapters.StreetTableAdapter gStreetTableAdapter = new Subs.Data.DeliveryAddressDocTableAdapters.StreetTableAdapter();

        private Subs.Data.DeliveryAddressDoc gDeliveryAddressDoc;
        private CollectionViewSource gDeliveryAddressViewSource;
        private CollectionViewSource gCountryViewSource;
        private CollectionViewSource gCityViewSource;
        private CollectionViewSource gSuburbViewSource;
        private CollectionViewSource gStreetViewSource;

        private DeliveryAddressData2 gSelectedAddress;

        #endregion

        #region Constructor and utilities
        public AdministrationDeliveryAddress()
        {
            try
            { 
                InitializeComponent();

                gDeliveryAddressDoc = ((DeliveryAddressDoc)(this.FindResource("deliveryAddressDoc")));
                gDeliveryAddressViewSource = (CollectionViewSource)(this.FindResource("deliveryAddressViewSource"));
                gCountryViewSource = (CollectionViewSource)Resources["countryViewSource"];
                gCityViewSource = (CollectionViewSource)Resources["provinceCityViewSource"];
                gSuburbViewSource = (CollectionViewSource)Resources["provinceCitySuburbViewSource"];
                gStreetViewSource = (CollectionViewSource)Resources["provinceCitySuburbStreetViewSource"];
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "AdministrationDeliveryAddress constructor", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);
            }
        }

        private void SetVisibility(object sender, RoutedEventArgs e)
        {
            Utilities.SetVisibility(sender);
        }

        private bool ListNonStandardAddresses()
        {
            // List the non standard deliveryaddresses
            List<MIMS_DeliveryAddressDoc_DeliveryAddress_NonStandardResult> lDeliveryAddressIds = gContext.MIMS_DeliveryAddressDoc_DeliveryAddress_NonStandard().ToList();
            
            List<DeliveryAddressData2> lDeliveryAddresses = new List<DeliveryAddressData2>();
            foreach (var item in lDeliveryAddressIds)
            {
                lDeliveryAddresses.Add(new DeliveryAddressData2(item.DeliveryAddressId));
            }
            gDeliveryAddressViewSource.Source = lDeliveryAddresses;

            return true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ListNonStandardAddresses())
                {
                    return;
                }

                // Load the standard addresses hierarchy


                gCountryTableAdapter.AttachConnection();
                gProvinceTableAdapter.AttachConnection();
                gCityTableAdapter.AttachConnection();
                gSuburbTableAdapter.AttachConnection();
                gStreetTableAdapter.AttachConnection();

                gCountryTableAdapter.Fill(gDeliveryAddressDoc.Country);
                gProvinceTableAdapter.Fill(gDeliveryAddressDoc.Province);
                gCityTableAdapter.Fill(gDeliveryAddressDoc.City);
                gSuburbTableAdapter.Fill(gDeliveryAddressDoc.Suburb);
                gStreetTableAdapter.Fill(gDeliveryAddressDoc.Street);

                DeliveryAddressDoc.CountryRow lCountry = gDeliveryAddressDoc.Country.FindByCountryId(61);
                int lCountryIndex = gDeliveryAddressDoc.Country.Rows.IndexOf(lCountry);

                gCountryViewSource.View.MoveCurrentToPosition(34);
                countryDataGrid.ScrollIntoView(gCountryViewSource.View.CurrentItem);

            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Window_Loaded", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Error in Window_Loaded");
            }
        }

        #endregion

        #region Non standard deliveryaddresses

        private void Click_Select(object sender, MouseButtonEventArgs e)
        {
            Select();
        }
        private void Click_Select(object sender, RoutedEventArgs e)
        {
            Select();
        }

        private void Select()
        { 
            try
            {
                gSelectedAddress = (DeliveryAddressData2)gDeliveryAddressViewSource.View.CurrentItem; 
                
                textCountry.Text = gSelectedAddress.CountryName;
                textProvince.Text = gSelectedAddress.Province;
                textCity.Text = gSelectedAddress.City;
                textSuburb.Text = gSelectedAddress.Suburb;
                textStreet.Text = gSelectedAddress.Street;
                textExtension.Text = gSelectedAddress.StreetExtension;
                textSuffix.Text = gSelectedAddress.StreetSuffix;

                gCountryViewSource.View.MoveCurrentToPosition(gDeliveryAddressDoc.Country.Rows.IndexOf(gDeliveryAddressDoc.Country.FindByCountryId(61)));
                countryDataGrid.ScrollIntoView(gCountryViewSource.View.CurrentItem);
                TabControl.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Select", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Error in Select");
            }
        }

        private void Click_GoToCustomer(object sender, RoutedEventArgs e)
        {
            gSelectedAddress = (DeliveryAddressData2)gDeliveryAddressViewSource.View.CurrentItem;
            CustomerPicker3 lPicker = new CustomerPicker3(); 
            lPicker.SetCurrentCustomer(gSelectedAddress.CustomerId);
            lPicker.ShowDialog();
        }

        #endregion

        #region Address Template

        private void SubmitCountry(object sender, RoutedEventArgs e)
        {
            gCountryTableAdapter.Update(gDeliveryAddressDoc.Country);
            gCountryTableAdapter.Fill(gDeliveryAddressDoc.Country);
        }

        private void SubmitProvince(object sender, RoutedEventArgs e)
        {
            gProvinceTableAdapter.Update(gDeliveryAddressDoc.Province);
            gProvinceTableAdapter.Fill(gDeliveryAddressDoc.Province);
        }

        private void SubmitCity(object sender, RoutedEventArgs e)
        {
            // Check all the modified rows for the existence of open or closed brackets.
            // Curly brackets are inadvertently as tuples
            
            foreach (var item in gDeliveryAddressDoc.City)
            {
                if (item.RowState == DataRowState.Added | item.RowState == DataRowState.Modified)
                {
                    if (item.CityName.Contains("("))
                    {
                        item.CityName = item.CityName.Replace("(", " - ");
                    }

                    if (item.CityName.Contains(")"))
                    {
                        item.CityName = item.CityName.Replace(")", "");
                    }
                }
            }
 
            gCityTableAdapter.Update(gDeliveryAddressDoc.City);
            gCityTableAdapter.Fill(gDeliveryAddressDoc.City);
        }

        private void CreateListOfCustomersByCity_Click(object sender, RoutedEventArgs e)
        {
            DataRowView lCityRowView = (DataRowView)gCityViewSource.View.CurrentItem;
            DeliveryAddressDoc.CityRow lCityRow = (DeliveryAddressDoc.CityRow)lCityRowView.Row;
            int lHits = DeliveryAddressData2.CreateCustomersByCityId(lCityRow.CityId, lCityRow.CityName);
            if (lHits > 0)
            {
                MessageBox.Show("A list of " + lHits.ToString() + " customerids have been created in c:\\Subs\\" + lCityRow.CityName + ".xml");
            }
            else
            {
                MessageBox.Show("There are no MIMS customers in :"  + lCityRow.CityName);
            }
        }

        private void SubmitSuburb(object sender, RoutedEventArgs e)
        {
            gSuburbTableAdapter.Update(gDeliveryAddressDoc.Suburb);
            gSuburbTableAdapter.Fill(gDeliveryAddressDoc.Suburb);
        }

        private void CreateListOfCustomersBySuburb_Click(object sender, RoutedEventArgs e)
        {
            DataRowView lSuburbRowView = (DataRowView)gSuburbViewSource.View.CurrentItem;
            DeliveryAddressDoc.SuburbRow lSuburbRow = (DeliveryAddressDoc.SuburbRow)lSuburbRowView.Row;
            int lHits = DeliveryAddressData2.CreateCustomersBySuburbId(lSuburbRow.SuburbId, lSuburbRow.SuburbName);
            if (lHits > 0)
            {
                MessageBox.Show("A list of " + lHits.ToString() + " customerids have been created in c:\\Subs\\" + lSuburbRow.SuburbName + ".xml");
            }
            else
            {
                MessageBox.Show("There are no MIMS customers in :" + lSuburbRow.SuburbName);
            }
        }

        private void SubmitStreet(object sender, RoutedEventArgs e)
        {
            gStreetTableAdapter.Update(gDeliveryAddressDoc.Street);
            gStreetTableAdapter.Fill(gDeliveryAddressDoc.Street);
        }

        private void SubmitStreetId(object sender, RoutedEventArgs e)
        {

            System.Data.DataRowView lView = (System.Data.DataRowView)gStreetViewSource.View.CurrentItem;
            if (lView == null)
            {
                MessageBox.Show("You have not selected a street. Please try again.");
            }
            else
            { 
                DeliveryAddressDoc.StreetRow lRow = (DeliveryAddressDoc.StreetRow)lView.Row;
                gSelectedAddress.StreetId = lRow.StreetId;
                // Consider clearing out all the superfluous fields.
                // Or keep them for backout purposes.

                gSelectedAddress.Update();
                MessageBox.Show("Address has been standardised successfully.");
                if (!ListNonStandardAddresses())
                {
                    return;
                }
                TabControl.SelectedIndex = 0;
            }
        }

        #endregion

        #region Utilities
        private void Button_MediaStandardiseAll(object sender, RoutedEventArgs e)
        {
            List<int> lDeliveryAddresses = new List<int>();
            int lCurrentDeliveryAddress = 0;
            SqlConnection lConnection = new SqlConnection();
            SqlCommand Command = new SqlCommand();
            SqlDataAdapter Adaptor = new SqlDataAdapter();
            lConnection.ConnectionString = Settings.ConnectionString;
            try 
            { 
                lConnection.Open();
                Command.Connection = lConnection;
                Command.CommandType = CommandType.Text;
                Command.CommandText = "select DeliveryAddressId from DeliveryAddress Where StreetId is not null and MediaDelivery is null";
           
                SqlDataReader lReader = Command.ExecuteReader();
                int lCounter = 0;

                this.Cursor = Cursors.Wait;
                while (lReader.Read())
                {
                    lCurrentDeliveryAddress = lReader.GetInt32(0);
                    DeliveryAddressData2 lDeliveryAddressData = new DeliveryAddressData2(lCurrentDeliveryAddress);

                    string lResult;
                    if ((lResult = CustomerBiz.SetMediaDeliveryFlag(lDeliveryAddressData)) != "OK")
                    {
                        MessageBox.Show(lResult);
                        return;
                    }
                    lCounter++;
                }
                MessageBox.Show("I have Media standardised " + lCounter.ToString() + " DeliveryAddresses");
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ButttonStandardiseAll", 
                        "DeliveryAddress "  + lCurrentDeliveryAddress.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Error in Select");
            }
            finally 
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        #endregion
    }
}
