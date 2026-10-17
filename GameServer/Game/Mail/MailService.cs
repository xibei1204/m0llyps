using MollyPS.Database.Player;
using MollyPS.Util;
using Proto;

namespace MollyPS.GameServer.Game.Mail;

 
public static class MailService
{
    public static MailData CreateMail(PlayerInstance player, string title, string content,
        string senderName = "system", long expireSeconds = 0)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var mail = new MailData
        {
            Id = player.NextMailId(), Title = title, Content = content,
            SenderName = senderName, SendTime = now,
            ExpireTime = expireSeconds > 0 ? checked(now + expireSeconds) : 0,
        };
        player.Mails.Add(mail);
        return mail;
    }

    public static void CheckAndSendWelcomeMail(PlayerInstance player)
    {
        var config = ConfigManager.Config.Game.WelcomeMail;
        if (player.ReceivedWelcomeMail || !config.Enable) return;
        CreateMail(player, config.Title, config.Content, config.Sender,
            config.ExpireDays > 0 ? (long)config.ExpireDays * 86400 : 0);
        player.ReceivedWelcomeMail = true;
        player.SaveToDatabase();
    }

    public static SC_SYNC_ALL_MAIL BuildSyncAllMail(PlayerInstance player)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var visible = player.Mails.Where(mail => mail.ExpireTime <= 0 || mail.ExpireTime > now).ToArray();
        var sync = new SC_SYNC_ALL_MAIL
        {
            NewMailTag = visible.Any(mail => !mail.IsRead || !mail.RedDotRead),
        };
        sync.MailIdList.AddRange(visible.Select(mail => mail.Id));
        return sync;
    }
}
