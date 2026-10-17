using MollyPS.GameServer.Game.Mail;
using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Proto;
using MollyPS.GameServer.Game;

namespace MollyPS.GameServer.Game.Mail;
public static class DeleteMailRequest
{
    private static readonly Logger Logger = new("DeleteMail");
    public static void Handle(EndfieldConnection connection, CS_DELETE_MAIL cmd)
    {
        var player = connection.Player;
        if (player == null)
            return;
        Logger.Debug($"DeleteMail: uid={player.Uid}, mail_id={cmd.MailId}");
        var response = new SC_DEL_MAIL_NOTIFY();
        var mail = player.Mails.FirstOrDefault(m => m.Id == cmd.MailId);
        if (mail != null)
        {
            player.Mails.Remove(mail);
            player.SaveToDatabase();
            response.MailIdList.Add(cmd.MailId);
        }
        else
        {
            response.MailIdList.Add(cmd.MailId);
        }

        connection.Send((int)SCMessageID.ScDelMailNotify, response);
    }
}
