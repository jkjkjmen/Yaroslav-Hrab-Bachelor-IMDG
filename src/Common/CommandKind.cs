using System;

namespace IMDG.Common
{
    public enum CommandKind
    {
        Get,
        Set,
        GetAll,
        Keys,
        Update,
        Remove,
        Find,
        Clear,
        HSet,
        HGetAll,
        HGet,
        HVal,
        HRemove,
        HKeys,
        LAdd,
        LGetAll,
        LRemove,
        LCount,
        Replay = int.MaxValue - 1,
        Sync = int.MaxValue, 
    }
}