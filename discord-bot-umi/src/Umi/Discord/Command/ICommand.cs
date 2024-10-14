using Discord;
using Discord.WebSocket;

namespace Umi.Discord.Command
{
    public interface ICommand
    {
        public ApplicationCommandProperties GetCommand();
        public string CommandName { get; }
        public Task CommandHandler(SocketSlashCommand command);
    }
}
