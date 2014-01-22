using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using EPiServer.Shell.Gadgets;

namespace EPiServer.Controllers
{
    [Gadget]    
    public class GoogleTalkController : Controller
    {
        //
        // GET: /GoogleTalk/

        public ActionResult Index()
        {
            return View();
        }

    }
}
