using Orleans.Runtime;
using Orleans.Storage;
using Orleans.Transactions;
using Orleans.Transactions.Abstractions;

namespace OrleansTransactionTest.Storage;

internal class MyTransactionalStorage<TState> : ITransactionalStateStorage<TState> where TState : class, new()
{
    private readonly IGrainContext _context;
    private readonly IGrainStorageSerializer _serializer;
    private readonly TransactionalStateRecord<TState> _record;

    private string _eTag = string.Empty;

    public MyTransactionalStorage(string _, IGrainContext context, IGrainStorageSerializer serializer)
    {
        _context = context;
        _serializer = serializer;
        _record = new TransactionalStateRecord<TState>();
    }

    public Task<TransactionalStorageLoadResponse<TState>> Load()
    {
        _eTag = Guid.NewGuid().ToString("N");

        var response = new TransactionalStorageLoadResponse<TState>(_eTag
            , _record.CommittedState
            , _record.CommittedSequenceId
            , _record.Metadata
            , _record.PendingStates);

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

        _record.Metadata = metadata;

        var pendingStates = _record.PendingStates;

        // abort
        if (abortAfter.HasValue && pendingStates.Count != 0)
        {
            var position = pendingStates.FindIndex(pendingState => pendingState.SequenceId > abortAfter.Value);

            if (position != -1)
            {
                pendingStates.RemoveRange(position, pendingStates.Count - position);
            }
        }

        // prepare
        if (statesToPrepare?.Count > 0)
        {
            foreach (var pendingItem in statesToPrepare)
            {
                var position = pendingStates.FindIndex(pendingState => pendingState.SequenceId >= pendingItem.SequenceId);

                if (position == -1)
                {
                    pendingStates.Add(pendingItem); //append
                }
                else if (pendingStates[position].SequenceId == pendingItem.SequenceId)
                {
                    pendingStates[position] = pendingItem;  //replace
                }
                else
                {
                    pendingStates.Insert(position, pendingItem); //insert
                }
            }
        }

        // commit
        if (commitUpTo.HasValue && commitUpTo.Value > _record.CommittedSequenceId)
        {
            var position = pendingStates.FindIndex(pendingItem => pendingItem.SequenceId == commitUpTo.Value);

            if (position != -1)
            {
                var committedState = pendingStates[position];
                _record.CommittedSequenceId = committedState.SequenceId;
                _record.CommittedState = committedState.State;
                pendingStates.RemoveRange(0, position + 1);
                return Task.FromResult(_eTag = Guid.NewGuid().ToString("N"));
            }
            else
            {
                throw new InvalidOperationException($"Transactional state corrupted. Missing prepared record (SequenceId={commitUpTo.Value}) for committed transaction.");
            }
        }

        return Task.FromResult(_eTag);
    }
}
