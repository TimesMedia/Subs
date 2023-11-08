using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Subs.Data;

namespace Subs.MimsWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            try { 
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

           
                Settings.ConnectionString = global::MimsWeb.Properties.Settings.Default.ConnectionString; ;
                Settings.CPDConnectionString = global::MimsWeb.Properties.Settings.Default.CPDConnectionString;
                Settings.DirectoryPath = global::MimsWeb.Properties.Settings.Default.DirectoryPath;
                

            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Application_Start", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);
            }
        }
    }
}
