using System.Collections.Generic;
using Lilac.Utilities;
using LLVMSharp;

namespace Lilac.Llvm
{
    public class LlvmTypeProvider
    {
        private static ulong _typeid;
        private Dictionary<string, LLVMTypeRef> Types { get; set; } = new Dictionary<string, LLVMTypeRef>();
        private Dictionary<string, LLVMValueRef> TypeInfo { get; set; } = new Dictionary<string, LLVMValueRef>();
        private LlvmContext LlvmContext { get; }

        public LlvmTypeProvider(LlvmContext llvmContext)
        {
            LlvmContext = llvmContext;
            CreateTypes();
        }

        private void CreateTypes()
        {
            IlinkType = CreateNamedStruct("ilink");
            TypeInfoType = CreateNamedStruct("typeinfo");

            IlinkType.StructSetBody(new []
            {
                LLVM.PointerType(TypeInfoType, 0),
                LLVM.PointerType(IlinkType, 0)
            }, false);

            TypeInfoType.StructSetBody(new[]
            {
                LLVM.Int32Type(),
                LLVM.PointerType(TypeInfoType, 0),
                LLVM.PointerType(IlinkType, 0)
            }, false);

            BoxType = CreateNamedStruct("box");
            BoxType.StructSetBody(new[]
            {
                LLVM.PointerType(TypeInfoType, 0),
                LLVM.PointerType(LLVM.Int8Type(), 0)
            }, false);

            UnitType = CreateNamedStruct("unit");
            UnitType.StructSetBody(new LLVMTypeRef[0], false);

            BoolType = Types["bool"] = LLVM.Int1Type();
            IntType = Types["int"] = LLVM.Int64Type();
            RealType = Types["real"] = LLVM.DoubleType();
            CharType = Types["char"] = LLVM.Int32Type();

            ListType = CreateNamedStruct("list");
            ListType.StructSetBody(new[]
            {
                LLVM.Int32Type(),
                LLVM.ArrayType(BoxType, 0)
            }, false);

            PairType = CreateNamedStruct("pair");
            PairType.StructSetBody(new[]
            {
                BoxType,
                BoxType
            }, false);
        }

        private void CreateTypeInfo()
        {
            TypeInfo["typeinfo"] = NewTypeInfo("typeinfo");
            TypeInfo["unit"] = NewTypeInfo("unit");
            TypeInfo["bool"] = NewTypeInfo("bool");
            TypeInfo["cmp"] = NewTypeInfo("cmp");
            TypeInfo["number"] = NewTypeInfo("number", interfaces: new[] {"cmp"});
            TypeInfo["int"] = NewTypeInfo("int", interfaces: new[] {"number"});
            TypeInfo["real"] = NewTypeInfo("real", interfaces: new[] {"number"});
            TypeInfo["char"] = NewTypeInfo("char", interfaces: new[] {"cmp"});
            TypeInfo["list"] = NewTypeInfo("list");
            TypeInfo["pair"] = NewTypeInfo("pair");
        }


        private LLVMValueRef NewTypeInfo(string name, string parent = null, string[] interfaces = null)
        {
            var global = LLVM.AddGlobal(LlvmContext.Module, TypeInfoType, name + ".typeinfo");
            global.SetInitializer(LLVM.ConstNamedStruct(TypeInfoType, new[]
            {
                LLVM.ConstInt(LLVM.Int32Type(), _typeid++, new LLVMBool(0)),
                parent != null
                    ? LLVM.GetNamedGlobal(LlvmContext.Module, parent + ".typeinfo")
                    : LLVM.ConstPointerNull(LLVM.PointerType(TypeInfoType, 0)),
                MakeIlink(name, interfaces ?? new string[0])
            }));
            return global;
        }

        private LLVMValueRef MakeIlink(string name, string[] interfaces)
        {
            var curr = LLVM.ConstPointerNull(LLVM.PointerType(IlinkType, 0));
            for (var i = interfaces.Length - 1; i >= 0; i--)
            {
                var global = LLVM.AddGlobal(LlvmContext.Module, IlinkType, name + ".ilink." + i);
                global.SetInitializer(LLVM.ConstNamedStruct(IlinkType, new[]
                {
                    LLVM.GetNamedGlobal(LlvmContext.Module, interfaces[i]),
                    curr
                }));
            }
            return curr;
        }

        public LLVMTypeRef CreateNamedStruct(string name) => Types[name] = LLVM.StructCreateNamed(LlvmContext.Context, name);

        public LLVMTypeRef GetNamedType(string name) => Types.GetValueOrDefault(name);

        public LLVMTypeRef GetBoxed(string name)
        {
            var boxedname = "boxed." + name;
            LLVMTypeRef type;
            if (Types.TryGetValue(boxedname, out type)) return type;
            type = CreateNamedStruct(boxedname);
            type.StructSetBody(new []
            {
                LLVM.PointerType(TypeInfoType, 0),
                LLVM.PointerType(Types[name], 0)
            }, false);
            return Types[boxedname] = type;
        }

        public LLVMTypeRef IlinkType { get; private set; }
        public LLVMTypeRef TypeInfoType { get; private set; }
        public LLVMTypeRef BoxType { get; private set; }
        public LLVMTypeRef UnitType { get; private set; }
        public LLVMTypeRef BoolType { get; private set; }
        public LLVMTypeRef IntType { get; private set; }
        public LLVMTypeRef RealType { get; private set; }
        public LLVMTypeRef CharType { get; private set; }
        public LLVMTypeRef ListType { get; private set; }
        public LLVMTypeRef PairType { get; private set; }
    }
}