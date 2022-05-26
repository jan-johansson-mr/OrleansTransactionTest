using Orleans;

namespace OrleansTransactionTest.Contracts;

public interface IMyThirdGrain : IGrainWithGuidKey
{
    [Transaction(TransactionOption.Create)]
    Task SetName(string forname, string surname);

    [Transaction(TransactionOption.Create)]
    Task SetNameAndFail(string forname, string surname);
}
