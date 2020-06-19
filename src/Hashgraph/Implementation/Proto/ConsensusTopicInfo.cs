using System.Linq;

namespace Proto
{
    public sealed partial class ConsensusTopicInfo
    {
        internal Hashgraph.TopicInfo ToTopicInfo()
        {
            return new Hashgraph.TopicInfo
            {
                Memo = Memo,
                RunningHash = RunningHash.ToArray(),
                SequenceNumber = SequenceNumber,
                Expiration = ExpirationTime.ToDateTime(),
                Administrator = AdminKey?.ToEndorsement(),
                Participant = SubmitKey?.ToEndorsement(),
                AutoRenewPeriod = AutoRenewPeriod.ToTimeSpan(),
                RenewAccount = AutoRenewAccount?.ToAddress()
            };
        }
    }
}