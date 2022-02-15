using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public static class Extensions
{
    public enum FontAwesomeMaterial
    {
        Light,
        LightShadow,
        LightOutline,
        Solid,
        SolidShadow,
        SolidOutline,
        Regular,
        RegularShadow,
        RegularOutline
    }

    static Random random = new Random();

    public static string Ipsum(int minWords = 2, int maxWords = 8, int minSentences = 2, int maxSentences = 5, int numParagraphs = 1)
    {
        var words = new[]
        {
            "lorem", "ipsum", "dolor", "sit", "amet", "consectetuer",
            "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod",
            "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"
        };

        var rand = new Random();
        int numSentences = rand.Next(maxSentences - minSentences)
                           + minSentences + 1;
        int numWords = rand.Next(maxWords - minWords) + minWords + 1;

        StringBuilder result = new StringBuilder();

        for (int p = 0; p < numParagraphs; p++)
        {
            result.Append("<p>");
            for (int s = 0; s < numSentences; s++)
            {
                for (int w = 0; w < numWords; w++)
                {
                    if (w > 0)
                    {
                        result.Append(" ");
                    }

                    result.Append(words[rand.Next(words.Length)]);
                }

                result.Append(". ");
            }

            result.Append("</p>");
        }

        return result.ToString();
    }

    public static string StandardisedDateTimeToString(this DateTime dateTime)
    {
        return dateTime.ToString("O", CultureInfo.InvariantCulture);
    }

    public static bool StandardisedStringToDateTime(this string dateTimeString, out DateTime dateTime)
    {
        if (DateTime.TryParseExact(dateTimeString, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dateTime))
        {
            return true;
        }

        return false;
    }

    public static DateTime StandardisedStringToDateTimeDirect(this string dateTimeString)
    {
        DateTime time = DateTime.Now;

        if (DateTime.TryParseExact(dateTimeString, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out time))
        {
            return time;
        }

        return time;
    }

    public static string ToShortWordDateString(this DateTime value, bool useOrdinal = false, string format = "{0:00}{1} {2}, {3}")
    {
        return string.Format(format, value.Day, useOrdinal ? value.Day.GetOrdinal() : "", value.Month.ToShortMonth(), value.Year);
    }

    public static string ToShortTimeDateString(this DateTime value, string format = "{0} {1}")
    {
        return string.Format(format, value.ToShortTimeString(), value.ToShortDateString());
    }

    public static string ToShortMonth(this int value)
    {
        switch (value)
        {
            case 1: return "Jan";
            case 2: return "Feb";
            case 3: return "Mar";
            case 4: return "Apr";
            case 5: return "May";
            case 6: return "Jun";
            case 7: return "Jul";
            case 8: return "Aug";
            case 9: return "Sep";
            case 10: return "Oct";
            case 11: return "Nov";
            case 12: return "Dec";

            default:
                throw new Exception("Month does not exist");
                return "Jan";
        }
    }

    public static string ReplaceAll(this string value, string replaceWith, params string[] toReplace)
    {
        string s = value;

        for (int i = 0; i < toReplace.Length; i++)
        {
            s = s.Replace(toReplace[i], replaceWith);
        }

        return s;
    }

    public static bool Contains(this String str, String substring, StringComparison comp)
    {
        if (substring == null)
        {
            throw new ArgumentNullException(nameof(substring), "substring cannot be null.");
        }
        else if (!Enum.IsDefined(typeof(StringComparison), comp))
        {
            throw new ArgumentException("comp is not a member of StringComparison", nameof(comp));
        }

        return str.IndexOf(substring, comp) >= 0;
    }

    public static bool Counter(this ref float value, float delta)
    {
        if (value > 0)
        {
            value -= delta;

            if (value < 0)
            {
                value = 0;
                return true;
            }

            return false;
        }
        else
        {
            return false;
        }
    }

    public static void Invert(this ref bool value)
    {
        value = !value;
    }

    public static float ClampAngle(this float angle)
    {
        if (angle < 0.0f)
            angle += 360.0f;
        if (angle > 360.0f)
            angle -= 360.0f;

        return angle;
    }

    public static float ClampAngle180(this float angle)
    {
        if (angle < 0.0f)
            angle += 180.0f;
        if (angle > 180.0f)
            angle -= 180.0f;

        return angle;
    }

    public static string CamelCaseToStandard(this string s)
    {
        return Regex.Replace(s, "(\\B[A-Z])", " $1");
    }

    public static string ToPascalCase(this string s)
    {
        string[] words = s.Split(new char[3] {'-', '_', ' '}, StringSplitOptions.RemoveEmptyEntries);

        StringBuilder sb = new StringBuilder(words.Sum(x => x.Length));

        for (int i = 0; i < words.Length; i++)
        {
            sb.Append(words[i][0].ToString().ToUpper() + words[i].Substring(1) + (i < words.Length - 1 ? " " : ""));
        }

        return sb.ToString();
    }

    public static string PascalCaseToStandard(this string s)
    {
        string w = Regex.Replace(s, "(\\B[A-Z])", " $1");
        string x = w.Substring(0, 1);
        w = w.Remove(0, 1);
        w = w.Insert(0, x.ToUpper());
        return w;
    }

    public static T GetLastEntry<T>(this List<T> collection)
    {
        if (collection.Count <= 0) return default;
        return collection[collection.Count - 1];
    }

    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = random.Next(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }

    public static List<T> Clone<T>(this List<T> collection)
    {
        if (collection.Count >= 200)
        {
            throw new Exception("This is a large list. Consider doing this differently");
        }

        List<T> list = new List<T>();

        for (int i = 0; i < collection.Count; i++)
        {
            list.Add(collection[i]);
        }

        return list;
    }

    public static int LoopIndex<T>(this List<T> collection, int index)
    {
        if (index >= collection.Count) index = 0;
        else if (index < 0) index = collection.Count - 1;

        return index;
    }

    public static float LerpTo(float from, float to, float delta)
    {
        if (from <= to)
        {
            from += delta;

            if (from >= to)
                from = to;
        }
        else if (from >= to)
        {
            from -= delta;

            if (from <= to)
                from = to;
        }

        return from;
    }

    public static int ToInt(this bool value)
    {
        switch (value)
        {
            case false:
                return 0;
            case true:
                return 1;
            default:
                return 0;
        }
    }

    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        HashSet<TKey> seenKeys = new HashSet<TKey>();

        foreach (TSource element in source)
        {
            if (seenKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }

    public static string Pluralise(this int value, string singular, string plural)
    {
        if (value == 1) return singular;
        return plural;
    }

    public static string PluraliseAndCombine(this int value, string singular, string plural, string format = "0")
    {
        if (value == 1) return (value.ToString(format) + " " + singular);
        return (value.ToString(format) + " " + plural);
    }

    public static string Pluralise(this float value, string singular, string plural)
    {
        if (value == 1.0f) return singular;
        return plural;
    }

    public static string PluraliseAndCombine(this float value, string singular, string plural, string format = "0")
    {
        if (value == 1.0f) return (value.ToString(format) + " " + singular);
        return (value.ToString(format) + " " + plural);
    }

    public static string GetOrdinal(this int value, bool combine = false)
    {
        string suffix = "th";

        if (value.ToString().EndsWith("11")) suffix = "th";
        if (value.ToString().EndsWith("12")) suffix = "th";
        if (value.ToString().EndsWith("13")) suffix = "th";
        if (value.ToString().EndsWith("1")) suffix = "st";
        if (value.ToString().EndsWith("2")) suffix = "nd";
        if (value.ToString().EndsWith("3")) suffix = "rd";

        return combine ? value.ToString() + suffix : suffix;
    }

    public static int GetYears(this TimeSpan timespan)
    {
        return (int) (timespan.Days / 365.2425);
    }

    public static int GetMonths(this TimeSpan timespan)
    {
        return (int) (timespan.Days / 30.436875);
    }

    public static string DisplayWithSuffix(this int num)
    {
        if (num.ToString().EndsWith("11")) return num.ToString() + "th";
        if (num.ToString().EndsWith("12")) return num.ToString() + "th";
        if (num.ToString().EndsWith("13")) return num.ToString() + "th";
        if (num.ToString().EndsWith("1")) return num.ToString() + "st";
        if (num.ToString().EndsWith("2")) return num.ToString() + "nd";
        if (num.ToString().EndsWith("3")) return num.ToString() + "rd";

        return num.ToString() + "th";
    }

    public static string Beautify(this int value, bool decimals = false)
    {
        if (decimals)
            return string.Format("{0:n}", value);
        else
            return string.Format("{0:n0}", value);
    }

    public static string Beautify(this float value, bool decimals = false)
    {
        if (decimals)
            return string.Format("{0:n}", value);
        else
            return string.Format("{0:n0}", value);
    }

    public static string Beautify(this string value, bool decimals = false)
    {
        int x = 0;

        if (int.TryParse(value, out x))
        {
            return Beautify(x);
        }
        else
        {
            throw new Exception("Value is not a valid number");
            return "";
        }
    }

    public static string SetColour(this string value, string colourHex)
    {
        if (colourHex.Substring(0, 1) != "#") colourHex = colourHex.Insert(0, "#");
        return string.Format("<color={0}>{1}</color>", colourHex, value);
    }

    public static string SetAlpha(this string value, float alpha, bool b)
    {
        string hex = "#FF";

        if (alpha <= 0.125f) hex = "#00";
        else if (alpha <= 0.125f * 2) hex = "#22";
        else if (alpha <= 0.125f * 3) hex = "#44";
        else if (alpha <= 0.125f * 4) hex = "#66";
        else if (alpha <= 0.125f * 5) hex = "#88";
        else if (alpha <= 0.125f * 6) hex = "#AA";
        else if (alpha <= 0.125f * 7) hex = "#CC";
        else if (alpha <= 0.125f * 8) hex = "#FF";

        return string.Format("<alpha={0}>{1}", hex, value);
    }

    public static string SetSize(this string value, float percent)
    {
        return string.Format("<size={0}%>{1}</size>", percent, value);
    }

    public static string BeautifyFileSize(long dataSize, string format = "{0:0} {1}")
    {
        if (dataSize / Math.Pow(10, 9) >= 1)
        {
            return string.Format(format, dataSize / Math.Pow(10, 9), "GB");
        }

        if (dataSize / Math.Pow(10, 6) >= 1)
        {
            return string.Format(format, dataSize / Math.Pow(10, 6), "MB");
        }

        if (dataSize / Math.Pow(10, 3) >= 1)
        {
            return string.Format(format, dataSize / Math.Pow(10, 3), "KB");
        }

        return string.Format(format, dataSize, "bytes");
    }
}