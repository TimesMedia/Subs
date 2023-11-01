using System.Data.SqlClient;
using System;

namespace CPD.Data
{
}


namespace CPD.Data.DataSet1TableAdapters
{
    partial class ResultTableAdapter
    {
        private SqlConnection gConnection = new SqlConnection();

        public bool AttachConnection()
        {
            try
            {

                gConnection.ConnectionString = Settings.CPDConnectionString;


                // Replace the designer's connection with yor own one.
                foreach (SqlCommand myCommand in CommandCollection)
                {
                    myCommand.Connection = gConnection;
                }

                this.Adapter.UpdateCommand.Connection = gConnection;
                this.Adapter.InsertCommand.Connection = gConnection;
                this.Adapter.DeleteCommand.Connection = gConnection;
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


    partial class ResultDetailTableAdapter
    {
        private SqlConnection gConnection = new SqlConnection();

        public bool AttachConnection()
        {
            try
            {

                gConnection.ConnectionString = Settings.CPDConnectionString;


                // Replace the designer's connection with yor own one.
                foreach (SqlCommand myCommand in CommandCollection)
                {
                    myCommand.Connection = gConnection;
                }

                this.Adapter.UpdateCommand.Connection = gConnection;
                this.Adapter.InsertCommand.Connection = gConnection;
                this.Adapter.DeleteCommand.Connection = gConnection;
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

    partial class AnswerTableAdapter
    {
        private SqlConnection gConnection = new SqlConnection();

        public bool AttachConnection()
        {
            try
            {
                gConnection.ConnectionString = Settings.CPDConnectionString;

                // Replace the designer's connection with yor own one.
                foreach (SqlCommand myCommand in CommandCollection)
                {
                    myCommand.Connection = gConnection;
                }

                this.Adapter.UpdateCommand.Connection = gConnection;
                this.Adapter.InsertCommand.Connection = gConnection;
                this.Adapter.DeleteCommand.Connection = gConnection;
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



    partial class Survey2TableAdapter
    {
        private SqlConnection lConnection = new SqlConnection();

        public bool AttachConnection()
        {
            try
            {
                // Set the connectionString for this object

                lConnection.ConnectionString = Settings.CPDConnectionString;


                // Replace the designer's connection with yor own one.
                foreach (SqlCommand myCommand in CommandCollection)
                {
                    myCommand.Connection = lConnection;
                }

                this.Adapter.UpdateCommand.Connection = lConnection;
                this.Adapter.InsertCommand.Connection = lConnection;
                this.Adapter.DeleteCommand.Connection = lConnection;

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


    partial class ModuleTableAdapter
    {
        private SqlConnection lConnection = new SqlConnection();

        public bool AttachConnection()
        {
            try
            {
                // Set the connectionString for this object

                lConnection.ConnectionString = Settings.CPDConnectionString;


                // Replace the designer's connection with yor own one.
                foreach (SqlCommand myCommand in CommandCollection)
                {
                    myCommand.Connection = lConnection;
                }

                this.Adapter.UpdateCommand.Connection = lConnection;
                this.Adapter.InsertCommand.Connection = lConnection;
                this.Adapter.DeleteCommand.Connection = lConnection;

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




    partial class ArticleTableAdapter
    {
        private SqlConnection lConnection = new SqlConnection();

        public bool AttachConnection()
        {
            try
            {
                // Set the connectionString for this object

                lConnection.ConnectionString = Settings.CPDConnectionString;


                // Replace the designer's connection with yor own one.
                foreach (SqlCommand myCommand in CommandCollection)
                {
                    myCommand.Connection = lConnection;
                }

                this.Adapter.UpdateCommand.Connection = lConnection;
                this.Adapter.InsertCommand.Connection = lConnection;
                this.Adapter.DeleteCommand.Connection = lConnection;

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




    partial class QuestionTableAdapter
    {
        private SqlConnection lConnection = new SqlConnection();

        public bool AttachConnection()
        {
            try
            {
                // Set the connectionString for this object

                lConnection.ConnectionString = Settings.CPDConnectionString;


                // Replace the designer's connection with yor own one.
                foreach (SqlCommand myCommand in CommandCollection)
                {
                    myCommand.Connection = lConnection;
                }

                this.Adapter.UpdateCommand.Connection = lConnection;
                this.Adapter.InsertCommand.Connection = lConnection;
                this.Adapter.DeleteCommand.Connection = lConnection;

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
