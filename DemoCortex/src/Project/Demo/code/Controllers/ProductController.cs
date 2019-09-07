using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private static int adjustMonth = 0;
        [HttpGet]
        public async Task<IHttpActionResult> SimilarProducts(string description)
        {
            var items = ItemService.Find(description).ToList();

            return Ok(items);
        }

        [HttpGet]
        public async Task<IHttpActionResult> History(string id)
        {
            var client = new RestClient(_mlServerUrl);
            var request = new RestRequest(_productHistoryUrl, Method.POST);
            request.AddQueryParameter("productId", id);
            var response = client.Execute<List<ProductStats>>(request);

            return Ok(SetAdjust(response.Data));
        }

        [HttpGet]
        public IHttpActionResult GetProductUnitDemandEstimation([FromUri]string productId, [FromUri]int year, [FromUri]int month, [FromUri]float units, [FromUri]float avg,
            [FromUri]int count, [FromUri]float prev)
        {
            var inputExample = new ProductStats
            {
                ProductId = productId,
                Year = year,
                Month = month,
                Count = count,
                Units = (int)units,
                Avg = (int)avg,
                Prev = (int)prev,
                Next = 0
            };

            var data = SetAdjust(inputExample);

            var client = new RestClient(_mlServerUrl);
            var request = new RestRequest(_productForecastUrl, Method.POST);
            request.AddJsonBody(data);
            var response = client.Execute<float>(request);

            return Ok(response.Data);
        }

        [HttpGet]
        public IHttpActionResult Test()
        {
            return Ok();
        }


        private List<ProductStats> SetAdjust(List<ProductStats> data)
        {
            var date2 = DateTime.Now;//.AddMonths(-1);
            var date1 = data.Last();
            var diff = Math.Abs(((date1.Year - date2.Year) * 12) + date1.Month - date2.Month);
            adjustMonth = diff;

            foreach (var d in data)
            {
                var dt = new DateTime(d.Year, d.Month, 1).AddMonths(adjustMonth);
                d.Year = dt.Year;
                d.Month = dt.Month;
            }

            return data;
        }

        private ProductStats SetAdjust(ProductStats d)
        {
            var dt = new DateTime(d.Year, d.Month, 1).AddMonths(adjustMonth);
            d.Year = dt.Year;
            d.Month = dt.Month;

            return d;
        }

        private List<CountryStats> SetAdjust(List<CountryStats> data)
        {
            var date2 = DateTime.Now;//.AddMonths(-1);
            var date1 = data.Last();
            var diff = Math.Abs(((date1.Year - date2.Year) * 12) + date1.Month - date2.Month);
            adjustMonth = diff;

            foreach (var d in data)
            {
                var dt = new DateTime(d.Year, d.Month, 1).AddMonths(adjustMonth);
                d.Year = dt.Year;
                d.Month = dt.Month;
            }

            return data;
        }

        private CountryStats SetAdjust(CountryStats d)
        {
            var dt = new DateTime(d.Year, d.Month, 1).AddMonths(adjustMonth);
            d.Year = dt.Year;
            d.Month = dt.Month;

            return d;
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
        public string GetCountryCodeByName(string name)
        {
            var country = _countries.FirstOrDefault(x => x.Name == name);
            if (country != null)
            {
                return country["Country Code"];
            }
            return name;
        }

        [HttpGet]
        public async Task<IHttpActionResult> CountryHistory(string code)
        {
            var client = new RestClient(_mlServerUrl);
            var request = new RestRequest(_countryHistoryUrl, Method.POST);
            request.AddQueryParameter("productId", code);
            var response = client.Execute<List<CountryStats>>(request);

            return Ok(SetAdjust(response.Data));
        }


        [HttpGet]
        public IHttpActionResult CountryForecast([FromUri]string country, [FromUri]int year, [FromUri]int month, [FromUri]float units, [FromUri]float avg,
            [FromUri]int count, [FromUri]float prev)
        {
            var inputExample = new CountryStats
            {
                Country = country,
                Year = year,
                Month = month,
                Count = count,
                Units = (int)units,
                Avg = (int)avg,
                Prev = (int)prev,
                Next = 0
            };

            var data = SetAdjust(inputExample);

            var client = new RestClient(_mlServerUrl);
            var request = new RestRequest(_countryForecastUrl, Method.POST);
            request.AddJsonBody(data);
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