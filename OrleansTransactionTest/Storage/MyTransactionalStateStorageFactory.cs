using Orleans.Runtime;
using Orleans.Storage;
using Orleans.Transactions.Abstractions;

namespace OrleansTransactionTest.Storage;

internal class MyTransactionalStateStorageFactory : ITransactionalStateStorageFactory
{
    private readonly IGrainStorageSerializer _grainStorageSerializer;

    public MyTransactionalStateStorageFactory(IGrainStorageSerializer grainStorageSerializer)
    {
        _grainStorageSerializer = grainStorageSerializer;
    }

    public ITransactionalStateStorage<TState> Create<TState>(string stateName, IGrainContext context) where TState : class, new()
    {
        return new MyTransactionalStorage<TState>(stateName, context, _grainStorageSerializer);
    }
}
