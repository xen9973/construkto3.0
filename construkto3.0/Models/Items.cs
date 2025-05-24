public class Item : ICloneable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public string Description { get; set; } // Это свойство добавлено
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string Source { get; set; }

    public object Clone()
    {
        return new Item
        {
            Id = this.Id,
            Name = this.Name,
            Category = this.Category,
            Description = this.Description,
            UnitPrice = this.UnitPrice,
            Quantity = this.Quantity,
            Source = this.Source
        };
    }
}