﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Demo.Foundation.ProcessingEngine.Extensions;
using Demo.Foundation.ProcessingEngine.Train.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sitecore.Processing.Engine.Abstractions;
using Sitecore.Processing.Engine.Agents;
using Sitecore.Processing.Tasks.Options;
using Sitecore.Processing.Tasks.Options.DataSources.DataExtraction;
using Sitecore.Processing.Tasks.Options.Workers.ML;
using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;

namespace Demo.Foundation.ProcessingEngine.Agents
{
    // used for register tasks from xConnect Processing Engine
    public class DemoAgent : RecurringAgent
    {
        private readonly ILogger<IAgent> _logger;
        private readonly ITaskManager _taskManager;

        public DemoAgent(IConfiguration options, ILogger<IAgent> logger, ITaskManager taskManager) : base(options, logger)
        {
            _logger = logger;
            _taskManager = taskManager;
        }

        // run once a day 
        protected override async Task RecurringExecuteAsync(CancellationToken token)
        {
            _logger.LogInformation("RecurringExecuteAsync: RegisterRfmModelTaskChain");
            var expiresAfter = TimeSpan.FromDays(1);
            await _taskManager.RegisterForecastTaskChainAsync(expiresAfter);
        }
    }
}