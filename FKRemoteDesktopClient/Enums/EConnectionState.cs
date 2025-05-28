//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Enums
{
    public enum EConnectionState : byte
    {
        eConnectionState_Closed = 1,
        eConnectionState_Listening = 2,
        eConnectionState_SYN_Sent = 3,
        eConnectionState_Syn_Recieved = 4,
        eConnectionState_Established = 5,
        eConnectionState_Finish_Wait_1 = 6,
        eConnectionState_Finish_Wait_2 = 7,
        eConnectionState_Closed_Wait = 8,
        eConnectionState_Closing = 9,
        eConnectionState_Last_ACK = 10,
        eConnectionState_Time_Wait = 11,
        eConnectionState_Delete_TCB = 12
    }
}