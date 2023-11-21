using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using Subs.MimsWeb.Models;
using CPD.Data;

namespace MimsWeb.Controllers
{
    public class CPDController : Controller
    {
        public CPDController()
        {
            
        }

        // GET: CPD
        public ActionResult Index()
        {
            LoginRequest lLoginRequest = Subs.MimsWeb.SessionHelper.GetLoginRequest(Session);
            ViewBag.Message = lLoginRequest.Email + " " + lLoginRequest.CustomerId.ToString();

            List<History> lHistory = ResultData.GetHistory("History", (int)lLoginRequest.CustomerId);

            return View("Index");
        }

        public ActionResult History()
        {
            LoginRequest lLoginRequest = Subs.MimsWeb.SessionHelper.GetLoginRequest(Session);
            List<History> lHistory = ResultData.GetHistory("History", (int)lLoginRequest.CustomerId);
            return View("History", lHistory);
        }

        public ActionResult Reissue(int Id)
        {
            try
            {
                //// Take precautions in case the session has expired. 
                //if (Session["CustomerId"] == null)
                //{
                //    Session["NextPage"] = "Enrol";
                //    return RedirectToAction("Index");
                //    //Response.Redirect("./Login2.aspx", false);
                //}

                //// Ensure that a module has been selected

                //if (this.GridViewHistory.SelectedIndex < 0)
                //{
                //    ViewBag.Message = "Please select a test that you have passed first";
                //    return RedirectToAction("History");

                //int lResultId = (int)this.GridViewHistory.SelectedValue;
                //ResultDoc.HistoryDataTable lResult = ResultData.GetByResultId(lResultId);

                //if (lResult[0].Verdict == "Failed")
                //{
                //    ViewBag.Message = "According to my records you did not pass this test. If you think you have a case, please contact MIMS at 011 280 5533";
                //    return RedirectToAction("History"); ;
                //}
                
                //Thread lWorkerThread = new Thread(ProcessCertificate);
                //lWorkerThread.SetApartmentState(ApartmentState.STA);
                //object pState = lResultId;  // Box it
                //lWorkerThread.Start(pState);
                //lWorkerThread.Join();
   
                //ViewBag.Message = "Certificate sucessfully reissued.";

                ViewBag.Message = Id.ToString();
                return RedirectToAction("History");
            
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ButtonReissue_Click", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);
                throw ex;
            }

        }

        //private void ProcessCertificate(object pState)

        //{
        //    try
        //    {
        //        CPD.Business.CPDCertificate lCertificate = new Business.CPDCertificate();

        //        {
        //            string lResult;
        //            if ((lResult = lCertificate.Render((int)pState)) != "OK")
        //            {
        //                LabelResponse.Text = "Error rendering certificate " + lResult;
        //            }
        //        }

        //        {
        //            string lResult;
        //            if ((lResult = lCertificate.EmailCertificate((int)pState)) != "OK")
        //            {
        //                LabelResponse.Text = "Error emailing certificate " + lResult;
        //                return;
        //            }
        //        }
        //        LabelResponse.Text = "Certificate sucessfully reissued.";
        //    }
        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ProcessCertificate", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);
        //    }
        //}


        public ActionResult Read()
        {
            LoginRequest lLoginRequest = Subs.MimsWeb.SessionHelper.GetLoginRequest(Session);
            if (lLoginRequest == null)
            {
                ViewBag.Message = "You have to log in first.";
            }

            List<AvailableSurvey> lSurveys = ModuleData.GetAvailableRead((int)lLoginRequest.CustomerId);
            return View("Read",lSurveys);
        }

        public ActionResult SelectRead()
        {
            LoginRequest lLoginRequest = Subs.MimsWeb.SessionHelper.GetLoginRequest(Session);
            if (lLoginRequest == null)
            {
                ViewBag.Message = "You have to log in first.";
            }

            // Display PDF.

            //List<AvailableSurvey> lSurveys = ModuleData.GetAvailableRead((int)lLoginRequest.CustomerId);
            return View();
        }




public ActionResult Enrol()
        {

            List<AvailableSurvey> lSurveys = ModuleData.GetAvailableTest(108244);

            return View(lSurveys);
        }

        public ActionResult Privacy()
        {
            return View();
        }

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}
    }
}




