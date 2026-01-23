using GeoEMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace GeoEMS.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Index(UserLogin login)
        {
            if (ModelState.IsValid)
            {
                UserLogin user = new UserLogin();
              EMSModel model = new EMSModel();  
                user = model.Login(login.UserName, login.Password);
                if (user != null)
                {
                    Session["UserName"] = login.UserName;
                    FormsAuthentication.SetAuthCookie(user.UserName, true);
                    var authTicket = new FormsAuthenticationTicket(1, login.UserName, DateTime.Now, DateTime.Now.AddMinutes(20), false, login.UserName);
                    string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                    var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    HttpContext.Response.Cookies.Add(authCookie);
                    return RedirectToAction("Index", "Home");
                }

                else
                {
                    ModelState.AddModelError("error", "Entered Invalid Username or Password");
                    ViewBag.errormessage = "Entered Invalid Username and Password";
                }
            }
            return View();
        }



        [HttpGet]
        public ActionResult Logout()
        {
            try
            {
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetExpires(DateTime.Now.AddSeconds(-1));
                Response.Cache.SetNoStore();
                FormsAuthentication.SignOut();
                HttpCookie Cookies = new HttpCookie("WebTime");
                Cookies.Value = "";
                Cookies.Expires = DateTime.Now.AddHours(-1);
                Response.Cookies.Add(Cookies);
                HttpContext.Session.Clear();
                Session.Abandon();
                return RedirectToAction("Index", "Account");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}