namespace Subs.Data
{
    public class Settings
    {
        public static string ConnectionString;
        public static string SUBSDWConnectionString;
        public static CustomerDoc2.CustomerDataTable gCustomerTable = new CustomerDoc2.CustomerDataTable();
        public static int CurrentCustomerId = 0;
        public static int Authority = 0;
        public static string DirectoryPath; // The Directory used for batch processing
        public static string Version;
    }

}
