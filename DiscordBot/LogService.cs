using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WowBot
{
    class LogService
    {
        public LogService(DiscordSocketClient client, CommandService commandService)
        {
            client.Log += LogAsync;
            commandService.Log += LogAsync;
        }

        private Task LogAsync(LogMessage message)
        {
            // TODO: exceptions, colors
            Log.Warn("Discord", message.ToString());
            return Task.CompletedTask;
        }
    }
}
