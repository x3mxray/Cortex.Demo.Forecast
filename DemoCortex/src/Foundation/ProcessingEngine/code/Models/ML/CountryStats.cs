namespace Demo.Foundation.ProcessingEngine.Models.ML
{
    public class CountryStats
    {
        public string Country { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Max { get; set; }
        public int Min { get; set; }
        public int Avg { get; set; }
        public int Count { get; set; }

        public int Units { get; set; }
        public int? Prev { get; set; }
        public int? Next { get; set; }
    }
}
