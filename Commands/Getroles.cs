using Discord;
using Discord.WebSocket;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using Robloxdotnet;
using Robloxdotnet.Utilities.Groups;
using Robloxdotnet.Utilities.Users;
using System.Data;
using System.Security.Cryptography.Xml;

namespace LoravianInternalAffairs.Commands
{
    public class Getroles
    {
        public static async Task UpdateUserRoles(DiscordSocketClient client, SocketSlashCommand command, LoginData loginData, bool force)
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
                EmbedBuilder embedBuilder;

                if (force)
                {
                    embedBuilder = new EmbedBuilder()
                    {
                        Title = "Updating failed!",
                        Description = "The target user is not verified! Please ask them to type **/verify**. ",
                        Color = new Color(Color.Red)
                    };
                } else
                {
                    embedBuilder = new EmbedBuilder()
                    {
                        Title = "Updating failed!",
                        Description = "You are currently not verified! Please type **/verify** to start!",
                        Color = new Color(Color.Red)
                    };
                }

                
                await command.RespondAsync(embed: embedBuilder.Build(), ephemeral: true);
            } else
            {

                ulong guildId = (ulong)command.GuildId;
                var server = client.GetGuild(guildId);
                SocketGuildUser user = server.GetUser(command.User.Id);

                if (force)
                {
                    var commandOptions = command.Data.Options;

                    foreach (var option in commandOptions)
                    {
                        if (option.Type == ApplicationCommandOptionType.User)
                        {
                            user = (SocketGuildUser)option.Value;
                            break;
                        }
                    }

                } 
                
                try
                {
                    cmd = new MySqlCommand("SELECT robloxid FROM verifications WHERE discordid = " + discordUserId + ";", con);
                    var robloxuserresult = await cmd.ExecuteScalarAsync();

                    string robloxNickname = await Roblox.GetUsernameFromId(Convert.ToUInt64(robloxuserresult));
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
                    UserGroupInfo userGroupInfo = await Robloxdotnet.Utilities.Groups.MemberManagement.GetUserGroupInfo(Convert.ToUInt64(robloxuserresult));

                    List<ulong> addedRolesList = new List<ulong>();
                    List<ulong> noRemoveRoles = new List<ulong>();
                    List<ulong> removedRolesList = new List<ulong>();
                    string addedRolesString = "";
                    string removedRolesString = "";

                    for (int i = 0; i < data.GetLength(0); i++ )
                    {
                        if (user.Roles.Contains(server.GetRole(Convert.ToUInt64(data[i, 0]))) == false && addedRolesList.Contains(Convert.ToUInt64(data[i, 0])) == false)
                        {
                            foreach (UserGroupData ugi in userGroupInfo.data) //Data is an object from Robloxdotnet's UserGroupInfo.cs
                            {
                                if (data[i, 2] != String.Empty)
                                {
                                    if (ugi.group.id == Convert.ToUInt64(data[i, 1]) && ugi.role.rank == Convert.ToUInt64(data[i, 2]))
                                    {
                                        await user.AddRoleAsync(server.GetRole(Convert.ToUInt64(data[i, 0])));
                                        addedRolesList.Add(Convert.ToUInt64(data[i, 0]));
                                        break;
                                    } 
                                } else if (ugi.group.id == Convert.ToUInt64(data[i, 1]))
                                {
                                    await user.AddRoleAsync(server.GetRole(Convert.ToUInt64(data[i, 0])));
                                    addedRolesList.Add(Convert.ToUInt64(data[i, 0]));
                                    break;
                                }
                            }
                        } else if (user.Roles.Contains(server.GetRole(Convert.ToUInt64(data[i, 0]))) 
                            && noRemoveRoles.Contains(Convert.ToUInt64(data[i, 0])) == false 
                            && removedRolesList.Contains(Convert.ToUInt64(data[i, 0])) == false)
                        {

                            bool remove = true;
                            foreach (UserGroupData ugi in userGroupInfo.data)
                            {
                                if (data[i, 2] != String.Empty)
                                {
                                    if (ugi.group.id == Convert.ToUInt64(data[i, 1]) && ugi.role.rank == Convert.ToUInt64(data[i, 2]))
                                    {
                                        remove = false;
                                        break;
                                    }
                                } else
                                {
                                    if (ugi.group.id == Convert.ToUInt64(data[i, 1]))
                                    {
                                        remove = false;
                                        break;
                                    }
                                }
                            }

                            if (remove)
                            {
                                await user.RemoveRoleAsync(server.GetRole(Convert.ToUInt64(data[i, 0])));
                                removedRolesList.Add(Convert.ToUInt64(data[i, 0]));
                            } else
                            {
                                noRemoveRoles.Add(Convert.ToUInt64(data[i, 0]));
                            }
                        }
                    }

                    ulong[] addedRolesArray = addedRolesList.ToArray();
                    ulong[] removedRolesArray = removedRolesList.ToArray();

                    foreach (ulong addedRole in addedRolesArray)
                    {
                        addedRolesString = addedRolesString + "<@&" + addedRole + ">\n";
                    }

                    foreach (ulong removedRole in removedRolesArray)
                    {
                        removedRolesString = removedRolesString + "<@&" + removedRole + ">\n";
                    }

                    var embedBuilder = new EmbedBuilder()
                    {
                        Title = "Updating succeeded!",
                        Description = "Roles and nickname have been updated successfully!",
                        Color = new Color(Color.Green)
                    };

                    if (addedRolesString == "")
                    {
                        addedRolesString = "No roles were added!";
                    }
                    if (removedRolesString == "")
                    {
                        removedRolesString = "No roles were removed!";
                    }

                    embedBuilder.AddField(new EmbedFieldBuilder()
                    {
                        Name = "Added roles:",
                        Value = "\n" + addedRolesString
                    } );
                    embedBuilder.AddField(new EmbedFieldBuilder()
                    {
                        Name = "Removed roles:",
                        Value = "\n" + removedRolesString
                    });
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
