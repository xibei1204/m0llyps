using MollyPS.GameServer.Game.Mail;
using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Proto;
using MollyPS.GameServer.Game;

namespace MollyPS.GameServer.Game.Mail;
public static class DeleteAllMailRequest
{
    private static readonly Logger Logger = new("DeleteAllMail");
    public static void Handle(EndfieldConnection connection, CS_DELETE_ALL_MAIL cmd)
    {
        var player = connection.Player;
        if (player == null)
            return;
        Logger.Debug($"DeleteAllMail: uid={player.Uid}");
         
        var toDelete = player.Mails.Where(m => m.IsRead && !m.IsStar).ToList();
        var response = new SC_DEL_MAIL_NOTIFY();
        foreach (var mail in toDelete)
        {
            player.Mails.Remove(mail);
            response.MailIdList.Add(mail.Id);
        }

        if (toDelete.Count > 0)
        {
            player.SaveToDatabase();
        }

        connection.Send((int)SCMessageID.ScDelMailNotify, response);
    }
}
