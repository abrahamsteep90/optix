using System.Globalization;
using System.Text;

namespace Movies.Domain;

/// <summary>
/// Turns text into the form used for searching: lower case, no accents, single spaces.
/// So "pokemon" finds "Pokémon" and "aeon flux" finds "Æon Flux".
/// </summary>
/// <remarks>
/// Lower-casing uses the invariant culture on purpose. With a Turkish culture, "I".ToLower() is "ı"
/// (dotless i), so "IT" would no longer match "it".
/// </remarks>
public static class SearchText
{
    public static string Normalize(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var result = new StringBuilder(decomposed.Length);
        var pendingSpace = false;

        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                continue; // the accent part of a letter like "é"
            }

            if (char.IsWhiteSpace(c))
            {
                pendingSpace = result.Length > 0;
                continue;
            }

            if (pendingSpace)
            {
                result.Append(' ');
                pendingSpace = false;
            }

            result.Append(Fold(char.ToLowerInvariant(c)));
        }

        return result.ToString().Normalize(NormalizationForm.FormC);
    }

    // Letters that have no accent-free decomposition in Unicode.
    private static string Fold(char c) => c switch
    {
        'ı' => "i",
        'ø' => "o",
        'æ' => "ae",
        'œ' => "oe",
        'ß' => "ss",
        'ł' => "l",
        'đ' => "d",
        _ => c.ToString(),
    };
}
