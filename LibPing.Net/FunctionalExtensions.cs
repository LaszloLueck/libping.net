namespace LibPing.Net;

/// <summary>
/// Functional extensions for usage in Libping.net library
/// </summary>
internal static class FunctionalExtensions
{
    /// <summary>
    /// method to return every nth element from a list beginning from list element 0
    /// </summary>
    /// <param name="list">the array </param>
    /// <param name="n">every n element from the list will returned. example n=2. Every 2nd element from the list would be returned</param>
    /// <typeparam name="T">the type of elements from the list</typeparam>
    /// <returns></returns>
    public static IEnumerable<T> GetNth<T>(this T[] list, int n)
    {
        for (var i = 0; i < list.Length; i += n)
            yield return list[i];
    }

    public static readonly Func<byte, IpAddressFamily, string> CalculateTypeString = (byteValue, addressFamily) =>
    {
        return addressFamily switch
        {
            IpAddressFamily.IpV4 => ((MessageTypeV4) byteValue).ToString(),
            IpAddressFamily.IpV6 => ((MessageTypeV6) byteValue).ToString(),
            _ => "IPAddressFamily not supported!"
        };
    };

    internal static async Task<IEnumerable<T>> ResolveTasks<T>(this IEnumerable<Task<T>> tasks) =>
        await Task.WhenAll(tasks);
}