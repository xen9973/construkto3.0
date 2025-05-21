namespace construkto3._0.Models
{
    public class Item : ICloneable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; } 
        public string Source { get; set; } 

        public object Clone()
        {
            return new Item
            {
                Name = this.Name,
                Category = this.Category,
                UnitPrice = this.UnitPrice,
                Quantity = this.Quantity,
                AvailableQuantity = this.AvailableQuantity,
                Source = this.Source
            };
        }
    }
}
