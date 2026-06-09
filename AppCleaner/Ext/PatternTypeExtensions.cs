#nullable disable
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AppCleaner.Ext;

public static class PatternTypeExtensions
{
    public static string GetDisplayName(this PatternType value)
    {
        var field = value.GetType().GetField(value.ToString());

        var attr = field?.GetCustomAttribute<DisplayAttribute>();

        return attr?.Name ?? value.ToString();
    }

    public static PatternType FromDisplayName(string value)
    {
        foreach (PatternType pattern in Enum.GetValues(typeof(PatternType)))
        {
            if (string.Equals(
                    pattern.GetDisplayName(),
                    value,
                    StringComparison.OrdinalIgnoreCase))
            {
                return pattern;
            }
        }

        return PatternType.CS;
    }
}