using BepInEx;
using RippleFriends.Addons;
using RippleFriends.Options;

namespace RippleFriendsExample;

[BepInPlugin(GUID, Name, Version)]
// Declare installed Ripple Friends plugins as dependencies instead of bundling their DLLs.
// Duplicate plugin IDs can load stale or incompatible code.
[BepInDependency(RippleFriends.Core.Plugin.GUID)]
public sealed class Plugin : AddonPlugin
{
    public const string GUID = "pjhgamma.ripplefriends.example";

    public const string Name = "Ripple Friends: Example";

    public const string Version = "0.1.0";

    public override string AddonDescription => "Adds friendship and ownership rules for Hunter Long Legs and the green Neuron.";

    // Declare the Ripple Friends version this addon was built against for compatibility checks.
    public override string RippleFriendsVersion => RippleFriends.Plugin.Version;

    // List the Ripple Friends addon IDs that must be applied before this addon's controls can be enabled.
    public override string[] AddonDependencies => [RippleFriends.Core.Plugin.GUID];

    // Override this property when the plugin needs its own independent Remix menu in addition to its addon panel.
    protected override OptionInterface RemixMenu => RippleFriendsExample.RemixMenu.Instance;

    // Bind addon options here so profiles and the main Addons tab can discover them.
    protected override void Bind(Addon addon) => AddonConfig.Bind(addon);

    // Build this addon's options inside its card in the shared Addons tab; omit the override when it has no controls.
    protected override void BuildMenu(MenuBuilder menu) => AddonMenu.Build(menu);

    // Register gameplay behavior when Ripple Friends enables this addon.
    protected override void Enable()
    {
        // Register external rules here when they need matching cleanup in Disable.
        RelationshipRules.Register();

        Logger.LogInfo($"Hunter Long Legs example enabled with {AddonContext.OptionCount} options");
    }

    // Unregister gameplay behavior when Ripple Friends disables this addon.
    protected override void Disable()
    {
        RelationshipRules.Unregister();

        Logger.LogInfo("Hunter Long Legs example disabled");
    }
}
