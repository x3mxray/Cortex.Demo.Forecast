# Sales Forecast with Sitecore Cortex and Machine Learning
Predict future sales by product and by country based on regression.
Example of usage Sitecore Cortex with ML.Net.

This demo shows how to use **Sitecore Cortex** in combination with **Machine Learning** in a simplistic way to fuel your company’s growth by applying the predictive approach to all your actions.

![Forecast](https://github.com/x3mxray/Cortex.Demo.Forecast/blob/master/documentation/images/forecast.jpg)

Demo contains 2 solutions:
- [DemoCortex](https://github.com/x3mxray/Cortex.Demo.Forecast/tree/master/DemoCortex)
- [MLServer](https://github.com/x3mxray/Cortex.Demo.Forecast/tree/master/MLServer)

## Business value

* It is a benchmark. You can use it as the business as usual level you are going to achieve if nothing changes in your strategy.
* You can calculate the incremental value of your new actions on top of this benchmark.
* It can be utilized for planning. You can plan your demand and supply actions by looking at the forecasts. It helps to see where to invest more.
* It is an excellent guide for planning budgets and targets.


# How to Deploy #

## Pre-requisites
Sitecore 9.1.0 (for 9.1.1 and higher versions package references in project should be updated to corresponding versions)

## Sitecore
* Install the sitecore package [Forecast module-1.0.zip](https://github.com/x3mxray/Cortex.Demo.Forecast/blob/master/install/Forecast_module-1.0.zip)
* Deploy marketing definitions (*Outcomes* is enough)
* Rebuild *sitecore_master_index*

## xConnect and jobs
* Extract and copy [xconnect](https://github.com/x3mxray/Cortex.Demo.Forecast/tree/master/install/xconnect.zip) to your xConnect instance
* Restart xConnect instance and xconnect jobs in windows services

# Populate xConnect with testing data #
* Build solution.
* Run Demo.Project.DemoDataExplorer.exe from project [Demo.Project.DemoDataExplorer](https://github.com/x3mxray/Cortex.Demo.Forecast/tree/master/DemoCortex/src/Project/DemoDataExplorer/code)
![Data Explorer](https://github.com/x3mxray/Cortex.Demo.Forecast/blob/master/documentation/images/DataExplorer.jpg)
* Copy your sitecore website root url to "API address"
* Click "Browse" and select [Online Retail.xlsx](https://github.com/x3mxray/Cortex.Demo.Forecast/blob/master/install/Online_Retail.xlsx) (~500k records)
* Click "Upload file". Wait for finishing uploading process (it takes ~10 min). During process you can see logs in sitecore instance and new contacts appearance in Experience Profile:
```
INFO  Excel import: 272 from 4339: CustomerID=15332
```

# Run ML server #
* Run [MLServer solution](https://github.com/x3mxray/Cortex.Demo.Forecast/tree/master/MLServer) in IIS Express (or install it as IIS application)
* Make sure that it is accessible by requesting http://localhost:56399/api/forecast

# Configure Machine learning endpoints #
* For Sitecore instance: change setting **MlHostUrl** in *App_Config\Include\Project\Demo.Forecast.config*
* For Processing Engine: change option **MLServerUrl** in *xconnect\App_Data\jobs\continuous\ProcessingEngine\App_Data\Config\Sitecore\Processing\sc.Processing.Services.MLNet.xml*

# Run processing task #
- POST request to http://sitecoreInstance.url/api/contactapi/RegisterTasks with POSTMAN
- Or change processing agent sleep period in [Processing Engine -> sc.Processing.Engine.DemoAgents.xml](https://github.com/x3mxray/Cortex.Demo.Forecast/blob/master/DemoCortex/src/Foundation/ProcessingEngine/code/App_Config/Processing/Demo/sc.Processing.Engine.DemoAgents.xml)
- All processes execution takes ~10 minutes (~500k records). During process you can see logs in Processing Engine job:
```
[Information] Registered Distributed Processing Task, TaskId: bcaec0ed-9d99-4c2a-9a4c-ae609a0157f7, Worker: Sitecore.Processing.Engine.ML.Workers.ProjectionWorker`1[[Sitecore.XConnect.Interaction, Sitecore.XConnect, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null]], Sitecore.Processing.Engine.ML, DataSource: Sitecore.Processing.Engine.DataSources.DataExtraction.InteractionDataSource, Sitecore.Processing.Engine
[Information] Registered Deferred Processing Task, Id: 8cba7a5b-28cc-4ca2-b9e0-ff5f7115c384, Worker: Sitecore.Processing.Engine.ML.Workers.MergeWorker, Sitecore.Processing.Engine.ML
[Information] Registered Deferred Processing Task, Id: b1778a4b-45f4-46ae-bd71-4c64fb9ff1e2, Worker: Sitecore.Processing.Engine.ML.Workers.TrainingWorker`1[[Sitecore.XConnect.Interaction, Sitecore.XConnect, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null]], Sitecore.Processing.Engine.ML
[Information] TaskAgent Executing worker. Machine: BRIMIT-SBA-PC, Process: 21584, AgentId: 9, TaskId: bcaec0ed-9d99-4c2a-9a4c-ae609a0157f7, TaskType: DistributedProcessing.
[Information] TaskAgent Worker execution completed. Machine: BRIMIT-SBA-PC, Process: 21584, AgentId: 9, TaskId: bcaec0ed-9d99-4c2a-9a4c-ae609a0157f7, TaskType: DistributedProcessing.
[Information] TaskAgent Executing worker. Machine: BRIMIT-SBA-PC, Process: 21584, AgentId: 9, TaskId: 8cba7a5b-28cc-4ca2-b9e0-ff5f7115c384, TaskType: DeferredAction.
[Information] TaskAgent Worker execution completed. Machine: BRIMIT-SBA-PC, Process: 21584, AgentId: 9, TaskId: 8cba7a5b-28cc-4ca2-b9e0-ff5f7115c384, TaskType: DeferredAction.
[Information] TaskAgent Executing worker. Machine: BRIMIT-SBA-PC, Process: 21584, AgentId: 9, TaskId: b1778a4b-45f4-46ae-bd71-4c64fb9ff1e2, TaskType: DeferredAction.
[Information] TaskAgent Worker execution completed. Machine: BRIMIT-SBA-PC, Process: 21584, AgentId: 9, TaskId: b1778a4b-45f4-46ae-bd71-4c64fb9ff1e2, TaskType: DeferredAction.
```

# See results #
* Navigate to Experience Analytics dashboard and you will see new *Forecasting* tab.
* For detailed information and settings see readme here:
- [DemoCortex](https://github.com/x3mxray/Cortex.Demo.Forecast/tree/master/DemoCortex)
- [MLServer](https://github.com/x3mxray/Cortex.Demo.Forecast/tree/master/MLServer)

# Feedback #
If you are faced with any issues or have questions/suggestions you can contact me in sitecore slack channel @x3mxray