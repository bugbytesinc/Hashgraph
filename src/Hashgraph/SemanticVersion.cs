using Hashgraph.Implementation;
using System;

namespace Hashgraph
{
    /// <summary>
    /// Represents a Semantic Version Number (major, minor, patch)
    /// </summary>
    public sealed class SemanticVersion : IEquatable<SemanticVersion>
    {
        /// <summary>
        /// Major version number. Changes between major version numbers
        /// indicate incompatible API Changes.
        /// </summary>
        public int Major { get; private set; }
        /// <summary>
        /// Minor verion number. Changes between minor version numbers
        /// indicate additions to the API and other backwards compatible
        /// changes.
        /// </summary>
        public int Minor { get; private set; }
        /// <summary>
        /// Patch version number. Changes between patch versions indicate
        /// backwards compatible bug fixes.
        /// </summary>
        public int Patch { get; private set; }
        /// <summary>
        /// A special designation of an semantic version that can't be created.
        /// It represents the absence of a information.  It will resolve to 
        /// the values of 0, 0, 0.
        /// </summary>
        public static SemanticVersion None { get; private set; } = new SemanticVersion();
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
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="other">
        /// The other <code>SemanticVersion</code> object to compare.
        /// </param>
        /// <returns>
        /// True if the  Major, Minor and Patch values are identical to the 
        /// other <code>SemanticVersion</code> object.
        /// </returns>
        public bool Equals(SemanticVersion other)
        {
            if (other is null)
            {
                return false;
            }
            return
                Major == other.Major &&
                Minor == other.Minor &&
                Patch == other.Patch;
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="obj">
        /// The other <code>SemanticVersion</code> object to compare (if it is
        /// an <code>SemanticVersion</code>).
        /// </param>
        /// <returns>
        /// If the other object is an SemanticVersion, then <code>True</code> 
        /// if the Major, Minor and Patch values are identical 
        /// to the other <code>SemanticVersion</code> object, otherwise 
        /// <code>False</code>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is SemanticVersion other)
            {
                return Equals(other);
            }
            return false;
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <returns>
        /// A unique hash of the contents of this <code>SemanticVersion</code> 
        /// object.  Only consistent within the current instance of 
        /// the application process.
        /// </returns>
        public override int GetHashCode()
        {
            return $"SemanticVersion:{Major}:{Minor}:{Patch}".GetHashCode();
        }
        /// <summary>
        /// Equals implementation.
        /// </summary>
        /// <param name="left">
        /// Left hand <code>SemanticVersion</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>SemanticVersion</code> argument.
        /// </param>
        /// <returns>
        /// True if the Major, Minor and Patch values are identical 
        /// within each <code>SemanticVersion</code> objects.
        /// </returns>
        public static bool operator ==(SemanticVersion left, SemanticVersion right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }
        /// <summary>
        /// Not equals implementation.
        /// </summary>
        /// <param name="left">
        /// Left hand <code>SemanticVersion</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>SemanticVersion</code> argument.
        /// </param>
        /// <returns>
        /// <code>False</code> if the Major, Minor and Patch versions 
        /// are identical within each <code>SemanticVersion</code> object.  
        /// <code>True</code> if they are not identical.
        /// </returns>
        public static bool operator !=(SemanticVersion left, SemanticVersion right)
        {
            return !(left == right);
        }
    }
}
