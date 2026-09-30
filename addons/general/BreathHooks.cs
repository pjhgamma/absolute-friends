using AbsoluteFriends.Core;
using AbsoluteFriends.Hooks;
using AbsoluteFriends.Options;
using UnityEngine;

namespace AbsoluteFriends.General;

internal class BreathHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.Breath];

    private static float? GetBreath(Creature creature) => creature switch
    {
        Player player => player.airInLungs,
        AirBreatherCreature airBreatherCreature => airBreatherCreature.lungs,
        _ => null
    };

    private static float RecoverBreath(Creature self, float breath)
    {
        float mostBreath = breath;

        foreach (var creature in self.room.Creatures)
        {
            if (!creature.dead && GetBreath(creature) is { } friendBreath && friendBreath > mostBreath && self.IsFriend(creature))
            {
                mostBreath = friendBreath;
            }
        }

        return Mathf.Lerp(breath, mostBreath, Config.BreathRatio.Value);
    }

    [HookPatch(typeof(On.Player), nameof(On.Player.LungUpdate))]
    private static void On_Player_LungUpdate(On.Player.orig_LungUpdate orig, Player self)
    {
        if (self is { dead: false, room: not null, airInLungs: < 1f })
        {
            self.airInLungs = RecoverBreath(self, self.airInLungs);
        }

        orig(self);
    }

    [HookPatch(typeof(On.Creature), nameof(On.Creature.Update))]
    private static void On_Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        orig(self, eu);

        if (self is AirBreatherCreature { dead: false, room: not null, lungs: < 1f } airBreatherCreature)
        {
            airBreatherCreature.lungs = RecoverBreath(airBreatherCreature, airBreatherCreature.lungs);
        }
    }
}
