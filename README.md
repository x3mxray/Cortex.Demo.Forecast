# Sales Forecast with Sitecore Cortex and Machine Learning
Predict future sales by product and by country based on regression.
Example of usage Sitecore Cortex with ML.Net.

Contains 2 solutions:
- [DemoCortex](https://github.com/x3mxray/Cortex.Demo.Forecast/tree/master/DemoCortex)
- [MLServer](https://github.com/x3mxray/Cortex.Demo.Forecast/tree/master/MLServer)

Full overview of demo located [here](https://www.brimit.com/blog/dive-sitecore-cortex-machine-learning-introduction)

# How to Deploy #

## Pre-requisites
Sitecore 9.1.0 (for 9.1.1 and higher versions package references in project should be updated to corresponding versions)

# How to populate xConnect with testing data #
* Build solution.
* Run Demo.Project.DemoDataExplorer.exe from project [Demo.Project.DemoDataExplorer](https://github.com/x3mxray/Cortex.Demo.Forecast/tree/master/DemoCortex/src/Project/DemoDataExplorer/code)
![Data Explorer](https://github.com/x3mxray/Cortex.Demo.Forecast/blob/master/documentation/images/DataExplorer.jpg)
* Copy your sitecore website root url to "API address"
* Click "Browse" and select [Online Retail.xlsx](https://github.com/x3mxray/Cortex.Demo.Forecast/blob/master/install/Online_Retail.xlsx) (~500k records)
* Click "Upload file". Wait for finishing uploading process (it takes ~10 min). During process you can see logs in sitecore instance and new contacts appearance in Experience Profile:
```
INFO  Excel import: 272 from 4339: CustomerID=15332
```

# How to run ML server #
* Run [MLServer solution](https://github.com/x3mxray/Cortex.Demo.Forecast/tree/master/MLServer) in IIS Express (or install it as IIS application)
* Make sure that it is accessible by requesting http://localhost:56399/api/forecast

# How to run processing task #
- POST request to http://sitecoreInstance.url/api/contactapi/RegisterTasks with POSTMAN
- Or change processing agent sleep period in [Processing Engine -> sc.Processing.Engine.DemoAgents.xml](https://github.com/x3mxray/Cortex.Demo.Forecast/blob/master/install/xconnect/App_Data/jobs/continuous/ProcessingEngine/App_Data/Config/Sitecore/Demo/sc.Processing.Engine.DemoAgents.xml)
- All processes execution takes ~10 minutes (~500k records). During process you can see logs in Processing Engine job:
```


# Feedback #
If you are faced with any issues or have questions/suggestions you can contact me in sitecore slack channel [#cortexmachinelearning](https://sitecorechat.slack.com/messages/CD0BU3QBV/) @x3mxray