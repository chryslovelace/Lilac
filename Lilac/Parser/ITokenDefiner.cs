using System.Collections.Generic;

namespace Lilac.Parser
{
    public interface ITokenDefiner
    {
        IEnumerable<TokenDefinition> GetTokenDefinitions();
    }
}