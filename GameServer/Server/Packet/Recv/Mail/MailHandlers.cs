using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Proto;
using MollyPS.GameServer.Game.Mail;

namespace MollyPS.GameServer.Server.Packet.Recv.Mail;
 
[PacketOpcode((int)CSMessageID.CsGetMail)]
public class GetMailHandler : Handler<CS_GET_MAIL>
{
    public override void OnHandle(EndfieldConnection connection, CS_GET_MAIL cmd)
    {
        global::MollyPS.GameServer.Game.Mail.GetMailRequest.Handle(connection, cmd);
    }
}

 
[PacketOpcode((int)CSMessageID.CsReadMail)]
public class ReadMailHandler : Handler<CS_READ_MAIL>
{
    public override void OnHandle(EndfieldConnection connection, CS_READ_MAIL cmd)
    {
        global::MollyPS.GameServer.Game.Mail.ReadMailRequest.Handle(connection, cmd);
    }
}

 
[PacketOpcode((int)CSMessageID.CsRedDotReadMail)]
public class RedDotReadMailHandler : Handler<CS_RED_DOT_READ_MAIL>
{
    public override void OnHandle(EndfieldConnection connection, CS_RED_DOT_READ_MAIL cmd)
    {
        global::MollyPS.GameServer.Game.Mail.RedDotReadMailRequest.Handle(connection, cmd);
    }
}

 
[PacketOpcode((int)CSMessageID.CsMarkStarMail)]
public class MarkStarMailHandler : Handler<CS_MARK_STAR_MAIL>
{
    public override void OnHandle(EndfieldConnection connection, CS_MARK_STAR_MAIL cmd)
    {
        global::MollyPS.GameServer.Game.Mail.MarkStarMailRequest.Handle(connection, cmd);
    }
}

 
[PacketOpcode((int)CSMessageID.CsGetMailAttachment)]
public class GetMailAttachmentHandler : Handler<CS_GET_MAIL_ATTACHMENT>
{
    public override void OnHandle(EndfieldConnection connection, CS_GET_MAIL_ATTACHMENT cmd)
    {
        global::MollyPS.GameServer.Game.Mail.GetMailAttachmentRequest.Handle(connection, cmd);
    }
}

 
[PacketOpcode((int)CSMessageID.CsGetAllMailAttachment)]
public class GetAllMailAttachmentHandler : Handler<CS_GET_ALL_MAIL_ATTACHMENT>
{
    public override void OnHandle(EndfieldConnection connection, CS_GET_ALL_MAIL_ATTACHMENT cmd)
    {
        global::MollyPS.GameServer.Game.Mail.GetAllMailAttachmentRequest.Handle(connection, cmd);
    }
}

 
[PacketOpcode((int)CSMessageID.CsDeleteMail)]
public class DeleteMailHandler : Handler<CS_DELETE_MAIL>
{
    public override void OnHandle(EndfieldConnection connection, CS_DELETE_MAIL cmd)
    {
        global::MollyPS.GameServer.Game.Mail.DeleteMailRequest.Handle(connection, cmd);
    }
}

 
[PacketOpcode((int)CSMessageID.CsDeleteAllMail)]
public class DeleteAllMailHandler : Handler<CS_DELETE_ALL_MAIL>
{
    public override void OnHandle(EndfieldConnection connection, CS_DELETE_ALL_MAIL cmd)
    {
        global::MollyPS.GameServer.Game.Mail.DeleteAllMailRequest.Handle(connection, cmd);
    }
}
