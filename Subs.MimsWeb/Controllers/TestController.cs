using System;
using System.Collections.Generic;
using System.Web.Mvc;
using CPD.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MimsWeb.Controllers
{
    public class TestController : Controller
    {
        public ActionResult EnrolModule(int Id)
        {
           
            List<AvailableModule> lModules = ModuleData.GetAvailableModules(Id);
            return View(lModules);
        }

        public ActionResult Test(int Id)
        {
            ViewBag.ModuleId = Id;

            return View();
        }

            public ActionResult Continue()
        {
            return View();
        }

    }
}
