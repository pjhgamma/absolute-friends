using RippleFriends.Friends;
using RippleFriends.Hooks;
using RippleFriends.Options;
using UnityEngine;

namespace RippleFriends.Gameplay.General;

internal class SenseHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.Deaf, Config.Blind, Config.Hypothermia];

    [HookPatch(typeof(On.Creature), nameof(On.Creature.Update))]
    private static void On_Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        orig(self, eu);

        bool isDeaf = Config.Deaf.IsActive && self.deaf > 0;
        bool isBlind = Config.Blind.IsActive && self.blind > 0;
        bool isHypothermia = Config.Hypothermia.IsActive && ModManager.HypothermiaModule && self.Hypothermia > 0f;

        if (!isDeaf && !isBlind && !isHypothermia)
        {
            return;
        }

        int deaf = self.deaf;
        int blind = self.blind;
        float hypothermia = self.Hypothermia;

        foreach (var creature in self.room.Creatures)
        {
            if (
                (!isDeaf || creature.deaf >= deaf)
                && (!isBlind || creature.blind >= blind)
                && (!isHypothermia || creature.Hypothermia >= hypothermia)
            )
            {
                continue;
            }

            if (!self.IsFriend(creature))
            {
                continue;
            }

            if (isDeaf)
            {
                deaf = Math.Min(creature.deaf, deaf);
            }
            if (isBlind)
            {
                blind = Math.Min(creature.blind, blind);
            }
            if (isHypothermia)
            {
                hypothermia = Mathf.Min(creature.Hypothermia, hypothermia);
            }
        }

        if (isDeaf)
        {
            self.deaf = (int)Mathf.Lerp(self.deaf, deaf, Config.DeafRatio.Value);
        }
        if (isBlind)
        {
            self.blind = (int)Mathf.Lerp(self.blind, blind, Config.BlindRatio.Value);
        }
        if (isHypothermia)
        {
            self.Hypothermia = Mathf.Lerp(self.Hypothermia, hypothermia, Config.HypothermiaRatio.Value);
        }
    }
}
