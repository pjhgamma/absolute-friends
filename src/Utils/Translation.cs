using RWCustom;

namespace RippleFriends.Utils;

internal static class Translation
{
    public const string Placeholder = "<PLACEHOLDER>";

    public static string Of(string text)
    {
        try
        {
            return Custom.rainWorld?.inGameTranslator?.Translate(text) ?? text;
        }
        catch
        {
            return text;
        }
    }
}
