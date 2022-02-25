using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace WowBot
{
    class CommandHandler
    {
        private readonly DiscordSocketClient client;
        private readonly CommandService commandService;
        private DateTime allowedToSpeakTime = new DateTime(0);
        private const long SHUTUP_MINUTES = 6;
        private int timesToldToBeQuiet = 0;

        public CommandHandler(DiscordSocketClient client, CommandService commandService)
        {
            this.client = client;
            this.commandService = commandService;
        }

        public async Task InstallCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;
            await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            if (msg == null) return;

            int argPos = 0;

            if (!msg.HasCharPrefix('!', ref argPos) && msg.Source == MessageSource.User)
            {
                if (msg.Attachments.Count > 0)
                {
                    string attatchementUrl = msg.Attachments.ElementAt(0).Url;
                    HttpClient httpClient = new HttpClient();
                    var result = await httpClient.GetAsync(attatchementUrl);
                    // TODO: FIx this

                    return;
                }
            }

            else if (msg.Author.IsBot) return;

            var context = new SocketCommandContext(client, msg);

            await commandService.ExecuteAsync(context, argPos, null);
        }
    }
}
