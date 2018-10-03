using Microsoft.Extensions.Configuration;
using System;

namespace Discord.Bot.Hangman
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .Build();
            
            string botToken = configuration.GetSection("bot_token").Value;
        }
    }
}
