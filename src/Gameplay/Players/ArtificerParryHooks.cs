using MonoMod.Cil;
using RippleFriends.Hooks;
using RippleFriends.Options;

namespace RippleFriends.Gameplay.Players;

internal class ArtificerParryHooks : DownpourHooks
{
    protected override Configurable<bool>[] Options => [Config.ArtificerParry];

    [HookPatch(typeof(IL.Player), nameof(IL.Player.ClassMechanicsArtificer))]
    [HookTest([892, 895, 910], ["ldarg.0; ldfld PhysicalObject::abstractPhysicalObject", "beq.s; ldarg.0; ldfld UpdatableAndDeletable::room", "ldarg.0; ldfld UpdatableAndDeletable::room"])]
    private static void IL_Player_ClassMechanicsArtificer(ILContext il) => il.BranchRipple();
}
