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

        static public SocketUser LastUser { get; set; }

#if DEBUG
        public static char Prefix = '$';
#else
        public static char Prefix = '!';
#endif

        private async Task MainAsync()
        {
            string botToken = ConfigurationService.Instance.Configuration.GetSection("bot_token").Value;

            client = new DiscordSocketClient();
            commands = new CommandService();

            services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .AddSingleton<LoggingService>()
                .BuildServiceProvider();

            services.GetRequiredService<LoggingService>();

            await RegisterCommandsAsync();

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

            UpdateLastUser(arg);

            if (message is null || message.Author.IsBot) return;

            int argPos = 0;

            if (message.HasCharPrefix(Prefix, ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))
            {
                var context = new SocketCommandContext(client, message);
                var result = await commands.ExecuteAsync(context, argPos, services);

                if (!result.IsSuccess)
                    await LoggingService.Log(result.ErrorReason, result.GetType(), LogSeverity.Info);
            }
        }

        public void UpdateLastUser(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;

            LastUser = message.Author;
        }
    }
}
