using CodeValueREST.Features.CodeValues.Models;
using CodeValueREST.Features.CodeValues.Providers;
using CodeValueREST.Features.CodeValues.Queries;
using MediatR;

namespace CodeValueREST.Features.CodeValues.Handlers;

public class GetCodeValuesQueryHandler(CodeValueProvider provider) : IRequestHandler<GetCodeValuesQuery, List<CodeValue>>
{
    private readonly CodeValueProvider _provider = provider;

    public async Task<List<CodeValue>> Handle(GetCodeValuesQuery request, CancellationToken cancellationToken)
    {
        var result = await _provider.ListAsync(request.Filter ?? new CodeValueFilter());
        return result.ToList();
    }
}
