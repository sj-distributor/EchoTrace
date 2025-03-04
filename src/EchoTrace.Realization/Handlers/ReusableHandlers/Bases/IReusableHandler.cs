namespace EchoTrace.Realization.Handlers.ReusableHandlers.Bases;

/// <summary>
/// 可複用的業務邏輯
/// </summary>
public interface IReusableHandler
{
}

public interface IReusableHandler<in TParameter> : IReusableHandler where TParameter : IReusableHandlerParameter
{
    Task Handle(TParameter parameter, CancellationToken cancellationToken = default);
}

public interface IReusableHandler<in TParameter, TReturn> : IReusableHandler
    where TParameter : IReusableHandlerParameter where TReturn : IReusableHandlerReturn
{
    Task<TReturn> Handle(TParameter parameter, CancellationToken cancellationToken = default);
}