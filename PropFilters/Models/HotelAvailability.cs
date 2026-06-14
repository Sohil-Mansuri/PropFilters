namespace PropFilters.Models
{
    public class HotelAvailability
    {
        public string HotelId { get; set; }
        public string SearchKey { get; set; }
        public string SupplierId { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public PriceInfo PriceInfo { get; set; }
        public string Refundable { get; set; }
        public int AvailableRooms { get; set; }
        public string AggregatorCode { get; set; }
        public string[] RateCodes { get; set; }
    }

    public class PriceInfo
    {
        public string CurrencyCode { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal BasePrice { get; set; }
        public decimal TotalTax { get; set; }
        public decimal AppliedDeal { get; set; }
        public decimal VendorCommission { get; set; }
    }

}
