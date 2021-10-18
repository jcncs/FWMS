using FWMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace FWMS.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _configuration;

        private const string NoUserFound = "User not found.";
        private const string FailedAuthentication = "Could not authenticate user.";

        private const string AUTHORIZATION = "Authorization";
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
                    httpWebRequestVerify.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    httpWebRequestVerify.Headers.Add(AUTHORIZATION, "Bearer " + resultLoginjson);

                    HttpWebResponse httpResponseVerify = (HttpWebResponse)httpWebRequestVerify.GetResponse();
                    using (StreamReader streamReader = new StreamReader(httpResponseVerify.GetResponseStream()))
                    {
                        responseTwo = streamReader.ReadToEnd();
                    }
                    httpResponseVerify.Close();

                    if (!(string.IsNullOrWhiteSpace(responseTwo)))
                    {
                        var resultVerifyjson = JsonConvert.DeserializeObject(responseTwo).ToString();
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
            string userId = "1";
            string response = string.Empty;
            var apiGateway = _configuration.GetSection("ApiGateway").Get<string>();
            var viewMyProfile = _configuration.GetSection("Authentication:GET:ViewMyProfile").Get<string>();
            viewMyProfile = viewMyProfile.Replace("{userId}", "1");
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(apiGateway+viewMyProfile);
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
            return View(result);
        }

        public IActionResult ForgetPassword()
        {
            return View();
        }

        public IActionResult CreateNewAccount()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public static T Deserialize<T>(string jsonData)
        {
            JsonSerializer json = new JsonSerializer();
            return json.Deserialize<T>(new JsonTextReader(new StringReader(jsonData)));
        }
    }
}
