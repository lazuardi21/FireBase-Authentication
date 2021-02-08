using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcApplication1.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
       
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult example1()
        {
            return View();
        }

        public ActionResult example2()
        {
            return View();
        }

        public ActionResult example3()
        {
            return View();
        }
        public ActionResult example4()
        {
            return View();
        }

    }
}
