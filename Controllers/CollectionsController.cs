using FWMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Linq;

namespace FWMS.Controllers
{
    public class CollectionsController : Controller
    {
        private readonly ILogger<CollectionsController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DonationsController> _donationlogger;

        private const string AUTHORIZATION = "Authorization";
        private const string ROLE = "Role";
        private const string USERID = "UserID";
        private const string USERNAME = "Username";

        public CollectionsController(ILogger<CollectionsController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            try
            {
                int currentrole = 0;
                string currentusername = string.IsNullOrWhiteSpace(HttpContext.Session.GetString(USERNAME))? "": HttpContext.Session.GetString(USERNAME);
                List<ViewDonationsModel> donationList = DonationList();
                List<ViewCollectionsModel> collectionList = CollectionList();
                List<ViewCollectionsModel> result = new List<ViewCollectionsModel>();

                if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString(ROLE)))
                {
                    currentrole = int.Parse(HttpContext.Session.GetString(ROLE));
                    //Admin
                    if (currentrole.Equals(1))
                    {
                        result = collectionList;
                    }
                    //Donor
                    else if (currentrole.Equals(3))
                    {
                        //Collector based om CreatedBy -> username
                         var query = (from ViewCollectionsModel in collectionList
                                                               join ViewDonationsModel in donationList
                                                               on ViewCollectionsModel.donationId equals ViewDonationsModel.donationId
                                                               where ViewDonationsModel.createdBy.Equals(currentusername)
                                                               select new ViewCollectionsModel {
                                                                   collectionId = ViewCollectionsModel.collectionId,
                                                                   collectionName = ViewCollectionsModel.collectionName,
                                                                   collectionDate = ViewCollectionsModel.collectionDate,
                                                                   donationId = ViewCollectionsModel.donationId
                                                               });
                        result = query.ToList();
                    }
                    //Collector
                    else if (currentrole.Equals(4))
                    {
                        //Donor based on ReservedBy -> username
                       var query  = (from ViewCollectionsModel in collectionList
                                                               join ViewDonationsModel in donationList
                                                               on ViewCollectionsModel.donationId equals ViewDonationsModel.donationId
                                                               where ViewDonationsModel.reservedBy.Equals(currentusername)
                                                               select new ViewCollectionsModel {
                                                                   collectionId = ViewCollectionsModel.collectionId,
                                                                   collectionName = ViewCollectionsModel.collectionName,
                                                                   collectionDate = ViewCollectionsModel.collectionDate,
                                                                   donationId = ViewCollectionsModel.donationId
                                                               });
                        result = query.ToList();
                    }
                }
                return View(result);
            }
            catch (System.Exception ex)
            {
                throw ex;
                return View("Error");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult CreateCollection()
        {
            try
            {
                CreateCollectionModel createCollectionModel = new CreateCollectionModel();
                //expiry datetime < collection datetime --> 2 hours
                createCollectionModel.DonationList = DonationList().Where(x => x.expiryDate > DateTime.Now.ToLocalTime().AddHours(-2) && string.IsNullOrWhiteSpace(x.reservedBy) && !(x.reservedDate.HasValue) && string.IsNullOrWhiteSpace(x.collectionId)).OrderByDescending(x => x.expiryDate).ToList();
                return View(createCollectionModel);
            }
            catch (System.Exception ex)
            {
                throw ex;
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult CreateCollection(CreateCollectionModel model)
        {
            try
            {
                //expiry datetime < collection datetime --> 2 hours
                model.DonationList = DonationList().Where(x => x.expiryDate > DateTime.Now.ToLocalTime().AddHours(-2) && string.IsNullOrWhiteSpace(x.reservedBy) && !(x.reservedDate.HasValue) && string.IsNullOrWhiteSpace(x.collectionId)).OrderByDescending(x => x.expiryDate).ToList();

                if (ModelState.IsValid)
                {
                    string responseOne = string.Empty;
                    var apiGateway = _configuration.GetSection("ApiGateway").Get<string>();
                    var createCollection = _configuration.GetSection("Collection:POST:AddNewReservation").Get<string>();
                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(apiGateway + createCollection);
                    httpWebRequest.ContentType = "application/json; charset=utf-8";
                    httpWebRequest.Method = "POST";
                    httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        CreateCollectionModel ccM = new CreateCollectionModel();
                        ccM.donationId = model.donationId;
                        ccM.CollectionName = model.CollectionName;
                        ccM.ReservedBy = HttpContext.Session.GetString(USERNAME);
                        ccM.CollectionDate = model.CollectionDate;

                        string json = JsonConvert.SerializeObject(ccM);

                        streamWriter.Write(json);
                    }

                    HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        responseOne = streamReader.ReadToEnd();
                    }
                    httpResponse.Close();
                }
                else
                {
                    ViewBag.ErrorMessage = "Please enter all fields.";
                }
                return ViewBag.ErrorMessage == null ? RedirectToAction("Index", "Collections") : View(model);
            }
            catch (System.Exception ex)
            {
                throw ex;
                return View("Error");
            }
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
        public List<ViewCollectionsModel> CollectionList()
        {
            string response = string.Empty;
            var apiGateway = _configuration["ApiGateway"];
            var viewCollectionss = _configuration["Collection:GET:GetAllCollections"];
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(apiGateway + viewCollectionss);
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
            return result;
        }

        public List<ViewDonationsModel> DonationList()
        {
            DonationsController donationsObj = new DonationsController(_donationlogger, _configuration);
            List<ViewDonationsModel> result = donationsObj.DonationList();

            return result;
        }
        #endregion
    }
}
