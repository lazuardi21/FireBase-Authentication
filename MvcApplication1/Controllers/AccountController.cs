using System.Web.Mvc;
using System.Web.Mvc.Html;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Firebase.Auth;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using MvcApplication1.Models;
using System.Web.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace MvcApplication1.Controllers
{
    public class AccountController : Controller
    {
        //private string connectionString = ConfigurationManager.ConnectionStrings["TEST"].ConnectionString;
        IFirebaseConfig config = new FireSharp.Config.FirebaseConfig
        {
            //AuthSecret = "kEAhYwvxB8mfq1wL25i4arAyauTxyfxKNDsZzkG2",
            //BasePath = "https://asp-mvc-with-android-d4bd2-default-rtdb.firebaseio.com/" 

            AuthSecret = WebConfigurationManager.AppSettings["AuthSecret"],
            BasePath = WebConfigurationManager.AppSettings["BasePath"]
        };
        IFirebaseClient client;

        //private static string ApiKey = "AIzaSyCvklIOr1wa8sRFEIvefGe3E2msUcur1D0";
        //private static string Bucket = "https://asp-mvc-with-android-d4bd2-default-rtdb.firebaseio.com/";
        // GET: Account

        private SqlConnection con;
        private void connection()
        {
            string constr = ConfigurationManager.ConnectionStrings["GrainConnection"].ToString();
            con = new SqlConnection(constr);
        }

        [HttpGet]
        public ActionResult SignUp()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public ActionResult Register(string id, string phone)
        {
            var r = new RegViewModel();
            r.id = id;
            r.phone = phone.Trim().Replace(" ", "");
            return View(r);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Register(RegViewModel model, string returnUrl)
        {
            try
            {
                client = new FireSharp.FirebaseClient(config);
                var data = model;
                data.phone = "+" + model.phone.Replace(" ", "");
                //PushResponse response = client.Push("User/" + data.Id, data);
                data.id = model.id;
                SetResponse setResponse = client.Set("User/" + data.phone, data);
                returnUrl = "/";
                return this.RedirectToLocal(returnUrl);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            //try
            //{
            //    // Verification.
            //    if (this.Request.IsAuthenticated)
            //    {
            //        //returnUrl = "/";
            //        return this.RedirectToLocal(returnUrl);

            //    }
            //}
            //catch (Exception ex)
            //{
            //    // Info
            //    Console.Write(ex);
            //}

            // Info.
            
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult SignUp(SignUpModel model, string returnUrl)
        {
            try
            {


                var token = model.token;
                var phone = model.phone;
                var datetime = model.datetime;
                returnUrl = "/";

                this.SignInUser(phone, token, datetime, false);
                //return this.RedirectToLocal(returnUrl);
                //ModelState.AddModelError(string.Empty, "Please Verify your email then login Plz.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {

            try
            {
                // Verification.
                if (ModelState.IsValid)
                {
                    var Data = model;
                    Data.phone = model.phone.Replace(" ", "");
                    client = new FireSharp.FirebaseClient(config);
                    FirebaseResponse response = client.Get("User/" + Data.phone);
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
                    dynamic d = JObject.Parse(data.ToString());
                    string phone = Data.phone;
                    var list = new List<Models.User>();
                    var ph = "";
                    var pass = "";
                    //foreach (Newtonsoft.Json.Linq.JProperty q in d)
                    //{

                    //foreach (Newtonsoft.Json.Linq.JObject p in q)
                    //{
                    foreach (var w in d)
                    {
                        string name = ((Newtonsoft.Json.Linq.JProperty)w).Name;
                        JToken value = ((Newtonsoft.Json.Linq.JProperty)w).Value;

                        if (name == "password")
                        {
                            pass = value.ToString();
                        }
                        if (name == "phone")
                        {
                            ph = value.ToString();
                        }

                        else
                        {

                        }
                    }
                    //}

                    //}
                    if (phone == ph && model.password == pass)
                    {
                        model.datetime = DateTime.Now;
                        returnUrl = "/";
                        this.SignInUser(model.phone, model.password, model.datetime, false);

                       
                        return this.RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        // Setting.
                        ModelState.AddModelError(string.Empty, "Invalid username or password.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Info
                Console.Write(ex);
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        private void InsertDataLog(string phone, DateTime datetime)
        {
            //To Prevent firing validation error on first page Load  

            if (ModelState.IsValid)
            {
                connection();
                SqlCommand com = new SqlCommand("InsertData", con);
                com.CommandType = CommandType.StoredProcedure;
                //com.Parameters.AddWithValue("@ID", 1);
                com.Parameters.AddWithValue("@PHONE", phone);
                com.Parameters.AddWithValue("@DATETIME", datetime);
                con.Open();
                int i = com.ExecuteNonQuery();
                con.Close();
               
            }

            ModelState.Clear();
        }

        private void SignInUser(string phone, string token, DateTime datetime, bool isPersistent)
        {
            // Initialization.
            var claims = new List<Claim>();

            try
            {
                // Setting
                claims.Add(new Claim(ClaimTypes.MobilePhone, phone));
                claims.Add(new Claim(ClaimTypes.Authentication, token));
                var claimIdenties = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                var ctx = Request.GetOwinContext();
                var authenticationManager = ctx.Authentication;
                // Sign In.
                authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, claimIdenties);
                
                this.InsertDataLog(phone, datetime);
            }
            catch (Exception ex)
            {
                // Info
                throw ex;
            }
        }

        private void ClaimIdentities(string username, bool isPersistent)
        {
            // Initialization.
            var claims = new List<Claim>();
            try
            {
                // Setting
                claims.Add(new Claim(ClaimTypes.Name, username));
                var claimIdenties = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

            }
            catch (Exception ex)
            {
                // Info
                throw ex;
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            try
            {
                // Verification.
                if (Url.IsLocalUrl(returnUrl))
                {
                    // Info.
                    return this.Redirect(returnUrl);
                }
            }
            catch (Exception ex)
            {
                // Info
                throw ex;
            }

            // Info.
            return this.RedirectToAction("LogOff", "Account");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult LogOff()
        {
            var ctx = Request.GetOwinContext();
            var authenticationManager = ctx.Authentication;
            authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

    }
}
