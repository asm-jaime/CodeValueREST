using CodeValueREST.Features.CodeValues.Commands;
using CodeValueREST.Features.CodeValues.Models;
using CodeValueREST.Features.CodeValues.Queries;
using CodeValueREST.Features.CodeValues.Validators;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CodeValueREST.Features.CodeValues;

[ApiController]
[Route("[controller]")]
public class CodeValuesController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPut]
    [ValidateValueToCodes]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Put([FromBody] List<Dictionary<string, string>> valueToCodes)
    {
        var codeValues = valueToCodes
            .SelectMany(dict => dict.Select(kvp => new CodeValue { Code = int.Parse(kvp.Key), Value = kvp.Value }))
            .ToList();

        var result = await _mediator.Send(new PutCodeValuesCommand(codeValues));

        return Ok(result);
    }

    [HttpGet]
    [ValidateGetCodeValuesResult]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get([FromQuery] CodeValueFilter filter)
    {
        var result = await _mediator.Send(new GetCodeValuesQuery(filter));

        return Ok(result);
    }
}
