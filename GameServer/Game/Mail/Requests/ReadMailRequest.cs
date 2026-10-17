using MollyPS.GameServer.Game.Mail;
using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Proto;
using MollyPS.GameServer.Game;

namespace MollyPS.GameServer.Game.Mail;
public static class ReadMailRequest
{
    private static readonly Logger Logger = new("ReadMail");
    public static void Handle(EndfieldConnection connection, CS_READ_MAIL cmd)
    {
        var player = connection.Player;
        if (player == null)
            return;
        Logger.Debug($"ReadMail: uid={player.Uid}, mail_id={cmd.MailId}");
        var mail = player.Mails.FirstOrDefault(m => m.Id == cmd.MailId);
        if (mail != null && !mail.IsRead)
        {
            mail.IsRead = true;
            mail.RedDotRead = true;
            player.SaveToDatabase();
        }

        connection.Send((int)SCMessageID.ScReadMail, new SC_READ_MAIL { MailId = cmd.MailId, });
    }
}
