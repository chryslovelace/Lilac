using System.Collections.Generic;
using System.IO;

namespace Lilac.Parser
{
    public interface ILexer
    {
        int TabWidth { get; set; }
        IEnumerable<Token> Tokenize(TextReader reader);
    }
}