using System;
using RobloxNET;
using dotenv.net;
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
        public async Task MainAsync()
        {
            string loginDataJSON = File.ReadAllText("./appsettings.env.json");
            LoginData loginData = JsonConvert.DeserializeObject<LoginData>(loginDataJSON); 

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
                case "hello":
                   await command.RespondAsync("Hello " + command.User.GlobalName + "!");
                   break;
                case "string":
                    await command.RespondAsync(command.Data.Options.First().Value.ToString());
                    break;
            }

        }

        public async Task ClientReady()
        {
            var globalCommand = new SlashCommandBuilder();
            globalCommand.WithName("string");
            globalCommand.WithDescription("Make the bot return a string");
            globalCommand.AddOption("user", ApplicationCommandOptionType.String, "The string you want to return!", isRequired: true);

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
    }
}