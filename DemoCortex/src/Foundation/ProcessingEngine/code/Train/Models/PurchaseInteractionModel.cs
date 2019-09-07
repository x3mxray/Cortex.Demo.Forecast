using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Demo.Foundation.ProcessingEngine.Models;
using Demo.Foundation.ProcessingEngine.Services;
using Sitecore.ContentTesting.ML.Workers;
using Sitecore.Processing.Engine.ML.Abstractions;
using Sitecore.Processing.Engine.Projection;
using Sitecore.Processing.Engine.Storage.Abstractions;
using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;

namespace Demo.Foundation.ProcessingEngine.Train.Models
{
  
    public class PurchaseInteractionModel : BaseWorker, IModel<Interaction>
    {
        private readonly ITableStoreFactory _tableStoreFactory;
        private readonly IMLNetService _mlNetService;

        public PurchaseInteractionModel(IReadOnlyDictionary<string, string> options, IMLNetService mlNetService, ITableStoreFactory tableStoreFactory) : base (tableStoreFactory)
        {

            _tableStoreFactory = tableStoreFactory;
            _mlNetService = mlNetService;

            Projection = Sitecore.Processing.Engine.Projection.Projection.Of<Interaction>()
                .CreateTabular("PurchaseOutcome",
                    interaction => interaction.Events.OfType<PurchaseOutcome>().Select(p => 
                        new
                        {
                            Purchase = p,
                            interaction.IpInfo()?.Country
                        }),
                    cfg => cfg.Key("Id", x => x.Purchase.Id)
                        .Attribute(nameof(PurchaseInvoice.Quantity), x => x.Purchase.Quantity)
                        .Attribute(nameof(PurchaseInvoice.TimeStamp), x => x.Purchase.Timestamp)
                        .Attribute(nameof(PurchaseInvoice.Price), x => x.Purchase.UnitPrice)
                        .Attribute(nameof(PurchaseInvoice.StockCode), x => x.Purchase.ProductId)
                        .Attribute(nameof(PurchaseInvoice.Country), x => x.Country, true)
                );
        }

        public async Task<ModelStatistics> TrainAsync(string schemaName, CancellationToken cancellationToken, params TableDefinition[] tables)
        {
            var tableStore = _tableStoreFactory.Create(schemaName);
            var data = await GetDataRowsAsync(tableStore, tables.First().Name, cancellationToken);
            return _mlNetService.TrainForecast(data);
        }

        public Task<IReadOnlyList<object>> EvaluateAsync(string schemaName, CancellationToken cancellationToken, params TableDefinition[] tables)
        {
            throw new NotImplementedException();
        }

        public IProjection<Interaction> Projection { get; set; }

    }
}