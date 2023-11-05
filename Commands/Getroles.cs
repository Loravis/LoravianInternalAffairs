﻿using Discord;
using Discord.WebSocket;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using Robloxdotnet;
using System.Data;

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
                await command.RespondAsync(embed: embedBuilder.Build(), ephemeral: true);
            } else
            {

                ulong guildId = (ulong)command.GuildId;
                var server = client.GetGuild(guildId);
                var user = server.GetUser(command.User.Id);

                try
                {
                    cmd = new MySqlCommand("SELECT robloxid FROM verifications WHERE discordid = " + discordUserId + ";", con);
                    var robloxuserresult = await cmd.ExecuteScalarAsync();

                    string robloxNickname = await Roblox.GetUsernameFromId(Convert.ToInt32(robloxuserresult));
                    await user.ModifyAsync(properties => properties.Nickname = robloxNickname);

                    //Add roles
                    string tableName = "server_" + command.GuildId.ToString();
                    string query = $"SELECT * FROM {tableName}";

                    string[,] data;

                    using (MySqlCommand importVerificationTable = new MySqlCommand(query, con))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(importVerificationTable))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            int numRows = dataTable.Rows.Count;
                            int numColumns = dataTable.Columns.Count;

                            // Create a 2D array to store the data
                            data = new string[numRows, numColumns];

                            // Copy data from DataTable to 2D array
                            for (int i = 0; i < numRows; i++)
                            {
                                for (int j = 0; j < numColumns; j++)
                                {
                                    data[i, j] = dataTable.Rows[i][j].ToString();
                                }
                            }
                        }
                    }

                    foreach (var row in data)
                    {
                        Console.WriteLine(row);
                    }

                    var embedBuilder = new EmbedBuilder()
                    {
                        Title = "Updating succeeded!",
                        Description = "Your roles and nickname have been updated successfully!",
                        Color = new Color(Color.Green)
                    };
                    await command.RespondAsync(embed: embedBuilder.Build(), ephemeral: true);

                } catch (Exception ex)
                {

                    Console.WriteLine(ex);
                    var embedBuilder = new EmbedBuilder()
                    {
                       Title = "Updating failed!",
                       Description = "I do not have permission to update the user!",
                       Color = new Color(Color.Red)
                    };
                    await command.RespondAsync(embed: embedBuilder.Build(), ephemeral: true);
                }
                
            }
            con.Close();
        }
    }
}
