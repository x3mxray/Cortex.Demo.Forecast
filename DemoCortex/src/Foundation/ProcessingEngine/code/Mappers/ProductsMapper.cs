using Demo.Foundation.ProcessingEngine.Models;
using Demo.Foundation.ProcessingEngine.Models.ML;
using Sitecore.Processing.Engine.Projection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Demo.Foundation.ProcessingEngine.Mappers
{
    public static class ProductsMapper
    {
        public static List<PurchaseInvoice> MapToProductsList(IReadOnlyList<IDataRow> dataRows)
        {
            var invoices = new List<PurchaseInvoice>();
            foreach (var data in dataRows)
            {
                invoices.Add(data.ToPurchaseOutcome());
            }
            return invoices;
        }

        public static PurchaseInvoice ToPurchaseOutcome(this IDataRow dataRow)
        {
            var result = new PurchaseInvoice
            {
                StockCode = dataRow.GetString(dataRow.Schema.GetFieldIndex(nameof(PurchaseInvoice.StockCode))),
                Quantity = (int) dataRow.GetInt64(dataRow.Schema.GetFieldIndex(nameof(PurchaseInvoice.Quantity))),
                TimeStamp = dataRow.GetDateTime(dataRow.Schema.GetFieldIndex(nameof(PurchaseInvoice.TimeStamp))),
                Price = (decimal) dataRow.GetDouble(dataRow.Schema.GetFieldIndex(nameof(PurchaseInvoice.Price)))
            };

            var country = dataRow.Schema.Fields.FirstOrDefault(x => x.Name == nameof(PurchaseInvoice.Country));
            if (country != null)
            {
                result.Country = dataRow.GetString(dataRow.Schema.GetFieldIndex(nameof(PurchaseInvoice.Country)));
            }

            return result;
        }

        public static List<ProductStats> CalculateProductStats(List<PurchaseInvoice> list)
        {

            var results = list.GroupBy(x => new { x.StockCode, x.TimeStamp.Year, x.TimeStamp.Month })
                .Select(x => new ProductStats
                {
                    ProductId = x.Key.StockCode,
                    Year = x.Key.Year,
                    Month = x.Key.Month,

                    Count = x.Count(),
                    Units = x.Sum(y => y.Quantity),
                    Min = x.Min(y => y.Quantity),
                    Avg = (int)Math.Ceiling(x.Average(y => y.Quantity)),
                    Max = x.Max(y => y.Quantity)
                })
                .ToList();

            foreach (var product in results)
            {
                var nextYear = product.Year;
                var nextMonth = product.Month;
                if (product.Month < 12)
                {
                    nextMonth++;
                }
                else
                {
                    nextMonth = 1;
                    nextYear++;
                }

                product.Next = results.FirstOrDefault(x => (x.ProductId == product.ProductId) && (x.Year == nextYear) && (x.Month == nextMonth))?.Units;

                var prevYear = product.Year;
                var prevMonth = product.Month;
                if (product.Month > 1)
                {
                    prevMonth--;
                }
                else
                {
                    prevMonth = 12;
                    prevYear--;
                }
                product.Prev = results.FirstOrDefault(x => (x.ProductId == product.ProductId) && (x.Year == prevYear) && (x.Month == prevMonth))?.Units;
            }

            return results.Where(x => x.Prev.HasValue && x.Next.HasValue).ToList();
        }

        public static List<CountryStats> CalculateCountryStats(List<PurchaseInvoice> list)
        {
            var results = list.Where(x => !string.IsNullOrEmpty(x.Country)).GroupBy(x => new { x.Country, x.TimeStamp.Year, x.TimeStamp.Month })
                .Select(x => new CountryStats
                {
                    Country = x.Key.Country,
                    Year = x.Key.Year,
                    Month = x.Key.Month,

                    Count = x.Count(),
                    Units = x.Sum(y => y.Quantity),
                    Min = x.Min(y => y.Quantity),
                    Avg = (int)Math.Ceiling(x.Average(y => y.Quantity)),
                    Max = x.Max(y => y.Quantity)
                })
                .ToList();

            foreach (var product in results)
            {
                var nextYear = product.Year;
                var nextMonth = product.Month;
                if (product.Month < 12)
                {
                    nextMonth++;
                }
                else
                {
                    nextMonth = 1;
                    nextYear++;
                }

                product.Next = results.FirstOrDefault(x => (x.Country == product.Country) && (x.Year == nextYear) && (x.Month == nextMonth))?.Units;

                var prevYear = product.Year;
                var prevMonth = product.Month;
                if (product.Month > 1)
                {
                    prevMonth--;
                }
                else
                {
                    prevMonth = 12;
                    prevYear--;
                }
                product.Prev = results.FirstOrDefault(x => (x.Country == product.Country) && (x.Year == prevYear) && (x.Month == prevMonth))?.Units;
            }

            return results.Where(x => x.Prev.HasValue && x.Next.HasValue).ToList();
        }
    }
}
