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
        static public string []Words { get; set; }

        static public string DefaultAlphabet
        {
            get
            {
                return "abcefghijklmnopqrstuvxwyz";
            }
        }

        [Command("tentar")]
        public async Task TryLetter(string argument)
        {
            if(string.IsNullOrEmpty(CurrentWord))
            {
                GenerateRandomWord();
            }

            var username = Program.LastUser.Username;

            #region Exceeded argument input logic

            if (argument.Length > 1)
            {
                if (argument.ToLower() != CurrentWord.ToLower())
                {
                    MissTryed(argument);

                    await ReplyAsync("Você errou em tentar acertar toda palavra" + "( " + username + " )" + " tentativas restantes: " + TryesLeft);
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
                await ReplyAsync("Acertou: " + "( " + username + " )" + GetUnderlinedWordRight() + new Emoji(":ok_hand:"));
            }
            else
            {
                if (TryesLeft > 1)
                {
                    MissTryed(argument);

                    await ReplyAsync(new Emoji(":no_entry:") + "Errou: " + "( " + username + " )" + " tentativas restantes: " + TryesLeft);
                }
                else if (TryesLeft == 1)
                {
                    MissTryed(argument);

                    await ReplyAsync(new Emoji(":no_entry:") + "Errou: " + "( " + username + " )" + " tentativas restantes: " + TryesLeft);

                    await Task.Delay(1000);

                    await ReplyAsync(new Emoji(":warning:") + " Acabaram todas as tentativas desta palavra, a palavra era: " + CurrentWord);
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

        private string GetUnderlinedWordEmpty()
        {
            string valueReturn = string.Empty;

            for (int i = 0; i < CurrentWord.Length; i++)
            {
                valueReturn += " - ";
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

        [Command("reiniciar")]
        public async Task RestartGame()
        {
            TryesLeft = 6;

            CorrectGuesses = string.Empty;

            GenerateRandomWord();

            await ReplyAsync("Jogo reiniciado e tentativas zeradas! " + new Emoji(":thumbsup:"));
        }

        [Command("comandos")]
        public async Task Commands()
        {
            await ReplyAsync("Lista de comandos :  !reinicar !atual !tentar !tentativas !comandos");
        }

        [Command("tentativas")]
        public async Task DebuTryesLeft()
        {
            if (string.IsNullOrEmpty(CurrentWord))
            {
                GenerateRandomWord();
            }

            await ReplyAsync(new Emoji(":point_right:") + " Tentativas restantes: " + TryesLeft);
        }

        [Command("atual")]
        public async Task GetCurrentWord()
        {
            if (string.IsNullOrEmpty(CurrentWord))
            {
                GenerateRandomWord();
            }

            if (CorrectGuesses == string.Empty)
            {
                await ReplyAsync(new Emoji(":point_right:") + " Palavra atual : " + GetUnderlinedWordEmpty());
            }
            else
            {
                await ReplyAsync(new Emoji(":point_right:") + " Palavra atual : " + GetUnderlinedWordRight());
            }
        }


        public void MissTryed(string arg)
        {
            TryesLeft--;

            IncorrectGuesses += arg.ToLower();
        }

        static public void GenerateRandomWord()
        {
            Random random = new Random();

            int randomInteger = random.Next(0, Words.Length -1);

            CurrentWord = Words[randomInteger];
        }

        
    }
}
