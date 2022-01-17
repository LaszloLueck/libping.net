namespace LibPing.Net;

public static class FunctionalExtensions
{
    public static IEnumerable<T> GetNth<T>(this T[] list, int n)
    {
        for (var i = 0; i < list.Length; i += n)
            yield return list[i];
    }
}