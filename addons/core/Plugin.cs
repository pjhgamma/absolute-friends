using BepInEx;
using RippleFriends.Options;
using System.Security.Permissions;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace RippleFriends.Core;

[BepInPlugin(GUID, Name, RippleFriends.Plugin.Version)]
public class Plugin : AddonPlugin
{
    public const string GUID = "pjhgamma.ripplefriends.core";

    public const string Name = "Ripple Friends: Core";

    public override string AddonDescription => "Controls who are Ripple Friends and how their relationships are shared and shown.";

    public override string RippleFriendsVersion => RippleFriends.Plugin.Version;

    protected override void Bind(Addon addon) => Core.Config.Bind(addon);

    protected override void BuildMenu(MenuBuilder menu) => Menu.Build(menu);
}
