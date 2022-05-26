namespace OrleansTransactionTest.Grains;

[Serializable]
[Orleans.GenerateSerializer]
internal class MyState
{
    [Orleans.Id(0)]
    public string Name { get; set; } = string.Empty;
}
