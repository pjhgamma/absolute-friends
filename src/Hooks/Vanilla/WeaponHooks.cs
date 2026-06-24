using static RippleFriends.Core.FriendTracker;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Vanilla;

internal class WeaponHooks : BaseHooks
{
    protected override Configurable<bool> Option => Config.Weapon;

    [HookPatch(typeof(On.Weapon), nameof(On.Weapon.HitThisObject))]
    private static bool On_Weapon_HitThisObject(On.Weapon.orig_HitThisObject orig, Weapon self, PhysicalObject obj)
    {
        if (IsFriend(self, obj))
        {
            return false;
        }
        return orig(self, obj);
    }
}
