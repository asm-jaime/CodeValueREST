using CodeValueREST.Features.CodeValues.Commands;
using CodeValueREST.Features.CodeValues.Models;
using CodeValueREST.Features.CodeValues.Providers;
using MediatR;

namespace CodeValueREST.Features.CodeValues.Handlers;

public class PutCodeValuesCommandHandler(CodeValueProvider provider) : IRequestHandler<PutCodeValuesCommand, List<CodeValue>>
{
    private readonly CodeValueProvider _provider = provider;

    public async Task<List<CodeValue>> Handle(PutCodeValuesCommand request, CancellationToken cancellationToken)
    {
        var result = await _provider.PutRange(request.CodeValues);
        return result;
    }

}

