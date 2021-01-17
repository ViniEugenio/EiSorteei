using EiSorteei.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EiSorteei.Areas.Admin.Controllers
{
    [FilterLogin]
    public class DashBoardController : Controller
    {
        // GET: Admin/DashBoard
        public ActionResult Index()
        {
            return View();
        }
       
    }
}