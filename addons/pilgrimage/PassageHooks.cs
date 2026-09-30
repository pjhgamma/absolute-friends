using AbsoluteFriends.Core;
using AbsoluteFriends.Hooks;

namespace AbsoluteFriends.Pilgrimage;

internal class PassageHooks : BaseHooks
{
    private static readonly string[] _entrySeparator = ["<cA>"];

    private static readonly string[] _fieldSeparator = ["<cB>"];

    protected override Configurable<bool>[] Options => [Config.Passage];

    [HookPatch(typeof(On.SaveState), nameof(On.SaveState.SaveToString))]
    private static string On_SaveState_SaveToString(On.SaveState.orig_SaveToString orig, SaveState self)
    {
        string saveString = orig(self);

        if (!ModManager.MMF && self?.progression?.rainWorld?.setup.cleanSpawns == false && self.pendingFriendCreatures is { Count: > 0 } pendingFriendCreatures)
        {
            saveString += "FRIENDS<svB>" + string.Concat(pendingFriendCreatures.Select(pendingFriendCreature => pendingFriendCreature + "<svC>")) + "<svA>";
        }

        return saveString;
    }

    [HookPatch(typeof(On.RegionState), nameof(On.RegionState.AdaptRegionStateToWorld))]
    private static void On_RegionState_AdaptRegionStateToWorld(On.RegionState.orig_AdaptRegionStateToWorld orig, RegionState self, int playerShelter, int activeGate)
    {
        List<string>? pendingFriendCreatures = self?.saveState?.pendingFriendCreatures;
        World? world = self?.world;
        AbstractRoom? abstractRoom = world?.GetAbstractRoom(playerShelter);

        if (pendingFriendCreatures != null && (abstractRoom?.shelter == true || abstractRoom?.name == SaveState.forcedEndRoomToAllowwSave || playerShelter < 0))
        {
            foreach (var abstractCreature in FriendUtils.TrackedFriends.ToList())
            {
                if (abstractCreature?.state?.alive == true && abstractCreature.saveCreature && abstractCreature.world == world)
                {
                    pendingFriendCreatures.Add(SaveState.AbstractCreatureToStringStoryWorld(abstractCreature));

                    abstractCreature.LoseAllStuckObjects();
                    abstractCreature.saveCreature = false;
                    abstractCreature.Untrack();
                }
            }
        }

        orig(self, playerShelter, activeGate);

        if (pendingFriendCreatures != null)
        {
            HashSet<string> entityIDs = [];

            for (int i = 0; i < pendingFriendCreatures.Count; ++i)
            {
                if (!entityIDs.Add(pendingFriendCreatures[i].Split(_entrySeparator, StringSplitOptions.None) is { Length: > 1 } fields ? fields[1].Split(_fieldSeparator, StringSplitOptions.None)[0] : pendingFriendCreatures[i]))
                {
                    pendingFriendCreatures.RemoveAt(i--);
                }
            }
        }
    }
}
