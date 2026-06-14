namespace PropFilters.Models
{
    public class HotelResponse
    {
        public string Token { get; set; }
        public int HotelCount { get; set; }
        public List<HotelAvailability> Hotel { get; set; }
        public List<HotelContent> Content { get; set; }
        public bool HasErrors { get; set; }
    }
}
