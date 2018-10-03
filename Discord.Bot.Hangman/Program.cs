using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Net;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Discord;
using System.Configuration;

namespace Discord.Bot.Hangman
{
    public class Program
    {
        public static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        static public SocketUser LastUser { get; set; }

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        private async Task RunBotAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            // Depency injection based on the client and commands to get the infos
            _services = new ServiceCollection().AddSingleton(_client).AddSingleton(_commands).BuildServiceProvider();

            string botToken = "";

            LastUser = null;

            Hangman.CurrentWord = "Palavra";

            // events inputs

            Hangman.TryesLeft = 6;

            _client.Log += Log;

            await RegisterCommandAsync();

            await _client.LoginAsync(TokenType.Bot, botToken);

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        public Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);

            return Task.CompletedTask;
        }

        public async Task RegisterCommandAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;

            // if the command is null or who typed is a bot

            if (message == null || message.Author.IsBot)
            {
                return;
            }

            int argumentIndexPosition = 0;

            if (message.HasStringPrefix("!", ref argumentIndexPosition) || message.HasMentionPrefix(_client.CurrentUser, ref argumentIndexPosition))
            {
                await UpdateLastUser(arg);

                var context = new SocketCommandContext(_client, message);

                var result = await _commands.ExecuteAsync(context, argumentIndexPosition, _services);

                if (!result.IsSuccess)
                {
                    Console.WriteLine(result.ErrorReason);
                }
            }
        }

        static public Task UpdateLastUser(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;

            LastUser = message.Author;

            return Task.CompletedTask;
        }
    }
}
