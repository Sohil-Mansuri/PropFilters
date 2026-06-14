using PropFilters.Models;

namespace PropFilters.PropFilter
{
    public class HotelResultProjector(NestedFieldProjector projector)
    {
        public (List<object> hotel, List<object> content) Project(
            HotelResponse source,
            FilterNode hotelTree,
            FilterNode contentTree)
        {
            var hotel = projector.ProjectList(source.Hotel, hotelTree);
            var content = projector.ProjectList(source.Content, contentTree);

            return (hotel, content);
        }
    }
}
