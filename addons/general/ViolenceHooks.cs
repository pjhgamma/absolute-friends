using AbsoluteFriends.Core;
using AbsoluteFriends.Hooks;

namespace AbsoluteFriends.General;

internal class ViolenceHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.Violence];

    [HookPatch(typeof(On.Creature), nameof(On.Creature.RippleViolenceCheck))]
    private static bool On_Creature_RippleViolenceCheck(On.Creature.orig_RippleViolenceCheck orig, Creature self, BodyChunk source)
    {
        return source?.owner.IsFriend(self) != true && orig(self, source);
    }
}
