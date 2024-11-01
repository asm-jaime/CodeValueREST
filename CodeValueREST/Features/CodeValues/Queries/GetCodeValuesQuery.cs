using CodeValueREST.Features.CodeValues.Models;
using MediatR;

namespace CodeValueREST.Features.CodeValues.Queries;

public class GetCodeValuesQuery(CodeValueFilter? filter) : IRequest<List<CodeValue>>
{
    public CodeValueFilter? Filter { get; } = filter;
}

