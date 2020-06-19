using System;
using System.Linq;

namespace Proto
{
    public sealed partial class ContractFunctionResult
    {
        internal Hashgraph.ContractCallResult ToContractCallResult()
        {
            return new Hashgraph.ContractCallResult
            {
                Result = new Hashgraph.ContractCallResultData(ContractCallResult.ToArray()),
                Error = ErrorMessage,
                Bloom = Bloom.ToArray(),
                Gas = GasUsed,
                Events = LogInfo?.Select(log => new Hashgraph.ContractEvent
                {
                    Contract = log.ContractID.ToAddress(),
                    Bloom = log.Bloom.ToArray(),
                    Topic = log.Topic.Select(bs => new ReadOnlyMemory<byte>(bs.ToArray())).ToArray(),
                    Data = new Hashgraph.ContractCallResultData(log.Data.ToArray()),
                }).ToArray() ?? new Hashgraph.ContractEvent[0],
                CreatedContracts = CreatedContractIDs?.Select(id => id.ToAddress()).ToArray() ?? new Hashgraph.Address[0]
            };
        }
    }
}
