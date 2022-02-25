using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord.WebSocket;

namespace WowBot
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("say")]
        [Summary("Echoes back a message")]
        public Task SayAsync([Remainder][Summary("The message to echo")] string echo)
            => ReplyAsync(echo);

        [Command("like")]
        [Summary("Likes your message")]
        public async Task LikeAsync()
        {
            Emoji e = new Emoji("\U0001f495");
            await Context.Message.AddReactionAsync(e);
        }

        [Command("account")]
        [Summary("wow account management")]
        public async Task AccountInfoAsync() => await ReplyAsync("account commands:\n - account create\n - account setgm");

        [Command("account create")]
        [Summary("wow account create")]
        public async Task AccountCreateInfoAsync() => await ReplyAsync("usage: account create <username> <password>\nUse a private message to keep your info safer.");

        [Command("account setgm")]
        [Summary("wow account setgm")]
        public async Task AccountSetGMInfoAsync() => await ReplyAsync("usage: account setgm <username> <gmlevel>\ngmlevel range: 1-3");

        [Command("realmlist")]
        [Summary("realmlist")]
        public async Task RealmlistInfoAsync() => await ReplyAsync($"set realmlist {Config.REALMLIST}");

        [Command("online")]
        [Summary("get num online players")]
        public async Task OnlinePlayersAsync()
        {
            int numOnline = AccountService.Instance.GetOnlineAccounts();
            if (numOnline == -1)
            {
                await ReplyAsync("Unspecified error fetching online players");
                return;
            }

            string accounts = numOnline == 1 ? "account" : "accounts";
            await ReplyAsync($"There is {numOnline} {accounts} currently connected ingame");
        }

        [Command("who")]
        [Summary("get who list of online characters")]
        public async Task OnlineCharactersAsync()
        {
            WowCharacter[] wowCharacters = AccountService.Instance.GetOnlineCharacters();
            if (wowCharacters == null)
            {
                await ReplyAsync("Unspecified error fetching online characters");
                return;
            }

            if (wowCharacters.Length == 0)
            {
                await ReplyAsync("No online characters found");
                return;

            }

            Embed embed = WhoEmbedBuilder.BuildEmbed(wowCharacters);
            await ReplyAsync(embed: embed);
            
        }


        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [Command("account create")]
        [Summary("Create a wow user account")]
        public async Task AccountCreateAsync([Remainder][Summary("Request query")] string query)
        {
            var message = await Context.Channel.GetMessagesAsync(1).Flatten().FirstOrDefaultAsync();
            await Context.Channel.DeleteMessageAsync(message);

            const int NUM_QUERY_PARTS = 2;
            const int MAX_ACCOUNT_LEN = 16;

            string[] parts = query.Split(" ");

            if (parts.Length != NUM_QUERY_PARTS)
            {
                await ReplyAsync("You have an error in your syntax.");
                await ReplyAsync("usage: account create <username> <password>\nUse a private message to keep your info safer.");
                return;
            }

            if (HasSpecialChars(parts[0]) || HasSpecialChars(parts[1]))
            {
                Log.Warn("wow", "Attempted input with special characters");
                await ReplyAsync("Are you trying to attack me? NO SPECIAL CHARACTERS!");
                return;
            }

            if (parts[0].Length > MAX_ACCOUNT_LEN)
            {
                await ReplyAsync("Username too long. Max 16 characters.");
                return;
            }

            if (parts[1].Length > MAX_ACCOUNT_LEN)
            {

                await ReplyAsync("Password too long. Max 16 characters. Sorry, Blizzard doesn't understand security =(");
                return;
            }

            bool success = AccountService.Instance.CreateAccount(parts[0], parts[1]);

            if (success)
            {
                Log.Debug("wow", $"Succesfully created account {parts[0]}");
                await ReplyAsync($"I have succesfully created your account!\nset realmlist {Config.REALMLIST}");
            }

            else
            {
                await ReplyAsync("There was a problem creating your account but the lazy programmer didn't specify what. Maybe the name is already in use?");
            }
        }

        private static bool HasSpecialChars(string yourString)
        {
            return yourString.Any(ch => !Char.IsLetterOrDigit(ch));
        }

        [Command("account setgm")]
        [Summary("Set GM level for account")]
        public async Task AccountSetGMAsync([Remainder][Summary("Requested GM level")] string query)
        {
            string[] parts = query.Split(" ");
            if (parts.Length != 2)
            {
                await ReplyAsync("You have an error in your syntax.");
                return;
            }

            string account = parts[0].ToUpper();

            if (HasSpecialChars(account))
            {
                Log.Warn("wow", "Attempted input with special characters");
                await ReplyAsync("You are offending me with the special characters. Shoo!");
                return;
            }

            if (parts[1].Length != 1)
            {
                await ReplyAsync("Are you stupid?");
                return;
            }

            int level;
            if (!int.TryParse(parts[1], out level))
            {
                await ReplyAsync("That's not even a number O.o");
                return;
            }

            switch (level)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    switch (AccountService.Instance.SetGMLevel(account, level))
                    {
                        case 0:
                            Log.Debug("wow", $"Setting GM Level {level} to account {account}");
                            await ReplyAsync("I bestow the power upon you");
                            break;
                        case 1:
                            await ReplyAsync("You don't have the power to edit admin accounts");
                            break;

                        case -1:
                            await ReplyAsync("There was an unspecified error accessing the matrix");
                            break;
                    }
                    break;

                default:
                    await ReplyAsync("No! I am the only admin here.");
                    break;
            }
        }
    }
}
