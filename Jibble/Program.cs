namespace Jibble
{
    using Jibble.Interfaces;
    using Jibble.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            // 1. Setup Generic Host for logging, configuration, and DI
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile("appsettings.json", optional: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // 2. Configure the settings model from the "ODataService" section
                    services.Configure<ODataSettings>(
                        context.Configuration.GetSection(ODataSettings.SectionName));

                    // 3. Register HttpClient and IPeopleService with configurable timeout
                    services.AddHttpClient<IPeopleService, PeopleService>((serviceProvider, client) =>
                    {
                        // Resolve configured settings for HttpClient setup
                        var settings = serviceProvider.GetRequiredService<IOptions<ODataSettings>>().Value;

                        // The HttpClient's base address is set from configuration
                        client.BaseAddress = new Uri(settings.BaseUrl);
                        client.Timeout = TimeSpan.FromSeconds(settings.RequestTimeoutSeconds);
                    })
                    .Services.AddTransient<AppRunner>();
                })
                .Build();

            // 4. Resolve and Run the Application Logic
            try
            {
                await host.Services.GetRequiredService<AppRunner>().RunAsync();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nFATAL ERROR: The application failed to start or run. {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
