using Subs.Business;
using Subs.Data;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace Subs.XMLService
{
    public class AuthorizationHeader : SoapHeader
    {
        public string Type;
        public string Source;
    }

    public class UserInfo
    {
        public string Name;
        public string Surname;
        public string CouncilNumber;
    }

    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Service : System.Web.Services.WebService
    {
        public AuthorizationHeader Authentication;

        public void Sevice()
        {

        }


        [SoapHeader("Authentication")]
        [WebMethod]
        public bool SendOTP(int pCustomerId, int pOTP)
        {
            try
            {
                if (Authentication.Source == "NJA" &&
                Authentication.Type == "MOBIMims")
                {
                    CustomerData3 lCustomerData = new CustomerData3(pCustomerId);

                    string lBody = "Your OTP is: " + pOTP + " This OTP will expire by " +  DateTime.Now.AddMinutes(10).ToString();

                    if (CustomerBiz.SendEmail("", lCustomerData.EmailAddress, "MobiMims OTP", lBody) != "OK")
                    {
                        return false;
                    }
                    return true;
                }
                else
                {
                    throw new Exception("Invalid Source and/or Type");
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "SendOTP", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                
                return false;
            }
        }


        [SoapHeader("Authentication")]
        [WebMethod]
        public SeatResult Authorize(int pProductId, int pReceiverId)
        {
            try
            {

                if (Authentication.Source == "NJA" &&
                Authentication.Type == "MOBIMims")
                {
                    return SubscriptionBiz.Authorize(pProductId, pReceiverId);
                }
                else
                {
                    throw new Exception("Invalid Source and/or Type");
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Authorize", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                SeatResult lSeatResult = new SeatResult();
                lSeatResult.Seats = 0;
                lSeatResult.Reason = "Failed due to technical error";

                return lSeatResult;
            }
        }


        [SoapHeader("Authentication")]
        [WebMethod]
        public MICResult AuthorizeMIC(int pReceiverId, string pPassword, int pProductId)
        {
            try
            {
                MICResult lMICResult = new MICResult();


                if (Authentication.Source == "NJA" &&
                Authentication.Type == "MOBIMims")
                {
                    if (!CustomerData3.Exists((int)pReceiverId))
                    {
                       lMICResult.Reason = "There is no such CustomerId";
                       lMICResult.Title = "NoTitle";
                       lMICResult.FirstName = "NoFirstName";
                       lMICResult.Surname = "NoSurname";
                       return lMICResult;
                    }

                    CustomerData3 lCustomerData = new CustomerData3(pReceiverId);

                    if (lCustomerData.Password1 != pPassword)
                    {
                        lMICResult.Reason = "Invalid Password";
                        return lMICResult;
                    }

                    SeatResult lSeatResult = SubscriptionBiz.Authorize(pProductId, pReceiverId);
                    lMICResult.ExpirationDate = lSeatResult.ExpirationDate;
                    lMICResult.Seats = lSeatResult.Seats;
                    lMICResult.Reason = lSeatResult.Reason;
                    lMICResult.Title = lCustomerData.Title;
                    lMICResult.FirstName = lCustomerData.FirstName;
                    lMICResult.Surname = lCustomerData.Surname;
                    return lMICResult;
                }
                else
                {
                    throw new Exception("Invalid Source and/or Type");
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Authorize with 3 parameters", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MICResult lMICResult = new MICResult();
                lMICResult.Seats = 0;
                lMICResult.Reason = "Failed due to technical error";

                return lMICResult;
            }
        }


        [SoapHeader("Authentication")]
        [WebMethod]
        public TokenResult AuthorizeToken(int pTokenId, int pProductId)
        {
            TokenResult lTokenResult = new TokenResult();
            try
            {
                int lReceiverId = pTokenId / pProductId;

                if (Authentication.Source == "NJA" &&
                Authentication.Type == "MOBIMims")
                {
                    if (!CustomerData3.Exists((int)lReceiverId))
                    {
                        lTokenResult.Reason = "There is no such CustomerId";
                        lTokenResult.Title = "NoTitle";
                        lTokenResult.FirstName = "NoFirstName";
                        lTokenResult.Surname = "NoSurname";
                        lTokenResult.CustomerId = "NoCustomerId";
                        lTokenResult.Password = "No Password Issued";
                        return lTokenResult;
                    }

                    CustomerData3 lCustomerData = new CustomerData3(lReceiverId);

                    SeatResult lSeatResult = SubscriptionBiz.Authorize(pProductId, lReceiverId);
                    lTokenResult.ExpirationDate = lSeatResult.ExpirationDate;
                    lTokenResult.Seats = lSeatResult.Seats;
                    lTokenResult.Reason = lSeatResult.Reason;
                    lTokenResult.Title = lCustomerData.Title;
                    lTokenResult.FirstName = lCustomerData.FirstName;
                    lTokenResult.Surname = lCustomerData.Surname;
                    lTokenResult.Password = lCustomerData.Password1;
                    lTokenResult.CustomerId = lCustomerData.CustomerId.ToString();
                    return lTokenResult;
                }
                else
                {
                    throw new Exception("Invalid Source and/or Type");
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "AuthorizeToken", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MICResult lMICResult = new MICResult();
                lMICResult.Seats = 0;
                lMICResult.Reason = "Failed due to technical error";

                return lTokenResult;
            }
        }





        [SoapHeader("Authentication")]
        [WebMethod]
        public List<AuthorizationResult> Authorizations()
        {
            List<AuthorizationResult> lResult = new List<AuthorizationResult>();

            int lProgress = 0;
            try
            {
                lProgress = 1;
                if (Authentication.Source == "NJA" &&
                Authentication.Type == "MOBIMims")
                {
                    lProgress = 2;
                    lResult = SubscriptionBiz.Authorizations();
                    lProgress = 3;
                    return lResult;
                }
                else
                {
                    lProgress = 4;
                    throw new Exception("Invalid Source and/or Type");
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Authorizations", "Progress = " + lProgress.ToString());
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return lResult;
            }
        }


        [SoapHeader("Authentication")]
        [WebMethod]
        public bool FindEMailByCustomerId(int pCustomerId, out string pEmailAddress)
        {
            pEmailAddress = "Non Existent";

            try
            {

                if (Authentication.Source == "NJA" &&
                Authentication.Type == "MOBIMims")
                {
                    if (CustomerData3.Exists(pCustomerId))
                    {
                        CustomerData3 lCustomerData = new CustomerData3(pCustomerId);
                        pEmailAddress = lCustomerData.EmailAddress;
                    }
                    return true;
                }
                else
                {
                    throw new Exception("Invalid Source and/or Type");
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "FindEmailByCustomer", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return false;
            }
        }


        [SoapHeader("Authentication")]
        [WebMethod]
        public bool FindCustomerIdByEmail(string EmailAddress, out int CustomerId)
        {
            try
            {


                if (Authentication.Source == "NJA" &&
                Authentication.Type == "MOBIMims")
                {
                    return Subs.Data.CustomerData3.FindCustomerId(EmailAddress, out CustomerId);
                }
                else
                {
                    throw new Exception("Invalid Source and/or Type");
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "FindCustomerIdByEmail", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                CustomerId = 0;

                return false;
            }
        }


        [SoapHeader("Authentication")]
        [WebMethod]
        public bool FindCustomerIdByNationalId(string NationalId, out int CustomerId)
        {
            try
            {


                if (Authentication.Source == "NJA" &&
                Authentication.Type == "MOBIMims")
                {
                    return Subs.Data.CustomerData3.FindCustomerIdByNationalId(NationalId, out CustomerId);
                }
                else
                {
                    throw new Exception("Invalid Source and/or Type");
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "FindCustomerIdByEmail", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                CustomerId = 0;

                return false;
            }
        }
    }
}
