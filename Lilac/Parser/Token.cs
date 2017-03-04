namespace Lilac.Parser
{
    public struct Token
    {
        public TokenType TokenType { get; set; }
        public string Content { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        public override string ToString()
        {
            return $"[{TokenType}:{Content} at {Line},{Column}]";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Token)) return false;
            return Equals((Token)obj);
        }

        private bool Equals(Token other)
        {
            return TokenType == other.TokenType && string.Equals(Content, other.Content) && Line == other.Line && Column == other.Column;
        }

        public override int GetHashCode() => ToString().GetHashCode();
    }
}