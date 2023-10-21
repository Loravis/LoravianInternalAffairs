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
        public SocketSlashCommand cmdGlobal;
        public async Task MainAsync()
        {
            string loginDataJSON = File.ReadAllText("./appsettings.env.json");
            loginData = JsonConvert.DeserializeObject<LoginData>(loginDataJSON); 

            client = new DiscordSocketClient();
            client.Log += Log;
            client.Ready += ClientReady;
            client.SlashCommandExecuted += SlashCommandHandler;
            client.ButtonExecuted += ButtonHandler;
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
                    await Commands.Verification.VerificationHandler(command, loginData);
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

        public async Task ButtonHandler(SocketMessageComponent component)
        {
            switch (component.Data.CustomId)
            {
                case "verification_phrase_done":
                    MessageProperties messageProperties = new MessageProperties()
                    {
                        Content = "Erm..... There's nothing here yet!! Check back later~ >.<",
                        Embed = null,
                        Components = null
                    };
                    await cmdGlobal.ModifyOriginalResponseAsync(x => { x.Content = "Erm..... There's nothing here yet!! Check back later~ >.<"; x.Embed = null; x.Components = null; }) ;
                    break;
            }
        }
    }

    public class LoginData
    {
        public string DiscordBotToken { get; set; }
        public string MySqlPassword { get; set; }
    }
}