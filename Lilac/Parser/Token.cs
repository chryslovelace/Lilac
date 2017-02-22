namespace Lilac.Parser
{
    public class Token
    {
        public TokenType TokenType { get; set; }
        public string Content { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        public override string ToString()
        {
            return $"[{TokenType}:{Content} at {Line},{Column}]";
        }
    }
}