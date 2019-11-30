# MLServer
**ML.Net** based standalone application. Communicate with **Processing Engine** and **Sitecore** instance by API requests.

## Forecasting

![Logo](https://github.com/x3mxray/Cortex.Demo.Forecast/blob/master/documentation/images/logo.jpg)

**MLServer** uses regression model to predict product and country unit sales for the next period.
*Time series* forecasting is one of the major building blocks of **Machine Learning**.
*Regression* is a supervised machine learning task that is used to predict the value of the next period (in this case, the sales prediction) from a set of related features/variables. 

## Prepare data

In this demo we want to forecast product purchases for the next month.
For product forecasting we grouped our orders dataset by product-year-month and calculate grouping statisticals metrics.
We will also define and populate two fields **Prev** and **Next** *(total product unit sales in month)* to make our model in *time-series* style.
Our ML data model looks like:
```
public class ProductStats
{
    public string ProductId { get; set; }
    public float Year { get; set; }
    public float Month { get; set; }
    public float Max { get; set; }
    public float Min { get; set; }
    public float Avg { get; set; }
    public float Count { get; set; }

    public float Units { get; set; }
    public float Prev { get; set; }
    public float Next { get; set; }
}
```
We will forecast field **Next** value *(this field is used as a targeted label for our trainer)*.


## Train and validate model

For trainer we use **Fast Tree Tweedie** algorithm:
```
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

```

To validate accuracy of our model we use cross-validation and you can results of validation every time when data will be processed (results are saved in MLServer file system in format *crossValResults_datetime.csv*).
```
var crossValidationResults = _mlContext.Regression.CrossValidate(data: trainingDataView, estimator: trainingPipeline, numFolds: 6, labelColumn: "Label");
SaveCrossValidationResults(crossValidationResults.Select(x => x.Metrics));
```
The most interesting metric is RSquared. It takes values between 0 and 1. The closer its value is to 1, the better the model is.
https://en.wikipedia.org/wiki/Coefficient_of_determination
