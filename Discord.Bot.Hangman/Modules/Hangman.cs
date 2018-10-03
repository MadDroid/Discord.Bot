using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Bot.Hangman.Modules
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
            var username = Program.LastUser.Username;

            #region Exceeded argument input logic

            if (argument.Length > 1)
            {
                if (argument.ToLower() != CurrentWord.ToLower())
                {
                    await ReplyAsync("Você errou em tentar acertar toda palavra" + "( " + username + " )");

                    MissTryed(argument);
                }
                else
                {
                    await ReplyAsync("Você acertou toda a palavra" + "( " + username + " )" + "a palavra era: " + CurrentWord);
                }

                return;
            }

            #endregion

            #region Normal argument input logic

            if (CurrentWord.ToLower().Contains(argument.ToLower()))
            {
                CorrectGuesses += argument.ToLower();
                await ReplyAsync("Acertou: " + "( " + username + " )" + GetUnderlinedWordRight());
            }
            else
            {
                if (TryesLeft > 1)
                {
                    MissTryed(argument);

                    await ReplyAsync("Errou: " + "( " + username + " )" + " tentativas restantes: " + TryesLeft);
                }
                else if (TryesLeft == 1)
                {
                    MissTryed(argument);

                    await ReplyAsync("Errou: " + "( " + username + " )" + " tentativas restantes: " + TryesLeft + " " + new Emoji(":que:"));

                    await Task.Delay(1000);

                    await ReplyAsync("Acabaram todas as tentativas desta palavra, a palavra era: " + CurrentWord);
                }
                else if (TryesLeft == 0)
                {
                    await ReplyAsync("Acabaram todas as tentativas desta palavra, a palavra era: " + CurrentWord);
                }
            }

            #endregion
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

            //await GenerateRandomWord();

            await ReplyAsync("Jogo reiniciado e tentativas zeradas!");
        }

        [Command("tryes")]
        public async Task DebuTryesLeft()
        {
            await ReplyAsync("Tentativas restantes: " + TryesLeft);
        }


        public void MissTryed(string arg)
        {
            TryesLeft--;

            IncorrectGuesses += arg.ToLower();
        }

        public Task GenerateRandomWord()
        {
            //CurrentWord = RandomWord;

            return Task.CompletedTask;
        }
    }
}
