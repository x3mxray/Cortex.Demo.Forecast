using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Demo.Foundation.ProcessingEngine.Models.ML;
using Demo.Project.Demo.Services;
using RestSharp;
using Sitecore.Data;
using Sitecore.Data.Items;
namespace Demo.Project.Demo.Controllers
{
    public class ProductModel
    {
        public ProductModel(string id, float price, string description)
        {
            this.id = id;
            this.price = price;
            this.description = description;
        }
        public string id { get; set; }
        public float price { get; set; }
        public string description { get; set; }
    }
    public class ProductController : ApiController
    {
        static readonly ID CountryFolder = new ID("{DBE138C0-160F-4540-9868-0098E2CE8174}");
        private static List<Item> _countries;

        static ProductController()
        {
            var folder = Sitecore.Context.Database.GetItem(CountryFolder);
            if (folder != null)
            {
                _countries = folder.Children.ToList();
            }
        }

        private string _mlServerUrl = "http://ml.demo";
        private string _productHistoryUrl = "/api/forecast/productstats";
        private string _productForecastUrl = "/api/forecast/productforecast";
        private string _countryHistoryUrl = "/api/forecast/countrystats";
        private string _countryForecastUrl = "/api/forecast/countryforecast";
        private string _countryUrl = "/api/forecast/getcountries";

        [HttpGet]
        public IHttpActionResult SimilarProducts(string description)
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
        public IHttpActionResult GetProductUnitDemandEstimation([FromUri]string productId, [FromUri]int year, [FromUri]int month)
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


        public CountryModel GetCountryModel(string code)
        {
            var country = _countries.FirstOrDefault(x => x["Country Code"] == code);
            if (country == null) return null;

            return new CountryModel
            {
                Code = code,
                Name = country.Name
            };
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
    }

    public class CountryModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}