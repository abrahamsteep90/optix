using Movies.Application.Common;
using Movies.Application.Movies;

namespace Movies.UnitTests;

public class MovieServiceTests
{
    private readonly FakeMovieRepository _repository = new();
    private readonly MovieService _service;

    public MovieServiceTests() => _service = new MovieService(_repository);

    [Fact]
    public async Task Normalises_the_search_text()
    {
        await _service.SearchAsync(new MovieSearchQuery { Search = "  Pokémon  " }, CancellationToken.None);

        Assert.Equal("pokemon", _repository.LastCriteria!.TitleContains);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Blank_search_text_means_no_title_filter(string? search)
    {
        await _service.SearchAsync(new MovieSearchQuery { Search = search }, CancellationToken.None);

        Assert.Null(_repository.LastCriteria!.TitleContains);
    }

    [Fact]
    public async Task Cleans_up_genre_and_actor_filters()
    {
        await _service.SearchAsync(
            new MovieSearchQuery
            {
                Genres = ["Action", " action ", "", "Science  Fiction"],
                Actors = ["Zoë", "zoe", " "],
            },
            CancellationToken.None);

        Assert.Equal(["action", "science fiction"], _repository.LastCriteria!.Genres);
        Assert.Equal(["zoe"], _repository.LastCriteria.ActorsContain);
    }

    [Theory]
    [InlineData(MovieSortBy.Title, SortDirection.Asc)]
    [InlineData(MovieSortBy.ReleaseDate, SortDirection.Desc)]
    [InlineData(MovieSortBy.Popularity, SortDirection.Desc)]
    [InlineData(MovieSortBy.Rating, SortDirection.Desc)]
    public async Task Picks_a_natural_sort_direction_when_none_is_given(MovieSortBy sortBy, SortDirection expected)
    {
        await _service.SearchAsync(new MovieSearchQuery { SortBy = sortBy }, CancellationToken.None);

        Assert.Equal(expected, _repository.LastCriteria!.SortDirection);
    }

    [Fact]
    public async Task Uses_the_sort_direction_it_is_given()
    {
        await _service.SearchAsync(
            new MovieSearchQuery { SortBy = MovieSortBy.Title, SortDirection = SortDirection.Desc },
            CancellationToken.None);

        Assert.Equal(SortDirection.Desc, _repository.LastCriteria!.SortDirection);
    }

    [Fact]
    public async Task Turns_the_page_number_into_skip_and_take()
    {
        await _service.SearchAsync(new MovieSearchQuery { Page = 3, PageSize = 25 }, CancellationToken.None);

        Assert.Equal(50, _repository.LastCriteria!.Skip);
        Assert.Equal(25, _repository.LastCriteria.Take);
    }

    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(1, 20, 1)]
    [InlineData(20, 20, 1)]
    [InlineData(21, 20, 2)]
    [InlineData(9827, 100, 99)]
    public async Task Works_out_the_number_of_pages(int totalCount, int pageSize, int expectedPages)
    {
        _repository.TotalCount = totalCount;

        var page = await _service.SearchAsync(new MovieSearchQuery { PageSize = pageSize }, CancellationToken.None);

        Assert.Equal(totalCount, page.TotalCount);
        Assert.Equal(expectedPages, page.TotalPages);
    }

    private sealed class FakeMovieRepository : IMovieRepository
    {
        public MovieSearchCriteria? LastCriteria { get; private set; }

        public int TotalCount { get; set; }

        public Task<(IReadOnlyList<MovieSummaryDto> Items, int TotalCount)> SearchAsync(
            MovieSearchCriteria criteria, CancellationToken cancellationToken)
        {
            LastCriteria = criteria;
            return Task.FromResult<(IReadOnlyList<MovieSummaryDto>, int)>(([], TotalCount));
        }

        public Task<MovieDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult<MovieDetailsDto?>(null);
    }
}
