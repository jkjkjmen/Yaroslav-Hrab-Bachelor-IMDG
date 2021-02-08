using System;

namespace IMDG.Common
{
    public static class CommandFacts
    {


        public static bool CanReplay(Command command) =>
            command.Kind switch
            {
                CommandKind.Get => false,
                CommandKind.Set => true,
                CommandKind.GetAll =>  false,
                CommandKind.Keys => false,
                CommandKind.Update =>  true,
                CommandKind.Remove => true,
                CommandKind.Find => false,
                CommandKind.Clear => true,
                CommandKind.HSet => true,
                CommandKind.HGetAll => false,
                CommandKind.HGet => false,
                CommandKind.HVal => false,
                CommandKind.HRemove => true,
                CommandKind.HKeys => false,
                CommandKind.LAdd => true,
                CommandKind.LGetAll => false,
                CommandKind.LRemove => true,
                CommandKind.LCount => false,
                CommandKind.Replay => false,
                CommandKind.Sync => false,
                _ => throw new ArgumentOutOfRangeException(nameof(command.Kind))
            };
    }
}