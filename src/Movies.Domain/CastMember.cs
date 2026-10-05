namespace Movies.Domain;

/// <summary>An actor's role in a movie.</summary>
public class CastMember
{
    public int MovieId { get; set; }

    public Movie Movie { get; set; } = null!;

    public int ActorId { get; set; }

    public Actor Actor { get; set; } = null!;

    public string? Character { get; set; }

    /// <summary>Position in the credits (0 = top billed).</summary>
    public int BillingOrder { get; set; }
}
