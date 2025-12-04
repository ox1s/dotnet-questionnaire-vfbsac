using Questionnaire.SharedKernel;
using Microsoft.Extensions.DependencyInjection;

namespace Questionnaire.Application.Abstractions.Messaging;

internal sealed class Mediator : ISender
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        Type handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        object? handler = _serviceProvider.GetRequiredService(handlerType);
        Type handleMethodType = typeof(Func<,,>).MakeGenericType(command.GetType(), typeof(CancellationToken), typeof(Task<Result<TResponse>>));
        var handleMethod = handlerType.GetMethod("Handle")!;
        var resultTask = (Task<Result<TResponse>>)handleMethod.Invoke(handler, new object[] { command, cancellationToken })!;
        return await resultTask;
    }

    public async Task<Result> Send(ICommand command, CancellationToken cancellationToken = default)
    {
        Type handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        object? handler = _serviceProvider.GetRequiredService(handlerType);
        var handleMethod = handlerType.GetMethod("Handle")!;
        var result = await (Task<Result>)handleMethod.Invoke(handler, new object[] { command, cancellationToken })!;
        return result;
    }

    public async Task<Result<TResponse>> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        Type handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        object? handler = _serviceProvider.GetRequiredService(handlerType);
        var handleMethod = handlerType.GetMethod("Handle")!;
        var result = await (Task<Result<TResponse>>)handleMethod.Invoke(handler, new object[] { query, cancellationToken })!;
        return result;
    }
}
