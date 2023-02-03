using BUTR.NativeAOT.Analyzer.Analyzers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Threading.Tasks;

using TestHelper;

namespace BUTR.NativeAOT.Analyzer.Test;

[TestClass]
public class AttributeAnalyzerTest : BaseTest
{
    private static ProjectBuilder CreateProjectBuilder() => new ProjectBuilder()
        .WithTargetFramework(TargetFramework.Net7_0)
        .WithAnalyzer<AttributeAnalyzer>();

    [TestMethod]
    public async Task Unnecessary_CodeFix_Test()
    {
        await CreateProjectBuilder().WithSourceCode(@$"
namespace BUTR.NativeAOT.Analyzer.Test
{{
    using System.Runtime.InteropServices;
    using BUTR.NativeAOT.Shared;

    unsafe class Test
    {{
        [UnmanagedCallersOnly, IsConst]
        public static void* Method10() => null;
        [UnmanagedCallersOnly, IsConst<IsPtrConst>]
        public static void* Method20() => null;
        [UnmanagedCallersOnly, IsNotConst<IsPtrConst>]
        public static void* Method30() => null;


        [UnmanagedCallersOnly, [||]IsConst]
        public static void Method11() {{ }}
        [UnmanagedCallersOnly, [||]IsConst<[||]IsPtrConst>]
        public static void Method21() {{ }}
        [UnmanagedCallersOnly, IsNotConst<[||]IsPtrConst>]
        public static void Method31() {{ }}


        [UnmanagedCallersOnly]
        public static void Method12([ConstMeta<[||]IsConst, IsConst<IsPtrConst>>] delegate* unmanaged[Cdecl]<char, void*> p) {{ }}
        [UnmanagedCallersOnly]
        public static void Method22([ConstMeta<[||]IsConst<[||]IsPtrConst>, IsConst<IsPtrConst>>] delegate* unmanaged[Cdecl]<char, void*> p) {{ }}
        [UnmanagedCallersOnly]
        public static void Method32([ConstMeta<IsNotConst<[||]IsPtrConst>, IsConst<IsPtrConst>>] delegate* unmanaged[Cdecl]<char, void*> p) {{ }}
    }}
}}{CodeBase}").WithCodeFixProvider<UnnecessaryCSCodeFixProvider>().ShouldBatchFixCodeWith(@$"
namespace BUTR.NativeAOT.Analyzer.Test
{{
    using System.Runtime.InteropServices;
    using BUTR.NativeAOT.Shared;

    unsafe class Test
    {{
        [UnmanagedCallersOnly, IsConst]
        public static void* Method10() => null;
        [UnmanagedCallersOnly, IsConst<IsPtrConst>]
        public static void* Method20() => null;
        [UnmanagedCallersOnly, IsNotConst<IsPtrConst>]
        public static void* Method30() => null;


        [UnmanagedCallersOnly]
        public static void Method11() {{ }}
        [UnmanagedCallersOnly]
        public static void Method21() {{ }}
        [UnmanagedCallersOnly]
        public static void Method31() {{ }}


        [UnmanagedCallersOnly]
        public static void Method12([ConstMeta<IsNotConst, IsConst<IsPtrConst>>] delegate* unmanaged[Cdecl]<char, void*> p) {{ }}
        [UnmanagedCallersOnly]
        public static void Method22([ConstMeta<IsNotConst, IsConst<IsPtrConst>>] delegate* unmanaged[Cdecl]<char, void*> p) {{ }}
        [UnmanagedCallersOnly]
        public static void Method32([ConstMeta<IsNotConst, IsConst<IsPtrConst>>] delegate* unmanaged[Cdecl]<char, void*> p) {{ }}
    }}
}}{CodeBase}").ValidateAsync();
    }

    [TestMethod]
    public async Task ReturnType_Test()
    {
        await CreateProjectBuilder().WithSourceCode(@$"
namespace BUTR.NativeAOT.Analyzer.Test
{{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using BUTR.NativeAOT.Shared;

    unsafe class Test
    {{
        [UnmanagedCallersOnly, IsConst]
        public static void* Method10() => null;
        [UnmanagedCallersOnly, IsConst<IsPtrConst>]
        public static void* Method20() => null;
        [UnmanagedCallersOnly, IsNotConst<IsPtrConst>]
        public static void* Method30() => null;


        [UnmanagedCallersOnly, [||]IsConst]
        public static void Method11() {{ }}
        [UnmanagedCallersOnly, [||]IsConst<[||]IsPtrConst>]
        public static void Method21() {{ }}
        [UnmanagedCallersOnly, IsNotConst<[||]IsPtrConst>]
        public static void Method31() {{ }}


        [UnmanagedCallersOnly]
        public static [||][||]void* Method12() => null;
        [UnmanagedCallersOnly]
        public static [||][||]void* Method22() => null;
        [UnmanagedCallersOnly]
        public static [||][||]void* Method32() => null;
    }}
}}{CodeBase}").ValidateAsync();
    }

    [TestMethod]
    public async Task Parameter_Test()
    {
        await CreateProjectBuilder().WithSourceCode(@$"
namespace BUTR.NativeAOT.Analyzer.Test
{{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using BUTR.NativeAOT.Shared;

    unsafe class Test
    {{
        [UnmanagedCallersOnly]
        public static void Method10([IsConst] char* p) {{ }}

        [UnmanagedCallersOnly]
        public static void Method20([IsConst<IsPtrConst>] char* p) {{ }}

        [UnmanagedCallersOnly]
        public static void Method30([IsNotConst<IsPtrConst>] char* p) {{ }}


        [UnmanagedCallersOnly]
        public static void Method10([[||]IsConst] char p) {{ }}

        [UnmanagedCallersOnly]
        public static void Method20([[||]IsConst<[||]IsPtrConst>] char p) {{ }}

        [UnmanagedCallersOnly]
        public static void Method30([IsNotConst<[||]IsPtrConst>] char p) {{ }}
    }}
}}{CodeBase}").ValidateAsync();
    }

    [TestMethod]
    public async Task FunctionalParameter_Test()
    {
        await CreateProjectBuilder().WithSourceCode(@$"
namespace BUTR.NativeAOT.Analyzer.Test
{{
    using System.Runtime.InteropServices;
    using BUTR.NativeAOT.Shared;

    unsafe class Test
    {{
        [UnmanagedCallersOnly]
        public static void Method10([ConstMeta<IsConst, IsConst>] delegate* unmanaged[Cdecl]<[||]char*, [||]void*> p) {{ }}
        [UnmanagedCallersOnly]
        public static void Method20([ConstMeta<IsConst<IsPtrConst>, IsConst<IsPtrConst>>] delegate* unmanaged[Cdecl]<char*, void*> p) {{ }}
        [UnmanagedCallersOnly]
        public static void Method30([ConstMeta<IsNotConst<IsPtrConst>, IsNotConst<IsPtrConst>>] delegate* unmanaged[Cdecl]<[||]char*, [||]void*> p) {{ }}


        [UnmanagedCallersOnly]
        public static void Method11([ConstMeta<[||]IsConst, IsConst<IsPtrConst>>] delegate* unmanaged[Cdecl]<char, void*> p) {{ }}
        [UnmanagedCallersOnly]
        public static void Method21([ConstMeta<[||]IsConst<[||]IsPtrConst>, IsConst<IsPtrConst>>] delegate* unmanaged[Cdecl]<char, void*> p) {{ }}
        [UnmanagedCallersOnly]
        public static void Method31([ConstMeta<IsNotConst<[||]IsPtrConst>, IsConst<IsPtrConst>>] delegate* unmanaged[Cdecl]<char, void*> p) {{ }}


        [UnmanagedCallersOnly]
        public static void Method12([ConstMeta<IsConst<IsPtrConst>, [||]IsConst>] delegate* unmanaged[Cdecl]<char*, void> p) {{ }}
        [UnmanagedCallersOnly]
        public static void Method22([ConstMeta<IsConst<IsPtrConst>, [||]IsConst<[||]IsPtrConst>>] delegate* unmanaged[Cdecl]<char*, void> p) {{ }}
        [UnmanagedCallersOnly]
        public static void Method32([ConstMeta<IsConst<IsPtrConst>, IsNotConst<[||]IsPtrConst>>] delegate* unmanaged[Cdecl]<char*, void> p) {{ }}
    }}
}}{CodeBase}").ValidateAsync();
    }
}