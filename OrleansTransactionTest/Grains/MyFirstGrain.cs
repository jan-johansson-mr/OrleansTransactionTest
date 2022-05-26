using Orleans.Concurrency;
using Orleans.Transactions.Abstractions;

using OrleansTransactionTest.Contracts;

namespace OrleansTransactionTest.Grains;

[Reentrant]
internal class MyFirstGrain : IMyFirstGrain
{
    private readonly ITransactionalState<MyState> _transactionalState;

    public MyFirstGrain(
        [TransactionalState(nameof(MyState), "TransactionStore")]
        ITransactionalState<MyState> transactionalState)
    {
        _transactionalState = transactionalState;
    }

    public async Task SetName(string name)
    {
        await _transactionalState.PerformUpdate(state => state.Name = name);
    }

    public async Task<string> GetName()
    {
        return await _transactionalState.PerformRead(state => state.Name);
    }
}
