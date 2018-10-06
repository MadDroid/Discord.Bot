using Discord.Bot.Hangman.Services;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Discord.Bot.Hangman
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient client;
        private CommandService commands;
        private IServiceProvider services;

        public static char Prefix = '!';

        private async Task MainAsync()
        {
            client = new DiscordSocketClient();
            commands = new CommandService();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("dictionary.json", true, true)
                .AddJsonFile("graphics.json",true,true)
                .AddInMemoryCollection()
                .Build();

            services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .AddSingleton<LoggingService>()
                .AddSingleton<IConfiguration>(configuration)
                .BuildServiceProvider();


            services.GetRequiredService<LoggingService>();

            await RegisterCommandsAsync();

            string botToken = configuration["bot_token"];

            await client.LoginAsync(TokenType.Bot, botToken);

            await client.StartAsync();

            await Task.Delay(-1);
        }

        public async Task RegisterCommandsAsync()
        {
            client.MessageReceived += MessageReceived;

            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task MessageReceived(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;

            if (message is null || message.Author.IsBot) return;

            int argPos = 0;

            if (message.HasCharPrefix(Prefix, ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))
            {
                var context = new SocketCommandContext(client, message);
                var result = await commands.ExecuteAsync(context, argPos, services);

                if (!result.IsSuccess)
                    await services.GetRequiredService<LoggingService>().Log(result.ErrorReason, result.GetType(), LogSeverity.Info);
            }
        }
    }
}
