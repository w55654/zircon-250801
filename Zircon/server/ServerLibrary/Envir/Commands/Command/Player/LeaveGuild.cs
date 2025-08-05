using Server.Models;

namespace Server.Envir.Commands.Command.Player
{
    internal class LeaveGuild : AbstractCommand<IPlayerCommand>
    {
        public override string VALUE => "LEAVEGUILD";

        public override void Action(PlayerObject player)
        {
            player.GuildLeave();
        }
    }
}