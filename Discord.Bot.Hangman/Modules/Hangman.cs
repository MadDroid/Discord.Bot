using Discord.Bot.Hangman.Services;
using Discord.Commands;
using MadDroid.Helpers;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Discord.Bot.Hangman.Modules
{
    [Name("Hangman")]
    public class Hangman : ModuleBase<SocketCommandContext>
    {
        #region Private Properties
        /// <summary>
        /// A list of the words
        /// </summary>
        List<string> Words { get; } = new List<string>();

        readonly LoggingService logging;
        #endregion

        #region Static Fields
        /// <summary>
        /// The current word that is being guessed
        /// </summary>
        static string currentWord;

        /// <summary>
        /// A list with the chars that already have ben attempted
        /// </summary>
        readonly static HashSet<char> doneChars = new HashSet<char>();

        /// <summary>
        /// A list with the char that already been guessed
        /// </summary>
        readonly static HashSet<char> rightChars = new HashSet<char>();
        #endregion

        #region Constructor
        public Hangman(IConfiguration configuration, LoggingService logging)
        {
            // Get the section with the words
            var section = configuration.GetSection("words");

            foreach (var item in section.AsEnumerable())
            {
                if (!string.IsNullOrEmpty(item.Value))
                    // Add the words to the list
                    Words.Add(item.Value);
            }

            // Define the logging
            this.logging = logging;

            // If current word is not set...
            if (string.IsNullOrEmpty(currentWord))
            {
                // Get a random word from the list
                currentWord = Words.Random();
                // Log the first word
                Log($"First word is {currentWord}").Wait();
            }
        }
        #endregion

        #region HangmanWord
        [Summary("Tenta acertar uma letra da palavra atual.")]
        [Command("hm")]
        public async Task HangmanWord(char ch)
        {
            // If there is more than 0 char is the current word...
            if (currentWord.Count(c => c == char.ToLower(ch)) > 0)
            {
                // If the char was already guessed...
                if (rightChars.Contains(ch))
                {
                    // Reply
                    await ReplyAsync("Letra já encontrada.");
                    return;
                }

                // Add the right char to the list
                rightChars.Add(ch);

                // Reply with the skeloton word
                await ReplyAsync($"`{GetWordSkeleton()}`");

                // If the skeleton word is the same as the current word...
                if (GetWordSkeleton().Replace(" ", "") == currentWord)
                {
                    // Reply end game
                    await ReplyAsync($"Fim de jogo. A palavra era {currentWord}");
                    // Reset the game
                    await Reset();
                }
            }
            else
            {
                // Reply wrong letter
                await ReplyAsync("Letra errada.");
            }

            // Add the attempt to the list
            doneChars.Add(ch);
        }

        [Summary("Tenta acertar a palavra atual.")]
        [Command("hm")]
        public async Task HangmanWord(string word)
        {
            // If the word is equal to the current word...
            if (word == currentWord.ToLower())
            {
                // Reply right word
                await ReplyAsync($"Acertou :clap:! A palavra era {currentWord}.");
                // Reset the game
                await Reset();
                return;
            }
            // Reply wrong word
            await ReplyAsync($"Errou :thumbsdown:! A palavra não é {word}.");
        }
        #endregion

        [Summary("Retorna o alfabeto com as letras que já foram usadas.")]
        [Command("alf")]
        public async Task Alphabet()
        {
            var builder = new StringBuilder();

            // TODO: Accented chars must work too

            // For each letter in the alphabet...
            for (char c = 'a'; c <= 'z'; c++)
            {
                // If the doneChars list contais the letter...
                if (doneChars.Contains(c))
                    // Append the letter to the builder
                    builder.Append(c);
                // Otherwise
                else
                    // Append a underline
                    builder.Append('_');
                // Append a space
                builder.Append(' ');
            }
            // Reply
            await ReplyAsync($"`{builder.ToString().Trim()}`");
        }

        [Command("add")]
        public async Task AddWord(string word)
        {
            if (Words.Contains(word.ToLower()))
            {
                // Reply existing word
                await ReplyAsync("Essa palavra já existe.");
                return;
            }

            // TODO: Accented words must work too

            // Check if is a word
            if (!Regex.IsMatch(word, @"^[a-zA-Z]+$"))
            {
                // Reply only words
                await ReplyAsync("Somente palavras que contanham letras.");
                return;
            }

            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();

            // If the dictionary file exists...
            if (File.Exists("dictionary.json"))
                // Load the words
                dict = await Storage.ReadAsync<Dictionary<string, List<string>>>("dictionary.json");

            // Add the word to the dictionary
            dict["words"].Add(word.ToLower());

            // Save the changes
            await Storage.SaveAsync("dictionary.json", dict);

            // Reply with the confirmation
            await ReplyAsync($"Palavra {word.ToLower()} adicionada.");
        }

        /// <summary>
        /// Gets the skeloton of the current word
        /// </summary>
        /// <returns></returns>
        string GetWordSkeleton()
        {
            var builder = new StringBuilder();

            // For each char in the current word
            foreach (var c in currentWord)
            {
                // If the rightChars list contains the word char...
                if (rightChars.Contains(c))
                    // Append the char to the builder
                    builder.Append(c);
                // Otherwise
                else
                    // Append a underline
                    builder.Append('_');
                // Append a space
                builder.Append(' ');
            }
            // Return the string
            return builder.ToString().Trim();
        }

        [Command("tip"), Alias("dica")]
        public async Task Tip()
        {
            // Reply the word size
            await ReplyAsync($"A palavra tem {currentWord.Length} letras.");
        }

        [Command("reset")]
        public async Task ResetCommand()
        {
            // Reset the game
            await Reset();
            // Reply the game was reseted
            await ReplyAsync("O jogo foi reiniciado.");
        }

        /// <summary>
        /// Reset the game
        /// </summary>
        private async Task Reset()
        {
            // Get a new word from the words list
            currentWord = Words.Random();
            // Clear the lists
            rightChars.Clear();
            doneChars.Clear();

            // Log the new word
            await Log($"Game reseted by {Context.User.Username}#{Context.User.Discriminator}. New word is {currentWord}");
        }

        /// <summary>
        /// A simple method to log to console
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <returns></returns>
        async Task Log(string message)
        {
            // Log the message to console
            await logging.Log<Hangman>(message, LogSeverity.Info);
        }
    }
}
