namespace Koalas.Extensions;

public static class IDictionaryExtension
{
    public static string Render<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> subject)
    {
        subject = subject.ToReadOnlyList();
        if (!subject.Any())
            return "<none>";

        ITextRowBuilder table = TextTableBuilder
            .Create()
            .AddIdentityColumn(rightPadding: 0)
            .AddStaticColumn(":")
            .AddColumn("Key", maxWidth: 60)
            .AddBorderColumn()
            .AddColumn("Value", maxWidth: 100)
            .AddHeadingRow()
            .AddBorderRow();

        foreach (KeyValuePair<TKey, TValue> pair in subject)
        {
            string value = pair.Value?.ToString() ?? string.Empty;
            string values = pair.Value is IEnumerable<object> enumerable
                ? enumerable.Select(o => o.ToString()).ToJoinNewlineString()
                : value;

            table.AddDataRow(value, values).AddBorderRow();
        }

        return table.Render();
    }

    public static string Render<TKey, TValue>(this IDictionary<TKey, TValue> subject)
    {
        return Render(subject as IEnumerable<KeyValuePair<TKey, TValue>>);
    }

    public static IDictionary<TKey, TValue> Set<TKey, TValue>(
        this IDictionary<TKey, TValue> subject,
        TKey key,
        TValue value
    )
    {
        if (subject == null)
            return new Dictionary<TKey, TValue>();

        subject[key] = value;

        return subject;
    }

    public static Dictionary<TKey, TValue> Set<TKey, TValue>(
        this Dictionary<TKey, TValue> subject,
        TKey key,
        TValue value
    )
    {
        return (Dictionary<TKey, TValue>)Set((IDictionary<TKey, TValue>)subject, key, value);
    }

    public static Dictionary<TKey, IReadOnlyList<TValue>> ToLookupDictionary<TKey, TValue>(
        this IEnumerable<TValue> subject,
        Func<TValue, TKey> keySelector
    )
    {
        return subject.GroupBy(keySelector).ToDictionary(c => c.Key, c => c.ToReadOnlyList());
    }

    public static Dictionary<TKey, IReadOnlyList<TResultValue>> ToLookupDictionary<
        TKey,
        TValue,
        TResultValue
    >(
        this IEnumerable<TValue> subject,
        Func<TValue, TKey> keySelector,
        Func<TValue, IReadOnlyList<TResultValue>> valueSelector
    )
    {
        return subject
            .GroupBy(keySelector)
            .ToDictionary(c => c.Key, c => c.SelectMany(valueSelector).ToReadOnlyList());
    }
}