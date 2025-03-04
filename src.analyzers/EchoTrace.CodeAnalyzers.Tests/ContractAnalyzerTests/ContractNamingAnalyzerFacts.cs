using System.Threading;
using System.Threading.Tasks;
using EchoTrace.CodeAnalyzer.ContractAnalyzer;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;

namespace EchoTrace.CodeAnalyzer.Tests.ContractAnalyzerTests;

public class ContractNamingAnalyzerFacts : CSharpAnalyzerTest<ContractNamingAnalyzer, XUnitVerifier>
{
}

public class
    ContractNamingFixProviderFacts : CSharpCodeFixTest<ContractNamingAnalyzer, ContractNamingFixProvider, XUnitVerifier>
{
    [Fact]
    public async Task FixWithoutContractInterfaceName()
    {
        var beforeCode = "public interface Interface1 : Test.Bases.IContract<string>{}";
        var afterCode = "public interface Interface1Contract : Test.Bases.IContract<string>{}";
        TestCode = beforeCode;
        FixedCode = afterCode;
        BatchFixedCode = afterCode;
        NumberOfIncrementalIterations = 1;
        NumberOfFixAllIterations = 1;
        await VerifyFixAsync(TestState, FixedState, BatchFixedState, Verify, CancellationToken.None);
    }
}