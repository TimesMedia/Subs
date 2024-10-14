using Subs.Data;
using Subs.XMLServiceTest.ServiceReference1;
using System;
using System.Linq;
//using Subs.Business;
using System.Reflection;
using System.Windows;

namespace Subs.XMLServiceTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string lConnectionString = global::Subs.XMLServiceTest.Properties.Settings.Default.MIMSConnectionString;

            if (lConnectionString == "")
            {
                throw new Exception("No connection string has been set.");
            }
            else
            {
                Settings.ConnectionString = lConnectionString;
            }
        }


        private void ButtonOTP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ServiceSoapClient lClient = new ServiceSoapClient();
                AuthorizationHeader lHeader = new AuthorizationHeader
                {
                    Source = "NJA",
                    Type = "MOBIMims"
                };


                SendOTPRequest lRequest = new SendOTPRequest();
                lRequest.AuthorizationHeader = lHeader;
                lRequest.pCustomerId = 120072;
                lRequest.pOTP = 912;


                SendOTPResponse lResponse = new SendOTPResponse();

                lResponse = lClient.SendOTP(lRequest);

                MessageBox.Show(lResponse.SendOTPResult.ToString());

            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ButtonOTP_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }
        }

    private void ButtonAuthorize_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ServiceSoapClient lClient = new ServiceSoapClient();
                AuthorizationHeader lHeader = new AuthorizationHeader
                {
                    Source = "NJA",
                    Type = "MOBIMims"
                };

                AuthorizeRequest lRequest = new AuthorizeRequest();
                lRequest.AuthorizationHeader = lHeader;
                lRequest.pProductId = 17;
                lRequest.pReceiverId = 912;


                AuthorizeResponse lResponse = new AuthorizeResponse();

                lResponse = lClient.Authorize(lRequest);

                MessageBox.Show(lResponse.AuthorizeResult.ExpirationDate.ToString() + " Seats " + lResponse.AuthorizeResult.Seats.ToString() + " " + lResponse.AuthorizeResult.Reason.ToString());

            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ButtonAuthorize_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonAuthorizations_Click(object sender, RoutedEventArgs e)
        {
            try

            {
                ServiceSoapClient lClient = new ServiceSoapClient();
                AuthorizationHeader lHeader = new AuthorizationHeader
                {
                    Source = "NJA",
                    Type = "MOBIMims"
                };


                AuthorizationsRequest lRequest2 = new AuthorizationsRequest();
                lRequest2.AuthorizationHeader = lHeader;

                // Authorizations

                AuthorizationResult[] lResponse2 = lClient.Authorizations(lRequest2).AuthorizationsResult;

                var Found = lResponse2.Where<AuthorizationResult>(f => f.CustomerId == 50929)
                            .Select(g => new { g.CustomerId, g.Seats, g.ProductId, g.ExpirationDate });

                foreach (var p in Found)
                {
                    MessageBox.Show(p.CustomerId.ToString() + "ProductId= " + p.ProductId.ToString() + "Seats=" + p.Seats.ToString() +
                        "Expire= " + p.ExpirationDate.ToString());
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ButtonAuthorizations_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
                return;
            }


        }

        private void ButtonFindCustomerByEmail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ServiceSoapClient lClient = new ServiceSoapClient();
                AuthorizationHeader lHeader = new AuthorizationHeader
                {
                    Source = "NJA",
                    Type = "MOBIMims"
                };

                FindCustomerIdByEmailRequest lRequest = new FindCustomerIdByEmailRequest();
                lRequest.AuthorizationHeader = lHeader;
                lRequest.EmailAddress = "heinreitmann@gmail.com";


                FindCustomerIdByEmailResponse lResponse = new FindCustomerIdByEmailResponse();

                lResponse = lClient.FindCustomerIdByEmail(lRequest);

                MessageBox.Show(lResponse.CustomerId.ToString());

            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ButtonFindCustomerByEmail_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }

        }


        private void ButtonFindCustomerByNationalId_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ServiceSoapClient lClient = new ServiceSoapClient();
                AuthorizationHeader lHeader = new AuthorizationHeader
                {
                    Source = "NJA",
                    Type = "MOBIMims"
                };

                FindCustomerIdByNationalIdRequest lRequest = new FindCustomerIdByNationalIdRequest();
                lRequest.AuthorizationHeader = lHeader;
                lRequest.NationalId = "9012145134082";


                FindCustomerIdByNationalIdResponse lResponse = new FindCustomerIdByNationalIdResponse();

                lResponse = lClient.FindCustomerIdByNationalId(lRequest);

                MessageBox.Show(lResponse.CustomerId.ToString());

            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ButtonFindCustomerByNationalId_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }
        }


        private void ButtonFindEmailByCustomer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ServiceSoapClient lClient = new ServiceSoapClient();
                AuthorizationHeader lHeader = new AuthorizationHeader
                {
                    Source = "NJA",
                    Type = "MOBIMims"
                };

                FindEMailByCustomerIdRequest lRequest = new FindEMailByCustomerIdRequest();
                lRequest.AuthorizationHeader = lHeader;
                lRequest.pCustomerId = 117224;


                FindEMailByCustomerIdResponse lResponse = new FindEMailByCustomerIdResponse();

                lResponse = lClient.FindEMailByCustomerId(lRequest);

                MessageBox.Show(lResponse.pEmailAddress);

            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ButtonFindEmailByCustomer_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }


        }


        //private void ButtonInsertCustomer_Click(object sender, RoutedEventArgs e)
        //{

        //    ServiceReference1.InsertCustomerResponse lResponse = new InsertCustomerResponse();

        //    try
        //    {

        //        try
        //        {
        //            ServiceSoapClient lClient = new ServiceSoapClient();
        //            AuthorizationHeader lHeader = new AuthorizationHeader
        //            {
        //                Source = "NJA",
        //                Type = "MIMS"
        //            };

        //            ServiceReference1.InsertCustomerRequest lRequest = new InsertCustomerRequest();
        //            lRequest.AuthorizationHeader = lHeader;

        //            ServiceReference1.CustomerData3 lCustomerData = new ServiceReference1.CustomerData3();

        //            lCustomerData.TitleId = 2;
        //            lCustomerData.CompanyId = 3;
        //            lCustomerData.CountryId = 61;
        //            lCustomerData.Initials = "HD";
        //            lCustomerData.Surname = "Reitmann";
        //            lCustomerData.CellPhoneNumber = "0829598631";
        //            lCustomerData.EmailAddress = "heinreitmann@gmail.com";
        //            //lCustomerData.PhoneNumber = "0128070533";
        //            lCustomerData.LoginEmail = "heinreitmann@gmail.com";

        //            lCustomerData.Address1 = "Remskoen 522";
        //            lCustomerData.Address2 = "Iets";
        //            lCustomerData.Address3 = "Die Wilgers";
        //            lCustomerData.Address4 = "Tshwane";
        //            lCustomerData.Address5 = "0041";
        //            lCustomerData.AddressType = ServiceReference1.AddressType.UnAssigned;

        //            lCustomerData.Correspondence2 = 1;
        //            lCustomerData.Liability = 0;
        //            lCustomerData.Status = ServiceReference1.CustomerStatus.Active;

        //            //lCustomerData.CheckpointInvoiceDate = DateTime.Now;
        //            //lCustomerData.CheckpointPaymentDate = DateTime.Now;
        //            //lCustomerData.VerificationDate = DateTime.Now;

        //            lCustomerData.ModifiedOn = DateTime.Now.Date;
        //            lCustomerData.ModifiedBy = "AVUSA\\REITMANNH";

        //            lRequest.pCustomerData = lCustomerData;

        //            lResponse = lClient.InsertCustomer(lRequest);
        //        }
        //        catch (Exception ex)
        //        {
        //            //Display all the exceptions

        //            Exception CurrentException = ex;
        //            int ExceptionLevel = 0;
        //            do
        //            {
        //                ExceptionLevel++;
        //                ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Update", "");
        //                CurrentException = CurrentException.InnerException;
        //            } while (CurrentException != null);

        //            if (ex.Message.Contains("Server was unable to read request"))
        //            {
        //                MessageBox.Show(ex.Message.Substring(ex.Message.IndexOf('>', 40) + 1));
        //                return;
        //            }
        //            else
        //            {
        //                throw ex;
        //            }
        //        }


        //        if (lResponse.InsertCustomerResult != "OK")

        //        {
        //            MessageBox.Show(lResponse.InsertCustomerResult + " " + lResponse.pCustomerData.CustomerId.ToString());
        //        }
        //        else
        //        {
        //            MessageBox.Show("CustomerId = " + lResponse.pCustomerData.CustomerId.ToString());
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
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ButtonInsertCustomer_Click", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        MessageBox.Show(ex.Message);
        //    }
        //}


        //private void ButtonGetCustomer_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        ServiceSoapClient lClient = new ServiceSoapClient();
        //        AuthorizationHeader lHeader = new AuthorizationHeader
        //        {
        //            Source = "NJA",
        //            Type = "MIMS"
        //        };

        //        ServiceReference1.GetCustomerRequest lRequest = new GetCustomerRequest();
        //        lRequest.AuthorizationHeader = lHeader;

        //        lRequest.pLoginEmail = "spadaccinod@tisoblackstar.co.za";

        //        ServiceReference1.GetCustomerResponse lResponse = new GetCustomerResponse();

        //        lResponse = lClient.GetCustomer(lRequest);

        //        if (lResponse.GetCustomerResult != "OK")

        //        {
        //            MessageBox.Show(lResponse.GetCustomerResult);
        //        }
        //        else
        //        {
        //            gCustomerDataShallow = lResponse.pCustomerData;
        //            MessageBox.Show("LoginEmail = " + lResponse.pCustomerData.LoginEmail + " " + lResponse.pCustomerData.NationalId1);
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
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ButtonGetCustomer_Click", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        MessageBox.Show(ex.Message);
        //    }
        //}






        //private void ButtonUpdateCustomer_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        ServiceSoapClient lClient = new ServiceSoapClient();
        //        AuthorizationHeader lHeader = new AuthorizationHeader
        //        {
        //            Source = "NJA",
        //            Type = "MIMS"
        //        };

        //        ServiceReference1.UpdateCustomerRequest lRequest = new UpdateCustomerRequest();
        //        lRequest.AuthorizationHeader = lHeader;

        //        if (gCustomerDataShallow == null)
        //        {
        //            MessageBox.Show("There is no customer to update.");
        //            return;
        //        }

        //        gCustomerDataShallow.PhoneNumber = "0128070533";

        //        lRequest.pCustomerData = gCustomerDataShallow;

        //        ServiceReference1.UpdateCustomerResponse lResponse = new UpdateCustomerResponse();

        //        lResponse = lClient.UpdateCustomer(lRequest);

        //        if (lResponse.UpdateCustomerResult != "OK")

        //        {
        //            MessageBox.Show(lResponse.UpdateCustomerResult);
        //        }
        //        else
        //        {
        //            MessageBox.Show("Address1 was updated to: " + lResponse.pCustomerData.Address1.ToString());
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
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ButtonUpdateCustomer_Click", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        MessageBox.Show(ex.Message);
        //    }
        //}

        private void ButtonReflection_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                Subs.Data.CustomerData3 lCustomerDataSource = new Data.CustomerData3();
                Subs.Data.CustomerData3 lCustomerDataTarget = new Data.CustomerData3(117224);

                lCustomerDataSource.FirstName = "HAHA";

                PropertyInfo[] lPropertiesSource = lCustomerDataSource.GetType().GetProperties();
                //PropertyInfo[] lPropertiesTarget = lCustomerDataTarget.GetType().GetProperties();


                PropertyInfo InitialsSource = lPropertiesSource[6];
                PropertyInfo InitialsTarget = lPropertiesSource[6];

                InitialsTarget.SetValue(lCustomerDataTarget, InitialsSource.GetValue(lCustomerDataSource, null));

                MessageBox.Show("Target value is for " + InitialsSource.Name + " " + lCustomerDataTarget.FirstName);




                //MessageBox.Show(prop.Name + " " + prop.GetValue(lCustomerDataSource, null).ToString());


                //MessageBox.Show("There are " + lProperties.Count().ToString() + " in CustomerData");

                //foreach (PropertyInfo prop in lProperties)
                //{
                //    MessageBox.Show(prop.Name + " " + prop.GetValue(lCustomerData, null).ToString());
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ButtonReflection", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonAuthorisationMIC_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ServiceSoapClient lClient = new ServiceSoapClient();
                AuthorizationHeader lHeader = new AuthorizationHeader
                {
                    Source = "NJA",
                    Type = "MOBIMims"
                };

                AuthorizeMICRequest lRequest = new AuthorizeMICRequest();
                lRequest.AuthorizationHeader = lHeader;
                lRequest.pProductId = 88;
                lRequest.pReceiverId = 999999; // 120072;
                lRequest.pPassword = "Sannie";


                AuthorizeMICResponse lResponse = new AuthorizeMICResponse();

                lResponse = lClient.AuthorizeMIC(lRequest);

                MessageBox.Show(lResponse.AuthorizeMICResult.ExpirationDate.ToString() + " Seats " 
                    + lResponse.AuthorizeMICResult.Seats.ToString() + " " 
                    + lResponse.AuthorizeMICResult.Reason.ToString() + " "
                    + lResponse.AuthorizeMICResult.Title.ToString() + " "
                    + lResponse.AuthorizeMICResult.FirstName.ToString() + " "
                    + lResponse.AuthorizeMICResult.Surname.ToString());


            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ButtonAuthorizeMIC_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }

        }

        private void ButtonAuthorisationToken_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ServiceSoapClient lClient = new ServiceSoapClient();
                AuthorizationHeader lHeader = new AuthorizationHeader
                {
                    Source = "NJA",
                    Type = "MOBIMims"
                };

                ServiceReference1.AuthorizeTokenRequest lRequest = new AuthorizeTokenRequest();
                lRequest.AuthorizationHeader = lHeader;
                lRequest.pProductId = 88;
                lRequest.pTokenId = 9525472; // 108244 - Riette;
               

                AuthorizeTokenResponse lResponse = new AuthorizeTokenResponse();

                lResponse = lClient.AuthorizeToken(lRequest);

                MessageBox.Show(lResponse.AuthorizeTokenResult.ExpirationDate.ToString() + " Seats "
                    + lResponse.AuthorizeTokenResult.Seats.ToString() + " "
                    + lResponse.AuthorizeTokenResult.Reason.ToString() + " "
                    + lResponse.AuthorizeTokenResult.Title.ToString() + " "
                    + lResponse.AuthorizeTokenResult.FirstName.ToString() + " "
                    + lResponse.AuthorizeTokenResult.Surname.ToString());


            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ButtonAuthorizeToken_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show(ex.Message);
            }
        }
    }
    
}
