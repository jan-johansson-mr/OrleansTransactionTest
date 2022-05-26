using Orleans;

namespace OrleansTransactionTest.Contracts;

public interface IMySecondGrain : IGrainWithGuidKey
{
    [Transaction(TransactionOption.CreateOrJoin)]
    Task<string> GetName();

    [Transaction(TransactionOption.Join)]
    Task SetName(string name);
}
