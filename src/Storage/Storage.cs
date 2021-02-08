using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using IMDG.Common;
using IMDG.Parser;

namespace IMDG.Storage
{
    public class Storage
    {
        private readonly FileHandler _fileHandler;
        private readonly ConcurrentDictionary<string, Value> _storage = new ConcurrentDictionary<string, Value>();


        public Storage(FileHandler fileHandler)
        {
            _fileHandler = fileHandler;
        }

        public async Task<string> RequestAsync(Command command)
        { 
            var result = command.Kind switch
            {
                CommandKind.Get => PerformGet((GetCommand) command),
                CommandKind.Set => PerformSet((SetCommand) command),
                CommandKind.GetAll => PerformGetAll((GetAllCommand) command),
                CommandKind.Keys => PerformKeys((GetKeysCommand) command),
                CommandKind.Update => PerformUpdate((UpdateCommand) command),
                CommandKind.Remove => PerformRemove((RemoveCommand) command),
                CommandKind.Find => PerformFind((FindCommand) command),
                CommandKind.Clear => PerformClear((ClearCommand) command),
                CommandKind.HSet => PerformHSet((HSetCommand) command),
                CommandKind.HGetAll => PerformHGetAll((HGetAllCommand) command),
                CommandKind.HGet => PerformHGet((HGetCommand) command),
                CommandKind.HVal => PerformHVal((HValCommand) command),
                CommandKind.HRemove => PerformHRemove((HRemoveCommand) command),
                CommandKind.HKeys => PerformHKeys((HKeysCommand) command),
                CommandKind.LAdd => PerformLAdd((LAddCommand) command),
                CommandKind.LGetAll => PerformLGetAll((LGetAllCommand) command),
                CommandKind.LRemove => PerformLRemove((LRemoveCommand) command),
                CommandKind.LCount => PerformLCount((LCountCommand) command),
                CommandKind.Replay => await PerformReplay((ReplayCommand) command),
                CommandKind.Sync => await PerformSync((SyncCommand) command),
                _ => "Invalid command"
            };
            return result;
        }

        private async Task<string> PerformReplay(ReplayCommand replayCommand)
        {
            var range = replayCommand.PartitionRange;
            var allCommands = await _fileHandler.GetFile();
            var toReplay = allCommands
                .Where(CommandFacts.CanReplay)
                .OfType<KeyCommand>()
                .Where(c => range.Contains(c.Key.GetDeterministicHashCode()));

            _storage.Clear();
            foreach (var command in toReplay) await RequestAsync(command);
            Console.WriteLine("================================");
            Console.WriteLine(PerformGetAll(new GetAllCommand()));
            Console.WriteLine("================================");
            return "Replay ok";
        }

        private async Task<string> PerformSync(SyncCommand command)
        {
            await _fileHandler.UpdateFileAsync(command.Commands);
            return "Sync Ok";
        }

        private string PerformHSet(HSetCommand command)
        {
            if (_storage.TryGetValue(command.Key, out var value))
            {
                if (value is HashValue hv)
                {
                    if (hv.Value.ContainsKey(command.Field))
                    {
                        return $"Hash map associated with key {command.Key} already have key {command.Field} ";
                    }
                    hv.Value.TryAdd(command.Field, command.Value);
                    return $"{command.Field} {command.Value} pair has been added to hash map associated with key {command.Key}";
                }

                return $"Type associated with key {command.Key} is not a hash-value type";
            }
            var newValue = new HashValue
            {
                Value = {[command.Field] = command.Value}
            };
            _storage[command.Key] = newValue;
            return $"{command.Field} {command.Value} pair has been added to hash map associated with key {command.Key}";
        }

        private string PerformHGetAll(HGetAllCommand command)
        {
            if (_storage.TryGetValue(command.Key, out var value))
            {
                if (value is HashValue hv)
                {
                    return hv.Display();
                }
                return $"Type associated with key {command.Key} is not a hash-value type";
            }
            return "Key does not exists";
        }
        private string PerformHGet(HGetCommand command)
        {
            if (_storage.TryGetValue(command.Key, out var value))
            {
                if (value is HashValue hv)
                {
                    if (!hv.Value.ContainsKey(command.Value))
                    {
                        return $"No such element in database associated with key {command.Key}";
                    }
                    return hv.Value[command.Value];
                }
                return $"Type associated with key {command.Key} is not a hash-value type";
            }
            return "Key does not exists";
        }

        private string PerformHVal(HValCommand command)
        {
            if (_storage.TryGetValue(command.Key, out var value))
            {
                if (value is HashValue hv)
                {
                    return hv.GetValues();
                }
                return $"Type associated with key {command.Key} is not a hash-value type";
            }
            return "Key does not exists";
        }

        private string PerformHKeys(HKeysCommand command)
        {
            if (_storage.TryGetValue(command.Key, out var value))
            {
                if (value is HashValue hv)
                {
                    return hv.GetKeys();
                }
                return $"Type associated with key {command.Key} is not a hash-value type";
            }
            return "Key does not exists";
        }
        private string PerformHRemove(HRemoveCommand command)
        {
            if (_storage.TryGetValue(command.Key, out var value))
            {
                if (value is HashValue hv)
                {
                    if (hv.Value.ContainsKey(command.Value))
                    {
                        hv.Value.TryRemove(command.Value, out var answer);
                        return answer + " " + "removed";
                    }
                    return $"No such element in database associated with key {command.Value}";
                }
                return $"Type associated with key {command.Key} is not a hash-value type";
            }
            return "Key does not exists";
        }
        private string PerformLAdd(LAddCommand command)
        {
            if (_storage.TryGetValue(command.Key, out var value))
            {
                if (value is ListValue lv)
                {
                    if (lv.Value.Contains(command.Value))
                    {
                        return $"List associated with key {command.Key} already have element {command.Value} ";
                    }
                    lv.Value.Add(command.Value);
                    return $"{command.Value} has been added to list associated with key {command.Key}";
                }

                return $"Type associated with key {command.Key} is not a hash-value type";
            }
            var newValue = new ListValue
            {
                Value = { command.Value }
            };
            _storage[command.Key] = newValue;
            return $"{command.Value} has been added to list associated with key {command.Key}";
        }
        private string PerformLGetAll(LGetAllCommand command)
        {
            if (_storage.TryGetValue(command.Key, out var value))
            {
                if (value is ListValue lv)
                {
                    return lv.Display();
                }
                return $"Type associated with key {command.Key} is not a list type";
            }
            return "Key does not exists";
        }
        private string PerformLRemove(LRemoveCommand command)
        {
            if (_storage.TryGetValue(command.Key, out var value))
            {
                if (value is ListValue lv)
                {
                    if (lv.Value.Contains(command.Value))
                    {
                        lv.Value.Remove(command.Value);
                        return $"{command.Value} has been removed from list associated with key {command.Key}";
                    }
                    return $"List associated with key {command.Key} don't have element {command.Value}";
                }
                return $"Type associated with key {command.Key} is not a list type";
            }
            return "Key does not exists";
        }
        private string PerformLCount(LCountCommand command)
        {
            if (_storage.TryGetValue(command.Key, out var value))
            {
                if (value is ListValue lv)
                {
                    return lv.Value.Count().ToString();
                }
                return $"Type associated with key {command.Key} is not a list type";
            }
            return "Key does not exists";
        }
        private string PerformSet(SetCommand command)
        {
            if (_storage.TryAdd(command.Key, new StringValue(command.Value)))
            {
                return command.Value;
            }

            // TODO: add error handling
            return "Key already exists";
        }

        private string PerformGetAll(GetAllCommand command)
        {
            if (!_storage.IsEmpty)
            {
                return _storage.Aggregate("", (current, keyValue) => current + ($"\nKey: {keyValue.Key}" + " " + $"Value: {keyValue.Value.Display()}"));
            }

            // TODO: add error handling
            return "The storage is empty";
        }
        private string PerformGet(GetCommand command)
        {
            if (_storage.TryGetValue(command.Key, out var value))
            {   if (value is StringValue)
                {
                    return value.Display();
                }
                return $"Type associated with key {command.Key} is not a common type";
            }
            return "Key does not exits";
        }
        private string PerformKeys(GetKeysCommand command)
        {
            if (!_storage.IsEmpty)
            {
                return _storage.Aggregate("", (current, keyValue) => ($"\n{keyValue.Key}"));
            }

            return "The storage is empty";
        }
        private string PerformUpdate(UpdateCommand command)
        {
            if (_storage.TryGetValue(command.Key, out var value))
            {
                if (value is StringValue)
                {
                    _storage.TryUpdate(command.Key, new StringValue(command.Value), _storage[command.Key]);
                    return "Updated";
                }
                return $"Type associated with key {command.Key} is not a common type";
            }
            return "No such element in database";
        }
        private string PerformRemove(RemoveCommand command)
        {
            if (_storage.ContainsKey(command.Key))
            {
                _storage.TryRemove(command.Key, out var answer);
                return answer.Display() + " " + "removed";
            }

            return "No such element in database";
        }
        private string PerformFind(FindCommand command)
        {
            if (_storage.ContainsKey(command.Key))
            {
                return $"The element with key {command.Key} is in database";
            }

            return "No such element in database";
        }
        private string PerformClear(ClearCommand command)
        {
            _storage.Clear();

            return "Database has been cleared";
        }
    }
}