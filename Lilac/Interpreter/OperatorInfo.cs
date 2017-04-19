using Lilac.AST;

namespace Lilac.Interpreter
{
    public class OperatorInfo
    {
        public decimal Precedence { get; set; }
        public Association Association { get; set; }

        public OperatorInfo(decimal precedence, Association association)
        {
            Precedence = precedence;
            Association = association;
        }
    }
}