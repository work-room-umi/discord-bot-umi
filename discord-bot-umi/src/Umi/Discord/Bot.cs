using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Umi.Discord.Command;

namespace Umi.Discord
{
    public class Bot
    {
        public Bot()
        {
            _client = new DiscordSocketClient();
            _client.Log += Log;
            _client.Ready += ClientReady;
        }

        public event Func<Task>? Ready;

        public async Task Start()
        {
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();
        }

        public string? Token
        {
            get { return _token; }
            set { _token = value; }
        }

        public bool IsReady
        {
            get { return _isReady; }
        }

        public DiscordSocketClient Client
        {
            get { return _client; }
        }

        public void RegisterCommand(ICommand command)
        {
            _commandUnregistered.Add(command);
            RegisterCommand();
        }

        private readonly DiscordSocketClient _client;
        private string? _token;
        private readonly List<ICommand> _commands = [];
        private readonly List<ICommand> _commandUnregistered = [];
        private bool _isReady = false;

        private async Task ClientReady()
        {
            if (Ready != null)
            {
                await Ready.Invoke();
            }
            _isReady = true;
            RegisterCommand();
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        void RegisterCommand()
        {
            if (!IsReady)
            {
                return;
            }
            try
            {
                var guildIdS = Environment.GetEnvironmentVariable("DISCORD_GUILD_ID");
                SocketGuild? guild = null;
                if (ulong.TryParse(guildIdS, out var guildId))
                {
                    _client.GetGuild(guildId);
                }
                _commandUnregistered.ForEach((c) =>
                {
                    guild?.CreateApplicationCommandAsync(c.GetCommand());
                    _client.SlashCommandExecuted += c.CommandHandler;
                    _commands.Add(c);
                });
            }
            catch (HttpException e)
            {
                var json = JsonConvert.SerializeObject(e.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }
        }
    }
}
