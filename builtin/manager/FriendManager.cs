using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;

namespace AbsoluteFriends.Manager;

public static class FriendManager
{
    private static readonly List<TabRegistration> _tabs = [];

    internal static IReadOnlyList<TabRegistration> Tabs => _tabs;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void RegisterTab(string id, string title, Action<FriendTab> build, Func<AbstractCreature, bool>? isVisible = null)
    {
        Assembly assembly = Assembly.GetCallingAssembly();

        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("A tab needs an ID.", nameof(id));
        }

        BepInEx.PluginInfo? plugin = Chainloader.PluginInfos.Values.FirstOrDefault(info => info.Instance != null && info.Instance.GetType().Assembly == assembly);
        TabRegistration registration = new(id, title ?? throw new ArgumentNullException(nameof(title)), build ?? throw new ArgumentNullException(nameof(build)), isVisible, plugin?.Metadata.Name, plugin?.Metadata.GUID);
        int index = _tabs.FindIndex(tab => tab.Id == id);

        if (index >= 0)
        {
            _tabs[index] = registration;
        }
        else
        {
            _tabs.Add(registration);
        }
    }

    public static void UnregisterTab(string id) => _tabs.RemoveAll(tab => tab.Id == id);

    internal sealed class TabRegistration(string id, string title, Action<FriendTab> build, Func<AbstractCreature, bool>? isVisible, string? modName = null, string? modId = null)
    {
        internal string Id { get; } = id;

        internal string Title { get; } = title;

        internal string? ModName { get; } = modName;

        internal string? ModId { get; } = modId;

        internal Action<FriendTab> Build { get; } = build;

        internal bool IsVisibleFor(AbstractCreature creature) => isVisible?.Invoke(creature) ?? true;
    }
}
