using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Demo.Foundation.ProcessingEngine.Models.ML;
using Demo.Project.Demo.Models;
using Demo.Project.Demo.Services;
using RestSharp;
using Sitecore.Data;
using Sitecore.Data.Items;
namespace Demo.Project.Demo.Controllers
{
    public class ProductApiController : ApiController
    {
        // Analytics->Lookup->Countries
        // used for displaying country friendly name
        static readonly ID CountryFolder = new ID("{DBE138C0-160F-4540-9868-0098E2CE8174}");
        private static readonly List<Item> Countries;

        static ProductApiController()
        {
            var folder = Sitecore.Context.Database.GetItem(CountryFolder);
            if (folder != null)
            {
                Countries = folder.Children.ToList();
            }
        }

        // API endpoints to ML
        private readonly string _mlServerUrl = Sitecore.Configuration.Settings.GetSetting("MlHostUrl");
        private readonly string _productHistoryUrl = Sitecore.Configuration.Settings.GetSetting("MlApi_ProductHistory");
        private readonly string _productForecastUrl = Sitecore.Configuration.Settings.GetSetting("MlApi_ProductForecast");
        private readonly string _countryHistoryUrl = Sitecore.Configuration.Settings.GetSetting("MlApi_CountryHistory");
        private readonly string _countryForecastUrl = Sitecore.Configuration.Settings.GetSetting("MlApi_CountryForecast");
        private readonly string _countryUrl = Sitecore.Configuration.Settings.GetSetting("MlApi_CountryList");

        [HttpGet]
        public IHttpActionResult FindProduct(string description)
        {
            var items = ItemService.Find(description).ToList();
            return Ok(items);
        }

        [HttpGet]
        public IHttpActionResult History(string id)
        {
            var client = new RestClient(_mlServerUrl);
            var request = new RestRequest(_productHistoryUrl, Method.POST);
            request.AddQueryParameter("productId", id);
            var response = client.Execute<List<ProductStats>>(request);
         
            return Ok(response.Data);
        }

        [HttpGet]
        public IHttpActionResult Forecast([FromUri]string productId, [FromUri]int year, [FromUri]int month)
        {
            var inputExample = new ProductStats
            {
                ProductId = productId,
                Year = year,
                Month = month
            };

            var client = new RestClient(_mlServerUrl);
            var request = new RestRequest(_productForecastUrl, Method.POST);
            request.AddJsonBody(inputExample);
            var response = client.Execute<float>(request);

            return Ok(response.Data);
        }

        [HttpGet]
        public IHttpActionResult Test()
        {
            return Ok();
        }

       
        [HttpGet]
        public IHttpActionResult GetCountries()
        {
            var client = new RestClient(_mlServerUrl);
            var request = new RestRequest(_countryUrl, Method.POST);
            var response = client.Execute<List<string>>(request);

            var codes = response.Data;

            var result = new List<CountryModel>();
            foreach (var code in codes)
            {
                var country = GetCountryModel(code);
                if (country != null)
                    result.Add(country);
            }

            return Ok(result);
        }

        [HttpGet]
        public IHttpActionResult CountryHistory(string code)
        {
            var client = new RestClient(_mlServerUrl);
            var request = new RestRequest(_countryHistoryUrl, Method.POST);
            request.AddQueryParameter("country", code);
            var response = client.Execute<List<CountryStats>>(request);

            return Ok(response.Data);
        }


        [HttpGet]
        public IHttpActionResult CountryForecast([FromUri]string country, [FromUri]int year, [FromUri]int month)
        {
            var inputExample = new CountryStats
            {
                Country = country,
                Year = year,
                Month = month
            };

            var client = new RestClient(_mlServerUrl);
            var request = new RestRequest(_countryForecastUrl, Method.POST);
            request.AddJsonBody(inputExample);
            var response = client.Execute<float>(request);

            return Ok(response.Data);
        }

        private CountryModel GetCountryModel(string code)
        {
            var country = Countries.FirstOrDefault(x => x["Country Code"] == code);
            if (country == null) return null;

            return new CountryModel
            {
                Code = code,
                Name = country.Name
            };
        }
    }

    
}