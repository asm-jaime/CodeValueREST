using System.Collections.Generic;

namespace DataAccess;

public interface IWriterBase<T1, T2, T3>
    where T2 : IHasIdKey
    where T3 : IHasIdKey
{
    void Add(T1 toAdd);
    void Update(IList<T2> toUpdate);
    void Remove(IEnumerable<T3> removeList);
}
