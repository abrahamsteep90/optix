using Microsoft.AspNetCore.Mvc;
using Movies.Application.Common;
using Movies.Application.Movies;

namespace Movies.Api.Controllers;

[ApiController]
[Route("api/movies")]
public sealed class MoviesController(IMovieService movies) : ControllerBase
{
    /// <summary>Search, filter, sort and page through movies.</summary>
    /// <remarks>Every parameter is optional. Without any, you get the 20 most popular movies.</remarks>
    /// <response code="200">One page of matching movies, plus the total count.</response>
    /// <response code="400">A parameter is invalid, for example pageSize above 100.</response>
    [HttpGet]
    [ProducesResponseType<PagedResult<MovieSummaryDto>>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<PagedResult<MovieSummaryDto>> Search(
        [FromQuery] MovieSearchQuery query, CancellationToken cancellationToken) =>
        movies.SearchAsync(query, cancellationToken);

    /// <summary>One movie, with its genres and cast.</summary>
    /// <response code="200">The movie.</response>
    /// <response code="404">There is no movie with this id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<MovieDetailsDto>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<MovieDetailsDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var movie = await movies.GetByIdAsync(id, cancellationToken);
        return movie is null ? NotFound() : movie;
    }
}
