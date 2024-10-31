using CodeValueREST.Features.CodeValues.Models;
using MediatR;

namespace CodeValueREST.Features.CodeValues.Commands;

public class SaveCodeValuesCommand : IRequest
{
    public List<CodeValue> CodeValues { get; }

    public SaveCodeValuesCommand(List<CodeValue> codeValues)
    {
        CodeValues = codeValues;
    }
}
