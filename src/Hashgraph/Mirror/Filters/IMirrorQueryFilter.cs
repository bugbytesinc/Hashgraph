namespace Hashgraph.Mirror.Filters;

/// <summary>
/// Represents a basic filter to a mirror node query, it
/// can represent a time constraint, account constraint etc.
/// </summary>
public interface IMirrorQueryFilter
{
    /// <summary>
    /// The query parameter name recognized by the remote mirror node.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// The value of the query parameter sent to the mirror node.
    /// </summary>
    public string Value { get; }
}
