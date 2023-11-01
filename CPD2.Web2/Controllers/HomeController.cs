using CPD2.Data;
using CPD2.Web2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CPD2.Web2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;

            IConfiguration config = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", true, true)
               .Build();
            Settings.CPDConnectionString = config["CPDConnectionString"];
        }

        public IActionResult Index()
        {
            //ISession lSession = (ISession)HttpContext.Session;

            //Requires SessionExtensions from sample download.
            //if (HttpContext.Session.Get<DateTime>(SessionKeyTime) == default)
            //{
            //    HttpContext.Session.Set<DateTime>(SessionKeyTime, currentTime);
            //}

            //HttpContext.Session.SetInt32("CustomerId", id );


            List<Data.History> lBuzy = ResultData.GetHistory("CurrentTests", 108244);

           return View(lBuzy);
        }

        public  IActionResult Login()
        { 
            ViewBag.Message = "You are now logged in";
            // Create a session state
            return View("Index");
        }


        public IActionResult History()
        {
            List<Data.History> lHistory = ResultData.GetHistory("History", 108244);
            return View(lHistory);
        }


        public IActionResult Read()
        {
            List<AvailableSurvey> lSurveys = ModuleData.GetAvailableRead(108244);
            return View(lSurveys);
        }




        public IActionResult Enrol()
        {

            List<AvailableSurvey> lSurveys = ModuleData.GetAvailableTest(108244);

            return View(lSurveys);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
