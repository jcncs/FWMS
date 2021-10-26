using FWMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace FWMS.Controllers
{
    public class CollectionsController : Controller
    {
        private readonly ILogger<CollectionsController> _logger;
        private readonly IConfiguration _configuration;

        public CollectionsController(ILogger<CollectionsController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            string response = string.Empty;
            var apiGateway = _configuration["ApiGateway"];
            var viewCollectionss = _configuration["Collection:GET:GetAllCollections"];
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(apiGateway+viewCollectionss);
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "GET";
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }
            httpResponse.Close();
            List<ViewCollectionsModel> result = Deserialize<List<ViewCollectionsModel>>(response);
            return View(result);
        }

        public IActionResult CreateDonation()
        {
            return View();
        }
        public IActionResult EditDonation()
        {
            return View();
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

        public List<ViewLocationsModel> LocationList()
        {

            string response = string.Empty;
            var apiGateway = _configuration["ApiGateway"];
            var viewLocations = _configuration["Collection:GET:GetLocationList"];
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(apiGateway + viewLocations);
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "GET";
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }
            httpResponse.Close();
            List<ViewLocationsModel> result = Deserialize<List<ViewLocationsModel>>(response);
            return result;
        }
        public List<ViewFoodDescriptionsModel> FoodDescriptionList()
        {

            string response = string.Empty;
            var apiGateway = _configuration["ApiGateway"];
            var viewFoodDescriptions = _configuration["Collection:GET:GetFoodDescList"];
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(apiGateway + viewFoodDescriptions);
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "GET";
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }
            httpResponse.Close();
            List<ViewFoodDescriptionsModel> result = Deserialize<List<ViewFoodDescriptionsModel>>(response);
            return result;
        }
    #endregion
    }
}
