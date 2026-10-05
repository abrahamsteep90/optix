using Microsoft.AspNetCore.Mvc;
using Movies.Application.Actors;
using Movies.Application.Common;

namespace Movies.Api.Controllers;

[ApiController]
[Route("api/actors")]
public sealed class ActorsController(IActorService actors) : ControllerBase
{
    /// <summary>Find actors by name, for example to suggest names in an actor filter.</summary>
    /// <remarks>Empty until cast data has been loaded from TMDB (see the README).</remarks>
    /// <response code="200">One page of actors, those in the most movies first.</response>
    /// <response code="400">A parameter is invalid.</response>
    [HttpGet]
    [ProducesResponseType<PagedResult<ActorDto>>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<PagedResult<ActorDto>> Search([FromQuery] ActorSearchQuery query, CancellationToken cancellationToken) =>
        actors.SearchAsync(query, cancellationToken);
}
