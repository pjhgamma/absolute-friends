using BepInEx;
using RippleFriends.Options;
using System.Security.Permissions;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace RippleFriends.General;

[BepInPlugin(GUID, Name, RippleFriends.Plugin.Version)]
[BepInDependency(Core.Plugin.GUID)]
public class Plugin : AddonPlugin
{
    public const string GUID = "pjhgamma.ripplefriends.general";

    public const string Name = "Ripple Friends: General";

    public override string AddonDescription => "Controls the general harm and effects that pass between Ripple Friends.";

    public override string[] AddonDependencies => [Core.Plugin.GUID];

    public override string RippleFriendsVersion => RippleFriends.Plugin.Version;

    protected override void Bind(Addon addon) => General.Config.Bind(addon);

    protected override void BuildMenu(MenuBuilder menu) => Menu.Build(menu);
}
