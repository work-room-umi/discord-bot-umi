using System.Text;
using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Umi.Discord.Command
{
    public class TodaysUmiCommand : InteractionModuleBase<SocketInteractionContext>, ICommand
    {
        public TodaysUmiCommand(DiscordSocketClient client)
        {
            _client = client;
            random = new Random();
            urlRegex = new Regex(
                "(https://vrchat\\.com/home/world/(wrld_[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}))",
                RegexOptions.IgnoreCase,
                TimeSpan.FromMicroseconds(1000));
            WorldParameter = Environment.GetEnvironmentVariable("WORLD_PARAMETER") ?? "";
        }

        public ulong ChannelId { get; set; }

        public string CommandName { get; } = "todaysumi";
        public string WorldParameter { get; private set; }

        public async Task CommandHandler(SocketSlashCommand command)
        {
            await command.RespondAsync(await CommandHandlerImpl(command));
        }

        public async Task<string> CommandHandlerImpl(SocketSlashCommand command)
        {
            var ch = _client.GetChannel(ChannelId) as IMessageChannel;
            if (ch == null)
            {
                return $"指定されたチャンネルが見つかりませんでした: {ChannelId}";
            }
            var allMessages = new List<IMessage>();
            IMessageChannel channel = ch;
            var msgs = await channel.GetMessagesAsync().FlattenAsync();
            allMessages.AddRange(msgs);

            while (msgs.Any())
            {
                var omi = msgs.Last().Id;
                msgs = await channel.GetMessagesAsync(omi, Direction.Before).FlattenAsync();
                allMessages.AddRange(msgs);
            }
            var wids = allMessages.Select(msg =>
            {
                return urlRegex.Match(msg.Content);
            })
            .Where(m => m.Success)
            .Select(m =>
            {
                return m.Groups[2].Value;
            })
            .Distinct()
            .ToList();
            if(wids == null)
            {
                return "ワールドが見つかりませんでした";
            }
            if (DateTime.Now.DayOfYear % 3 == 0)
            {
                Console.WriteLine("DeepBlue chance day!");
                Console.WriteLine($"Worlds count {wids.Count}");
                var c = wids.Count * 3;
                for (int i = 0; i < c; i++)
                {
                    wids.Add("wrld_f7a383bc-c925-4696-85c2-2996c0a40112");
                }
            }
            Console.WriteLine($"Worlds count {wids.Count}");
            var n = random.Next(wids.Count);
            var wid = wids[n];
            var link = new StringBuilder("https://vrchat.com/home/launch?worldId=");
            link.Append(wid);
            link.Append("&instanceId=");
            link.Append(random.Next(99999));
            if (!string.IsNullOrEmpty(WorldParameter))
            {
                link.Append($"{WorldParameter}");
            }
            return link.ToString();
        }

        public ApplicationCommandProperties GetCommand()
        {
            return new SlashCommandBuilder()
                .WithName(CommandName)
                .WithDescription("今日の海を選びます")
                .WithIntegrationTypes(ApplicationIntegrationType.UserInstall)
                .Build();
        }

        DiscordSocketClient _client;
        readonly Random random;
        readonly Regex urlRegex;
    }
}
