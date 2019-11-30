# Demo Cortex 

## After module installation

You can see a list of products in **Sitecore** that will be used for demo:

![Products](https://github.com/x3mxray/Cortex.Demo.Forecast/blob/master/documentation/images/products.jpg)

A new **Purchase Outcome** event will appear in **Marketing Control Panel**

![Outcome](https://github.com/x3mxray/Cortex.Demo.Forecast/blob/master/documentation/images/outcome.jpg)

And a new tab will appear in **Experience Analytics** dashboard:

![Products Tab](https://github.com/x3mxray/Cortex.Demo.Forecast/blob/master/documentation/images/tab.jpg)


After module installation you should also:

* Deploy marketing definitions (*Outcomes* is enough)
* Rebuild *sitecore_master_index*


## After populating xConnect with testing data

You can see a lot of demo contacts in **Experience Profile** with list of **Purchase Outcome** events.

![Contacts](https://github.com/x3mxray/Cortex.Demo.Forecast/blob/master/documentation/images/events.jpg)


# How it works

* Contact submit an order
* Purchase Outcome (with order info) is triggered as a part of contacts interaction
* After populating xConnect with testing data we have a big historical dataset of contacts orders in xDB
* Processing Engine extracts this orders dataset from xDB (all Purchase Outcomes) and push it to MLServer
* MLServer prepared data and train a model to predict future orders
* EA Forecasting module makes requests to Sitecore about forecast that user is interested
* Sitecore asks MLServer about this forecast and returns results to EA Forecasting module

![Forecast](https://github.com/x3mxray/Cortex.Demo.Forecast/blob/master/documentation/images/forecast.jpg)