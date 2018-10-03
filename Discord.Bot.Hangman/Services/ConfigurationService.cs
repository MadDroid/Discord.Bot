using Microsoft.Extensions.Configuration;

namespace Discord.Bot.Hangman.Services
{
    public class ConfigurationService
    {
        public static ConfigurationService Instance { get; private set; } = new ConfigurationService();
        
        public IConfiguration Configuration { get; }

        public ConfigurationService()
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .Build();
        }
    }
}
