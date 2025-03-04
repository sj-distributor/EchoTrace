using System.Reflection;
using AutoBogus;
using Autofac;
using Bogus;
using EchoTrace.Engines.Bases;
using EchoTrace.Infrastructure.Bases;
using EchoTrace.Primary.Contracts.Bases;
using EchoTrace.Realization.Bases;
using FluentValidation;
using Mediator.Net;
using Mediator.Net.Autofac;
using Mediator.Net.Binding;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EchoTrace.Engines.MediatorEngines;

public class RegisterMediator : IBuilderEngine
{
    private readonly ContainerBuilder builder;

    public RegisterMediator(ContainerBuilder builder)
    {
        this.builder = builder;
    }

    public void Run()
    {
        var mediatorBuilder = new MediatorBuilder();
        var realizationAssembly = typeof(IRealization).Assembly;
        var iContractType = typeof(IContract<>);
        var contractTypes = iContractType.Assembly
            .ExportedTypes
            .Where(x =>
                x.GetInterfaces().Any(e => e.IsGenericType && e.GetGenericTypeDefinition() == iContractType) &&
                x is { IsInterface: true, IsGenericType: false })
            .ToArray();

        var realizationTypes = realizationAssembly?
            .ExportedTypes
            .Where(e =>
                e.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == iContractType) &&
                e is { IsClass: true, IsAbstract: false })
            .ToArray();
        builder.RegisterType<DoValidatePipe>()
            .AsSelf()
            .InstancePerLifetimeScope();
        builder.RegisterType<EfCorePipe>()
            .AsSelf()
            .InstancePerLifetimeScope();
        List<SyntaxTree> trees = new();
        if (contractTypes.Any())
        {
            var messageBindings = new List<MessageBinding>();
            foreach (var contractType in contractTypes)
            {
                var realizationType = realizationTypes?.FirstOrDefault(contractType.IsAssignableFrom);
                if (realizationType != null)
                {
                    builder.RegisterType(realizationType)
                        .As(realizationType)
                        .As(contractType)
                        .InstancePerLifetimeScope();
                    var handler = realizationType.GetMethod("Handle");
                    if (handler != null)
                    {
                        var msgType = handler.GetParameters()[0].ParameterType.GenericTypeArguments[0];
                        messageBindings.Add(new MessageBinding(msgType, realizationType));
                        var validatorMethod = realizationType.GetMethod(nameof(IContract<IMessage>.Validate));
                        if (validatorMethod != null)
                            builder.Register(context =>
                                {
                                    var ret = Activator.CreateInstance(validatorMethod.GetParameters()[0]
                                        .ParameterType)!;
                                    var realizationObj = context.Resolve(realizationType);
                                    validatorMethod?.Invoke(realizationObj, new[] { ret });
                                    return ret;
                                })
                                .As(typeof(IValidator<>).MakeGenericType(msgType))
                                .InstancePerDependency();
                    }
                }
                else
                {
                    var syntaxTree = GetSyntaxTree(contractType);
                    if (syntaxTree != null) trees.Add(syntaxTree);
                }
            }

            var fakerAssembly = GetFakerAssembly(trees);
            if (fakerAssembly != null)
            {
                var handlerTypes = fakerAssembly.ExportedTypes;
                foreach (var handlerType in handlerTypes)
                {
                    var contractType = handlerType.GetInterfaces().FirstOrDefault();
                    if (contractType != null)
                    {
                        var iHandlerType = GetContractInputAndOutputType(contractType);
                        if (iHandlerType != null)
                        {
                            messageBindings.Add(new MessageBinding(iHandlerType.GenericTypeArguments[0],
                                handlerType));
                            var validatorType =
                                typeof(ContractValidator<>).MakeGenericType(iHandlerType.GenericTypeArguments[0]);
                            builder.Register(context =>
                                {
                                    var command = Activator.CreateInstance(validatorType)!;
                                    return command;
                                })
                                .As(typeof(IValidator<>).MakeGenericType(iHandlerType.GenericTypeArguments[0]))
                                .InstancePerDependency();
                        }
                    }
                }
            }

            mediatorBuilder.RegisterHandlers(() => messageBindings);
            mediatorBuilder.ConfigureGlobalReceivePipe(c =>
            {
                var doValidatePipe = c.DependencyScope.Resolve<DoValidatePipe>();
                var efCorePipe = c.DependencyScope.Resolve<EfCorePipe>();
                c.AddPipeSpecification(doValidatePipe);
                c.AddPipeSpecification(efCorePipe);
            });
        }

        builder.RegisterMediator(mediatorBuilder);
    }

    private static Type? GetContractInputAndOutputType(Type contractType)
    {
        return contractType.GetInterfaces().FirstOrDefault(e =>
            e.IsGenericType &&
            new[] { typeof(ICommandHandler<>), typeof(ICommandHandler<,>), typeof(IRequestHandler<,>) }
                .Contains(e.GetGenericTypeDefinition()));
    }

    private static SyntaxTree? GetSyntaxTree(Type contractType)
    {
        var iHandlerType = GetContractInputAndOutputType(contractType);
        if (iHandlerType != null)
        {
            var root = SyntaxFactory.CompilationUnit();
            var returnTypeString = nameof(Task);
            var body = $"await {nameof(Task)}.{nameof(Task.CompletedTask)};";
            var testContextString =
                $"{nameof(TestContext<IMessage>)}<{iHandlerType.GenericTypeArguments[0].Name}>";
            if (iHandlerType.GenericTypeArguments.Length == 2)
            {
                returnTypeString = $"{nameof(Task)}<{iHandlerType.GenericTypeArguments[1].Name}>";
                body =
                    $"return await {nameof(BusinessFaker<object>)}<{iHandlerType.GenericTypeArguments[1].Name}>.{nameof(BusinessFaker<object>.CreateAsync)}();";
                testContextString =
                    $"{nameof(TestContext<IMessage>)}<{iHandlerType.GenericTypeArguments[0].Name},{iHandlerType.GenericTypeArguments[1].Name}>";
            }

            root = root.AddUsings(
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IContract<>).Namespace!)))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(contractType.Namespace!)))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IRealization).Namespace!)))
                .AddUsings(SyntaxFactory.UsingDirective(
                    SyntaxFactory.ParseName(typeof(AbstractValidator<>).Namespace!)))
                .AddUsings(SyntaxFactory.UsingDirective(
                    SyntaxFactory.ParseName(typeof(IReceiveContext<>).Namespace!)))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(Task).Namespace!)))
                .AddUsings(SyntaxFactory.UsingDirective(
                    SyntaxFactory.ParseName(typeof(CancellationToken).Namespace!)))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(object).Namespace!)))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(ISetting).Namespace!)));

            var classDeclaration = SyntaxFactory.ClassDeclaration($"{contractType.Name}FakerHandler")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(contractType.Name)));

            var handleMethodDeclaration = SyntaxFactory
                .MethodDeclaration(SyntaxFactory.ParseTypeName(returnTypeString),
                    nameof(ICommandHandler<ICommand>.Handle))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("context"))
                        .WithType(SyntaxFactory.ParseTypeName(
                            $"IReceiveContext<{iHandlerType.GenericTypeArguments[0].Name}>")),
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                        .WithType(SyntaxFactory.ParseTypeName(nameof(CancellationToken)))
                )
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement(body)
                ));

            var testMethodDeclaration = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    nameof(ITestable<IMessage>.Test))
                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("context"))
                    .WithType(SyntaxFactory.ParseTypeName(testContextString)))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement("throw new NotImplementedException();")
                ));

            var validatorMethodDeclaration = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    nameof(IContract<IMessage>.Validate))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("validator"))
                    .WithType(SyntaxFactory.ParseTypeName(
                        $"{nameof(ContractValidator<IMessage>)}<{iHandlerType.GenericTypeArguments[0].Name}>")))
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement("return;")
                ));
            classDeclaration = classDeclaration.AddMembers(handleMethodDeclaration, testMethodDeclaration,
                validatorMethodDeclaration);
            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(
                SyntaxFactory.ParseName(!string.IsNullOrWhiteSpace(contractType.Namespace)
                    ? contractType.Namespace + ".FakerHandlers"
                    : "FakerHandlers"));
            namespaceDeclaration = namespaceDeclaration.AddMembers(classDeclaration);
            root = root.AddMembers(namespaceDeclaration);
            var tree = SyntaxFactory.SyntaxTree(root);
            return tree;
        }

        return null;
    }

    private static Assembly? GetFakerAssembly(List<SyntaxTree> trees)
    {
        if (trees.Any())
        {
            var references = new List<Assembly>
            {
                typeof(object).Assembly,
                typeof(IContract<>).Assembly,
                typeof(ISetting).Assembly,
                typeof(IRealization).Assembly,
                typeof(IMediator).Assembly,
                typeof(AbstractValidator<>).Assembly,
                typeof(Task<>).Assembly,
                typeof(AutoFaker).Assembly,
                typeof(Faker).Assembly,
                typeof(void).Assembly,
                Assembly.Load("System.Runtime"),
                Assembly.Load("netstandard")
            };
            var compilation = CSharpCompilation
                .Create("IContractFakerImplementationAssembly")
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddSyntaxTrees(trees)
                .AddReferences(references.Distinct().Select(x => MetadataReference.CreateFromFile(x.Location)));
            using var stream = new MemoryStream();
            var compileResult = compilation.Emit(stream);
            if (compileResult.Success)
            {
                var fakerImplementationAssembly = Assembly.Load(stream.GetBuffer());
                return fakerImplementationAssembly;
            }
        }

        return null;
    }
}