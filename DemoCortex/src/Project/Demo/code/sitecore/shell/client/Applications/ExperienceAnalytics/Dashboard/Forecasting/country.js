define(["sitecore", "/sitecore/shell/client/Speak/Assets/lib/core/1.1/plotly-basic.js"],
  function (sitecore, Plotly) {

    function getCountries() {
      return $.getJSON(`${apiUri.countrylist}`);
    }

    getCountries()
      .done(function (countries) {
        $.each(countries, function (i, val) {
          var $link = $('<a class="list-inline-item col-2 py-1" href="#" data-code="' + val.Code + '" />');
          $link.html(val.Name);
          $("#CountryList").append($link);
        });
      });

    onLoadCountryForecasting();


    $("body").on("click", ".list-inline-item.col-2.py-1", function () {
      var code = $(this).data('code');
      getCountryData(code);
    });

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

    function onLoadCountryForecasting() {
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
      $("#tableHeaderCaption")
        .text(`Sales ${units ? "units" : (1).toCurrencyLocaleString().replace("1.00", "")} / month`);
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


    function getCountryData(country) {
      $.getJSON(`${apiUri.countryhistory}?code=${country}`)
        .done(function (history) {
          if (history.length < 4) return;
          $.when(
            getCountryForecast(history[history.length - 1])
          ).done(function (forecast) {
            plotLineChartCountry(forecast, history, country)
          });
        });
    }

    function getCountryForecast(st) {
      var surl = `?month=${st.Month}&year=${st.Year}&avg=${st.Avg}&prev=${st.Prev}&count=${st.Count}&units=${st.Units}&country=${st.Country}`;
      var fullUrl = `${apiUri.countryforecast}${surl}`;
      return $.getJSON(fullUrl);
    }

    function plotLineChartCountry(forecast, historyItems, country) {
      for (var i = 0; i < historyItems.length; i++) {
        historyItems[i].sales = historyItems[i].Units;
      }

      $("footer").removeClass("sticky");

      updateCountryStatistics(country, historyItems, forecast); //.slice(historyItems.length - 12)

      var traceReal = getTraceCountryHistory(historyItems);

      var traceForecast = getTraceCountryForecast(
        traceReal.x,
        nextMonth(historyItems[historyItems.length - 1]),
        nextFullMonth(historyItems[historyItems.length - 1]),
        traceReal.text[traceReal.text.length - 1],
        traceReal.y,
        forecast);

      var trace_mean = traceMean(traceReal.x.concat(traceForecast.x), traceReal.y, '#999999');

      var layout = {
        xaxis: {
          tickangle: 0,
          showgrid: false,
          showline: false,
          zeroline: false,
          range: [traceReal.x.length - 12, traceReal.x.length],
        },
        yaxis: {
          showgrid: false,
          showline: false,
          zeroline: false,
          tickformat: '$,.0'
        },
        //dragmode: 'pan',
        hovermode: "closest",
        legend: {
          orientation: "h",
          xanchor: "center",
          yanchor: "top",
          y: 1.2,
          x: 0.85,
        }
      };

      Plotly.newPlot('lineChart', [traceReal, traceForecast, trace_mean], layout);
    }

    function getTraceCountryHistory(historyItems) {
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
          color: '#ffb131'
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
        fillcolor: '#ffb131',
        marker: {
          symbol: "circle",
          color: "white",
          size: 10,
          line: {
            color: "black",
            width: 3,
          }
        },
      };
    }

    function getTraceCountryForecast(labels, nextYLabel, nextText, prevText, values, forecast) {
      return {
        x: [labels[labels.length - 1], nextYLabel],
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

    function updateCountryStatistics(country, historyItems, forecasting) {
      showStatsLayers();

      populateForecastDashboard(country, historyItems, forecasting);
      populateHistoryTable(historyItems);

      refreshHeightSidebar();
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