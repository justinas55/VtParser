using NUnit.Framework;
using System.Text;
using System.Linq;

namespace VtParser.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            //arrange
            string input = "[2J[?25l[m[HServer starting with version 255";
            var parser = new VtParser();
            StringBuilder printOutput = new StringBuilder();
            StringBuilder output = new StringBuilder();
            parser.Callback = (parser, action, ch) =>
            {
                if(action == Actions.VTPARSE_ACTION_PRINT)
                {
                    printOutput.Append(ch);
                }

                output.Append($"{action.GetActionName()}:" + (ch < 32 ? $"<{(byte)ch:X2}>" : ch.ToString()) + $":{parser.IntermediateChars}:{string.Join(',', parser.Parameters.ToArray().Select(x => x.ToString()))};");
            };

            //act
            parser.PutString(input);

            //assert
            System.Console.WriteLine($"Printed: \"{printOutput}\"");
            System.Console.WriteLine($"All: \"{output}\"");
            Assert.AreEqual("Server starting with version 255", printOutput.ToString());
        }
    }
}