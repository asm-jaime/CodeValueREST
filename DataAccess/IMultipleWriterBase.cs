using System.Collections.Generic;

namespace DataAccess;

public interface IMultipleWriterBase<T1, T2, T3>
    where T2 : IHasIdKey
    where T3 : IHasIdKey
{
    void AddRange(IList<T1> toAddItems);
    void Update(IList<T2> toUpdate);
    void Remove(IEnumerable<T3> removeList);
}
