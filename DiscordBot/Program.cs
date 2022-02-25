using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WowBot
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private DiscordSocketClient client;
        private CommandService commandService;
        private LogService logService;
        private CommandHandler commandHandler;

        public async Task MainAsync()
        {
            try
            {
                client = new DiscordSocketClient();
                commandService = new CommandService();
                logService = new LogService(client, commandService);
                commandHandler = new CommandHandler(client, commandService);
                await commandHandler.InstallCommandsAsync();

                const string token = Config.DISCORD_TOKEN;

                await client.LoginAsync(TokenType.Bot, token);
                await client.StartAsync();

                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                Log.Error("Discord", e.Message);
            }
        }
    }
}
