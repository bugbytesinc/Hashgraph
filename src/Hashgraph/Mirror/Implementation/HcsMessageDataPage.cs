using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror.Implementation;
/// <summary>
/// Contains a paged list of HCS Message information.
/// </summary>
internal class HcsMessageDataPage : Page<HcsMessageData>
{
    /// <summary>
    /// List of HCS Message.
    /// </summary>
    [JsonPropertyName("messages")]
    public HcsMessageData[]? Messages { get; set; }
    /// <summary>
    /// Enumerates the list of messages.
    /// </summary>
    /// <returns>
    /// An enumerator listing the messages in the list.
    /// </returns>
    public override IEnumerable<HcsMessageData> GetItems()
    {
        return Messages ?? Array.Empty<HcsMessageData>();
    }
}