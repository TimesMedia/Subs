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
        // GET: CPD
        public ActionResult Index()
        {
              
             LoginRequest lLoginRequest = Subs.MimsWeb.SessionHelper.GetLoginRequest(Session);
     

            ViewBag.Message = lLoginRequest.Email + " " + lLoginRequest.CustomerId.ToString();

            return View("CPDLayout");
        }


        public ActionResult History()
        {
            List<History> lHistory = ResultData.GetHistory("History", 108244);
            return View(lHistory);
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
