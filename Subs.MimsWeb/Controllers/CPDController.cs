using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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

        public ActionResult Reissue(int pResultId)
        {
            return View(pResultId);
        }

        public ActionResult Read()
        {
            List<AvailableSurvey> lSurveys = ModuleData.GetAvailableRead(108244);
            return View(lSurveys);
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
