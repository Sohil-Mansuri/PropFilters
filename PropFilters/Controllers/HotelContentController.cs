using Microsoft.AspNetCore.Mvc;
using PropFilters.Models;
using PropFilters.PropFilter;

namespace PropFilters.Controllers
{
    [Route("api/content/v1")]
    [ApiController]
    public class HotelContentController(HotelResultProjector hotelResultProjector) : ControllerBase
    {
        [HttpPost("result")]
        public async Task<IActionResult> GetHotelContentAsync(HotelSearchRequest hotelSearchRequest)
        {
            string json = System.IO.File.ReadAllText("hotel-response.json");

            var response = System.Text.Json.JsonSerializer.Deserialize<HotelResponse>(json);

            var (hotelTree, contentTree) = FilterTreeBuilder.BuildFromPropFilters(hotelSearchRequest.PropFilters);

            var (hotel, content) = hotelResultProjector.Project(response, hotelTree, contentTree);

            var result = new
            {
                response.Token,
                HotelCount = hotel.Count,
                Hotels = hotel,
                Content = content,
                response.HasErrors
            };

            return Ok(result);
        }
    }
}
