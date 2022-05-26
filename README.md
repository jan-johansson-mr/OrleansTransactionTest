# Orleans transaction test

This is a sample of where Orleans "transaction engine" consistently fails, depending on how you set timing in the execution path.

The sample initially fails with "normal" execution path, where there is [no delay](https://github.com/jan-johansson-mr/OrleansTransactionTest/blob/6aae7c42991d46737e4ae3b61f4e431654a14316/OrleansTransactionTest/Program.cs#L71) when running.

The transaction succeeds when changing the delay to maybe [100ms or more](https://github.com/jan-johansson-mr/OrleansTransactionTest/blob/6aae7c42991d46737e4ae3b61f4e431654a14316/OrleansTransactionTest/Program.cs#L71), but will take some additional time to execute due to delays in the transaction handling by Orleans.

The topmost exception in the sample is [OrleansTransactionAbortedException](https://docs.microsoft.com/en-us/dotnet/api/orleans.transactions.orleanstransactionabortedexception?view=orleans-3.0). The [documentation](https://docs.microsoft.com/en-us/dotnet/orleans/grains/transactions) states that the transaction can be retried when OrleansTransactionAbortedException is detected (but the retry is failing, depending on timing, as the sample illustrates).
