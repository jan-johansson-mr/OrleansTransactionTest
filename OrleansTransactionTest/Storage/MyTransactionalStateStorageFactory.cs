using Orleans.Runtime;
using Orleans.Transactions.Abstractions;

namespace OrleansTransactionTest.Storage;

internal class MyTransactionalStateStorageFactory : ITransactionalStateStorageFactory
{
    public ITransactionalStateStorage<TState> Create<TState>(string stateName, IGrainContext context) where TState : class, new()
    {
        return new MyTransactionalStorage<TState>(stateName, context);
    }
}
