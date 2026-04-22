using System.ComponentModel.DataAnnotations;

namespace SpaceMarket.Api.Models
{
    public class Items
    {
        [Key]
        public int Item_Id { get; set; }
        public string ItemName { get; set; }
        public int MaxBuyPrice { get; set; }
        public bool IsActive { get; set; }


        public int UserId { get; set; }
        public Users User { get; set; }
    }
}
