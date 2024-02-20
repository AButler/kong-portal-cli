namespace Kong.Portal.CLI.Services.Models;

internal record Difference<T>(DifferenceType DifferenceType, string? SyncId, string? Id, T Entity);

internal static class Difference
{
    public static Difference<T> Add<T>(string syncId, T entity) => new(DifferenceType.Add, syncId, null, entity);

    public static Difference<T> Update<T>(string syncId, string id, T entity) => new(DifferenceType.Update, syncId, id, entity);

    public static Difference<T> Delete<T>(string id, T entity) => new(DifferenceType.Delete, null, id, entity);

    public static Difference<T> NoChange<T>(string syncId, string id, T entity) => new(DifferenceType.NoChange, syncId, id, entity);

    public static Difference<T> UpdateOrNoChange<T>(string syncId, string id, T serverEntity, T localEntity)
        where T : IEquatable<T>
    {
        var differenceType = serverEntity.Equals(localEntity) ? DifferenceType.NoChange : DifferenceType.Update;

        return new Difference<T>(differenceType, syncId, id, localEntity);
    }
}

internal enum DifferenceType
{
    NoChange,
    Add,
    Update,
    Delete
}
