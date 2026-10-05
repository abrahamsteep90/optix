using System.Net;
using System.Text;
using Movies.Infrastructure.Tmdb;

namespace Movies.UnitTests;

public class TmdbClientTests
{
    [Fact]
    public async Task Searches_by_title_and_year_and_reads_the_results()
    {
        var handler = new StubHandler(HttpStatusCode.OK, """
            {"page":1,"results":[{"adult":false,"id":634649,"title":"Spider-Man: No Way Home",
            "original_title":"Spider-Man: No Way Home","release_date":"2021-12-15",
            "poster_path":"/1g0dhYtq4irTY1GPXvft6k4YLjm.jpg","vote_average":7.9}],"total_pages":1,"total_results":1}
            """);

        var results = await CreateClient(handler).SearchMoviesAsync("Spider-Man: No Way Home", 2021, CancellationToken.None);

        var movie = Assert.Single(results);
        Assert.Equal(
            new TmdbMovie(634649, "Spider-Man: No Way Home", "Spider-Man: No Way Home", "2021-12-15", "/1g0dhYtq4irTY1GPXvft6k4YLjm.jpg"),
            movie);

        var url = handler.LastRequest!.RequestUri!.AbsoluteUri;
        Assert.StartsWith("https://api.test/3/search/movie?", url);
        Assert.Contains("query=Spider-Man%3A%20No%20Way%20Home", url);
        Assert.Contains("primary_release_year=2021", url);
    }

    [Fact]
    public async Task Searching_without_a_year_leaves_the_year_out()
    {
        var handler = new StubHandler(HttpStatusCode.OK, """{"page":1,"results":[]}""");

        var results = await CreateClient(handler).SearchMoviesAsync("Encanto", null, CancellationToken.None);

        Assert.Empty(results);
        Assert.DoesNotContain("year", handler.LastRequest!.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task Reads_the_cast_of_a_movie()
    {
        var handler = new StubHandler(HttpStatusCode.OK, """
            {"id":634649,"cast":[{"id":1136406,"name":"Tom Holland","character":"Peter Parker / Spider-Man",
            "order":0,"profile_path":"/tom.jpg","known_for_department":"Acting"}],"crew":[]}
            """);

        var cast = await CreateClient(handler).GetCastAsync(634649, CancellationToken.None);

        Assert.Equal([new TmdbCastCredit(1136406, "Tom Holland", "Peter Parker / Spider-Man", 0, "/tom.jpg")], cast);
        Assert.StartsWith("https://api.test/3/movie/634649/credits", handler.LastRequest!.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task A_rejected_token_comes_back_as_401()
    {
        var handler = new StubHandler(HttpStatusCode.Unauthorized, """{"status_code":7,"success":false}""");

        var error = await Assert.ThrowsAsync<HttpRequestException>(
            () => CreateClient(handler).SearchMoviesAsync("Encanto", null, CancellationToken.None));

        Assert.Equal(HttpStatusCode.Unauthorized, error.StatusCode);
    }

    private static TmdbClient CreateClient(HttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://api.test/3/") });

    private sealed class StubHandler(HttpStatusCode status, string json) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });
        }
    }
}
