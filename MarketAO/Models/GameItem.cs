namespace MarketAO.Models
{
    public class GameItem
    {
        public string ItemName { get; set; } 
        public string Category { get; set; }
        public int Tier { get; set; } 
        public int Enchantment { get; set; }
        public string IconPath { get; set; } 
        public string TierString => $"T{Tier}.{Enchantment}";
    }
}