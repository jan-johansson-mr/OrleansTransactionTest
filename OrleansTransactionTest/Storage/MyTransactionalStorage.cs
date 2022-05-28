using Orleans.Runtime;
using Orleans.Transactions.Abstractions;

namespace OrleansTransactionTest.Storage;

internal class MyTransactionalStorage<TState> : ITransactionalStateStorage<TState> where TState : class, new()
{
    private readonly IGrainContext _context;
    private readonly List<PendingTransactionState<TState>> _pendingStates = new();

    private string _eTag = string.Empty;

    public MyTransactionalStorage(string _, IGrainContext context)
    {
        _context = context;
    }

    public Task<TransactionalStorageLoadResponse<TState>> Load()
    {
        _eTag = Guid.NewGuid().ToString("N");

        var response = new TransactionalStorageLoadResponse<TState>(_eTag
            , new TState()
            , 0L
            , new TransactionalStateMetaData()
            , _pendingStates);

        return Task.FromResult(response);
    }

    public Task<string> Store(
        string expectedETag
        , TransactionalStateMetaData metadata
        , List<PendingTransactionState<TState>> statesToPrepare
        , long? commitUpTo, long? abortAfter)
    {
        if (_eTag != expectedETag)
        {
            throw new ArgumentException("Etag does not match", nameof(expectedETag));
        }

        // abort
        if (abortAfter.HasValue && _pendingStates.Count != 0)
        {
            var index = _pendingStates.FindIndex(pendingState => pendingState.SequenceId > abortAfter.Value);

            if (index != -1)
            {
                _pendingStates.RemoveRange(index, _pendingStates.Count - index);
            }
        }

        // prepare
        if (statesToPrepare?.Count > 0)
        {
            foreach (var stateToPrepare in statesToPrepare)
            {
                var index = _pendingStates.FindIndex(pendingState => pendingState.SequenceId >= stateToPrepare.SequenceId);

                if (index == -1)
                {
                    _pendingStates.Add(stateToPrepare); //append
                }
                else if (_pendingStates[index].SequenceId == stateToPrepare.SequenceId)
                {
                    _pendingStates[index] = stateToPrepare;  //replace
                }
                else
                {
                    _pendingStates.Insert(index, stateToPrepare); //insert
                }
            }
        }

        // commit
        if (commitUpTo.HasValue)
        {
            var index = _pendingStates.FindIndex(pendingState => pendingState.SequenceId == commitUpTo.Value);

            if (index != -1)
            {
                var committedState = _pendingStates[index];
                _pendingStates.RemoveRange(0, index + 1);
                _eTag = Guid.NewGuid().ToString("N");
            }
            else if (commitUpTo.Value < _pendingStates.Select(pendingState => pendingState.SequenceId).Max())
            {
                throw new InvalidOperationException($"Transactional state corrupted. Missing prepared record (SequenceId={commitUpTo.Value}) for committed transaction.");
            }
        }

        return Task.FromResult(_eTag);
    }
}
