using MollyPS.GameServer.Game.Mail;
using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Proto;
using MollyPS.GameServer.Game;

namespace MollyPS.GameServer.Game.Mail;
public static class MarkStarMailRequest
{
    private static readonly Logger Logger = new("MarkStarMail");
    public static void Handle(EndfieldConnection connection, CS_MARK_STAR_MAIL cmd)
    {
        var player = connection.Player;
        if (player == null)
            return;
        Logger.Debug($"MarkStarMail: uid={player.Uid}, mail_id={cmd.MailId}, star={cmd.IsStar}");
        var mail = player.Mails.FirstOrDefault(m => m.Id == cmd.MailId);
        if (mail != null)
        {
            mail.IsStar = cmd.IsStar;
            player.SaveToDatabase();
        }

        connection.Send((int)SCMessageID.ScMarkStarMail, new SC_MARK_STAR_MAIL { MailId = cmd.MailId, IsStar = cmd.IsStar, });
    }
}
