
using PropFilters.PropFilter;
using System.Text.Json.Serialization;

namespace PropFilters
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });


            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            builder.Services.AddSingleton<HotelResultProjector>();
            builder.Services.AddSingleton<NestedFieldProjector>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
