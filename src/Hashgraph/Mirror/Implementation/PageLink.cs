using System.Text.Json.Serialization;

namespace Hashgraph.Mirror.Implementation;
/// <summary>
/// A links object typically returned from the mirror node when more 
/// paged data is available.
/// </summary>
public class PageLink
{
    /// <summary>
    /// The URL of a link to call to retrieve the next set of paged data.
    /// </summary>
    [JsonPropertyName("next")]
    public string? Next { get; set; }
}
