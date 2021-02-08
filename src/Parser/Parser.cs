using System.Collections.Generic;
using System.Linq;
using IMDG.Common;

namespace IMDG.Parser
{
    public class Parser
    {
        private readonly string _input;
        private int _current = 0;

        private const string Eof = "<EOF>";
        private const char Space = ' ';
        public List<string> Errors { get; } = new List<string>();

        public bool HasErrors => Errors.Any();

        public Parser(string input)
        {
            _input = input + Space + Eof;
        }

        public Command Parse()
        {
            var command = ParseCommand();
            if (command is null)
            {
                return null;
            }
            MatchEof();
            return command;
        }


        public static Command Parse(string input) => new Parser(input).Parse();

        private Command ParseCommand()
        {
            var keyword = MatchWord().ToLowerInvariant();
            switch (keyword)
            {
                case "getall":
                    return new GetAllCommand();
                case "set":
                {
                    var (key, value) = MatchKeyValue();
                    return new SetCommand(key, value);
                }
                case "get":
                {
                    var key = MatchWord();
                    return new GetCommand(key);
                }
                case "remove":
                {
                    var key = MatchWord();
                    return new RemoveCommand(key);
                }

                case "find":
                {
                    var key = MatchWord();
                    return new FindCommand(key);
                }

                case "update":
                {
                    var (key, value) = MatchKeyValue();
                    return new UpdateCommand(key, value);
                }
                case "keys":
                    return new GetKeysCommand();
                case "clear":
                    return new ClearCommand();
                case "hset":
                {
                    var key = MatchWord();
                    var field = MatchWord();
                    var value = MatchWord();
                    return new HSetCommand(key, field, value);
                }
                case "hgetall":
                {
                    var key = MatchWord();
                    return new HGetAllCommand(key);
                }
                case "hget":
                {
                    var key = MatchWord();
                    var field = MatchWord();
                    return new HGetCommand(key, field);
                }
                case "hval":
                {
                    var key = MatchWord();
                    return new HValCommand(key);    
                }
                case "hremove":
                {
                    var key = MatchWord();
                    var field = MatchWord();
                    return new HRemoveCommand(key, field);
                }
                case "hkeys":    
                {
                    var key = MatchWord();
                    return new HKeysCommand(key);
                }
                case "ladd":
                {
                    var key = MatchWord();
                    var value = MatchWord();
                    return new LAddCommand(key, value);
                }
                case "lgetall":
                {
                    var key = MatchWord();
                    return new LGetAllCommand(key);
                }
                case "lremove":
                {
                     var key = MatchWord();
                     var field = MatchWord();
                     return new LRemoveCommand(key, field);
                }
                case "lcount":
                {
                     var key = MatchWord();
                     return new LCountCommand(key);
                }
                case Eof:
                {
                    return null;
                }
                default:
                {
                    Errors.Add("Invalid command");
                    return null;
                }
            }
        }
      
        private string MatchWord()
        {
            SkipWhitespace();
            var index = 0;
            while (_current + index < _input.Length && IsValidWordChar(_input[_current + index]))
            {
                index++;
            }
            if (index == 0)
            {
                Errors.Add("Invalid command");
                return null;
            }
            var result = _input[_current..(_current + index)];
            _current += index;
            return result;
            
            static bool IsValidWordChar(char ch)
            {
                return char.IsLetterOrDigit(ch) || char.IsPunctuation(ch) || char.IsSymbol(ch);
            }
        }

        private (string Key, string Value) MatchKeyValue()
        {
            SkipWhitespace();
            var key = MatchWord();
            SkipWhitespace();
            var value = MatchWord();
            return (key, value);
        }

        private string MatchWord(string word)
        {
            var actualWord = MatchWord();
            if (actualWord == word)
            {
                return word;
            }

            Errors.Add($"expected {word}, got `{actualWord}`");
            return null;
        }


        private void MatchEof()
        {
            SkipWhitespace();
            MatchWord(Eof);
        }

        private void SkipWhitespace()
        {
            var index = 0;
            while (_current + index < _input.Length && char.IsWhiteSpace(_input[_current + index]))
            {
                index++;
            }

            _current += index;
        }
    }
}