using MollyPS.GameServer.Game.Mail;
using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Proto;
using MollyPS.GameServer.Game;

namespace MollyPS.GameServer.Game.Mail;
public static class GetMailRequest
{
    private static readonly Logger Logger = new("GetMail");
    public static void Handle(EndfieldConnection connection, CS_GET_MAIL cmd)
    {
        var player = connection.Player;
        if (player == null)
            return;
        Logger.Debug($"GetMail: uid={player.Uid}, requested {cmd.MailIdList.Count} mails");
        var response = new SC_GET_MAIL();
        var targetMails = cmd.MailIdList.Count > 0 ? player.Mails.Where(m => cmd.MailIdList.Contains(m.Id)).ToList() : player.Mails.ToList();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        foreach (var mail in targetMails)
        {
            if (mail.ExpireTime > 0 && mail.ExpireTime <= now)
                continue;
            response.MailList.Add(MailPresentation.Build(mail));
        }

        connection.Send((int)SCMessageID.ScGetMail, response);
    }
}
