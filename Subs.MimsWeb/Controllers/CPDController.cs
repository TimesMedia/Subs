using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using Subs.MimsWeb.Models;
using CPD.Data;
using CPD.Business;

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

        public ActionResult Reissue(int ResultId = 0)
        {
            try
            {
                // Take precautions in case the session has expired. 
                if (Session["CustomerId"] == null)
                {
                    Session["NextPage"] = "Reissue";
                    return RedirectToAction("Index");
                    
                }
                    List<History> lResult = ResultData.GetByResultId(ResultId);

                    if (lResult[0].Verdict == "Failed")
                    {
                        ViewBag.Message = "According to my records you did not pass this test. If you think you have a case, please contact MIMS at 011 280 5533";
                        return View("History"); 
                    }

                    Thread lWorkerThread = new Thread(ProcessCertificate);
                    lWorkerThread.SetApartmentState(ApartmentState.STA);
                    object pState = ResultId;  // Box it
                    lWorkerThread.Start(pState);
                    lWorkerThread.Join();

                    ViewBag.Message = "Certificate sucessfully reissued.";

                    ViewBag.Message = ResultId.ToString();
                return View("History");
            
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

        private void ProcessCertificate(object pState)

        {
            try
            {
                CPD.Business.CPDCertificate lCertificate = new CPD.Business.CPDCertificate();

                {
                    string lResult;
                    if ((lResult = lCertificate.Render((int)pState)) != "OK")
                    {
                        throw new Exception("Error rendering certificate " + lResult);
                    }
                }

                {
                    string lResult;
                    if ((lResult = lCertificate.EmailCertificate((int)pState)) != "OK")
                    {
                        throw new Exception("Error rendering certificate " + lResult);
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ProcessCertificate", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);
            }
        }

        [HttpGet]
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


        [HttpPost]
        public ActionResult Read(int pIssueId)
        {
            return("DisplayReading");
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




