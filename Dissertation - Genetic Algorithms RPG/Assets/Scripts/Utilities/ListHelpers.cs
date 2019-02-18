using System.Collections.Generic;

public static class ListHelpers
{
    public static List<T> ListWith<T>(this T item, params T[] otherItems)
    {
        var list = new List<T> { item };

        foreach(var otherItem in otherItems)
        {
            list.Add(otherItem);
        }
        return list;
    }
}
