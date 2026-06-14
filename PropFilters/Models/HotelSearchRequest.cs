using System.Text.Json;
using System.Text.Json.Serialization;

namespace PropFilters.Models
{
    public class HotelSearchRequest
    {
        public DateOnly CheckIn { get; set; }
        public DateOnly CheckOut { get; set; }
        public Occupancy[] Occupancies { get; set; }
        public List<string> PropFilters { get; set; }
    }

    public class Occupancy
    {
        public int Adults { get; set; }
        public List<int> Ages { get; set; }
    }
}
