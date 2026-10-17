using MollyPS.Database.Player;
using Newtonsoft.Json.Linq;
using Proto;

namespace MollyPS.GameServer.Game.Mail;

 

public static class MailPresentation
{
    public const string SystemSender = "system";
    public const string SystemIcon = "Mail/mail_endfield";
    private static readonly Dictionary<string, string> Senders = new(StringComparer.Ordinal)
    { [SystemSender] = SystemIcon };

    public static void Load(string directory)
    {
        Senders.Clear();
        Senders[SystemSender] = SystemIcon;
        var path = Path.Combine(directory, "MailSenderCatalog.json");
        if (!File.Exists(path)) return;
        var root = JObject.Parse(File.ReadAllText(path));
        ResourceCatalogIdentity.Validate(root, directory);
        if (root["senders"] is JObject senders)
            foreach (var (id, icon) in senders)
                if (icon?.ToObject<string>() is { Length: > 0 } value) Senders[id] = value;
    }

    public static CS_MAIL_DEF Build(MailData mail)
    {
         
         
        var sender = mail.SenderName ?? SystemSender;
        if (!Senders.ContainsKey(sender)) sender = SystemSender;
        var result = new CS_MAIL_DEF
        {
            MailType = 0, MailId = mail.Id, MailSubType = mail.MailSubType,
            SendTime = mail.SendTime, ExpireTime = mail.ExpireTime,
            IsRead = mail.IsRead, IsAttachmentGot = true,
            IsStar = mail.IsStar, RedDotRead = mail.RedDotRead,
            MailContent = new MAIL_CONTENT
            {
                TemplateId = "", Title = mail.Title ?? "", Content = mail.Content ?? "",
                SenderName = sender, SenderIcon = Senders[sender],
            },
        };
        if (mail.Params != null)
            foreach (var (key, value) in mail.Params) result.MailContent.Params[key] = value;
        return result;
    }
}
