namespace Proto
{
    public sealed partial class ContractGetInfoResponse
    {
        public static partial class Types
        {
            public sealed partial class ContractInfo
            {
                internal Hashgraph.ContractInfo ToContractInfo()
                {
                    return new Hashgraph.ContractInfo
                    {
                        Contract = ContractID.ToAddress(),
                        Address = AccountID.ToAddress(),
                        SmartContractId = ContractAccountID,
                        Administrator = AdminKey?.ToEndorsement(),
                        Expiration = ExpirationTime.ToDateTime(),
                        RenewPeriod = AutoRenewPeriod.ToTimeSpan(),
                        Size = Storage,
                        Memo = Memo,
                        Balance = Balance,
                        Tokens = TokenRelationships.ToBalances(),
                        Deleted = Deleted
                    };
                }
            }
        }
    }
}
