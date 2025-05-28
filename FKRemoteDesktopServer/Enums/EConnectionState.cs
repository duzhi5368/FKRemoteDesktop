//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Enums
{
    public enum EConnectionState : byte
    {
        eConnetionState_Closed = 1,
        eConnetionState_Listening = 2,
        eConnetionState_Syn_Sent = 3,
        eConnetionState_Syn_Recieved = 4,
        eConnetionState_Established = 5,
        eConnetionState_Finish_Wait_1 = 6,
        eConnetionState_Finish_Wait_2 = 7,
        eConnetionState_Closed_Wait = 8,
        eConnetionState_Closing = 9,
        eConnetionState_Last_ACK = 10,
        eConnetionState_Time_Wait = 11,
        eConnetionState_Delete_TCB = 12
    }
}
