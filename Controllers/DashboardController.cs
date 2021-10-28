using FWMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Linq;
using Newtonsoft.Json;

namespace FWMS.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            
            List<ViewDashboardModel> result = Deserialize<List<ViewDashboardModel>>(RetrieveData("GET", "Donation:GET:ViewAvailableDonations"));
            return View(result);
        }

        public IActionResult CancelDonation()
        {

            List<ViewDashboardModel> result = Deserialize<List<ViewDashboardModel>>(RetrieveData("PUT", "Donation:PUT:CancelDonation"));
            return View(result);
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

        public static T Deserialize<T>(string jsonData)
        {
            JsonSerializer json = new JsonSerializer();
            return json.Deserialize<T>(new JsonTextReader(new StringReader(jsonData)));
        }

        public string RetrieveData(string requestMethod, string invoke)
        {
            string response = string.Empty;
            var apiGateway = _configuration["ApiGateway"];
            var invokePath = _configuration[invoke];
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(apiGateway + invokePath);
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = requestMethod;
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }
            httpResponse.Close();
            return response;
        }
    }
}
