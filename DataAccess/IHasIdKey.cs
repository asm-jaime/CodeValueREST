using System;

namespace DataAccess;

public interface IHasIdKey
{
    Guid Id { get; set; }
}
