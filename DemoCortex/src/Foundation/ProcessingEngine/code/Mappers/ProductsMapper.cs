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

            // remove statistics of last month (it is not finished)
            var products = results.GroupBy(x => x.ProductId);

            var ret = new List<ProductStats>();
            foreach (var product in products)
            {
                var maxYear = product.Max(y => y.Year);
                var maxMonth = product.Where(x => x.Year==maxYear).Max(y => y.Month);
            
                foreach (var p in product)
                {
                    if(!(p.Year== maxYear && p.Month==maxMonth))
                        ret.Add(p);
                }
            }

            // assign Prev and Next months sold units fields 
            foreach (var product in ret)
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

                product.Next = ret.FirstOrDefault(x => (x.ProductId == product.ProductId) && (x.Year == nextYear) && (x.Month == nextMonth))?.Units ?? -1 ;

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
                product.Prev = ret.FirstOrDefault(x => (x.ProductId == product.ProductId) && (x.Year == prevYear) && (x.Month == prevMonth))?.Units ?? -1;
            }

            return ret; //.Where(x => x.Prev.HasValue && x.Next.HasValue).ToList();
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

            // remove statistics of last month (it is not finished)
            var countries = results.GroupBy(x => x.Country);
            foreach (var country in countries)
            {
                var maxYear = country.Max(y => y.Year);
                var maxMonth = country.Where(x => x.Year == maxYear).Max(y => y.Month);
                results.RemoveAll(x => x.Country == country.Key && x.Year == maxYear && x.Month == maxMonth);
            }

            // assign Prev and Next months sold units fields 
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

                product.Next = results.FirstOrDefault(x => (x.Country == product.Country) && (x.Year == nextYear) && (x.Month == nextMonth))?.Units ?? -1;

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
                product.Prev = results.FirstOrDefault(x => (x.Country == product.Country) && (x.Year == prevYear) && (x.Month == prevMonth))?.Units ?? -1;
            }

            return results;//.Where(x => x.Prev.HasValue && x.Next.HasValue).ToList();
        }
    }
}
