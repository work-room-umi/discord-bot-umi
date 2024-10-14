using Umi.Discord;
using Umi.Discord.Command;

var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
var worldsChannelIdS = Environment.GetEnvironmentVariable("DISCORD_WORLDS_CHANNEL_ID");
if (!ulong.TryParse(worldsChannelIdS, out var worldsChannelId))
{
    throw new ArgumentException($"Environment variable 'DISCORD_WORLDS_CHANNEL_ID' is invalid: '{worldsChannelIdS}'");
}
if (token == null)
{
    throw new ArgumentException("Environment variable 'DISCORD_TOKEN' is not set");
}
Bot bot = new Bot
{
    Token = token
};

var tuc = new TodaysUmiCommand(bot.Client);
tuc.ChannelId = worldsChannelId;
bot.RegisterCommand(tuc);

await bot.Start();

await Task.Delay(-1);
