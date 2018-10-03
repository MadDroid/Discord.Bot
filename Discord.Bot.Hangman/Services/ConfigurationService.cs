using Microsoft.Extensions.Configuration;

namespace Discord.Bot.Hangman.Services
{
    public class ConfigurationService
    {
        public IConfiguration Configuration { get; }

        public ConfigurationService()
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("dictionary.json", false, true)
                .Build();
        }
    }
}
