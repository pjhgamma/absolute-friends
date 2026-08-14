using MonoMod.Cil;
using static RippleFriends.Core.ILUtils;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Vanilla;

internal class CollisionHooks : BaseHooks
{
    protected override Configurable<bool> Option => Config.Collision;

    [HookPatch(typeof(IL.Room), nameof(IL.Room.Update))]
    [HookTest([1029, 1037, 1057], ["ldfld AbstractPhysicalObject::rippleLayer", "ldfld AbstractPhysicalObject::rippleLayer", "ldarg.0; ldfld Room::physicalObjects"])]
    private static void IL_Room_Update(ILContext il)
    {
        IL_Branch_Ripple(il);
    }
}
