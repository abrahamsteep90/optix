namespace Movies.Domain;

public class Actor
{
    private string _name = "";

    public int Id { get; set; }

    /// <summary>The person's TMDB id; unique, so the same actor is stored once across all movies.</summary>
    public int TmdbPersonId { get; set; }

    public required string Name
    {
        get => _name;
        set
        {
            _name = value;
            SearchName = SearchText.Normalize(value);
        }
    }

    /// <summary>Lower-case, accent-free copy of <see cref="Name"/>, kept in sync automatically. Used for searching.</summary>
    public string SearchName { get; private set; } = "";

    /// <summary>TMDB image path of the actor's photo, e.g. "/abc123.jpg".</summary>
    public string? ProfilePath { get; set; }

    public List<CastMember> Roles { get; set; } = [];
}
