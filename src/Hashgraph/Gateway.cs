namespace Hashgraph
{
    public sealed class Gateway : Address
    {
        public string Url { get; private set; }
        public Gateway(string url, long realmNum, long shardNum, long accountNum) :
            base(realmNum, shardNum, accountNum)
        {
            Url = url;
        }
    }
}
