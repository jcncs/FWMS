﻿using FWMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System;

namespace FWMS.Controllers
{
    public class DonationsController : Controller
    {
        private readonly ILogger<DonationsController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CollectionsController> _collectionlogger;

        private const string AUTHORIZATION = "Authorization";
        private const string ROLE = "Role";
        private const string USERID = "UserID";
        private const string USERNAME = "Username";

        public DonationsController(ILogger<DonationsController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            List<ViewDonationsModel> result = DonationList();
            return View(result);
        }

        public IActionResult CreateDonation()
        {
            CollectionsController collectionsObj = new CollectionsController(_collectionlogger, _configuration);
            CreateDonationModel createDonationModel = new CreateDonationModel();
            createDonationModel.LocationList = collectionsObj.LocationList();
            createDonationModel.FoodDescriptionList = collectionsObj.FoodDescriptionList();
            return View(createDonationModel);
        }

        [HttpPost]
        public ActionResult CreateDonation(CreateDonationModel model)
        {
            try
            {
                CollectionsController collectionsObj = new CollectionsController(_collectionlogger, _configuration);
                model.LocationList = collectionsObj.LocationList();
                model.FoodDescriptionList = collectionsObj.FoodDescriptionList();

                if (ModelState.IsValid)
                {
                    int resultInt = 0;
                    if (!int.TryParse(model.Quantity, out resultInt))
                    {
                        ViewBag.ErrorMessage = "Please enter a valid Quantity.";
                        return View(model);
                    }

                    string responseOne = string.Empty;
                    var apiGateway = _configuration.GetSection("ApiGateway").Get<string>();
                    var createDonation = _configuration.GetSection("Donation:POST:AddNewDonation").Get<string>();
                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(apiGateway + createDonation);
                    httpWebRequest.ContentType = "application/json; charset=utf-8";
                    httpWebRequest.Method = "POST";
                    httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        CreateDonationModel cdM = new CreateDonationModel();
                        cdM.DonationName = model.DonationName;
                        cdM.Quantity = model.Quantity;
                        cdM.ExpiryDate = DateTime.Now.ToLocalTime().AddDays(1);
                        cdM.CreatedBy = HttpContext.Session.GetString(USERNAME);
                        cdM.LocationId = model.LocationId;
                        cdM.FoodId = model.FoodId;
                        cdM.UserId = HttpContext.Session.GetString(USERID);

                        string json = JsonConvert.SerializeObject(cdM);

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
                return ViewBag.ErrorMessage == null ? RedirectToAction("Index", "Donations") : View(model);
            }
            catch (System.Exception)
            {
                return View("Error");
                throw;
            }
        }

        public IActionResult EditDonation()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CancelDonation(CancelDonationModel model)
        {
            try
            {
                CollectionsController collectionsObj = new CollectionsController(_collectionlogger, _configuration);

                if (ModelState.IsValid)
                {
                    if (!string.IsNullOrEmpty(model.DonationId))
                    {
                        string responseOne = string.Empty;
                        var apiGateway = _configuration.GetSection("ApiGateway").Get<string>();
                        var createDonation = _configuration.GetSection("Donation:PUT:CancelDonation").Get<string>();
                        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(apiGateway + createDonation);
                        httpWebRequest.ContentType = "application/json; charset=utf-8";
                        httpWebRequest.Method = "PUT";
                        httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        {
                            CancelDonationModel cdM = new CancelDonationModel();
                            cdM.DonationId = model.DonationId;

                            string json = JsonConvert.SerializeObject(cdM);

                            streamWriter.Write(json);
                        }

                        List<ViewDonationsModel> result = DonationList();
                        return View("Index", result);
                        //HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        //using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        //{
                        //    responseOne = streamReader.ReadToEnd();
                        //}
                        //httpResponse.Close();
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Please enter all fields.";
                }
                return ViewBag.ErrorMessage == null ? RedirectToAction("Index", "Dashboard") : View();
            }
            catch (System.Exception)
            {
                return View("Error");
                throw;
            }
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
        public List<ViewDonationsModel> DonationList()
        {
            string response = string.Empty;
            var apiGateway = _configuration["ApiGateway"];
            var viewDonations = _configuration["Donation:GET:ViewDonations"];
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(apiGateway + viewDonations);
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "GET";
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }
            httpResponse.Close();
            List<ViewDonationsModel> result = Deserialize<List<ViewDonationsModel>>(response);
            return result;
        }
        #endregion
    }
}
