using Hashgraph.Implementation;

namespace Hashgraph
{
    /// <summary>
    /// Represents a Semantic Version Number (major, minor, patch)
    /// </summary>
    public sealed record SemanticVersion
    {
        /// <summary>
        /// Major version number. Changes between major version numbers
        /// indicate incompatible API Changes.
        /// </summary>
        public int Major { get; private init; }
        /// <summary>
        /// Minor verion number. Changes between minor version numbers
        /// indicate additions to the API and other backwards compatible
        /// changes.
        /// </summary>
        public int Minor { get; private init; }
        /// <summary>
        /// Patch version number. Changes between patch versions indicate
        /// backwards compatible bug fixes.
        /// </summary>
        public int Patch { get; private init; }
        /// <summary>
        /// A special designation of an semantic version that can't be created.
        /// It represents the absence of a information.  It will resolve to 
        /// the values of 0, 0, 0.
        /// </summary>
        public static SemanticVersion None { get; } = new SemanticVersion();
        /// <summary>
        /// Internal Constructor representing the "None" version of an
        /// version.  This is a special construct indicating the version
        /// number is not known or is not specified.
        /// </summary>
        private SemanticVersion()
        {
            Major = 0;
            Minor = 0;
            Patch = 0;
        }
        /// <summary>
        /// Public Constructor, an <code>SemanticVersion</code> is immutable after creation.
        /// </summary>
        /// <param name="major">
        /// Major Version Number.
        /// </param>
        /// <param name="minor">
        /// Minor Version Number.
        /// </param>
        /// <param name="patch">
        /// Patch Version Number
        /// </param>
        public SemanticVersion(int major, int minor, int patch)
        {
            Major = RequireInputParameter.MajorNumber(major);
            Minor = RequireInputParameter.MinorNumber(minor);
            Patch = RequireInputParameter.PatchNumber(patch);
        }
    }
}
