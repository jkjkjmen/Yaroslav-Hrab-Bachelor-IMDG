using IMDG.Common;
using Xunit;

namespace IMDG.Parser.Tests
{
    public class ParserTests
    {
        [Fact]
        public void Parse()
        {
            // arrange
            var parser = new Parser("set 1 2");
            // act
            var command = parser.Parse();
            // assert
            var setCommand = Assert.IsType<SetCommand>(command);
            Assert.Empty(parser.Errors);
            Assert.Equal("1", setCommand.Key);
            Assert.Equal("2", setCommand.Value);
        }
    }
}