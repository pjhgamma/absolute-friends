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

    private static void UpdateGlow(Creature creature)
    {
        Vector2 pos = creature.mainBodyChunk.pos;

        if (_lightSources.TryGetValue(creature, out var lightSource))
        {
            if (lightSource.slatedForDeletetion || lightSource.room != creature.room || creature.room.Darkness(pos) == 0f || !HasGlowingPlayerFriend(creature))
            {
                _lightSources.Remove(creature);

                return;
            }

            lightSource.stayAlive = true;
            lightSource.setPos = pos;

            return;
        }

        if (creature.room.Darkness(pos) > 0f && HasGlowingPlayerFriend(creature))
        {
            lightSource = new LightSource(pos, false, Color.Lerp(Color.white, creature.abstractCreature.ShortcutColor, 0.5f), creature)
            {
                requireUpKeep = true,
                setRad = 300f,
                setAlpha = 1f
            };

            creature.room.AddObject(lightSource);

            _lightSources.Add(creature, lightSource);
        }
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

        if (self.IsPlayer || self.room == null || self.abstractCreature == null || self.mainBodyChunk == null || self.slatedForDeletetion)
        {
            return;
        }

        if (self is Player { glowing: true })
        {
            RemoveGlow(self);

            return;
        }

        UpdateGlow(self);
    }
}
