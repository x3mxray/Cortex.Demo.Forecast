using System;
using System.Collections.Generic;
using Demo.Foundation.ProcessingEngine.Mappers;
using Microsoft.Extensions.Configuration;
using RestSharp;
using Sitecore.Processing.Engine.ML.Abstractions;
using Sitecore.Processing.Engine.Projection;

namespace Demo.Foundation.ProcessingEngine.Services
{
    public class MLNetService : IMLNetService
    {
        private readonly string _mlServerUrl;
        private readonly string _trainForecastUrl;
        private readonly string _trainForecastCountryUrl;

        public MLNetService(IConfiguration configuration)
        {
            _mlServerUrl = configuration.GetValue<string>("MLServerUrl");
            _trainForecastUrl = configuration.GetValue<string>("TrainProductUrl");
            _trainForecastCountryUrl = configuration.GetValue<string>("TrainCountryUrl");
        }

        public ModelStatistics TrainForecast(IReadOnlyList<IDataRow> data)
        {
            var mlData = ProductsMapper.MapToProductsList(data);

            // products

            var stats = ProductsMapper.CalculateProductStats(mlData);
            var client = new RestClient(_mlServerUrl);
            var request = new RestRequest(_trainForecastUrl, Method.POST);
            request.AddJsonBody(stats);
            var response = client.Execute<bool>(request);
            var ok = response.Data;
            if (!ok)
            {
                throw new Exception("something is wrong with ML engine, check it");
            }

            // countries 

            var countryStats = ProductsMapper.CalculateCountryStats(mlData);
            var client2 = new RestClient(_mlServerUrl);
            var request2 = new RestRequest(_trainForecastCountryUrl, Method.POST);
            request2.AddJsonBody(countryStats);
            var response2 = client2.Execute<bool>(request2);
            var ok2 = response2.Data;
            if (!ok2)
            {
                throw new Exception("something is wrong with ML engine, check it");
            }

            return null;
        }
    }

    public interface IMLNetService
    {
        ModelStatistics TrainForecast(IReadOnlyList<IDataRow> data);
    }
}
