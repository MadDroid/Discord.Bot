using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord.Bot.Hangman
{
    public class Hangman : ModuleBase<SocketCommandContext>
    {
        static public int TryesLeft { get; set; }
        static public string CurrentWord { get; set; }
        static public string CorrectGuesses { get; set; }
        static public string IncorrectGuesses { get; set; }

        static public string DefaultAlphabet
        {
            get
            {
                return "abcefghijklmnopqrstuvxwyz";
            }
        }

        [Command("try")]
        public async Task TryLetter(string argument)
        {
            if (argument.Length > 1 && argument.ToLower() != CurrentWord.ToLower())
            {
                await ReplyAsync("Você não pode chutar mais que uma letra por vez! ( " + argument.ToString() + " : " + (argument.Length - 1) + " letras excedidas )");
            }
            else
            {

            }

            var username = Program.LastUser.Username;

            if (CurrentWord.ToLower().Contains(argument.ToLower()))
            {
                CorrectGuesses += argument.ToLower();
                await ReplyAsync("Acertou: " + "( " + username + " )" + GetUnderlinedWordRight());
            }
            else
            {
                if (TryesLeft > 1)
                {
                    await MissTryed(argument);

                    await ReplyAsync("Errou: " + "( " + username + " )" + " tentativas restantes: " + TryesLeft);
                }
                else if (TryesLeft == 1)
                {
                    await MissTryed(argument);

                    await ReplyAsync("Errou: " + "( " + username + " )" + " tentativas restantes: " + TryesLeft + " " + new Emoji(":que:"));

                    await Task.Delay(1000);

                    await ReplyAsync("Acabaram todas as tentativas desta palavra, a palavra era: " + CurrentWord);
                }
                else if (TryesLeft == 0)
                {
                    await ReplyAsync("Acabaram todas as tentativas desta palavra, a palavra era: " + CurrentWord);
                }
            }
        }

        private string GetUnderlinedWordRight()
        {
            string wordToCheck = CurrentWord.ToLower();

            string valueReturn = string.Empty;

            for (int i = 0; i < CurrentWord.Length; i++)
            {
                if (CompareChars(CurrentWord[i].ToString(), CorrectGuesses))
                {
                    valueReturn += " " + CurrentWord[i].ToString() + " ";
                }
                else
                {
                    valueReturn += " - ";
                }
            }

            return valueReturn;
        }

        public bool CompareChars(string currentIndex, string build)
        {
            if (build.ToLower().Contains(currentIndex.ToLower()))
            {
                return true;
            }

            return false;
        }

        [Command("restart")]
        public async Task RestartGame()
        {
            TryesLeft = 6;

            CorrectGuesses = string.Empty;

            await ReplyAsync("Jogo reiniciado e tentativas zeradas!");
        }

        [Command("tryes")]
        public async Task DebuTryesLeft()
        {
            await ReplyAsync("Tentativas restantes: " + TryesLeft);
        }


        public Task MissTryed(string arg)
        {
            Console.WriteLine(arg);

            TryesLeft--;

            IncorrectGuesses += arg.ToLower();

            return Task.CompletedTask;
        }
    }
}
