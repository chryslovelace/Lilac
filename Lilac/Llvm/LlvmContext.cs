using LLVMSharp;

namespace Lilac.Llvm
{
    public class LlvmContext
    {
        public LLVMContextRef Context { get; private set; }
        public LLVMModuleRef Module { get; private set; }
        public LLVMBuilderRef Builder { get; private set; }
    }
}