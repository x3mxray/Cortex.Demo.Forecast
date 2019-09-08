using System.Collections.Generic;
using System.Linq;
using Demo.Project.Demo.Models;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;

namespace Demo.Project.Demo.Services
{
    public class ItemService
    {
        public static IEnumerable<ProductModel> Find(string term)
        {
            term = term.ToUpper();

            var index = ContentSearchManager.GetIndex("sitecore_master_index");
            using (var context = index.CreateSearchContext())
            {
                var results = context.GetQueryable<SearchResultItem>()
                .Where(x => x.TemplateName.Equals("Product") && x["Description"].Contains(term))
                .Take(5).ToList();

                return results.Select(x => new ProductModel(x.GetField("StockCode").Value, float.Parse(x.GetField("Price").Value), x.GetField("Description").Value));
            }
        }
    }
}