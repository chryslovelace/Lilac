namespace Lilac.AST.Definitions
{
    public class OperatorDefinition : Definition
    {
        public decimal Precedence { get; set; }
        public Association Association { get; set; }

        public OperatorDefinition(string name, decimal precedence, Association association) : base(name)
        {
            Precedence = precedence;
            Association = association;
        }
    }
}