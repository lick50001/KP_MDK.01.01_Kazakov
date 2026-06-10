namespace MarketAO.Models
{
    public class MarketItem
    {
        public int Id { get; set; }           // ID записи (строки на рынке)
        public int ItemId { get; set; }       // ID самого предмета (для связи таблиц)
        public string ItemName { get; set; }  // Название
        public string Tier { get; set; }      // T4, T5 и т.д.
        public int TierInt { get; set; }      // Число (для фильтра)
        public long Price { get; set; }       // Цена (число)
        public int Quantity { get; set; }     // Количество

        public string PriceValue => Price.ToString("N0");
    }
}
