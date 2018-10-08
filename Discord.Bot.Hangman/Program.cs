using Discord.Bot.Hangman.Services;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;
using MadDroid.DependencyInjection.Logging;

namespace Discord.Bot.Hangman
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient client;
        private CommandService commands;
        private IServiceProvider services;
        private ILogger logger;

        public static char Prefix = '!';

        private async Task MainAsync()
        {
            client = new DiscordSocketClient();
            commands = new CommandService();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .AddInMemoryCollection()
                .Build();

            services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .AddSingleton<IConfiguration>(configuration)
                .AddLogging(options =>
                {
                    options.AddConsole();
                    options.AddDebug();
                    options.AddFile("logs.txt");
                })
                .BuildServiceProvider();

            logger = services.GetService<ILogger<Program>>();

            client.Log += OnLog;
            commands.Log += OnLog;
            client.Connected += Client_Connected;

            await RegisterCommandsAsync();

            string botToken = configuration["bot_token"];

            await client.LoginAsync(TokenType.Bot, botToken);

            await client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Client_Connected()
        {
            //logger.LogInformation("Logged as {0}#{1}", client.CurrentUser.Username, client.CurrentUser.Discriminator);
            logger.Log(LogLevel.Information, new EventId(), this, null, (state, exception) =>
            {
                return $"Logged in as {client.CurrentUser.Username}#{client.CurrentUser.Discriminator}";
            });
            return Task.CompletedTask;
        }

        Task OnLog(LogMessage arg)
        {
            logger.LogInformation(arg.Exception, arg.Message);
            return Task.CompletedTask;
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
                    logger.LogInformation(result.ErrorReason);
            }
        }
    }
}
