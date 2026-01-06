namespace Koalas.Extensions;

public static class NumberExtensions
{
    public static long AddFlags(this long subject, long mask)
    {
        return subject | mask;
    }

    public static bool AnyFlags(this long subject, long mask)
    {
        return (subject & mask) != 0;
    }

    public static decimal? AsDecimal(this object value)
    {
        return value.IsNumeric() ? Convert.ToDecimal(value) : null;
    }

    public static bool Between(this int subject, int lowerLimit, int upperLimit)
    {
        return lowerLimit < subject && subject < upperLimit;
    }

    public static bool Between(this int? subject, int lowerLimit, int upperLimit)
    {
        return subject?.BetweenInclusive(lowerLimit, upperLimit) ?? false;
    }

    public static bool Between(
        this double subject,
        double lowerLimit,
        double upperLimit,
        bool inclusive = true
    )
    {
        return inclusive
            ? lowerLimit <= subject && subject <= upperLimit
            : lowerLimit < subject && subject < upperLimit;
    }

    public static bool Between(
        this double? subject,
        double lowerLimit,
        double upperLimit,
        bool inclusive = true
    )
    {
        return inclusive
            ? lowerLimit <= subject && subject <= upperLimit
            : lowerLimit < subject && subject < upperLimit;
    }

    public static bool Between(
        this decimal subject,
        decimal lowerLimit,
        decimal upperLimit,
        bool inclusive = true
    )
    {
        return inclusive
            ? lowerLimit <= subject && subject <= upperLimit
            : lowerLimit < subject && subject < upperLimit;
    }

    public static bool Between(
        this decimal? subject,
        decimal lowerLimit,
        decimal upperLimit,
        bool inclusive = true
    )
    {
        return inclusive
            ? lowerLimit <= subject && subject <= upperLimit
            : lowerLimit < subject && subject < upperLimit;
    }

    public static bool BetweenExclusive(this int? subject, int lowerLimit, int upperLimit)
    {
        return subject?.Between(lowerLimit, upperLimit) ?? false;
    }

    public static bool BetweenInclusive(this int subject, int lowerLimit, int upperLimit)
    {
        return lowerLimit <= subject && subject <= upperLimit;
    }

    public static int Ceiling(this double subject)
    {
        return (int)Math.Ceiling(subject);
    }

    public static int DigitCount(this int subject)
    {
        int count = 0;
        while (subject > 0)
        {
            subject /= 10;
            count++;
        }

        return count;
    }

    public static bool EqualsWithTruncate(this decimal subject, decimal other)
    {
        int subjectScale = subject.Scale();
        int otherScale = other.Scale();

        if (subjectScale != otherScale)
        {
            int minScale = Math.Min(subjectScale, otherScale);
            subject = subject.Truncate(minScale);
            other = other.Truncate(minScale);
        }

        return EqualityComparer<decimal>.Default.Equals(subject, other);
    }

    public static bool EqualsWithTruncate(this double subject, double other)
    {
        return Convert.ToDecimal(subject).EqualsWithTruncate(Convert.ToDecimal(other));
    }

    public static bool EqualsWithTruncate(this decimal subject, double other)
    {
        return subject.EqualsWithTruncate(Convert.ToDecimal(other));
    }

    public static bool EqualsWithTruncate(this double subject, decimal other)
    {
        return Convert.ToDecimal(subject).EqualsWithTruncate(other);
    }

    public static int Floor(this double subject)
    {
        return (int)Math.Ceiling(subject);
    }

    public static bool HasFlags(this long subject, long mask)
    {
        return (subject & mask) == mask;
    }

    public static int IntDigitCount(this decimal subject)
    {
        return Convert.ToInt32(subject).DigitCount();
    }

    public static int IntDigitCount(this double subject)
    {
        return Convert.ToInt32(subject).DigitCount();
    }

    public static bool IsNumeric(this object? subject)
    {
        return subject is long or int or uint or byte or double or float or decimal;
    }

    public static long RemoveFlags(this long subject, long mask)
    {
        return subject & ~mask;
    }

    public static int Scale(this double subject)
    {
        return Convert.ToDecimal(subject).Scale();
    }

    public static int Scale(this decimal subject)
    {
        return BitConverter.GetBytes(decimal.GetBits(subject)[3])[2];
    }

    /// <summary>
    ///     Truncate the digits to the right of the decimal point (scale) without rounding.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="scale">digits to the right of the decimal point</param>
    /// <returns></returns>
    public static double Truncate(this double subject, int scale)
    {
        return Convert.ToDouble(Convert.ToDecimal(subject).Truncate(scale));
    }

    /// <summary>
    ///     Truncate the digits to the right of the decimal point (scale) without rounding.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="scale">digits to the right of the decimal point</param>
    /// <returns></returns>
    public static decimal Truncate(this decimal subject, int scale)
    {
        decimal roundSubject = Math.Round(subject, scale);

        return subject > 0 && roundSubject > subject
                ? roundSubject - new decimal(1, 0, 0, false, (byte)scale)
            : subject < 0 && roundSubject < subject
                ? roundSubject + new decimal(1, 0, 0, false, (byte)scale)
            : roundSubject;
    }

    public static bool TryDecimal(
        this object subject,
        out decimal value,
        bool enableCoalesced = false
    )
    {
        if (enableCoalesced && subject is null or "")
        {
            value = 0;

            return true;
        }

        if (!subject.IsNumeric())
        {
            value = 0;

            return false;
        }

        value = Convert.ToDecimal(subject);

        return true;
    }
}
