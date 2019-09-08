require.config({
  paths: {
    bloodhound: "/sitecore/shell/client/Speak/Assets/lib/core/1.1/typeahead.bundle",
    typeahead: "/sitecore/shell/client/Speak/Assets/lib/core/1.1/typeahead.bundle",
    plotly: "/sitecore/shell/client/Speak/Assets/lib/core/1.1/plotly-basic"
  }
});

define(["sitecore", "bloodhound", "plotly", "typeahead"],

  function (sitecore, Bloodhound, Plotly, Typeahead) {

    onLoadProductForecasting();

    var months = ["",
      "Jan", "Feb", "Mar",
      "Apr", "May", "Jun", "Jul",
      "Aug", "Sep", "Oct",
      "Nov", "Dec"
    ];

    var fullMonths = ["",
      "January", "February", "March",
      "April", "May", "June", "July",
      "August", "September", "October",
      "November", "December"];

    function nextMonth(predictor) {
      if (predictor.Month === 12)
        return `${months[1]}<br>${predictor.Year + 1}`;
      else
        return `${months[predictor.Month + 1]}<br>${predictor.Year}`;
    }

    function nextFullMonth(predictor, includeYear = false) {
      if (predictor.Month === 12)
        return `${fullMonths[1]}`;
      else
        return `${fullMonths[predictor.Month + 1]}${includeYear ? ' ' + predictor.Year : ''}`;
    }


    function onLoadProductForecasting() {
      setUpProductDescriptionTypeahead();
      setResponsivePlots();
      $("footer").addClass("sticky");
    }

    function setResponsivePlots(plotSelector = ".responsive-plot") {
      var d3 = Plotly.d3;
      var gd3 = d3.selectAll(plotSelector);
      var nodesToResize = gd3[0];
      window.onresize = function () {
        for (var i = 0; i < nodesToResize.length; i++) {
          Plotly.Plots.resize(nodesToResize[i]);
        }
      };
    }

    function setUpProductDescriptionTypeahead(typeaheadSelector = "#remote .typeahead") {
      var productDescriptions = new Bloodhound({
        datumTokenizer: Bloodhound.tokenizers.obj.whitespace('value'),
        queryTokenizer: Bloodhound.tokenizers.whitespace,
        remote: {
          url: `${apiUri.findproduct}?description=%QUERY`,
          wildcard: '%QUERY'
        }
      });
      $(typeaheadSelector)
        .typeahead
        ({
          minLength: 3,
          highlight: true
        },
          {
            name: 'products',
            display: 'description',
            limit: 10,
            source: productDescriptions
          })
        .on('typeahead:selected', function (e, data) {
          updateProductInfo(data);
          getProductData(data);
        });
    }

    function updateProductInfo(data) {
      $("#product").removeClass("d-none");
      $("#productName").text(data.description);
      $("#productPrice").text(`${data.price.toCurrencyLocaleString()}`);
      $("#productImage").attr("src", data.pictureUri).attr("alt", data.description);
    }

    function getProductData(product) {
      var productId = product.id;
      var description = product.description;

      getHistory(productId)
        .done(function (history) {
          if (history.length < 4) return;
          $.when(
            getForecast(history[history.length - 1], product)
          ).done(function (forecast) {
            plotLineChart(forecast, history, description, product.price)
          });
        });
    }

    function getForecast(st) {
      var surl = `?productId=${st.ProductId}&month=${st.Month}&year=${st.Year}&avg=${st.Avg}&count=${st.Count}&prev=${st.Prev}&units=${st.Units}`;
      return $.getJSON(`${apiUri.forecasting}${surl}`);
    }



    function getHistory(productId) {
      return $.getJSON(`${apiUri.history}?id=${productId}`);
    }


    function plotLineChart(forecast, history, description, price) {
      for (var i = 0; i < history.length; i++) {
        history[i].sales = history[i].Units * price;
      }
      forecast *= price;

      $("footer").removeClass("sticky");

      updateProductStatistics(description, history, forecast); //.slice(history.length - 12)

      var traceReal = traceProductHistory(history);

      var traceForecast = traceProductForecast(
        traceReal.x,
        nextMonth(history[history.length - 1]),
        nextFullMonth(history[history.length - 1]),
        traceReal.text[traceReal.text.length - 1],
        traceReal.y,
        forecast);

      var mean = traceMean(traceReal.x.concat(traceForecast.x), traceReal.y, '#ffcc33');

      var layout = {
        xaxis: {
          tickangle: 0,
          showgrid: false,
          showline: false,
          zeroline: false,
          range: [traceReal.x.length - 12, traceReal.x.length]
        },
        yaxis: {
          showgrid: false,
          showline: false,
          zeroline: false,
          tickformat: '$,.0'
        },
        hovermode: "closest",
        //dragmode: 'pan',
        legend: {
          orientation: "h",
          xanchor: "center",
          yanchor: "top",
          y: 1.2,
          x: 0.85
        }
      };

      Plotly.newPlot('lineChart', [traceReal, traceForecast, mean], layout);
    }

    function traceProductHistory(historyItems) {
      var y = $.map(historyItems, function (d) { return d.sales; });
      var x = $.map(historyItems, function (d) { return `${months[d.Month]}<br>${d.Year}`; });
      var texts = $.map(historyItems, function (d) { return `${fullMonths[d.Month]}<br><b>${d.sales.toCurrencyLocaleString()}</b>`; });

      return {
        x: x,
        y: y,
        mode: 'lines+markers',
        name: 'history',
        line: {
          shape: 'spline',
          color: '#dd1828'
        },
        hoveron: 'points',
        hoverinfo: 'text',
        hoverlabel: {
          bgcolor: '#333333',
          bordercolor: '#333333',
          font: {
            color: 'white'
          }
        },
        text: texts,
        fill: 'tozeroy',
        fillcolor: '#dd1828',
        marker: {
          symbol: "circle",
          color: "white",
          size: 10,
          line: {
            color: "black",
            width: 3
          }
        }
      };
    }

    function traceProductForecast(labels, nextXLabel, nextText, prevText, values, forecast) {
      return {
        x: [labels[labels.length - 1], nextXLabel],
        y: [values[values.length - 1], forecast],
        text: [prevText, `${nextText}<br><b>${forecast.toCurrencyLocaleString()}</b>`],
        mode: 'lines+markers',
        name: 'forecasting',
        hoveron: 'points',
        hoverinfo: 'text',
        hoverlabel: {
          bgcolor: '#333333',
          bordercolor: '#333333',
          font: {
            color: 'white'
          }
        },
        line: {
          shape: 'spline',
          color: '#00A69C'
        },
        fill: 'tozeroy',
        fillcolor: '#00A69C',
        marker: {
          symbol: "circle",
          color: "white",
          size: 10,
          line: {
            color: "black",
            width: 3
          }
        }
      };
    }

    function traceMean(labels, values, color) {
      var yMean = values.slice(0, values.length - 2).reduce((previous, current) => current += previous) / values.length;
      return {
        x: labels,
        y: Array(labels.length).fill(yMean),
        name: 'average',
        mode: 'lines',
        hoverinfo: 'none',
        line: {
          color: color,
          width: 3
        }
      };
    }

    function updateProductStatistics(product, historyItems, forecasting) {
      showStatsLayers();
      populateForecastDashboard(product, historyItems, forecasting);
      populateHistoryTable(historyItems);

      refreshHeightSidebar();
    }


    function showStatsLayers() {
      $("#plot,#tableHeader,#tableHistory").removeClass('d-none');
    }

    function populateForecastDashboard(country, historyItems, forecasting, units = false) {
      var lastyear = historyItems[historyItems.length - 1].Year;
      var values = historyItems.map(y => (y.Year === lastyear) ? y.sales : 0);
      var total = values.reduce((previous, current) => current += previous);

      $("#labelTotal").text(`${lastyear} sales`);
      $("#valueTotal").text(units ? total.toNumberLocaleString() : total.toCurrencyLocaleString());
      $("#labelForecast").text(`${nextFullMonth(historyItems[historyItems.length - 1], true).toLowerCase()} sales`);
      $("#valueForecast").text(units ? forecasting.toNumberLocaleString() : forecasting.toCurrencyLocaleString());
      $("#labelItem").text(country);
      $("#tableHeaderCaption").text(`Sales ${units ? "units" : (1).toCurrencyLocaleString().replace("1.00", "")} / month`);
    }

    function populateHistoryTable(historyItems) {
      var table = '';
      var lastYear = '';
      for (var i = 0; i < historyItems.length; i++) {
        if (historyItems[i].Year !== lastYear) {
          lastYear = historyItems[i].Year;
          table += `<div class="col-11 border-bottom-highlight-table month font-weight-bold">${lastYear}</div>`;
        }
        table += `<div class="col-8 border-bottom-highlight-table month">${fullMonths[historyItems[i].Month]}</div> <div class="col-3 border-bottom-highlight-table">${historyItems[i].sales.toLocaleString()}</div >`;
      }
      $("#historyTable").empty().append($(table));
    }

    function refreshHeightSidebar() {
      $("aside").css('height', $(document).height());
    }

    Number.prototype.toCurrencyLocaleString = function toCurrencyLocaleString() {
      var currentLocale = navigator.languages ? navigator.languages[0] : navigator.language;
      return this.toLocaleString(currentLocale, { style: 'currency', currency: 'USD' });
    }

    Number.prototype.toNumberLocaleString = function toNumberLocaleString() {
      var currentLocale = navigator.languages ? navigator.languages[0] : navigator.language;
      return this.toLocaleString(currentLocale, { useGrouping: true }) + " units";
    }

  });