using System;
using RobloxNET;
using dotenv.net;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Discord.Net;

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

        public async Task ClientReady()
        {
            if (File.Exists("./commands.txt") == false)
            {
                await Task.Run(() => File.Create("./commands.txt"));
            }
        }

        public async Task CreateNewSlashCommand(string name, string desc)
        {
            var globalCommand = new SlashCommandBuilder();
            globalCommand.WithName("hello");
            globalCommand.WithDescription("Make the bot say hello!");

            try
            {
                Console.WriteLine("Creating command....");
                await client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
            } catch (HttpException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}