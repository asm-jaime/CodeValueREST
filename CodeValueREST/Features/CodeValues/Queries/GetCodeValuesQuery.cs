using CodeValueREST.Features.CodeValues.Models;
using MediatR;

namespace CodeValueREST.Features.CodeValues.Queries;

public class GetCodeValuesQuery : IRequest<List<CodeValue>>
{
    public CodeValueFilter? Filter { get; }

    public GetCodeValuesQuery(CodeValueFilter? filter)
    {
        Filter = filter;
    }
}

