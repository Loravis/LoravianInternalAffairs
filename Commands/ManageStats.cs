using Discord;
using Discord.WebSocket;
using MySql.Data.MySqlClient;
using Robloxdotnet;
using System.Net.NetworkInformation;

namespace LoravianInternalAffairs.Commands
{
    public class ManageStats
    {
        public static async Task ManageStatsHandler(DiscordSocketClient client, SocketSlashCommand command, LoginData data)
        {
            string option = command.Data.Options.First().Name;
            if (option == "add")
            {
                await AddStats(client, command, data);
            }
        }
        public static async Task AddStats(DiscordSocketClient client, SocketSlashCommand command, LoginData loginData)
        {
            var commandOptions = command.Data.Options.First().Options;
            var server = client.GetGuild((ulong)command.GuildId);
            SocketUser user = server.GetUser(command.User.Id);
            string stat = "";
            long amount = 0;

            foreach (var option in commandOptions)
            {
                if (option.Type == ApplicationCommandOptionType.String)
                {
                    stat = (string)option.Value;
                    Console.WriteLine(stat);
                } else if (option.Type == ApplicationCommandOptionType.Integer)
                {
                    amount = (long)option.Value;
                    Console.WriteLine(amount);
                } else if (option.Type == ApplicationCommandOptionType.User)
                {
                    Console.WriteLine("Hello moewmeow");
                    user = (SocketUser)option.Value; 
                }
            }

            string mysqlPassword = loginData.MySqlPassword;
            string cs = @"server=localhost;userid=Loraviis;password=" + mysqlPassword + ";database=playerdata";
            using var con = new MySqlConnection(cs);
            con.Open();

            var cmd = new MySqlCommand("SELECT *\r\nFROM verifications\r\nWHERE discordid LIKE '%" + Convert.ToString(user.Id) + "%';\r\n", con);
            var result = await cmd.ExecuteScalarAsync();

            if (result == null)
            {
                EmbedBuilder embedBuilder = new EmbedBuilder()
                {
                    Title = "Profile error",
                    Description = "The targetted user is not verified! Please verify using **/verify**.",
                    Color = new Color(Color.Red)
                };

                await command.RespondAsync(embed: embedBuilder.Build(), ephemeral: true);
            }
            else
            {
                cmd = new MySqlCommand("SELECT robloxid FROM verifications WHERE discordid = " + Convert.ToString(user.Id) + ";", con);
                var robloxuserresult = await cmd.ExecuteScalarAsync();

                cmd = new MySqlCommand("SELECT * FROM statistics WHERE robloxid LIKE '%" + robloxuserresult + "%';", con);
                var statisticsresult = await cmd.ExecuteScalarAsync();

                if (statisticsresult == null)
                {
                    cmd = new MySqlCommand("INSERT INTO statistics (robloxid, experience, resistance, strength, intelligence, agility, charm) VALUES ('" + robloxuserresult + "', 0, 0, 0, 0, 0, 0);", con);
                    await cmd.ExecuteScalarAsync();
                }

                cmd.CommandText = "SELECT " + stat + " FROM statistics WHERE robloxid=" + robloxuserresult + ";";
                int statValue = (int)await cmd.ExecuteScalarAsync();

                cmd = new MySqlCommand("UPDATE statistics SET " + stat + " = " + Convert.ToString(statValue + amount) + " WHERE robloxid LIKE '%" + robloxuserresult + "%';", con);
                await cmd.ExecuteScalarAsync();

                await command.RespondAsync("Hello", ephemeral: true);
            }
        }
    }
}