namespace MarketAO.Models // Проверь, чтобы namespace совпадал с твоим проектом
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