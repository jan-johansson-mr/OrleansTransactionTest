using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Transactions.Abstractions;

using OrleansTransactionTest.Contracts;
using OrleansTransactionTest.Storage;

namespace OrleansTransactionTest
{
    internal class Program
    {
        static async Task Main()
        {
            using var host = new HostBuilder()
                .UseOrleans(builder =>
                {
                    builder.UseTransactions();
                    builder.UseLocalhostClustering();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingletonNamedService<ITransactionalStateStorageFactory>("TransactionStore", (provider, name) =>
                    {
                        return new MyTransactionalStateStorageFactory();
                    });
                })
                .Build();

            await host.StartAsync();

            var factory = host.Services.GetRequiredService<IGrainFactory>();

            var id = Guid.NewGuid();
            var grain = factory.GetGrain<IMyThirdGrain>(id);

            // Unit of work
            await grain.SetName("First", "Last");

            var firstGrain = factory.GetGrain<IMyFirstGrain>(id);
            var secondGrain = factory.GetGrain<IMySecondGrain>(id);

            var firstName = await firstGrain.GetName();
            var lastName = await secondGrain.GetName();

            Console.WriteLine($"First name '{firstName}', last name '{lastName}'");

            try
            {
                await grain.SetNameAndFail("First", "Second");
            }
            catch
            {
            }

            firstName = await firstGrain.GetName();
            lastName = await secondGrain.GetName();

            Console.WriteLine($"First name '{firstName}', last name '{lastName}'");

            try
            {
                await grain.SetName("First", "First");
            }
            catch
            {
                //await Task.Delay(100);
                await grain.SetName("First name", "Last name");
            }

            firstName = await firstGrain.GetName();
            lastName = await secondGrain.GetName();

            Console.WriteLine($"First name '{firstName}', last name '{lastName}'");

            await grain.SetName("First", "Last");

            firstGrain = factory.GetGrain<IMyFirstGrain>(id);
            secondGrain = factory.GetGrain<IMySecondGrain>(id);

            firstName = await firstGrain.GetName();
            lastName = await secondGrain.GetName();

            Console.WriteLine($"First name '{firstName}', last name '{lastName}'");

            await host.StopAsync();
        }
    }
}
