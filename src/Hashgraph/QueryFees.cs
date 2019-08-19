namespace Hashgraph
{
    /// <summary>
    /// Temporary Location to Store Network Query Fees
    /// </summary>
    /// <remarks>
    /// Once a query fee framework is established by the network
    /// we expect this class to be removed and replaced by a more
    /// robust mechanism.  In the meanwhile, all of the values
    /// in this class are writable so that client software can
    /// update query fees as a work-around if fees change suddenly
    /// in the future before this library can be updated.
    /// </remarks>
    public static class QueryFees
    {
        public static long GetClaim = 0;
        public static long GetContractRecords = 3;
        public static long GetAccountRecords = 2;
        public static long GetFileContent = 8;
        public static long GetStakers = 0;
        public static long GetAccountInfo = 8;
        public static long GetAccountBalance = 0;
        public static long QueryContract = 400_000;
        public static long GetAddressFromSmartContractId = 8;
        public static long GetContractBytecode = 41_666_666;  // This is excessivley high in most cases.
        public static long GetContractInfo = 8;
        public static long GetFileInfo = 8;

        // Each Transaction Record can Charge a different amount for retrieval.
        public static long GetTransactionRecord_UnknownType = 8;
        public static long GetTransactionRecord_Transfer = 8;
        public static long GetTransactionRecord_CreateContract = 9;
        public static long GetTransactionRecord_CreateAccount = 8;
        public static long GetTransactionRecord_AppendFile = 8;
        public static long GetTransactionRecord_CreateFile = 8;
        public static long GetTransactionRecord_AddClaim = 1;
        public static long GetTransactionRecord_CallContract = 9;
        public static long GetTransactionRecord_DeleteFile = 7;
        public static long GetTransactionRecord_UpdateFile = 8;
        public static long GetTransactionRecord_DeleteClaim = 1;
    }
}
