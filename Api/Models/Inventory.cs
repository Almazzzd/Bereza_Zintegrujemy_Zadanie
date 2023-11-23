using CsvHelper.Configuration;

namespace Api.Models
{
    public class Inventory
    {
        public int ProductID { get; set; }
        public string SKU { get; set; }
        public string Unit { get; set; }
        public decimal Quantity { get; set; }  // Poprawiono typ na decimal
        public string ManufacturerName { get; set; }  // Dodano brakujące pole
        public string ManufacturerRefNum { get; set; } // Zmieniono typ na string
        public string Shipping { get; set; }
        public decimal? ShippingCost { get; set; }  // Poprawiono typ na decimal
    }

    public sealed class InventoryMap : ClassMap<Inventory>
    {
        public InventoryMap()
        {
            Map(m => m.ProductID).Index(0);
            Map(m => m.SKU).Index(1);
            Map(m => m.Unit).Index(2);
            Map(m => m.Quantity).Index(3);
            Map(m => m.ManufacturerName).Index(4);
            Map(m => m.ManufacturerRefNum).Index(5).TypeConverterOption.NullValues(string.Empty); // Ignorowanie pustych wartości
            Map(m => m.Shipping).Index(6);
            Map(m => m.ShippingCost).Index(7).TypeConverterOption.NullValues(string.Empty); // Ignorowanie pustych wartości
        }
    }


}
