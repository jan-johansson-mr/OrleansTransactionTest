using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;

using OrleansTransactionTest.Contracts;

namespace OrleansTransactionTest.Grains;

[StatelessWorker]
internal class MyThirdGrain : IMyThirdGrain
{
    private readonly IGrainContext _context;
    private readonly IClusterClient _clusterClient;

    public MyThirdGrain(IGrainContext context, IClusterClient clusterClient)
    {
        _context = context;
        _clusterClient = clusterClient;
    }

    public async Task SetName(string forname, string surname)
    {
        IMyFirstGrain myFirstGrain = _clusterClient.GetGrain<IMyFirstGrain>(_context.GrainId.GetGuidKey());
        IMySecondGrain mySecondGrain = _clusterClient.GetGrain<IMySecondGrain>(_context.GrainId.GetGuidKey());

        await myFirstGrain.SetName(forname);
        await mySecondGrain.SetName(surname);
    }

    public async Task SetNameAndFail(string forname, string surname)
    {
        IMyFirstGrain myFirstGrain = _clusterClient.GetGrain<IMyFirstGrain>(_context.GrainId.GetGuidKey());
        IMySecondGrain mySecondGrain = _clusterClient.GetGrain<IMySecondGrain>(_context.GrainId.GetGuidKey());

        await myFirstGrain.SetName(forname);
        await mySecondGrain.SetName(surname);

        throw new InvalidOperationException();
    }
}
