using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MLServer.Models;
using MLServer.Services;

namespace MLServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForecastController : ControllerBase
    {
        public DateTime Index()
        {
            return DateTime.Now;
        }

        [HttpPost("trainforecast")]
        public bool TrainForecast([FromBody] List<ProductStats> data)
        {
            if (data != null && data.Any())
            {
                var segmentator = new ForecastEngine();
                segmentator.TrainForecast(data);
                return true;
            }

            return false;
        }

        [HttpPost("productstats")]
        public List<ProductStats> ProductStats(string productId)
        {
            return ForecastEngine.ProductHistory(productId);
        }

        [HttpPost("productforecast")]
        public float ProductForecast(ProductStats data)
        {
            return ForecastEngine.ProductForecast(data);
        }

        [HttpPost("trainforecastcountry")]
        public bool TrainForecastCountry([FromBody] List<CountryStats> data)
        {
            if (data != null && data.Any())
            {
                var segmentator = new ForecastEngine();
                segmentator.TrainForecastCountry(data);
                return true;
            }

            return false;
        }

        [HttpPost("countrystats")]
        public List<CountryStats> CountryStats(string country)
        {
            return ForecastEngine.CountryHistory(country);
        }

        [HttpPost("countryforecast")]
        public float CountryForecast(CountryStats data)
        {
            return ForecastEngine.CountryForecast(data);
        }

        [HttpPost("getcountries")]
        public List<string> GetCountries()
        {
            return ForecastEngine.GetCountries();
        }
    }
}
