using Discord;
using Discord.WebSocket;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using Robloxdotnet;

namespace LoravianInternalAffairs.Commands
{
    public class Getroles
    {
        public static async Task UpdateUserRoles(DiscordSocketClient client, SocketSlashCommand command, LoginData loginData)
        {
            string mysqlPassword = loginData.MySqlPassword;
            string discordUserId = command.User.Id.ToString();
            string cs = @"server=localhost;userid=Loraviis;password=" + mysqlPassword + ";database=playerdata";
            using var con = new MySqlConnection(cs);
            con.Open();

            var cmd = new MySqlCommand("SELECT *\r\nFROM verifications\r\nWHERE discordid LIKE '%" + discordUserId + "%';\r\n", con);
            var result = await cmd.ExecuteScalarAsync();

            if (result == null)
            {
                var embedBuilder = new EmbedBuilder()
                {
                    Title = "Updating failed!",
                    Description = "You are currently not verified! Please type **/verify** to start!",
                    Color = new Color(Color.Red)
                };
                await command.RespondAsync(embed: embedBuilder.Build());
            } else
            {

                ulong guildId = (ulong)command.GuildId;
                var server = client.GetGuild(guildId);
                var user = server.GetUser(command.User.Id);

                try
                {
                    await user.ModifyAsync(properties => properties.Nickname = "NewNickname");

                    var embedBuilder = new EmbedBuilder()
                    {
                        Title = "Updating succeeded!",
                        Description = "Your roles and nickname have been updated successfully!",
                        Color = new Color(Color.Green)
                    };
                    await command.RespondAsync(embed: embedBuilder.Build());

                } catch (Exception ex)
                {
                    var embedBuilder = new EmbedBuilder()
                    {
                        Title = "Updating failed!",
                        Description = "I do not have permission to update the user!",
                        Color = new Color(Color.Red)
                    };
                    await command.RespondAsync(embed: embedBuilder.Build());
                }
                
            }
        }
    }
}
