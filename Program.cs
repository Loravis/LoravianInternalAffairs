using System;
using RobloxNET;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Discord.Net;
using Discord.Commands;
using RobloxNET.Exceptions;

namespace LoravianInternalAffairs
{
    class Program
    {
        public static Task Main(string[] args) => new Program().MainAsync();

        private DiscordSocketClient client;
        private LoginData loginData;
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
            switch (command.Data.Name)
            {
                case "verify":
                    try
                    {
                        var id = await Roblox.GetIdFromUsername(command.Data.Options.First().Value.ToString());
                        var response = await Commands.Verification.CheckIfVerified(id.ToString(), "", loginData.MySqlPassword);

                        if (response == true)
                        {
                            await command.RespondAsync("You are already verified!");
                        } else
                        {
                            await command.RespondAsync(Commands.Verification.InitiateVerification());
                        }
                    } catch (InvalidUsernameException)
                    {
                        await command.RespondAsync("The provided username is invalid!");
                    }

                    
                    break;
            }

        }

        public async Task ClientReady()
        {
            var globalCommand = new SlashCommandBuilder();
            globalCommand.WithName("verify");
            globalCommand.WithDescription("Verify your Roblox account");
            globalCommand.AddOption("username", ApplicationCommandOptionType.String, "Your Roblox username", isRequired: true);

            try
            {
                await client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
            } catch (HttpException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    public class LoginData
    {
        public string DiscordBotToken { get; set; }
        public string MySqlPassword { get; set; }
    }
}