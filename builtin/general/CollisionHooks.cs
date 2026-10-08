using AbsoluteFriends.Core;
using AbsoluteFriends.Hooks;
using MonoMod.Cil;

namespace AbsoluteFriends.General;

internal class CollisionHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.Collision];

    [HookPatch(typeof(IL.Room), nameof(IL.Room.Update))]
    [HookTest([1030, 1038, 1057], ["ldarg.0; ldfld Room::physicalObjects", "beq.s; ldarg.0; ldfld Room::physicalObjects", "ldarg.0; ldfld Room::physicalObjects"])]
    private static void IL_Room_Update(ILContext il) => il.FriendBranch();
}
