using System.Security.Permissions;
using AbsoluteFriends.Options;
using BepInEx;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace AbsoluteFriends.Core;

[BepInPlugin(GUID, Name, AbsoluteFriends.Plugin.Version)]
public class Plugin : AddonPlugin
{
    public const string GUID = "pjhgamma.absolutefriends.core";

    public const string Name = "Absolute Friends: Core";

    public override string AddonDescription => "Controls which things are friendly and how friendship is shared and shown.";

    public override string AbsoluteFriendsVersion => AbsoluteFriends.Plugin.Version;

    protected override void Bind(Addon addon) => Core.Config.Bind(addon);

    protected override void BuildMenu(MenuBuilder menu) => Menu.Build(menu);
}
