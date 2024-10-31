using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections;

namespace CodeValueREST.Features.CodeValues.Validators;

public class ValidateGetCodeValuesResultAttribute : ResultFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult objectResult)
        {
            var value = objectResult.Value;

            if (value == null)
            {
                context.Result = new NotFoundObjectResult("No data found.");
                return;
            }

            if (value is IEnumerable enumerable && !enumerable.GetEnumerator().MoveNext())
            {
                context.Result = new NotFoundObjectResult("No data found.");
                return;
            }
        }

        base.OnResultExecuting(context);
    }
}
