using System;
using System.Collections.Generic;
using System.Linq;
using Demo.Foundation.ProcessingEngine.Models;
using Demo.Project.Demo.Controllers;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data.Items;

namespace Demo.Project.Demo.Services
{
    public class ItemService
    {
        private static string GetItemName(string name)
        {
            return ItemUtil.ProposeValidItemName(name);
        }
        public void CreateItems(List<PurchaseInvoice> products)
        {
            using (new Sitecore.SecurityModel.SecurityDisabler())
            {
                // Get the master database
                var master = Sitecore.Data.Database.GetDatabase("master");
                // Get the template to base the new item on
                TemplateItem template = master.GetItem("/sitecore/templates/Feature/Product");

                // Get the place in the site tree where the new item must be inserted
                Item parentItem = master.GetItem("/sitecore/content/Imdad/Home/Products");

                foreach (var product in products)
                {
                    // Add the item to the site tree
                    Item newItem = parentItem.Add(GetItemName(product.Description), template);

                    newItem.Editing.BeginEdit();
                    try
                    {
                        // Assign values to the fields of the new item
                        newItem.Fields["Description"].Value = product.Description;
                        newItem.Fields["Price"].Value = product.Price.ToString();
                        newItem.Fields["StockCode"].Value = product.StockCode;

                        // End editing will write the new values back to the Sitecore
                        // database (It's like commit transaction of a database)
                        newItem.Editing.EndEdit();
                    }
                    catch (Exception ex)
                    {
                        // The update failed, write a message to the log
                        Sitecore.Diagnostics.Log.Error("Could not update item " + newItem.Paths.FullPath + ": " + ex.Message, this);

                        // Cancel the edit (not really needed, as Sitecore automatically aborts
                        // the transaction on exceptions, but it wont hurt your code)
                        newItem.Editing.CancelEdit();
                    }
                }

            }
        }

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