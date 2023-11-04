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

                await client.BulkOverwriteGlobalApplicationCommandsAsync(applicationCommandProperties.ToArray());
                
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
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