using System.Collections.Generic;
using IMDG.Manager;

namespace IMDG.Common
{
    public abstract class Command
    {
        public abstract CommandKind Kind { get; }
    }

    public class SyncCommand : Command
    {
        public IList<Command> Commands { get; set; }
        public override CommandKind Kind => CommandKind.Sync;
    }

    public class ReplayCommand : Command
    {
        public PartitionRange PartitionRange { get; }

        public ReplayCommand(PartitionRange partitionRange) => PartitionRange = partitionRange;

        public override CommandKind Kind => CommandKind.Replay;
    }

    public abstract class KeyCommand : Command
    {
        protected KeyCommand(string key)
        {
            Key = key;
        }

        public string Key { get; }
    }

    public abstract class KeyValueCommand : KeyCommand
    {
        protected KeyValueCommand(string key, string value) : base(key)
        {
            Value = value;
        }
        
        public string Value { get; }
    }

    public class GetCommand : KeyCommand
    {
        public GetCommand(string key) : base(key)
        {
        }

        public override CommandKind Kind => CommandKind.Get;
    }

    public class SetCommand : KeyValueCommand
    {
        public SetCommand(string key, string value) : base(key, value)
        {
        }

        public override CommandKind Kind => CommandKind.Set;
    }

    public class GetAllCommand : Command
    {
        public override CommandKind Kind => CommandKind.GetAll;
    }

    public class UpdateCommand : KeyValueCommand
    {
        public override CommandKind Kind => CommandKind.Update;

        public UpdateCommand(string key, string value) : base(key, value)
        {
        }
    }

    public class GetKeysCommand : Command
    {
        public override CommandKind Kind => CommandKind.Keys;
    }

    public class RemoveCommand : KeyCommand
    {
        public RemoveCommand(string key) : base(key)
        {
        }

        public override CommandKind Kind => CommandKind.Remove;
    }

    public class FindCommand : KeyCommand
    {
        public FindCommand(string key) : base(key)
        {
        }

        public override CommandKind Kind => CommandKind.Find;
    }

    public class ClearCommand : Command
    {
        public override CommandKind Kind => CommandKind.Clear;
    }

    public class HSetCommand : KeyValueCommand
    {
        public string Field { get; }
        public HSetCommand(string key, string field, string value) : base(key, value)
        {
            Field = field;
        }

        public override CommandKind Kind => CommandKind.HSet;
    }
    public class HGetAllCommand : KeyCommand
    {
        public HGetAllCommand(string key) : base(key)
        {
        }

        public override CommandKind Kind => CommandKind.HGetAll;
    }
    public class HRemoveCommand : KeyValueCommand
    {
        public HRemoveCommand(string key, string value) : base(key, value)
        {
        }
        public override CommandKind Kind => CommandKind.HRemove;
    }
    public class HGetCommand : KeyValueCommand
    {
        public HGetCommand(string key, string value) : base(key, value)
        {
        }

        public override CommandKind Kind => CommandKind.HGet;
    }
    public class HValCommand : KeyCommand
    {
        public HValCommand(string key) : base(key)
        {
        }

        public override CommandKind Kind => CommandKind.HVal;
    }

    public class HKeysCommand : KeyCommand
    {
        public HKeysCommand(string key) : base(key)
        {
        }

        public override CommandKind Kind => CommandKind.HKeys;
    }
    public class LAddCommand : KeyValueCommand
    {
        public LAddCommand(string key, string value) : base(key, value)
        {
        }

        public override CommandKind Kind => CommandKind.LAdd;
    }
    public class LGetAllCommand : KeyCommand
    {
        public LGetAllCommand(string key) : base(key)
        {
        }

        public override CommandKind Kind => CommandKind.LGetAll;
    }
    public class LRemoveCommand : KeyValueCommand
    {
        public LRemoveCommand(string key, string value) : base(key, value)
        {
        }
        public override CommandKind Kind => CommandKind.LRemove;
    }
    public class LCountCommand : KeyCommand
    {
        public LCountCommand(string key) : base(key)
        {
        }

        public override CommandKind Kind => CommandKind.LCount;
    }
}