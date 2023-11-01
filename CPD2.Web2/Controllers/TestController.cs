using CPD2.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPD2.Web2.Controllers
{
    public class TestController : Controller
    {
        public IActionResult EnrolModule(int Id)
        {
           
            List<AvailableModule> lModules = ModuleData.GetAvailableModules(Id);
            return View(lModules);
        }

        public IActionResult Test(int Id)
        {
            ViewBag.ModuleId = Id;

            return View();
        }

            public IActionResult Continue()
        {
            return View();
        }

    }
}
