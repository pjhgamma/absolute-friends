using Mono.Cecil.Cil;
using MonoMod.Cil;
using RippleFriends.Core;
using RippleFriends.Diagnostics;
using RippleFriends.Hooks;
using RippleFriends.Options;

namespace RippleFriendsExample;

internal sealed class FriendshipHooks : DownpourHooks
{
    // Hook groups use any active option by default. Override IsOptionEnabled when customized condition is needed.
    protected override Configurable<bool>[] Options => [AddonConfig.Friendship];

    // Override Subject to give failure notifications a clearer title than the option label or hook group name.
    // protected override string? Subject => "Hunter Daddy";

    // Override the default Any check when the hook group requires every listed option to be active.
    protected override bool IsOptionEnabled => Options.All(option => option.IsActive);

    [HookPatch(typeof(On.ArtificialIntelligence), nameof(On.ArtificialIntelligence.StaticRelationship))]
    // The On_Type_Method and IL_Type_Method names are a readability convention; HookPatch binds any valid method name.
    private static CreatureTemplate.Relationship On_ArtificialIntelligence_StaticRelationship(On.ArtificialIntelligence.orig_StaticRelationship orig, ArtificialIntelligence self, AbstractCreature otherCreature)
    {
        return self.creature.IsHunterDaddy && self.creature.IsFriend(otherCreature, direct: true)
            ? new(CreatureTemplate.Relationship.Type.Pack, 1f)
            : orig(self, otherCreature);
    }

    [HookPatch(typeof(IL.DaddyAI), nameof(IL.DaddyAI.Update))]
    // HookTest verifies where this IL hook changed the original method; anchors may be added for a stronger baseline.
    [HookTest([0], ["ldarg.0; call ArtificialIntelligence::get_noiseTracker"])]
    private static void IL_DaddyAI_Update(ILContext il)
    {
        ILCursor c = new(il);

        c.Emit(OpCodes.Ldarg_0);
        // EmitGuarded reports an exception from the delegate and disables its hook group instead of disrupting later updates.
        c.EmitGuarded((DaddyAI self) =>
        {
            if (!self.creature.IsHunterDaddy)
            {
                return;
            }

            if (AddonConfig.Friendship.IsActive && self.creature.IsFriendOfPlayer)
            {
                self.creature.Track();
            }
            else
            {
                self.creature.Untrack();
            }
        });
    }

    [HookPatch(typeof(On.ArtificialIntelligence), nameof(On.ArtificialIntelligence.CurrentPlayerAggression))]
    private static float On_ArtificialIntelligence_CurrentPlayerAggression(On.ArtificialIntelligence.orig_CurrentPlayerAggression orig, ArtificialIntelligence self, AbstractCreature player)
    {
        return self.creature.IsHunterDaddy && self.creature.IsFriend(player) ? 0f : orig(self, player);
    }
}
