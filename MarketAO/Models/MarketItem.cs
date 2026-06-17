namespace MarketAO.Models
{
    public class MarketItem
    {
        public int Id { get; set; }
        public int ItemId { get; set; } 
        public string ItemName { get; set; }
        public string Tier { get; set; }
        public int TierInt { get; set; }
        public long Price { get; set; }
        public int Quantity { get; set; }

        public string PriceValue => Price.ToString("N0");
    }
}
