namespace PropFilters.Models
{
    public class HotelContent
    {
        public string HotelId { get; set; }
        public string Name { get; set; }
        public string AccommodationType { get; set; }
        public double StarRating { get; set; }
        public double Popularity { get; set; }
        public double Score { get; set; }
        public string Description { get; set; }
        public HotelChain HotelChain { get; set; }
        public HotelChain[] HotelBrands { get; set; }
        public List<Facility> Facilities { get; set; }
        public List<Image> Images { get; set; }
        public List<Neighbourhood> Neighbourhoods { get; set; }
        public List<Landmark> Landmarks { get; set; }
        public HotelContact HotelContact { get; set; }
        public List<Sustainability> Sustainability { get; set; }
        public List<RatingSource> RatingSources { get; set; }
        public HotelPolicies[] HotelPolicies { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public HotelMapping[] HotelMappings { get; set; } = null;
        public double DistanceInKm { get; set; }
    }
    public class HotelMapping
    {
        public string Provider { get; set; }
        public string SupplierName { get; set; }
        public string HotelId { get; set; }
        public double Confidence { get; set; }
    }
    public class HotelPolicies
    {
        public string PolicyType { get; set; }
        public string[] Contains { get; set; }
    }
    public class HotelBrand
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class HotelChain
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Facility
    {
        public string Title { get; set; }
        public bool IsFree { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
    }

    public class Image
    {
        public string Url { get; set; }
        public string ImageType { get; set; }
        public bool MainPhoto { get; set; }
        public string Type { get; set; }
        public string Resolution { get; set; }
    }

    public class Neighbourhood
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public double DistanceInKm { get; set; }
    }

    public class Landmark
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public double DistanceInKm { get; set; }
    }

    public class HotelContact
    {
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string CityName { get; set; }
        public string StateName { get; set; }
    }

    public class Sustainability
    {
        public string CertificateType { get; set; }
        public string CertificateSource { get; set; }
        public string Icon { get; set; }
        public string Definition { get; set; }
    }

    public class RatingSource
    {
        public string Source { get; set; }
        public string Remarks { get; set; }
        public double Rating { get; set; }
        public int Count { get; set; }
    }
}
