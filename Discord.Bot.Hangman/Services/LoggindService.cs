using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Discord.Bot.Hangman.Services
{
    public class LoggingService
    {
        readonly DiscordSocketClient client;
        readonly CommandService commands;
        readonly IConfiguration configuration;
        readonly string logsPath;
        readonly string logFile = $"{DateTime.Now.ToString("dd-MM-yyyy")}.txt";

        public LoggingService(DiscordSocketClient client, CommandService commands, IConfiguration configuration)
        {
            this.client = client;
            this.commands = commands;

            this.configuration = configuration;

            client.Log += OnLogAsync;
            commands.Log += OnLogAsync;

            logsPath = configuration.GetSection("logging")["directory"] ?? string.Empty;

            if (!Directory.Exists(logsPath))
                Directory.CreateDirectory(logsPath);
        }

        private Task OnLogAsync(LogMessage arg)
        {
            
            string logText = $"{DateTime.Now.ToLongTimeString()} [{arg.Severity}] {arg.Source}: {arg.Exception?.ToString() ?? arg.Message}";
            File.AppendAllText(Path.Combine(logsPath, logFile), logText + "\n");

            return Console.Out.WriteLineAsync(logText);
        }

        public Task Log<T>(string msg, LogSeverity severity)
        {
            string logText = $"{DateTime.Now.ToLongTimeString()} [{severity}] {typeof(T).Name}: {msg}";

            File.AppendAllText(Path.Combine(logsPath, logFile), logText + "\n");

            return Console.Out.WriteLineAsync(logText);
        }

        public Task Log(string msg, Type type, LogSeverity severity)
        {
            string logText = $"{DateTime.Now.ToLongTimeString()} [{severity}] {type.GetType().Name}: {msg}";

            File.AppendAllText(Path.Combine(logsPath, logFile), logText + "\n");

            return Console.Out.WriteLineAsync(logText);
        }
    }
}
