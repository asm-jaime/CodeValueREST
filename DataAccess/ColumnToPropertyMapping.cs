using System;

namespace GpnDs.ISDR.DataAccess;

public class ColumnToPropertyMapping
{
    public string Column { get; private set; }

    public string Property { get; private set; }

    public ColumnToPropertyMapping(string column, string property)
    {
        _ = column ?? throw new ArgumentNullException(nameof(column));
        _ = property ?? throw new ArgumentNullException(nameof(property));

        Column = column;
        Property = property;
    }
}
