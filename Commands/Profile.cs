using Discord;
using Discord.WebSocket;
using MySql.Data.MySqlClient;
using Robloxdotnet;

namespace LoravianInternalAffairs.Commands
{
    public class Profile
    {
        public static async Task ViewProfile(DiscordSocketClient client, SocketSlashCommand command, LoginData loginData)
        {
            var commandOptions = command.Data.Options;
            var server = client.GetGuild((ulong)command.GuildId);
            var user = server.GetUser((ulong)command.User.Id);

            foreach (var option in commandOptions)
            {
                if (option.Type == ApplicationCommandOptionType.User)
                {
                    user = (SocketGuildUser)option.Value;
                    break;
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
            } else
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

                cmd.CommandText = "SELECT experience FROM statistics WHERE robloxid=" + robloxuserresult + ";";
                var experience = await cmd.ExecuteScalarAsync();
                cmd.CommandText = "SELECT resistance FROM statistics WHERE robloxid=" + robloxuserresult + ";";
                var resistance = await cmd.ExecuteScalarAsync();
                cmd.CommandText = "SELECT strength FROM statistics WHERE robloxid=" + robloxuserresult + ";";
                var strength = await cmd.ExecuteScalarAsync();
                cmd.CommandText = "SELECT intelligence FROM statistics WHERE robloxid=" + robloxuserresult + ";";
                var intelligence = await cmd.ExecuteScalarAsync();
                cmd.CommandText = "SELECT agility FROM statistics WHERE robloxid=" + robloxuserresult + ";";
                var agility = await cmd.ExecuteScalarAsync();
                cmd.CommandText = "SELECT charm FROM statistics WHERE robloxid=" + robloxuserresult + ";";
                var charm = await cmd.ExecuteScalarAsync();

                var robloxName = await Roblox.GetUsernameFromId(Convert.ToUInt64(robloxuserresult));
                var robloxThumbnail = await Roblox.GetUserThumbnail(Convert.ToUInt64(robloxuserresult), "420x420", "Png", false, "avatar-headshot");

                EmbedBuilder embed = new EmbedBuilder
                {
                    Title = robloxName + "'s profile",
                    ThumbnailUrl = robloxThumbnail,
                    Color = Color.DarkMagenta,
                    Description = "This is the requested user's profile that applies to all Loravia associated groups. " +
                    "User statistics change based on activities within the community and games."
                };

                embed.AddField("User statistics", ":star: Experience: " + experience + "\n:shield: Resistance: " + resistance + "\n:muscle: Strength: " + strength + 
                    "\n:bulb: Intelligence: " + intelligence + "\n:arrow_right: Agility: " + agility + "\n:revolving_hearts: Charm: " + charm);

                await command.RespondAsync(embed: embed.Build(), ephemeral: true);
            }
        }
    }
}
