using BepInEx;
using RippleFriends.Options;
using System.Security.Permissions;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace RippleFriends.Items;

[BepInPlugin(GUID, Name, RippleFriends.Plugin.Version)]
[BepInDependency(Core.Plugin.GUID)]
public class Plugin : AddonPlugin
{
    public const string GUID = "pjhgamma.ripplefriends.items";

    public const string Name = "Ripple Friends: Items";

    public override string AddonDescription => "Controls interactions with the items Ripple Friends throw or hold.";

    public override string[] AddonDependencies => [Core.Plugin.GUID];

    public override string RippleFriendsVersion => RippleFriends.Plugin.Version;

    protected override void Bind(Addon addon) => Items.Config.Bind(addon);

    protected override void BuildMenu(MenuBuilder menu) => Menu.Build(menu);
}
