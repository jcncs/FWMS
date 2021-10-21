using FWMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Text;
using System;

namespace FWMS.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _configuration;

        private const string NoUserFound = "User not found.";
        private const string FailedAuthentication = "Could not authenticate user.";

        private const string AUTHORIZATION = "Authorization";
        private const string ROLE = "Role";
        private const string USERID = "UserID";
        private const string USERNAME = "Username";
        public AdminController(ILogger<AdminController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Login
                string responseOne = string.Empty;
                var apiGateway = _configuration.GetSection("ApiGateway").Get<string>();
                var login = _configuration.GetSection("Authentication:POST:Login").Get<string>();
                HttpWebRequest httpWebRequestLogin = (HttpWebRequest)WebRequest.Create(apiGateway + login);
                httpWebRequestLogin.ContentType = "application/json; charset=utf-8";
                httpWebRequestLogin.Method = "POST";
                httpWebRequestLogin.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                using (var streamWriter = new StreamWriter(httpWebRequestLogin.GetRequestStream()))
                {
                    LoginModel loginM = new LoginModel();
                    loginM.userName = model.userName;
                    loginM.pwdHash = model.pwdHash;

                    string json = JsonConvert.SerializeObject(loginM);

                    streamWriter.Write(json);
                }

                HttpWebResponse httpResponseLogin = (HttpWebResponse)httpWebRequestLogin.GetResponse();
                using (StreamReader streamReader = new StreamReader(httpResponseLogin.GetResponseStream()))
                {
                    responseOne = streamReader.ReadToEnd();
                }
                httpResponseLogin.Close();

                var resultLoginjson = JsonConvert.DeserializeObject(responseOne).ToString();

                // 2. verify
                if (!(resultLoginjson.Trim().Equals(NoUserFound) || resultLoginjson.Trim().Equals(FailedAuthentication)))
                {
                    string responseTwo = string.Empty;
                    var verify = _configuration.GetSection("Authentication:GET:Verify").Get<string>();
                    HttpWebRequest httpWebRequestVerify = (HttpWebRequest)WebRequest.Create(apiGateway + verify);
                    httpWebRequestVerify.ContentType = "application/json; charset=utf-8";
                    httpWebRequestVerify.Method = "GET";
                    //httpWebRequestVerify.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    httpWebRequestVerify.Headers.Add(AUTHORIZATION, "Bearer " + resultLoginjson);

                    HttpWebResponse httpResponseVerify = (HttpWebResponse)httpWebRequestVerify.GetResponse();

                    using (StreamReader streamReadertwo= new StreamReader(httpResponseVerify.GetResponseStream()))
                    {
                        responseTwo = streamReadertwo.ReadToEnd();
                    }

                    httpResponseVerify.Close();

                    if (!(string.IsNullOrWhiteSpace(responseTwo)))
                    {
                        ViewBag.ErrorMessage = "The username or password is incorrect.";
                    }
                    else 
                    {
                        LoginUserInfoModel resultUserInfo = RetrieveUserInfo(model.userName);
                        HttpContext.Session.SetString(ROLE, resultUserInfo.roleId);
                        HttpContext.Session.SetString(USERNAME, resultUserInfo.userName);
                        HttpContext.Session.SetString(USERID, resultUserInfo.userId);
                    }
                }
                else 
                {
                    ViewBag.ErrorMessage ="The username or password is incorrect.";
                }
            }
            else
            {
                ViewBag.ErrorMessage =  "Please enter your username and password.";
            }

            return ViewBag.ErrorMessage == null ? RedirectToAction("Index","Dashboard") : View("Login", model);
        }
        public IActionResult Profile()
        {
            string userid = String.IsNullOrWhiteSpace(HttpContext.Session.GetString(USERID))? "": HttpContext.Session.GetString(USERID);
            return View(RetrieveProfile(userid));
        }

        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgetPassword(ForgetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (IsValidEmail(model.email))
                {
                    string response = string.Empty;
                    var apiGateway = _configuration.GetSection("ApiGateway").Get<string>();
                    var sendemail = _configuration.GetSection("Notification:POST:SendEmail").Get<string>();
                    var postData = "ToEmail=" + model.email;
                    postData += "&Subject=Reset Password";
                    postData += "&Body=Kindly use this password to go to the change password page.";
                    var data = Encoding.ASCII.GetBytes(postData);
                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(apiGateway + sendemail);
                    httpWebRequest.Method = "POST";
                    httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                    httpWebRequest.ContentLength = data.Length;
                    using (var stream = httpWebRequest.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                    HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    var responseString = new StreamReader(httpResponse.GetResponseStream()).ReadToEnd();
                }
                else 
                {
                    ViewBag.ErrorMessage = "Please a valid email.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Please enter your email.";
            }
            return ViewBag.ErrorMessage == null ? View("Login") : View("ForgetPassword");
        }
        public IActionResult CreateNewAccount()
        {
            return View();
        }

        public IActionResult Logout()
        {
            var currentrole = HttpContext.Session.GetString(ROLE);
            if (!(string.IsNullOrWhiteSpace(currentrole)))
            {
                HttpContext.Session.Remove(ROLE);
                HttpContext.Session.Remove(USERNAME);
                HttpContext.Session.Remove(USERID);
                HttpContext.Session.Clear();
            }
            return View("Login");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region functions
        public static T Deserialize<T>(string jsonData)
        {
            JsonSerializer json = new JsonSerializer();
            return json.Deserialize<T>(new JsonTextReader(new StringReader(jsonData)));
        }
        private static bool IsValidEmail(string email)
        {
            if (email.Trim().EndsWith("."))
            {
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        private ViewProfileModel RetrieveProfile(string userId)
        {
            string response = string.Empty;
            var apiGateway = _configuration.GetSection("ApiGateway").Get<string>();
            var viewMyProfile = _configuration.GetSection("Authentication:GET:ViewMyProfile").Get<string>();
            viewMyProfile = viewMyProfile.Replace("{userId}", userId);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(apiGateway + viewMyProfile);
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "GET";
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }
            httpResponse.Close();
            ViewProfileModel result = Deserialize<ViewProfileModel>(response);

            return result;
        }
        private LoginUserInfoModel RetrieveUserInfo(string username)
        {
            string response = string.Empty;
            var apiGateway = _configuration.GetSection("ApiGateway").Get<string>();
            var loginUserInfo = _configuration.GetSection("Authentication:GET:GetUserByUsername").Get<string>();
            loginUserInfo = loginUserInfo.Replace("{username}", username);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(apiGateway + loginUserInfo);
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "GET";
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }
            httpResponse.Close();
            LoginUserInfoModel result = Deserialize<LoginUserInfoModel>(response);

            return result;
        }
        #endregion
    }
}
