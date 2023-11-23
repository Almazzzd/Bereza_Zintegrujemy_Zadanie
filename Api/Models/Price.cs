using CsvHelper.Configuration;


namespace Api.Models
{
    public class Price
    {
        public string InternalID { get; set; }
        public string SKU { get; set; }
        public string NetPrice { get; set; }
        public string NetPriceAfterDiscount { get; set; }
        public string VATRate { get; set; }
        public string NetPriceAfterDiscountForLogisticUnit { get; set; }
    }

    public class PriceMap : ClassMap<Price>
    {
        public PriceMap()
        {
            Map(m => m.InternalID).Index(0);
            Map(m => m.SKU).Index(1);
            Map(m => m.NetPrice).Index(2);
            Map(m => m.NetPriceAfterDiscount).Index(3);
            Map(m => m.VATRate).Index(4);
            Map(m => m.NetPriceAfterDiscountForLogisticUnit).Index(5);
        }
    }

}
