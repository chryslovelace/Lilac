namespace Lilac.Parser
{
    public enum TokenType
    {
        DecimalNumber,
        BinaryNumber,
        HexNumber,
        RationalNumber,
        ComplexNumber,
        Number,
        Identifier,
        String,
        OpenGroup,
        CloseGroup,
        OpenList,
        CloseList,
        Period,
        Backquote,
        Comma,
        Newline,
        Whitespace,
        Comment,
        ReservedWord,
        Unrecognized
    }
}