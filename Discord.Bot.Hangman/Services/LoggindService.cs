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
        DiscordSocketClient client;
        CommandService commands;
        readonly ConfigurationService ConfigurationService = ConfigurationService.Instance;

        readonly string logsDirectory;
        readonly string logsFile;

        public LoggingService(DiscordSocketClient client, CommandService commands)
        {
            this.client = client;
            this.commands = commands;

            client.Log += OnLogAsync;
            commands.Log += OnLogAsync;

            logsDirectory = ConfigurationService.Configuration.GetSection("logging")["directory"];
            logsFile = ConfigurationService.Configuration.GetSection("logging")["files"];
        }

        private Task OnLogAsync(LogMessage arg)
        {
            
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
                Log("Diretótio de logs criado", GetType(), LogSeverity.Info);
            }

            if (!File.Exists(logsFile))
            {
                File.Create(logsFile).Dispose();
                Log("Arquivo de logs criado", GetType(), LogSeverity.Info);
            }

            string logText = $"{DateTime.Now.ToLongTimeString()} [{arg.Severity}] {arg.Source}: {arg.Exception?.ToString() ?? arg.Message}";
            File.AppendAllText(logsFile, logText + "\n");

            return Console.Out.WriteLineAsync(logText);
        }

        public static Task Log(string msg, Type type, LogSeverity severity)
        {
            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
                Log("Diretótio de logs criado", typeof(LoggingService), LogSeverity.Info);
            }

            if (!File.Exists("Logs/log.txt"))
            {
                File.Create("Logs/log.txt").Dispose();
                Log("Arquivo de logs criado", typeof(LoggingService), LogSeverity.Info);
            }

            string logText = $"{DateTime.Now.ToLongTimeString()} [{severity}] {type.Name}: {msg}";

            File.AppendAllText("Logs/log.txt", logText + "\n");

            return Console.Out.WriteLineAsync(logText);
        }
    }
}
