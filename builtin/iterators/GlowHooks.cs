using System.Runtime.CompilerServices;
using AbsoluteFriends.Core;
using AbsoluteFriends.Hooks;
using AbsoluteFriends.Utils;
using UnityEngine;

namespace AbsoluteFriends.Iterators;

internal class GlowHooks : BaseHooks
{
    private static readonly ConditionalWeakTable<Creature, LightSource> _lightSources = new();

    protected override Configurable<bool>[] Options => [Config.Glow];

    private static bool CanGlow(Creature creature)
    {
        return creature is { slatedForDeletetion: false, room: not null, abstractCreature: not null, mainBodyChunk: not null } and not Player { glowing: true }
            && !creature.IsPlayer
            && creature.room.Darkness(creature.mainBodyChunk.pos) > 0f
            && HasGlowingPlayerFriend(creature);
    }

    private static bool HasGlowingPlayerFriend(Creature creature)
    {
        foreach (var player in creature.room.game.RealizedPlayers)
        {
            if (player.glowing && creature.abstractCreature.IsTrackedFor(player.abstractCreature))
            {
                return true;
            }
        }

        return false;
    }

    private static void RemoveGlow(Creature creature)
    {
        if (_lightSources.TryGetValue(creature, out var lightSource))
        {
            lightSource.Destroy();

            _lightSources.Remove(creature);
        }
    }

    [HookPatch(typeof(On.Creature), nameof(On.Creature.Update))]
    private static void On_Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        orig(self, eu);

        if (!CanGlow(self))
        {
            RemoveGlow(self);

            return;
        }

        Vector2 pos = self.mainBodyChunk.pos;

        if (_lightSources.TryGetValue(self, out var lightSource) && !lightSource.slatedForDeletetion && lightSource.room == self.room)
        {
            lightSource.stayAlive = true;
            lightSource.setPos = pos;

            return;
        }

        RemoveGlow(self);

        lightSource = new LightSource(pos, false, Color.Lerp(Color.white, self.abstractCreature.ShortcutColor, 0.5f), self)
        {
            requireUpKeep = true,
            setRad = 300f,
            setAlpha = 1f
        };

        self.room.AddObject(lightSource);

        _lightSources.Add(self, lightSource);
    }
}
