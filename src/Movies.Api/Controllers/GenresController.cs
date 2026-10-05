using Microsoft.AspNetCore.Mvc;
using Movies.Application.Genres;

namespace Movies.Api.Controllers;

[ApiController]
[Route("api/genres")]
public sealed class GenresController(IGenreService genres) : ControllerBase
{
    /// <summary>All genres with how many movies each has, for building a genre filter.</summary>
    /// <response code="200">Every genre, A–Z.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<GenreDto>>(StatusCodes.Status200OK, "application/json")]
    public Task<IReadOnlyList<GenreDto>> GetAll(CancellationToken cancellationToken) =>
        genres.GetAllAsync(cancellationToken);
}
