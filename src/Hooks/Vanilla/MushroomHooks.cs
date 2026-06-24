using static RippleFriends.Core.OwnerTracker;
using static RippleFriends.Core.PlayerUtils;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Vanilla;

internal class MushroomHooks : BaseHooks
{
    protected override Configurable<bool> Option => Config.Mushroom;

    private static void MulticastMushroom(Player owner)
    {
        foreach (var player in GetOtherPlayers(owner))
        {
            player.mushroomCounter += 320;
        }
    }

    [HookPatch(typeof(On.Mushroom), nameof(On.Mushroom.BitByPlayer))]
    private static void On_Mushroom_BitByPlayer(On.Mushroom.orig_BitByPlayer orig, Mushroom self, Creature.Grasp grasp, bool eu)
    {
        if (grasp?.grabber is Player player)
        {
            MulticastMushroom(player);
        }

        orig(self, grasp, eu);
    }

    [HookPatch(typeof(On.Spear), nameof(On.Spear.HitSomethingWithoutStopping))]
    private static void On_Spear_HitSomethingWithoutStopping(On.Spear.orig_HitSomethingWithoutStopping orig, Spear self, PhysicalObject obj, BodyChunk chunk, PhysicalObject.Appendage appendage)
    {
        if (GetThrower(self) is Player player && obj is Mushroom)
        {
            MulticastMushroom(player);
        }

        orig(self, obj, chunk, appendage);
    }

    [HookPatch(typeof(On.Player), nameof(On.Player.Update))]
    private static void On_Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (!self.inShortcut)
        {
            return;
        }

        foreach (var player in GetOtherPlayers(self))
        {
            if (!player.inShortcut)
            {
                self.mushroomCounter = player.mushroomCounter;

                break;
            }
        }
    }
}
