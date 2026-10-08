using System.Runtime.CompilerServices;
using AbsoluteFriends.Core;
using AbsoluteFriends.Hooks;
using AbsoluteFriends.Utils;
using UnityEngine;

namespace AbsoluteFriends.Iterators;

internal class MarkHooks : BaseHooks
{
    private static readonly ConditionalWeakTable<Creature, MarkOverlay> _overlays = new();

    protected override Configurable<bool>[] Options => [Config.Mark];

    public static bool CanBearMark(Creature creature)
    {
        return creature is { dead: false, slatedForDeletetion: false, room: not null }
            && creature.room.game.session is StoryGameSession { saveState.deathPersistentSaveData.theMark: true }
            && creature.IsTracked;
    }

    public static float MarkAlpha(Creature friend)
    {
        float alpha = 0f;

        foreach (var player in friend.room.game.RealizedPlayers)
        {
            if (player.room == friend.room && player.graphicsModule is PlayerGraphics graphics && friend.abstractCreature.IsTrackedFor(player.abstractCreature))
            {
                alpha = Mathf.Max(alpha, graphics.markAlpha);
            }
        }

        return alpha;
    }

    [HookPatch(typeof(On.PlayerGraphics), nameof(On.PlayerGraphics.Update))]
    private static void On_PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);

        if (self.player is { isNPC: true } slugpup && CanBearMark(slugpup))
        {
            self.markAlpha = MarkAlpha(slugpup);
        }
    }

    [HookPatch(typeof(On.Creature), nameof(On.Creature.Update))]
    private static void On_Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        orig(self, eu);

        if (self is Player || !CanBearMark(self))
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
