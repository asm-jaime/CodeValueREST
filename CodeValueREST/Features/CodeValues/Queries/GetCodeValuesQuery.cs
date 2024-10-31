﻿using CodeValueREST.Features.CodeValues.Models;
using MediatR;

namespace CodeValueREST.Features.CodeValues.Queries;

public class GetCodeValuesQuery : IRequest<List<CodeValue>>
{
    public CodeValueFilter? CodeFilter { get; }

    public GetCodeValuesQuery(CodeValueFilter? filter)
    {
        CodeFilter = filter;
    }
}

