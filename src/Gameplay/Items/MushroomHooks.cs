using MonoMod.Cil;
using RippleFriends.Friends;
using RippleFriends.Hooks;
using RippleFriends.Options;
using System.Runtime.CompilerServices;

namespace RippleFriends.Gameplay.Items;

internal class MushroomHooks : BaseHooks
{
    private static readonly ConditionalWeakTable<AbstractCreature, StrongBox<int>> _mushroomCounters = new();

    private static readonly ConditionalWeakTable<AbstractCreature, StrongBox<float>> _mushroomEffects = new();

    protected override Configurable<bool>[] Options => [Config.Mushroom];

    private static void SynchronizeMushroom<T>(Player self, ConditionalWeakTable<AbstractCreature, StrongBox<T>> shared, Func<Player, T> read, Action<Player, T> write)
    {
        T value = read(self);

        if (shared.TryGetValue(self.abstractCreature, out StrongBox<T> pending) && !EqualityComparer<T>.Default.Equals(value, pending.Value))
        {
            write(self, pending.Value);
            shared.Remove(self.abstractCreature);

            return;
        }

        foreach (var abstractSlugcat in FriendUtils.TrackedFriendsWithPlayers)
        {
            if (abstractSlugcat.realizedCreature is Player slugcat && !EqualityComparer<T>.Default.Equals(read(slugcat), value))
            {
                shared.GetOrCreateValue(abstractSlugcat).Value = value;
            }
        }
    }

    [HookPatch(typeof(On.Player), nameof(On.Player.Update))]
    private static void On_Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        SynchronizeMushroom(self, _mushroomCounters, slugcat => slugcat.mushroomCounter, (slugcat, mushroomCounter) => slugcat.mushroomCounter = mushroomCounter);
        SynchronizeMushroom(self, _mushroomEffects, slugcat => slugcat.mushroomEffect, (slugcat, mushroomEffect) => slugcat.mushroomEffect = mushroomEffect);
    }

    [HookPatch(typeof(IL.Mushroom), nameof(IL.Mushroom.BitByPlayer))]
    [HookTest([16], ["ldarg.1; ldfld Grasp::grabber"])]
    private static void IL_Mushroom_BitByPlayer(ILContext il) => il.ShareMushroom();

    [HookPatch(typeof(IL.Spear), nameof(IL.Spear.HitSomethingWithoutStopping))]
    [HookTest([92], ["ldarg.1; callvirt UpdatableAndDeletable::Destroy"])]
    private static void IL_Spear_HitSomethingWithoutStopping(ILContext il) => il.ShareMushroom();

    [HookPatch(typeof(IL.Lizard), nameof(IL.Lizard.Update))]
    [HookTest([1319], ["ldloc.s; ldc.i4.1; add"])]
    private static void IL_Lizard_Update(ILContext il) => il.ShareMushroom();

    [HookPatch(typeof(IL.Player), nameof(IL.Player.ProcessChatLog))]
    [HookTest([22], ["ldarg.0; ldc.i4.s; callvirt Creature::Stun"])]
    private static void IL_Player_ProcessChatLog(ILContext il) => il.ShareMushroom();

    [HookPatch(typeof(IL.Expedition.ExpeditionGame.SlowTimeTracker), nameof(IL.Expedition.ExpeditionGame.SlowTimeTracker.Update))]
    [HookTest([95], ["ldloc.s; ldc.i4.1; add"])]
    private static void IL_SlowTimeTracker_Update(ILContext il) => il.ShareMushroom();
}
