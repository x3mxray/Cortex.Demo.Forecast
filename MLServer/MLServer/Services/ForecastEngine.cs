using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using Microsoft.ML;
using Microsoft.ML.Data;
using MLServer.Models;

namespace MLServer.Services
{
    public class ForecastEngine
    {
        private static MLContext _mlContext;
        private static string TrainedModelFileProduct => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "product.model");
        private static string TrainedModelFileCountry => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "country.model");

        private static ITransformer _transformer;
        private static ITransformer _transformerCountry;
        private static List<ProductStats> _stats;
        private static List<CountryStats> _statsCountry;

        private static List<string> _countries;

        private const string ProductStatsFile = "productStats.csv";
        private const string CountryStatsFile = "countryStats.csv";

        static ForecastEngine()
        {
            _mlContext = new MLContext(1);
        }

        private static List<string> Countries
        {
            get
            {
                if (_countries != null) return _countries;
                _countries = StatsCountry.Select(x => x.Country).Distinct().ToList();
                return _countries;
            }
            set => _countries = value;
        }

        private static List<ProductStats> Stats
        {
            get => _stats ?? ReadProductStats(ProductStatsFile).ToList();
            set
            {
                _stats = value;
                SaveProductStats(ProductStatsFile, _stats);
            }
        }
        private static List<CountryStats> StatsCountry
        {
            get => _statsCountry ?? ReadCountryStats(CountryStatsFile).ToList();
            set
            {
                _statsCountry = value;
                SaveCountryStats(CountryStatsFile, _statsCountry);
            }
        }

        private static ITransformer TrainModel
        {
            get
            {
                if (_transformer != null) return _transformer;
                return LoadModel();
            }
            set
            {
                _transformer = value;
                SaveModel(_transformer);
            }
        }
        private static ITransformer TrainModelCountry
        {
            get
            {
                if (_transformerCountry != null) return _transformerCountry;
                return LoadModel(TrainedModelFileCountry);
            }
            set
            {
                _transformerCountry = value;
                SaveModel(_transformerCountry, TrainedModelFileCountry);
            }
        }

        private static ITransformer LoadModel(string path=null)
        {
            ITransformer model;
            var fname = path ?? TrainedModelFileProduct;
            using (var stream = new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                model = _mlContext.Model.Load(stream);
            }

            return model;
        }
        private static void SaveModel(ITransformer model, string path = null)
        {
            var fname = path ?? TrainedModelFileProduct;
            using (var fileStream = new FileStream(fname, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                _mlContext.Model.Save(model, fileStream);
            }
        }

        // products
        public void TrainForecast(List<ProductStats> list)
        {
            Stats = list;
            _mlContext = new MLContext(1);
            var trainingDataView = _mlContext.Data.LoadFromEnumerable(list.Where(x => x.Prev>=0 && x.Next>=0));

            var trainer = _mlContext.Regression.Trainers.FastTreeTweedie();
            var trainingPipeline = _mlContext.Transforms
                .Concatenate("NumFeatures", 
                nameof(ProductStats.Year),
                nameof(ProductStats.Month),
                nameof(ProductStats.Units),
                nameof(ProductStats.Avg),
                nameof(ProductStats.Count),
                nameof(ProductStats.Max), 
                nameof(ProductStats.Min),
                nameof(ProductStats.Prev))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CatFeatures", inputColumnName: nameof(ProductStats.ProductId)))
                .Append(_mlContext.Transforms.Concatenate( "Features", "NumFeatures", "CatFeatures"))
                .Append(_mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(ProductStats.Next)))
                .Append(trainer);

            var crossValidationResults = _mlContext.Regression.CrossValidate(data: trainingDataView, estimator: trainingPipeline, numFolds: 6, labelColumn: "Label");
            SaveCrossValidationResults(crossValidationResults.Select(x => x.Metrics));

            var model = trainingPipeline.Fit(trainingDataView);
            TrainModel = model;
        }
        public static List<ProductStats> ProductHistory(string productId)
        {
            return Stats.Where(x => x.ProductId == productId).OrderBy(x => x.Year).ThenBy(x => x.Month).ToList();
        }
        public static float ProductForecast(ProductStats data)
        {
            var currentMonthStats = ProductHistory(data.ProductId).First(x => x.Year == data.Year && x.Month == data.Month);

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ProductStats, ProductUnitPrediction>(TrainModel);
            ProductUnitPrediction nextMonthPrediction = predictionEngine.Predict(currentMonthStats);

            return nextMonthPrediction.Score;
        }

        // countries
        public void TrainForecastCountry(List<CountryStats> list)
        {
            StatsCountry = list;
            Countries = list.Select(x => x.Country).Distinct().ToList();

            _mlContext = new MLContext(1);
            var trainingDataView = _mlContext.Data.LoadFromEnumerable(list.Where(x => x.Prev >= 0 && x.Next >= 0));

            var trainer = _mlContext.Regression.Trainers.FastTreeTweedie();
            var trainingPipeline = _mlContext.Transforms
                .Concatenate( "NumFeatures",
                nameof(CountryStats.Year),
                nameof(CountryStats.Month),
                nameof(CountryStats.Units),
                nameof(CountryStats.Avg),
                nameof(CountryStats.Count),
                nameof(ProductStats.Max),
                nameof(ProductStats.Min),
                nameof(CountryStats.Prev))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CatFeatures", inputColumnName: nameof(CountryStats.Country)))
                .Append(_mlContext.Transforms.Concatenate( "Features", "NumFeatures", "CatFeatures"))
                .Append(_mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(CountryStats.Next)))
                .Append(trainer);

            var crossValidationResults = _mlContext.Regression.CrossValidate(data: trainingDataView, estimator: trainingPipeline, numFolds: 6, labelColumn: "Label");
            SaveCrossValidationResults(crossValidationResults.Select(x => x.Metrics));

            var model = trainingPipeline.Fit(trainingDataView);
            TrainModelCountry = model;
        }
        public static List<CountryStats> CountryHistory(string country)
        {
            return StatsCountry.Where(x => x.Country == country).OrderBy(x => x.Year).ThenBy(x => x.Month).ToList();
        }
        public static float CountryForecast(CountryStats data)
        {
            var currentMonthStats = CountryHistory(data.Country).First(x => x.Year == data.Year && x.Month == data.Month);
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<CountryStats, ProductUnitPrediction>(TrainModelCountry);

            ProductUnitPrediction nextMonthPrediction = predictionEngine.Predict(currentMonthStats);
            return nextMonthPrediction.Score;
        }

        public static List<string> GetCountries()
        {
            return Countries;
        }

        // read/write statistics for reusing 
        public static void SaveProductStats(string path, List<ProductStats> records)
        {
            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(records);
            }
        }
        public static void SaveCountryStats(string path, List<CountryStats> records)
        {
            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(records);
            }
        }
        public static IEnumerable<ProductStats> ReadProductStats(string path)
        {
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader))
            {
                var records =  csv.GetRecords<ProductStats>().ToList();
                return records;
            }
        }
        public static IEnumerable<CountryStats> ReadCountryStats(string path)
        {
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader))
            {
                var records = csv.GetRecords<CountryStats>().ToList();
                return records;
            }
        }

        // save cross validation results to evaluate models
        // RSquared metric - takes values between 0 and 1. The closer its value is to 1, the better the model is.
        public void SaveCrossValidationResults(IEnumerable<RegressionMetrics> crossValidationResults)
        {
            var path = "crossValResults_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".csv";

            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(crossValidationResults);
            }
        }
    }
}
