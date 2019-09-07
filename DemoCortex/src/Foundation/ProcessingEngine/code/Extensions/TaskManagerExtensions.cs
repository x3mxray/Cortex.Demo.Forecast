using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.Foundation.ProcessingEngine.Train.Models;
using Sitecore.Processing.Engine.Abstractions;
using Sitecore.Processing.Tasks.Options;
using Sitecore.Processing.Tasks.Options.DataSources.DataExtraction;
using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;

namespace Demo.Foundation.ProcessingEngine.Extensions
{
    public static class TaskManagerExtensionsCustom
    {
        public static async Task RegisterForecastTaskChainAsync(
          this ITaskManager taskManager,
          TimeSpan expiresAfter)
        {
            // Define workers parameters

            // datasource for PurchaseOutcomeModel projection
            var interactionDataSourceOptionsDictionary = new InteractionDataSourceOptionsDictionary(new InteractionExpandOptions(IpInfo.DefaultFacetKey), 5, 10);

            var modelTrainingOptions = new ModelTrainingTaskOptions(
                // assembly name of our processing engine model (PurchaseInteractionModel:IModel<Interaction>) 
                typeof(PurchaseInteractionModel).AssemblyQualifiedName,
                // assembly name of entity for our processing engine model  (PurchaseInteractionModel:IModel<Interaction>) 
                typeof(Interaction).AssemblyQualifiedName,
                // custom options that we pass to PurchaseInteractionModel
                new Dictionary<string, string> { ["TestCaseId"] = "Id" },
                // projection tableName of PurchaseOutcomeModel, must be equal to first parameter of 'CreateTabular' method => PurchaseInteractionModel.cs: CreateTabular("PurchaseOutcome", ...)
                "PurchaseOutcome",
                // name of resulted table (any name)
                "DemoResultTable");

            await taskManager.RegisterModelTrainingTaskChainAsync(modelTrainingOptions,
                interactionDataSourceOptionsDictionary, expiresAfter);
        }
    }
}
