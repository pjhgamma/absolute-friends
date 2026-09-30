using AbsoluteFriends.Hooks;
using AbsoluteFriends.Utils;

namespace AbsoluteFriends.Core;

internal class FriendTrackerHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.FriendSlugcat, Config.FriendCreature, Config.FriendNeutralCreature, Config.FriendSharing, Config.FriendChaining];

    protected override bool IsOptionEnabled => base.IsOptionEnabled || CreatureRules.HasConfiguredRules;

    protected override string? Subject => "Friend Tracker";

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.ctor))]
    private static void On_RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);

        FriendUtils.ClearTrackedFriends();
    }

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.Update))]
    private static void On_RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);

        FriendUtils.PruneTrackedFriends();
    }

    [HookPatch(typeof(On.FriendTracker), nameof(On.FriendTracker.Update))]
    private static void On_FriendTracker_Update(On.FriendTracker.orig_Update orig, FriendTracker self)
    {
        orig(self);

        if (self.AI?.creature is not { } abstractCreature || abstractCreature.IsPlayer)
        {
            return;
        }

        self.AdoptSlugcatFriend();

        if (abstractCreature.IsNPC)
        {
            if (self.friend?.abstractCreature is { } abstractPlayer && abstractPlayer.IsPlayer && !abstractPlayer.slatedForDeletion && abstractPlayer.state?.dead != true)
            {
                abstractCreature.TrackPupFriend(abstractPlayer);
            }
            else if (!abstractCreature.HasRemotePupFriend)
            {
                abstractCreature.Untrack();
            }

            return;
        }

        if (
            (self.HasSlugcatFriend && (abstractCreature.IsFriendlyAllowed || abstractCreature.IsFriend(self.friend?.abstractCreature)))
            || abstractCreature.IsFriendOfPlayer
        )
        {
            abstractCreature.Track();
        }
        else
        {
            abstractCreature.Untrack();
        }
    }
}
