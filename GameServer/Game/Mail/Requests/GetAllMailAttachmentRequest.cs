using MollyPS.GameServer.Server;
using Proto;

namespace MollyPS.GameServer.Game.Mail;

 
public static class GetAllMailAttachmentRequest
{
    public static void Handle(EndfieldConnection connection, CS_GET_ALL_MAIL_ATTACHMENT cmd)
    {
        if (connection.Player == null) return;
        var response = new SC_GET_MAIL_ATTACHMENT();
        connection.Send((int)SCMessageID.ScGetMailAttachment, response);
    }
}
