using Discord;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System.Net;
using CrypticWizard.RandomWordGenerator;
using static CrypticWizard.RandomWordGenerator.WordGenerator;
using Discord.Commands;
using Discord.WebSocket;
using RobloxNET.Exceptions;
using RobloxNET;

namespace LoravianInternalAffairs.Commands
{
    public static class Verification
    {
        static SocketSlashCommand cmdGlobal;
        static DiscordSocketClient clientGlobal;
        public static async Task VerificationHandler(DiscordSocketClient client, SocketSlashCommand command, LoginData loginData)
        {
            cmdGlobal = command;
            clientGlobal = client;

            clientGlobal.ButtonExecuted += ButtonHandler;
            
            try
            {
                var id = await Roblox.GetIdFromUsername(command.Data.Options.First().Value.ToString());
                var response = await Commands.Verification.CheckIfVerified(id.ToString(), "", loginData.MySqlPassword);

                if (response == true)
                {
                    await command.RespondAsync("You are already verified!");
                }
                else
                {
                    var builder = new ComponentBuilder().WithButton("Done", "verification_phrase_done", ButtonStyle.Success);
                    await command.RespondAsync(embed: Commands.Verification.InitiateVerification().Build(), components: builder.Build());
                }
            }
            catch (InvalidUsernameException)
            {
                await command.RespondAsync("The provided username is invalid!");
            }
        }

        public static async Task<bool> CheckIfVerified(string robloxId, string discordUserId, string mysqlPassword)
        {
            string cs = @"server=localhost;userid=Loraviis;password=" + mysqlPassword + ";database=playerdata";
            using var con = new MySqlConnection(cs);
            con.Open();

            var cmd = new MySqlCommand("SELECT *\r\nFROM verifications\r\nWHERE robloxid LIKE '%" + robloxId + "%';\r\n", con);
            var result = await cmd.ExecuteScalarAsync();
            if (result == null)
            {
                return false;  
            } else
            {
                return true;
            }
        }

        public static EmbedBuilder InitiateVerification()
        {
            WordGenerator myWordGenerator = new WordGenerator();
            string phrase = myWordGenerator.GetWord(PartOfSpeech.noun) + " " + myWordGenerator.GetWord(PartOfSpeech.noun) + " " + myWordGenerator.GetWord(PartOfSpeech.noun) + " " + myWordGenerator.GetWord(PartOfSpeech.noun) + " " + myWordGenerator.GetWord(PartOfSpeech.noun) + " ";

            var embed = new EmbedBuilder()
            {
                Title = "Verification process started!",
                Description = "Please enter the phrase below into your Roblox profile description. \n \n" + "**" + phrase + "**" + "\n \nPress the \"Done\" button once you're finished.",
                Color = new Color(Color.Blue)
            };

            return embed;
        }

        public static async Task ButtonHandler(SocketMessageComponent component)
        {
            switch (component.Data.CustomId)
            {
                case "verification_phrase_done":
                    MessageProperties messageProperties = new MessageProperties()
                    {
                        Content = "Erm..... There's nothing here yet!! Check back later~ >.<",
                        Embed = null,
                        Components = null
                    };
                    await cmdGlobal.ModifyOriginalResponseAsync(x => { x.Content = "Erm..... There's nothing here yet!! Check back later~ >.<"; x.Embed = null; x.Components = null; });
                    break;
            }
        }
    }
}
