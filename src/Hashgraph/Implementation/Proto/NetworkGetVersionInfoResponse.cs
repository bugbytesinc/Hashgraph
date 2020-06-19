namespace Proto
{
    public sealed partial class NetworkGetVersionInfoResponse
    {
        internal Hashgraph.VersionInfo ToVersionInfo()
        {
            return new Hashgraph.VersionInfo
            {
                ApiProtobufVersion = FromSemanticVersion(HapiProtoVersion),
                HederaServicesVersion = FromSemanticVersion(HederaServicesVersion)
            };
        }
        private static Hashgraph.SemanticVersion FromSemanticVersion(SemanticVersion semanticVersion)
        {
            return semanticVersion != null ? new Hashgraph.SemanticVersion(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch) : Hashgraph.SemanticVersion.None;
        }
    }
}
