namespace Koalas.Extensions;

public static partial class Extension {
    public static bool Between(this int subject, int lowerLimit, int upperLimit, bool inclusive = true) {
        return inclusive
                   ? lowerLimit <= subject && subject <= upperLimit
                   : lowerLimit < subject && subject < upperLimit;
    }

    public static bool Between(this int? subject, int lowerLimit, int upperLimit, bool inclusive = true) {
        return inclusive
                   ? lowerLimit <= subject && subject <= upperLimit
                   : lowerLimit < subject && subject < upperLimit;
    }

    public static bool Between(this double subject, double lowerLimit, double upperLimit, bool inclusive = true) {
        return inclusive
                   ? lowerLimit <= subject && subject <= upperLimit
                   : lowerLimit < subject && subject < upperLimit;
    }

    public static bool Between(this double? subject, double lowerLimit, double upperLimit, bool inclusive = true) {
        return inclusive
                   ? lowerLimit <= subject && subject <= upperLimit
                   : lowerLimit < subject && subject < upperLimit;
    }

    public static bool Between(this decimal subject, decimal lowerLimit, decimal upperLimit, bool inclusive = true) {
        return inclusive
                   ? lowerLimit <= subject && subject <= upperLimit
                   : lowerLimit < subject && subject < upperLimit;
    }

    public static bool Between(this decimal? subject, decimal lowerLimit, decimal upperLimit, bool inclusive = true) {
        return inclusive
                   ? lowerLimit <= subject && subject <= upperLimit
                   : lowerLimit < subject && subject < upperLimit;
    }
}