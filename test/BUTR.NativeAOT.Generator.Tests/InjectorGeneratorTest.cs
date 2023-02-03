using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BUTR.NativeAOT.Generator.Tests
{
    public static class CSharpSourceGeneratorVerifier<TSourceGenerator>
        where TSourceGenerator : ISourceGenerator, new()
    {
        public class Test : CSharpSourceGeneratorTest<TSourceGenerator, MSTestVerifier>
        {
            public Test()
            {
            }

            protected override CompilationOptions CreateCompilationOptions()
            {
                var compilationOptions = base.CreateCompilationOptions();
                return compilationOptions.WithSpecificDiagnosticOptions(
                    compilationOptions.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler()));
            }

            public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.Default;

            private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
            {
                string[] args = { "/warnaserror:nullable" };
                var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
                var nullableWarnings = commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;

                return nullableWarnings;
            }

            protected override ParseOptions CreateParseOptions()
            {
                return ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
            }
        }
    }
    
    [TestClass]
    public class InjectorGeneratorTest
    {
        [TestMethod]
        public async Task SimpleGeneratorTest()
        {
            var code = @"
using System.Runtime.InteropServices;
namespace MyCode
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct param_ptr { }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct param_bool { }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct param_int { }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct param_uint { }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct param_string { }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct param_json { }


    [StructLayout(LayoutKind.Sequential)]
    public readonly struct return_value_void { }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct return_value_string { }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct return_value_json { }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct return_value_bool { }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct return_value_int32 { }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct return_value_uint32 { }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct return_value_ptr { }

    public class Program
    {
        [UnmanagedCallersOnly(EntryPoint = ""bfv_get_version"", CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static return_value_void* GetVersion(param_ptr* handle) { return default; }

        public static void Main(string[] args)
        {
        }
    }
}
";
            await new CSharpSourceGeneratorVerifier<InjectorGenerator>.Test
            {
                TestState = 
                {
                    Sources = { code },
                    ReferenceAssemblies = new ReferenceAssemblies("net6.0", new PackageIdentity("Microsoft.NETCore.App.Ref", "6.0.0"), Path.Combine("ref", "net6.0"))
                    //GeneratedSources =
                    //{
                    //    (typeof(YourGenerator), "GeneratedFileName", SourceText.From(generated, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                    //},
                },
            }.RunAsync();
        }
    }
}