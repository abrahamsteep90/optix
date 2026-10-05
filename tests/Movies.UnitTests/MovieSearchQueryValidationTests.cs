using System.ComponentModel.DataAnnotations;
using Movies.Application.Movies;

namespace Movies.UnitTests;

/// <summary>
/// The API rejects these automatically ([ApiController] runs the same DataAnnotations checks)
/// and answers 400 with the list of problems.
/// </summary>
public class MovieSearchQueryValidationTests
{
    [Fact]
    public void Defaults_are_valid() => Assert.Empty(Validate(new MovieSearchQuery()));

    [Theory]
    [InlineData(0, 20, nameof(MovieSearchQuery.Page))]
    [InlineData(1, 0, nameof(MovieSearchQuery.PageSize))]
    [InlineData(1, 101, nameof(MovieSearchQuery.PageSize))]
    public void Rejects_paging_out_of_range(int page, int pageSize, string invalidMember)
    {
        var error = Assert.Single(Validate(new MovieSearchQuery { Page = page, PageSize = pageSize }));

        Assert.Equal([invalidMember], error.MemberNames);
    }

    [Fact]
    public void Accepts_the_largest_page_size() =>
        Assert.Empty(Validate(new MovieSearchQuery { PageSize = MovieSearchQuery.MaxPageSize }));

    [Fact]
    public void Rejects_very_long_search_text()
    {
        var error = Assert.Single(Validate(new MovieSearchQuery { Search = new string('a', 201) }));

        Assert.Equal([nameof(MovieSearchQuery.Search)], error.MemberNames);
    }

    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }
}
