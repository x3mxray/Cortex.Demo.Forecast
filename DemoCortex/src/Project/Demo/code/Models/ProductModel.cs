namespace Demo.Project.Demo.Models
{
    public class ProductModel
    {
        public ProductModel(string id, float price, string description)
        {
            this.id = id;
            this.price = price;
            this.description = description;
        }
        public string id { get; set; }
        public float price { get; set; }
        public string description { get; set; }
    }
}