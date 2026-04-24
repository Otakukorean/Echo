using MediatR;
using Shared.Contracts.CQRS;

namespace Shared.Contracts.CQRS;

public interface IQueryHandler<in TQuery , TResult> : IRequestHandler<TQuery , TResult> where TQuery : IQuery<TResult> where TResult : notnull
{
    
}