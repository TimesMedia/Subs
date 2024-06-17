using System;
using System.Data.SqlClient;

namespace Subs.Data
{
}

namespace Subs.Data.DeliveryDocTableAdapters
{
    public partial class DeliveryProposalTableAdapter
    {
        private readonly SqlConnection lConnection = new SqlConnection();

        public bool AttachConnection()
        {
            try
            {
                // Set the connectionString for this object

                lConnection.ConnectionString = Settings.ConnectionString;


                // Replace the designer's connection with yor own one.
                foreach (SqlCommand myCommand in CommandCollection)
                {
                    myCommand.Connection = lConnection;
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "AttachConnection", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return false;
            }
        }
    }
}
namespace Subs.Data
{


    partial class DeliveryDoc
    {
        partial class DeliveryProposalDataTable
        {
        }
    }
}
