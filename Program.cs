using System;
using Robloxdotnet;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Discord.Net;
using Discord.Commands;

namespace LoravianInternalAffairs
{
    class Program
    {
        public static Task Main(string[] args) => new Program().MainAsync();

        private DiscordSocketClient client;
        private LoginData loginData;
        public SocketSlashCommand cmdGlobal;
        public async Task MainAsync()
        {
            string loginDataJSON = File.ReadAllText("./appsettings.env.json");
            loginData = JsonConvert.DeserializeObject<LoginData>(loginDataJSON); 

            client = new DiscordSocketClient();
            client.Log += Log;
            client.Ready += ClientReady;
            client.SlashCommandExecuted += SlashCommandHandler;
            var token = loginData.DiscordBotToken;
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            cmdGlobal = command;

            switch (command.Data.Name)
            {
                case "verify":
                    await Commands.Verification.VerificationHandler(client, command, loginData);
                    break;
                case "getroles":
                    await Commands.Getroles.UpdateUserRoles(client, command, loginData);
                    break;
                case "bind":
                    await Commands.Bind.ManageRoleBinds(client, command, loginData);
                    break;
            }

        }

        public async Task ClientReady()
        {
            List<ApplicationCommandProperties> applicationCommandProperties = new();
            try
            {
                var verifyCommand = new SlashCommandBuilder();
                verifyCommand.WithName("verify");
                verifyCommand.WithDescription("Verify your Roblox account");
                verifyCommand.AddOption("username", ApplicationCommandOptionType.String, "Your Roblox username", isRequired: true);
                applicationCommandProperties.Add(verifyCommand.Build());

                var getrolesCommand = new SlashCommandBuilder();
                getrolesCommand.WithName("getroles");
                getrolesCommand.WithDescription("Get your roles.");
                applicationCommandProperties.Add(getrolesCommand.Build());

                var bindCommand = new SlashCommandBuilder();
                bindCommand.WithName("bind");
                bindCommand.WithDescription("Bind a role to a group or group rank (ADMIN ONLY).");
                bindCommand.WithDefaultMemberPermissions(GuildPermission.Administrator);
                var bindOption = new SlashCommandOptionBuilder()
                    .WithName("add")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .WithDescription("Add a new group role bind. (ADMIN ONLY)")
                    .AddOption("role", ApplicationCommandOptionType.Role, "The role you are looking to bind", isRequired: true)
                    .AddOption("groupid", ApplicationCommandOptionType.Integer, "The group you are looking to bind the role to", isRequired: true)
                    .AddOption("rank", ApplicationCommandOptionType.Integer, "The group rank (0-255) you are looking to bind the role to. Empty = bind the entire group.", isRequired: false);

                var unbindOption = new SlashCommandOptionBuilder()
                    .WithName("remove")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .WithDescription("Remove an existing role bind. (ADMIN ONLY)")
                    .AddOption("role", ApplicationCommandOptionType.Role, "The role that you are looking to unbind", isRequired: true)
                    .AddOption("groupid", ApplicationCommandOptionType.Integer, "The group that the role is bound to", isRequired: true)
                    .AddOption("rank", ApplicationCommandOptionType.Integer, "The group rank (0-255) that the role is bound to. Empty = bind the entire group.", isRequired: false);

                bindCommand.AddOption(bindOption);
                bindCommand.AddOption(unbindOption);
                applicationCommandProperties.Add(bindCommand.Build());

                await client.BulkOverwriteGlobalApplicationCommandsAsync(applicationCommandProperties.ToArray());
                
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

    }

    public class LoginData
    {
        public string DiscordBotToken { get; set; }
        public string MySqlPassword { get; set; }
        public string ROBLOSECURITY { get; set; }
    }
}