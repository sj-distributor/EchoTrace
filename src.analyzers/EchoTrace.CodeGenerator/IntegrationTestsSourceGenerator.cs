using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace EchoTrace.CodeGenerator;


[Generator]
public class IntegrationTestsSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        try
        {
            var realizationReference =
                context.Compilation.References.FirstOrDefault(e =>
                    e.Display != null && e.Display.Contains(".Realization"));
            if (realizationReference != null)

            {
                var realizationAssembly = context.Compilation.GetAssemblyOrModuleSymbol(realizationReference);
                var typeSymbols = GetTypeSymbols(realizationAssembly);
                foreach (var typeSymbol in typeSymbols)
                {
                    var namespaceSet = new HashSet<string>()
                    {
                        "Autofac",
                        "EchoTrace.Primary.Contracts.Bases",
                        "Xunit"
                    };
                    var contextType = "";
                    var sendString = "";
                    if (typeSymbol.ContainingNamespace is not null)
                    {
                        namespaceSet.Add(typeSymbol.ContainingNamespace.ToString());
                    }

                    if (typeSymbol.GetMembers().FirstOrDefault(e => e is IMethodSymbol
                        {
                            Name: "Test"
                        }) is IMethodSymbol { Parameters.Length: 1 } testMethod)
                    {
                        var parameter = testMethod.Parameters[0];
                        contextType = GetNewTestContextString(parameter.Type, namespaceSet);

                        if (parameter.Type is INamedTypeSymbol { TypeArguments.Length: > 0 } namedTypeSymbol
                           )
                        {
                            sendString = GetMediatorSendString(namedTypeSymbol.TypeArguments.ToArray(),
                                namespaceSet);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(contextType) && !string.IsNullOrWhiteSpace(sendString))
                    {
                        var className = typeSymbol.Name;
                        if (typeSymbol.ContainingNamespace!.ToString().Contains(".ForApp"))
                        {
                            className = $"App{typeSymbol.Name}";
                        }
                        else if (typeSymbol.ContainingNamespace.ToString().Contains(".ForWeb"))
                        {
                            className = $"Web{typeSymbol.Name}";
                        }
                        else if (typeSymbol.ContainingNamespace.ToString().Contains(".ForAll"))
                        {
                            className = $"All{typeSymbol.Name}";
                        }

                        var code = GetCode(className, typeSymbol.Name,
                            string.Join("", namespaceSet.Select(ns => $"using {ns};\n")),
                            contextType, sendString);
                        context.AddSource($"{className}Test.cs", code);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private string GetNewTestContextString(ITypeSymbol typeSymbol, HashSet<string> namespaceSet)
    {
        return $@"var context = new {GetGenericTypeFullName(typeSymbol, namespaceSet)}
        {{ 
            LifetimeScope = TestLifetimeScope,
            Mediator = TestMediator,
            DbContext = TestDbContext
        }};";
    }

    private string GetMediatorSendString(ITypeSymbol[] typeSymbols, HashSet<string> namespaceSet)
    {
        if (typeSymbols is not { Length: > 0 })
        {
            return "";
        }

        var hasResponse = typeSymbols.Length >= 2;
        var requestOrCommandType = typeSymbols[0];
        var isRequest = requestOrCommandType.AllInterfaces.Any(e => e.Name == "IRequest");
        var isCommand = requestOrCommandType.AllInterfaces.Any(e => e.Name == "ICommand");
        if (!isRequest && !isCommand)
        {
            return "";
        }

        var handlerResultString =
            $"var handlerResult = new HandlerResult{(hasResponse ? $"<{GetGenericTypeFullName(typeSymbols[1], namespaceSet)}>" : "")}();";
        var sendOrRequestString =
            $@"{(hasResponse ? "handlerResult.Response = " : "")}await TestMediator.{(isRequest ? "RequestAsync" : "SendAsync")}<{string.Join(", ", typeSymbols.Select(type => GetGenericTypeFullName(type, namespaceSet)))}>(testCase.Message);";

        var fullSendString =
            $@"
                        {handlerResultString}
                        try
                        {{
                            {sendOrRequestString}
                        }}
                        catch(Exception exception)
                        {{
                            handlerResult.Exception = exception;
                        }}";
        return fullSendString;
    }

    private static string GetGenericTypeFullName(ITypeSymbol typeSymbol, HashSet<string> namespaceSet)
    {
        var typeBuilder = new StringBuilder();
        typeBuilder.Append(typeSymbol.Name);
        namespaceSet.Add(typeSymbol.ContainingNamespace.ToString());
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.IsGenericType)
            {
                typeBuilder.Append("<");
                var typeArgumentList = new List<string>();
                foreach (var typeArgument in namedTypeSymbol.TypeArguments)
                {
                    typeArgumentList.Add(GetGenericTypeFullName(typeArgument, namespaceSet));
                }

                typeBuilder.Append(string.Join(", ", typeArgumentList));
                typeBuilder.Append(">");
            }
        }

        return typeBuilder.ToString();
    }

    private static List<ITypeSymbol> GetTypeSymbols(ISymbol symbol)
    {
        List<ITypeSymbol> typeSymbols = new List<ITypeSymbol>();
        if (symbol is INamespaceSymbol namespaceSymbol)
        {
            var members = namespaceSymbol.GetMembers();
            foreach (var member in members)
            {
                typeSymbols.AddRange(GetTypeSymbols(member));
            }
        }
        else if (symbol is IAssemblySymbol assemblySymbol)
        {
            typeSymbols.AddRange(GetTypeSymbols(assemblySymbol.GlobalNamespace));
        }
        else if (symbol is IModuleSymbol moduleSymbol)
        {
            typeSymbols.AddRange(GetTypeSymbols(moduleSymbol.GlobalNamespace));
        }
        else if (symbol is ITypeSymbol typeSymbol)
        {
            if (typeSymbol.AllInterfaces.Any(e =>
                    e.ToDisplayString().Contains("EchoTrace.Primary.Contracts.Bases.ITestable")))
            {
                typeSymbols.Add(typeSymbol);
            }
        }

        return typeSymbols;
    }

    private static string GetCode(string className, string handlerName, string usingString, string contextString,
        string sendString)
    {
        return
            $@"
    {usingString}
namespace EchoTrace.IntegrationTests.AutoGenerated;

public class {className}Test : TestBase
{{
    [Fact]
    public async Task ShouldCan{(className.EndsWith("Handler") ? className.Replace("Handler", "") : "")}()
    {{
        {contextString}
        var handler = TestLifetimeScope.Resolve<{handlerName}>();
        if (handler != null)
        {{
            handler.Test(context);
            if (context.TestCases.Any())
            {{
                if(!context.NoDatabase)
                {{
                    await StartupInfrastructure();
                }}
                try
                {{
                    foreach (var testCase in context.TestCases)
                    {{
                        if (testCase.Build != null || testCase.CurrentUser != null)
                        {{
                            Build(testCase);
                            context.LifetimeScope = TestLifetimeScope;
                            context.Mediator = TestMediator;
                            context.DbContext = TestDbContext;
                        }}

                        if (testCase.DatabaseCleanupRequired && !context.NoDatabase)
                        {{
                            await CleanupInfrastructure();
                        }}
                        var arrangeTask = testCase.Arrange?.Invoke();
                        if (arrangeTask is not null)
                        {{
                            await arrangeTask;
                            await context.DbContext.SaveChangesAsync();
                        }}
                        {sendString}
                        var assertTask = testCase.Assert?.Invoke(handlerResult);
                        if (assertTask is not null)
                        {{
                            await assertTask;
                        }}
                    }}                
                }}
                finally
                {{
                    if(!context.NoDatabase)
                    {{
                        await CleanupInfrastructure();
                    }}
                }}
            }}
        }}
    }}
}}";
    }
}