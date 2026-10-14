using System.Runtime.CompilerServices;
using AbsoluteFriends.Hooks;

namespace AbsoluteFriends.Iterators;

internal class MarkHooks : BaseHooks
{
    private static readonly ConditionalWeakTable<Creature, MarkOverlay> _overlays = new();

    protected override Configurable<bool>[] Options => [Config.Mark];

    [HookPatch(typeof(On.PlayerGraphics), nameof(On.PlayerGraphics.Update))]
    private static void On_PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);

        if (self.player is { isNPC: true } slugpup && MarkOverlay.CanBearMark(slugpup))
        {
            self.markAlpha = MarkOverlay.MarkAlpha(slugpup);
        }
    }

    [HookPatch(typeof(On.Creature), nameof(On.Creature.Update))]
    private static void On_Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        orig(self, eu);

        if (MarkOverlay.Failed || self is Player || !MarkOverlay.CanBearMark(self))
        {
            return;
        }

        if (_overlays.TryGetValue(self, out MarkOverlay overlay))
        {
            if (!overlay.slatedForDeletetion && overlay.room == self.room)
            {
                return;
            }

            _overlays.Remove(self);
        }

        overlay = new MarkOverlay(self);

        _overlays.Add(self, overlay);
        self.room.AddObject(overlay);
    }
}
