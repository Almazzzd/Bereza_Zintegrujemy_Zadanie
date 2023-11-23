using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Api.Models;

namespace Api.Models
{
    public class Products
    {
        public int ID { get; set; }
        public string SKU { get; set; }
        public string name { get; set; }
        public string EAN { get; set; }
        public string ProducerName { get; set; }
        public string Category { get; set; }
        public bool IsWire { get; set; }
        public bool Available { get; set; }
        public bool IsVendor { get; set; }
        public string DefaultImage { get; set; }
    }
}


//mapowanie Produktów z Pliku CSV
public class ProductsMap : ClassMap<Products>
{
    public ProductsMap()
    {
        {
            Map(m => m.ID).Name("ID");
            Map(m => m.SKU).Name("SKU");
            Map(m => m.name).Name("name");
            Map(m => m.EAN).Name("EAN");
            Map(m => m.ProducerName).Name("producer_name");
            Map(m => m.Category).Name("category");
            Map(m => m.IsWire).Name("is_wire").TypeConverter<BooleanConverter>();
            Map(m => m.Available).Name("available").TypeConverter<BooleanConverter>();
            Map(m => m.IsVendor).Name("is_vendor").TypeConverter<BooleanConverter>();
            Map(m => m.DefaultImage).Name("default_image");
        }
    }
}