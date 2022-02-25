using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowBot
{
    class WhoEmbedBuilder
    {
        public static Embed BuildEmbed(WowCharacter[] wowCharacters)
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("!who");
            builder.AddField(BuildField("result", GetWhoListString(wowCharacters)));
            builder.WithFooter(BuildEmbedFooter(wowCharacters.Length));

            //builder.WithColor(IsSuccess ? Color.Green : Color.Red);
            builder.WithColor(Color.Green);
            return builder.Build();
        }

        private static string GetWhoListString(WowCharacter[] wowCharacters)
        {
            string result = "";

            for (int i = 0; i < wowCharacters.Length; i++)
            {
                result += wowCharacters[i].Name + " | " +
                    wowCharacters[i].GetRaceString() + " | " +
                    wowCharacters[i].GetGenderString() + " | " +
                    wowCharacters[i].GetClassString() + " | " +
                    wowCharacters[i].Level + " | " + 
                    wowCharacters[i].MapId + "\n";
            }

            return result;
        }

        private static EmbedFooterBuilder BuildEmbedFooter(int totalCharacters)
        {
            EmbedFooterBuilder footerBuilder = new EmbedFooterBuilder();

            footerBuilder.WithText($"Total: {totalCharacters} Characters online");
            return footerBuilder;
        }

        private static EmbedFieldBuilder BuildField(string title, string text)
        {
            EmbedFieldBuilder fieldBuilder = new EmbedFieldBuilder();
            fieldBuilder.WithName(title);
            fieldBuilder.WithValue(text);
            return fieldBuilder;
        }
    }
}
