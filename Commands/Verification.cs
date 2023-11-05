using Discord;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System.Net;
using CrypticWizard.RandomWordGenerator;
using static CrypticWizard.RandomWordGenerator.WordGenerator;
using Discord.Commands;
using Discord.WebSocket;
using Robloxdotnet.Exceptions;
using Robloxdotnet;
using System.Text.RegularExpressions;

namespace LoravianInternalAffairs.Commands
{
    public static class Verification
    {
        static SocketSlashCommand cmdGlobal;
        static DiscordSocketClient clientGlobal;
        static int robloxIdGlobal;
        static string verificationPhrase;
        static string mysqlPasswordGlobal;
        public static async Task VerificationHandler(DiscordSocketClient client, SocketSlashCommand command, LoginData loginData)
        {
            cmdGlobal = command;
            clientGlobal = client;

            clientGlobal.ButtonExecuted += ButtonHandler;
            
            try
            {
                var id = await Roblox.GetIdFromUsername(command.Data.Options.First().Value.ToString());
                
                var builder = new ComponentBuilder().WithButton("Done", "verification_phrase_done", ButtonStyle.Success);
                await command.RespondAsync(embed: Commands.Verification.InitiateVerification().Build(), components: builder.Build());
            }
            catch (InvalidUsernameException)
            {
                await command.RespondAsync("The provided username is invalid!");
            }
        }

        public static EmbedBuilder InitiateVerification()
        {
            WordGenerator myWordGenerator = new WordGenerator();
            verificationPhrase = myWordGenerator.GetWord(PartOfSpeech.noun) + " " + myWordGenerator.GetWord(PartOfSpeech.noun) + " " + myWordGenerator.GetWord(PartOfSpeech.noun) + " " + myWordGenerator.GetWord(PartOfSpeech.noun) + " " + myWordGenerator.GetWord(PartOfSpeech.noun) + " ";

            var embed = new EmbedBuilder()
            {
                Title = "Verification process started",
                Description = "Please enter the phrase below into your Roblox profile description. \n \n" + "**" + verificationPhrase + "**" + "\n \nPress the \"Done\" button once you're finished.",
                Color = new Color(Color.Blue)
            };

            return embed;
        }

        public static async Task ButtonHandler(SocketMessageComponent component)
        {
            switch (component.Data.CustomId)
            {
                case "verification_phrase_done":
                    string description = await Robloxdotnet.Roblox.GetUserDescription(robloxIdGlobal);

                    if (GetAlphabeticalLetters(description).Contains(GetAlphabeticalLetters(verificationPhrase)))
                    {
                        try
                        {
                            string cs = @"server=localhost;userid=Loraviis;password=" + mysqlPasswordGlobal + ";database=playerdata";
                            using var con = new MySqlConnection(cs);
                            con.Open();

                            var checkIfVerified = new MySqlCommand("SELECT *\r\nFROM verifications\r\nWHERE robloxid LIKE '%" + robloxIdGlobal + "%';\r\n", con);
                            var result = await checkIfVerified.ExecuteScalarAsync();
                            if (result != null)
                            {
                                checkIfVerified = new MySqlCommand("DELETE FROM verifications WHERE robloxid = " + robloxIdGlobal + ";", con);
                                await checkIfVerified.ExecuteScalarAsync();
                            }

                            var verificationCmd = new MySqlCommand("INSERT INTO verifications (robloxid, discordid) VALUES ('" + robloxIdGlobal.ToString() + "', '" + cmdGlobal.User.Id.ToString() + "');\r\n");
                            verificationCmd.Connection = con;
                            await verificationCmd.ExecuteNonQueryAsync();

                            var embed = new EmbedBuilder()
                                {
                                    Title = "Verification success",
                                    Description = "You've successfully verified! You may now use **/getroles** to get your group roles! \n\n" +
                                    "If you wish to verify with a different account, verify again using **/verify**.",
                                    Color = new Color(Color.Green)
                                };

                            await cmdGlobal.ModifyOriginalResponseAsync(x => {
                                x.Content = String.Empty;
                                x.Embed = embed.Build();
                                x.Components = null;
                            });
                        } catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            await cmdGlobal.ModifyOriginalResponseAsync(x => {
                                x.Content = "There was an error completing the verification. Please try again and contact @loravis if this error persists.";
                                x.Embed = null;
                                x.Components = null;
                            });
                        }
                        
                    } else
                    {
                        var embed = new EmbedBuilder()
                        {
                            Title = "Verification failed",
                            Description = "The verification phrase could not be found in the user's description! \n\n" +
                            "Ensure you enter the __correct__ username and paste the verification phrase into your Roblox profile description before continuing!",
                            Color = new Color(Color.Red)
                        };

                        await cmdGlobal.ModifyOriginalResponseAsync(x => { 
                            x.Content = String.Empty; 
                            x.Embed = embed.Build(); 
                            x.Components = null; 
                        });

                    }

                    break;
            }
        }

        public static string GetAlphabeticalLetters(string input)
        {
            string result = "";

            foreach (char character in input)
            {
                if (char.IsLetter(character))
                {
                    result += character;
                }
            }

            return result;
        }
    }
}
