using Hashgraph.Test.Fixtures;
using Xunit;

namespace Hashgraph.Tests;

public class SemanticVersionTests
{
    [Fact(DisplayName = "SemanticVersion: Equivalent SemanticVersions are considered Equal")]
    public void EquivalentSemanticVersionAreConsideredEqual()
    {
        var major = Generator.Integer(0, 200);
        var minor = Generator.Integer(0, 200);
        var patch = Generator.Integer(0, 200);
        var sv1 = new SemanticVersion(major, minor, patch);
        var sv2 = new SemanticVersion(major, minor, patch);
        Assert.Equal(sv1, sv2);
        Assert.True(sv1 == sv2);
        Assert.False(sv1 != sv2);
        Assert.True(sv1.Equals(sv2));
        Assert.True(sv2.Equals(sv1));
        Assert.True(null as SemanticVersion == null as SemanticVersion);
    }
    [Fact(DisplayName = "SemanticVersion: Disimilar SemanticVersions are not considered Equal")]
    public void DisimilarSemanticVersionsAreNotConsideredEqual()
    {
        var major = Generator.Integer(0, 200);
        var minor = Generator.Integer(0, 200);
        var patch = Generator.Integer(0, 200);
        var sv = new SemanticVersion(major, minor, patch);
        Assert.NotEqual(sv, new SemanticVersion(major, minor + 1, patch));
        Assert.NotEqual(sv, new SemanticVersion(major + 1, minor, patch));
        Assert.NotEqual(sv, new SemanticVersion(major, minor, patch + 1));
        Assert.False(sv == new SemanticVersion(major, minor, patch + 1));
        Assert.True(sv != new SemanticVersion(major, minor, patch + 1));
        Assert.False(sv.Equals(new SemanticVersion(major + 1, minor, patch)));
        Assert.False(sv.Equals(new SemanticVersion(major, minor + 1, patch)));
        Assert.False(sv.Equals(new SemanticVersion(major, minor, patch + 1)));
    }
    [Fact(DisplayName = "SemanticVersion: Comparing with null are not considered equal.")]
    public void NullSemanticVersionsAreNotConsideredEqual()
    {
        object asNull = null;
        var major = Generator.Integer(0, 200);
        var minor = Generator.Integer(0, 200);
        var patch = Generator.Integer(0, 200);
        var sv = new SemanticVersion(major, minor, patch);
        Assert.False(sv == null);
        Assert.False(null == sv);
        Assert.True(sv != null);
        Assert.False(sv.Equals(null));
        Assert.False(sv.Equals(asNull));
    }
    [Fact(DisplayName = "SemanticVersion: Comparing with other objects are not considered equal.")]
    public void OtherObjectsAreNotConsideredEqual()
    {
        var major = Generator.Integer(0, 200);
        var minor = Generator.Integer(0, 200);
        var patch = Generator.Integer(0, 200);
        var sv = new SemanticVersion(major, minor, patch);
        Assert.False(sv.Equals("Something that is not an SemanticVersion"));
    }
    [Fact(DisplayName = "SemanticVersion: SemanticVersion cast as object still considered equivalent.")]
    public void SemanticVersionCastAsObjectIsconsideredEqual()
    {
        var major = Generator.Integer(0, 200);
        var minor = Generator.Integer(0, 200);
        var patch = Generator.Integer(0, 200);
        var sv = new SemanticVersion(major, minor, patch);
        object equivalent = new SemanticVersion(major, minor, patch);
        Assert.True(sv.Equals(equivalent));
        Assert.True(equivalent.Equals(sv));
    }
    [Fact(DisplayName = "SemanticVersion: SemanticVersion as objects but reference equal are same.")]
    public void ReferenceEqualIsconsideredEqual()
    {
        var major = Generator.Integer(0, 200);
        var minor = Generator.Integer(0, 200);
        var patch = Generator.Integer(0, 200);
        var sv = new SemanticVersion(major, minor, patch);
        object reference = sv;
        Assert.True(sv.Equals(reference));
        Assert.True(reference.Equals(sv));
    }
}