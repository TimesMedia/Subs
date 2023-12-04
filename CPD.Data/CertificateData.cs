using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Subs.Data;

namespace CPD.Data
{
    public struct Certificate
    {
        public int CustomerId;
        public string Customer;
        public string CouncilNumber;
        public string Module;
        public string Publication;
        public string AccreditationNumber;
        public decimal NormalPoints;
        public decimal? EthicsPoints;
        public string EMailAddress;
        public DateTime Datum;
        public string AccreditationNumber2;
    }

    public static class CertificateData
    {

        public static Certificate GetCertificate(int pResultId)
        {   
            Certificate lCertificate = new Certificate();
            try
            {
               // Get the first part from the CPD database

                ResultData lResultData = new ResultData();

                History lResult = lResultData.GetByResultId(pResultId)[0];

                if (lResult == null)
                {
                    throw new Exception("No certificate data found.");
                }

                lCertificate.Publication = lResult.Publication; 
                lCertificate.Module = lResult.Module;

                if (lResult.AccreditationNumber != null)
                {
                    lCertificate.AccreditationNumber = lResult.AccreditationNumber;
                }
             
                if (lResult.Datum != null)
                {
                    lCertificate.Datum = lResult.Datum;
                }
  
                lCertificate.NormalPoints = lResult.NormalPoints;
                lCertificate.EthicsPoints = lResult.EthicsPoints;
                lCertificate.CustomerId = lResult.CustomerId;

                // Get the rest of the data

                CustomerData3 lCustomerData = new CustomerData3(lCertificate.CustomerId);

                lCertificate.CouncilNumber = lCustomerData.CouncilNumber;
                lCertificate.EMailAddress = lCustomerData.EmailAddress;
                lCertificate.Customer = lCustomerData.FullName;
                
                return lCertificate;
            }
            catch (Exception Ex)
            {
                // Record the event in the Exception table
                ExceptionData.WriteException(1, Ex.Message, "static CertificateData", "GetCertificate", "ResultId = " + pResultId.ToString());
                throw Ex;
            }
            
        }

        //public static Certificate GetCertificate(int ResultId)
        //{
        //    SqlConnection lConnection = new SqlConnection(Subs.Data.Settings.CPDConnectionString);
        //    Certificate lCertificate = new Certificate();
        //    try
        //    {
        //        // Get the first part from the CPD database

        //        SqlCommand Command = new SqlCommand();
        //        SqlDataAdapter Adaptor = new SqlDataAdapter();
        //        lConnection.Open();
        //        Command.Connection = lConnection;
        //        Command.CommandType = CommandType.StoredProcedure;
        //        Command.CommandText = "[CertificateData.GetCertificate]";
        //        SqlCommandBuilder.DeriveParameters(Command);
        //        Command.Parameters["@ResultId"].Value = ResultId;

        //        SqlDataReader lReader = Command.ExecuteReader();

        //        if (!lReader.Read())
        //        {
        //            throw new Exception("No certificate data found.");
        //        }

        //        lCertificate.Publication = (string)lReader[nameof(lCertificate.Publication)];
        //        lCertificate.Module = (string)lReader[nameof(lCertificate.Module)];

        //        if (lReader[nameof(lCertificate.AccreditationNumber)] != System.DBNull.Value)
        //        {
        //            lCertificate.AccreditationNumber = (string)lReader[nameof(lCertificate.AccreditationNumber)];
        //        }

        //        if (lReader[nameof(lCertificate.AccreditationNumber2)] != System.DBNull.Value)
        //        {
        //            lCertificate.AccreditationNumber2 = (string)lReader[nameof(lCertificate.AccreditationNumber2)];
        //        }

        //        if (lReader[nameof(lCertificate.Datum)] != System.DBNull.Value)
        //        {
        //            lCertificate.Datum = (DateTime)lReader[nameof(lCertificate.Datum)];
        //        }

        //        if (lReader[nameof(lCertificate.Datum)] != System.DBNull.Value)
        //        {
        //            lCertificate.Datum = (DateTime)lReader[nameof(lCertificate.Datum)];
        //        }

        //        lCertificate.NormalPoints = (decimal)lReader[nameof(lCertificate.NormalPoints)];
        //        lCertificate.EthicsPoints = (decimal)lReader[nameof(lCertificate.EthicsPoints)];
        //        lCertificate.CustomerId = (int)lReader[nameof(lCertificate.CustomerId)];

        //        // Get the rest of the data

        //        CustomerData3 lCustomerData = new CustomerData3(lCertificate.CustomerId);

        //        lCertificate.CouncilNumber = lCustomerData.CouncilNumber;
        //        lCertificate.EMailAddress = lCustomerData.EmailAddress;
        //        lCertificate.Customer = lCustomerData.FullName;

        //        return lCertificate;
        //    }
        //    catch (Exception Ex)
        //    {
        //        // Record the event in the Exception table
        //        ExceptionData.WriteException(1, Ex.Message, "static CertificateData", "GetCertificate", "ResultId = " + ResultId.ToString());
        //        throw Ex;
        //    }
        //    finally
        //    {
        //        lConnection.Close();
        //    }
        //}


        public static void IssueOfCertificate(int pResultId, DateTime pDate)
            {
                try
                {
                    // Get all the answers
                  

                    DataSet1TableAdapters.ResultTableAdapter lAdapter = new DataSet1TableAdapters.ResultTableAdapter();
                    lAdapter.AttachConnection();
                    lAdapter.IssueCertificate(pResultId, DateTime.Now);
                    lAdapter.Connection.Close();

                }
            catch (Exception Ex)
            {
                // Record the event in the Exception table
                ExceptionData.WriteException(1, Ex.Message, "static CertificateData", "IssueOfCertificate", "ResultId = " + pResultId.ToString());
                throw Ex;
            }

        }
    }
}
