using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CodeValueREST.Features.CodeValues.Validators;

/// <summary>
/// Initializes a new instance of the <see cref="ValidateValueToCodesAttribute"/> class.
/// </summary>
/// <param name="parameterName">The name of the action parameter to validate. Defaults to "valueToCodes".</param>
public class ValidateValueToCodesAttribute(string parameterName = "valueToCodes") : ActionFilterAttribute
{
    private readonly string _parameterName = parameterName;

    /// <summary>
    /// Called before the action executes. Validates the specified parameter.
    /// </summary>
    /// <param name="context">The action executing context.</param>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if(context.ActionArguments.TryGetValue(_parameterName, out var value) is false)
        {
            context.Result = new BadRequestObjectResult($"Missing parameter '{_parameterName}'.");
            return;
        }

        if(value is not IEnumerable<Dictionary<string, string>>)
        {
            context.Result = new BadRequestObjectResult("Input data is empty.");
            return;
        }

        if(value is not IEnumerable<Dictionary<string, string>> data)
        {
            context.Result = new BadRequestObjectResult("Input data is empty.");
            return;
        }

        if(data.Any() is false)
        {
            context.Result = new BadRequestObjectResult($"Invalid type for parameter '{_parameterName}'. Expected a list of dictionaries.");
            return;
        }

        base.OnActionExecuting(context);
    }

}

