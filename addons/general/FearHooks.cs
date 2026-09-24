using MonoMod.Cil;
using MoreSlugcats;
using AbsoluteFriends.Core;
using AbsoluteFriends.Diagnostics;
using AbsoluteFriends.Hooks;

namespace AbsoluteFriends.General;

internal class FearHooks : BaseHooks
{
    private static PhysicalObject? _scareOwner;

    protected override Configurable<bool>[] Options => [Config.Fear];

    private static bool IsFriendAI(ArtificialIntelligence? artificialIntelligence)
    {
        return artificialIntelligence?.creature.IsFriend(_scareOwner) == true;
    }

    [HookPatch(typeof(On.ThreatTracker), nameof(On.ThreatTracker.AddThreatCreature))]
    private static void On_ThreatTracker_AddThreatCreature(On.ThreatTracker.orig_AddThreatCreature orig, ThreatTracker self, Tracker.CreatureRepresentation creature)
    {
        if (self.AI?.creature.IsFriend(creature?.representedCreature) == true)
        {
            return;
        }

        orig(self, creature);
    }

    [HookPatch(typeof(On.ThreatTracker), nameof(On.ThreatTracker.AddThreatPoint))]
    private static ThreatTracker.ThreatPoint On_ThreatTracker_AddThreatPoint(On.ThreatTracker.orig_AddThreatPoint orig, ThreatTracker self, CreatureTemplate crit, WorldCoordinate pos, float severity)
    {
        return IsFriendAI(self.AI) ? new(crit, pos, severity) : orig(self, crit, pos, severity);
    }

    [HookPatch(typeof(On.FirecrackerPlant.ScareObject), nameof(On.FirecrackerPlant.ScareObject.Update))]
    private static void On_ScareObject_Update(On.FirecrackerPlant.ScareObject.orig_Update orig, FirecrackerPlant.ScareObject self, bool eu)
    {
        if (self.init || self.Owner is not { } owner)
        {
            orig(self, eu);

            return;
        }

        PhysicalObject? scared = _scareOwner;

        _scareOwner = owner.realizedObject;

        try
        {
            orig(self, eu);
        }
        finally
        {
            _scareOwner = scared;
        }
    }

    [HookPatch(typeof(On.FirecrackerPlant.ScareObject), nameof(On.FirecrackerPlant.ScareObject.MakeCreatureLeaveRoom))]
    private static void On_ScareObject_MakeCreatureLeaveRoom(On.FirecrackerPlant.ScareObject.orig_MakeCreatureLeaveRoom orig, FirecrackerPlant.ScareObject self, ArtificialIntelligence AI)
    {
        if (IsFriendAI(AI))
        {
            return;
        }

        orig(self, AI);
    }

    [HookPatch(typeof(IL.ItemTracker.ItemRepresentation), nameof(IL.ItemTracker.ItemRepresentation.Update))]
    [HookTest([135], ["ldarg.0; ldfld ItemRepresentation::parent"])]
    private static void IL_ItemRepresentation_Update(ILContext il)
    {
        ILCursor c = new(il);

        if (c.TryGotoNext(
            i => i.MatchCallvirt<IUseItemTracker>("SeeThrownWeapon")
        ))
        {
            c.Remove();
            c.EmitGuarded((IUseItemTracker useItemTracker, PhysicalObject obj, Creature thrower) =>
            {
                if (useItemTracker is ArtificialIntelligence artificialIntelligence && thrower.IsFriend(artificialIntelligence.creature))
                {
                    return;
                }

                useItemTracker.SeeThrownWeapon(obj, thrower);
            }, (useItemTracker, obj, thrower) => useItemTracker.SeeThrownWeapon(obj, thrower));
        }
    }

    [HookPatch(typeof(IL.FirecrackerPlant), nameof(IL.FirecrackerPlant.PopLump))]
    [HookTest([296], ["stfld FirecrackerPlant::scareObj"])]
    private static void IL_FirecrackerPlant_PopLump(ILContext il) => il.ScareObject<FirecrackerPlant>();
}

internal class DownpourFearHooks : DownpourHooks
{
    protected override Configurable<bool>[] Options => [Config.Fear];

    [HookPatch(typeof(IL.JokeRifle), nameof(IL.JokeRifle.Use))]
    [HookTest([499], ["stfld JokeRifle::scareObj"])]
    private static void IL_JokeRifle_Use(ILContext il) => il.ScareObject<JokeRifle>();

    [HookPatch(typeof(IL.MoreSlugcats.SingularityBomb), nameof(IL.MoreSlugcats.SingularityBomb.CreateFear))]
    [HookTest([11], ["stfld SingularityBomb::scareObj"])]
    private static void IL_SingularityBomb_CreateFear(ILContext il) => il.ScareObject<SingularityBomb>();
}
