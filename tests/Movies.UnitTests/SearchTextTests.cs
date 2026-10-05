using System.Globalization;
using Movies.Domain;

namespace Movies.UnitTests;

public class SearchTextTests
{
    [Theory]
    [InlineData("Pokémon Detective Pikachu", "pokemon detective pikachu")]
    [InlineData("¡Qué Despadre!", "¡que despadre!")]
    [InlineData("Æon Flux", "aeon flux")]
    [InlineData("Utøya: July 22", "utoya: july 22")]
    [InlineData("  Spider-Man:   No Way Home ", "spider-man: no way home")]
    [InlineData("", "")]
    public void Normalize_lower_cases_removes_accents_and_tidies_spaces(string input, string expected) =>
        Assert.Equal(expected, SearchText.Normalize(input));

    [Fact]
    public void Normalize_gives_the_same_result_whatever_the_current_culture()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            // The trap: with a Turkish culture, ToLower() turns "I" into a dotless "ı".
            Assert.Equal("ıt follows", "IT FOLLOWS".ToLower());

            Assert.Equal("it follows", SearchText.Normalize("IT FOLLOWS"));
            Assert.Equal("istanbul", SearchText.Normalize("İstanbul"));
            Assert.Equal("agir", SearchText.Normalize("Ağır"));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
