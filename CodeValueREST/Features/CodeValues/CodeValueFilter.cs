﻿namespace CodeValueREST.Features.CodeValues;

public class CodeValueFilter
{
    public IList<Guid>? Ids { get; set; }
    public IList<int>? Codes { get; set; }
    public IList<string>? Values { get; set; }
}
