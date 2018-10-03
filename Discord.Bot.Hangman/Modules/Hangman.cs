using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Bot.Hangman.Modules
{
    public class Hangman : ModuleBase<SocketCommandContext>
    {
        [Command("hm")]
        public async Task TryChar(char c)
        {

        }
    }
}
