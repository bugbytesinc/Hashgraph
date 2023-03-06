#pragma warning disable CS1591
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;
/// <summary>
/// Contains a paged list of HCS Message information.
/// </summary>
public class HcsMessageList : PagedList<HcsMessage>
{
    /// <summary>
    /// List of HCS Message.
    /// </summary>
    [JsonPropertyName("messages")]
    public HcsMessage[]? Messages { get; set; }
    /// <summary>
    /// Enumerates the list of messages.
    /// </summary>
    /// <returns>
    /// An enumerator listing the messages in the list.
    /// </returns>
    public override IEnumerable<HcsMessage> GetItems()
    {
        return Messages ?? Array.Empty<HcsMessage>();
    }
}