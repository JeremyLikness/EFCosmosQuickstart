# EF Core Azure Cosmos DB Quickstart Comparison

Mirrors the Azure Cosmos DB quickstart using EF Core 5 and EF Core 6.

The projects are pre-configured to run against the [Azure Cosmos DB emulator](https://docs.microsoft.com/azure/cosmos-db/local-emulator) on your local machine, but you can override that in the `Program.cs`.

> ⚠ **NOTE:** apps are for demo purposes _only_. Storing keys and other sensitive information in the source code is a major security risk. The connection information contained in these projects
are well-known and [hard-coded in the emulator](https://docs.microsoft.com/azure/cosmos-db/local-emulator?#authenticate-requests).

## Azure Cosmos DB SDK

The `todonoef` folder contains the quickstart that demonstrates using the Azure Cosmos DB SDK (4.x). 

## EF Core 5

This implements the quickstart using the EF Core 5 provider for Azure Cosmos DB. Notable changes:

- Arrays for `Parent[]` and `Child[]` and `Pet[]` were converted to `IList<T>` because EF Core doesn't support navigation properties that use an array.
- The `FamilyContext` class is added to map the `Family` class to the provider.
- The `CreateContainerAsync` method goes away (EF Core does this automatically).
- `QueryItemsAsync` uses strongly-typed LINQ for the query instead of a magic SQL string.

Note that EF Core 5 defaults to complex types stored as separate entities with a foreign key relationship. To model the proper behavior and embed child instances in the parent
requires some configuration of the model:

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.Entity<Family>()
        .HasPartitionKey(nameof(Family.LastName))
        .OwnsMany(f => f.Parents);

    builder.Entity<Family>()
        .OwnsMany(f => f.Children)
            .OwnsMany(c => c.Pets);

    builder.Entity<Family>()
        .OwnsOne(f => f.Address);
}
```

Otherwise running this should produce identical results compared to the direct SDK version.

## EF Core 6

EF Core 6 comes with caveats as EF Core 5 with the exception of complex types. EF Core now detects when the provider is a document database and changes the convention from foreign keys to implicit 
ownership. The model configuraton now just needs to provide the partition key.

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.Entity<Family>()
        .HasPartitionKey(nameof(Family.LastName));
}
```

.



