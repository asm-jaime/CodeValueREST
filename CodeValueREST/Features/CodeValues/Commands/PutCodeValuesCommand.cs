using CodeValueREST.Features.CodeValues.Models;
using MediatR;

namespace CodeValueREST.Features.CodeValues.Commands;

public class PutCodeValuesCommand(List<CodeValue> codeValues) : IRequest<List<CodeValue>>
{
    public List<CodeValue> CodeValues { get; } = codeValues;
}
