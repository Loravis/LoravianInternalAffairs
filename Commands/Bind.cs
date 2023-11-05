using Discord.WebSocket;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using Discord;

namespace LoravianInternalAffairs.Commands
{
    public class Bind
    {
        public static async Task ManageRoleBinds(DiscordSocketClient client, SocketSlashCommand command, LoginData loginData)
        {
            string option = command.Data.Options.First().Name;
            if (option == "add")
            {
                await AddRoleBind(client, command, loginData);
            } else if (option == "remove")
            {
                await RemoveRoleBind(client, command, loginData);
            }
        }

        private static async Task AddRoleBind(DiscordSocketClient client, SocketSlashCommand command, LoginData loginData)
        {
            string mysqlPassword = loginData.MySqlPassword;
            string cs = @"server=localhost;userid=Loraviis;password=" + mysqlPassword + ";database=playerdata";
            using var con = new MySqlConnection(cs);
            con.Open();

            var createTableIfNotExists = new MySqlCommand("CREATE TABLE IF NOT EXISTS server_" + command.GuildId.ToString() + " ( roleid VARCHAR(255), groupid VARCHAR(255), grouprank VARCHAR(255));", con);
            await createTableIfNotExists.ExecuteScalarAsync();

            var commandOptions = command.Data.Options.First().Options;
            List<long> bindData = new List<long>();

            foreach(var option in commandOptions)
            {
                if (option.Type == ApplicationCommandOptionType.Role)
                {
                    SocketRole roleId = (SocketRole)option.Value;
                    bindData.Add((long)roleId.Id);
                } else
                {
                    bindData.Add((long)option.Value);
                }
            }

            long[] bindDataArray = bindData.ToArray();

            MySqlCommand addBindToTable;
            if (bindDataArray.Length < 3)
            {
                addBindToTable = new MySqlCommand("SELECT *\r\nFROM server_" + command.GuildId.ToString() + "\r\n" +
                    "WHERE roleid = " + bindDataArray[0].ToString() + " " +
                    "AND groupid = " + bindDataArray[1].ToString() + ";\r\n", 
                    con);
            } else
            {
                addBindToTable = new MySqlCommand("SELECT *\r\nFROM server_" + command.GuildId.ToString() + "\r\n" +
                    "WHERE roleid = " + bindDataArray[0].ToString() + " AND groupid = " + bindDataArray[1].ToString() + " " +
                    "AND grouprank = " + bindDataArray[2].ToString() + ";\r\n", 
                    con);
            }

            var result = await addBindToTable.ExecuteReaderAsync();
            if (result.Read() == false)
            {
                await result.CloseAsync();
                if (bindDataArray.Length < 3)
                {
                    addBindToTable.CommandText = "INSERT INTO server_" + command.GuildId.ToString() + " (roleid, groupid, grouprank)\r\n" +
                        "VALUES (" + bindDataArray[0] + ", " + bindDataArray[1]+ ", '');";
                } else
                {
                    addBindToTable.CommandText = "INSERT INTO server_" + command.GuildId.ToString() + " (roleid, groupid, grouprank)\r\n" +
                        "VALUES (" + bindDataArray[0] + ", " + bindDataArray[1] + ", " + bindDataArray[2] + ");";
                }
                await addBindToTable.ExecuteScalarAsync();

                await command.RespondAsync("Erm.... There's nothing here yet!! Check back later~ >.<");
            } else
            {
                await result.CloseAsync();
                Console.WriteLine(result.ToString());
                var embedBuilder = new EmbedBuilder()
                {
                    Title = "Bind error",
                    Description = "The role bind already exists!",
                    Color = new Color(Color.Red)
                };
                await command.RespondAsync(embed: embedBuilder.Build());
            }
            
            con.Close();
        }

        private static async Task RemoveRoleBind(DiscordSocketClient client, SocketSlashCommand command, LoginData loginData)
        {
            string mysqlPassword = loginData.MySqlPassword;
            string cs = @"server=localhost;userid=Loraviis;password=" + mysqlPassword + ";database=playerdata";
            using var con = new MySqlConnection(cs);
            con.Open();

            try
            {
                var commandOptions = command.Data.Options.First().Options;
                List<long> bindData = new List<long>();

                foreach (var option in commandOptions)
                {
                    if (option.Type == ApplicationCommandOptionType.Role)
                    {
                        SocketRole roleId = (SocketRole)option.Value;
                        bindData.Add((long)roleId.Id);
                    }
                    else
                    {
                        bindData.Add((long)option.Value);
                    }
                }

                long[] bindDataArray = bindData.ToArray();

                MySqlCommand removeBindFromTable;
                if (bindDataArray.Length < 3)
                {
                    removeBindFromTable = new MySqlCommand("DELETE FROM server_" + command.GuildId.ToString() + "\r\n" +
                        "WHERE roleid = " + bindDataArray[0].ToString() + " " +
                        "AND groupid = " + bindDataArray[1].ToString() + ";\r\n",
                        con);
                }
                else
                {
                    removeBindFromTable = new MySqlCommand("DELETE FROM server_" + command.GuildId.ToString() + "\r\n" +
                        "WHERE roleid = " + bindDataArray[0].ToString() + " AND groupid = " + bindDataArray[1].ToString() + " " +
                        "AND grouprank = " + bindDataArray[2].ToString() + ";\r\n",
                        con);
                }

                var result = await removeBindFromTable.ExecuteNonQueryAsync();
                
                if (result == 0)
                {
                    throw new Exception("Bind does not exist!");
                }

                var embedBuilder = new EmbedBuilder()
                {
                    Title = "Bind removed successfully!",
                    Description = "The bind has successfully been deleted!",
                    Color = new Color(Color.Green)
                };
                await command.RespondAsync(embed: embedBuilder.Build());

            } catch (Exception)
            {
                var embedBuilder = new EmbedBuilder()
                {
                    Title = "Bind error",
                    Description = "The requested bind does not exist.",
                    Color = new Color(Color.Red)
                };
                await command.RespondAsync(embed: embedBuilder.Build());
            }
        }
    }
}
