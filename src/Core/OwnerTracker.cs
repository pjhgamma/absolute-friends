using MoreSlugcats;
using RippleFriends.Options;
using System.Runtime.CompilerServices;
using Watcher;

namespace RippleFriends.Core;

internal static class OwnerTracker
{
    private static readonly ConditionalWeakTable<UpdatableAndDeletable, Creature> _ownerCWT = new();

    public static Creature? GetGrabber(UpdatableAndDeletable? source)
    {
        if (source is PhysicalObject physicalObject && physicalObject?.grabbedBy?.Count > 0)
        {
            return physicalObject.grabbedBy[0].grabber;
        }
        return null;
    }

    public static Creature? GetThrower(UpdatableAndDeletable? source)
    {
        return source switch
        {
            Weapon weapon when weapon.thrownBy is { } owner => owner,
            Explosion explosion when explosion.killTagHolder is { } owner => owner,
            SporePlant.Bee bee when bee.owner?.thrownBy is { } owner => owner,
            FireEgg fireEgg when fireEgg.thrownBy is { } owner => owner,
            Pomegranate pomegranate when pomegranate.killTagHolder is { } owner => owner,
            Frog frog when frog.thrownBy is { } owner => owner,
            { } when _ownerCWT.TryGetValue(source, out var owner) && owner != null => owner,
            { } when source is Creature creature => creature,
            _ => null
        };
    }

    public static void SetThrower(UpdatableAndDeletable? source, Creature? target = null)
    {
        if (source == null)
        {
            return;
        }

        _ownerCWT.Remove(source);

        if (target == null || GetThrower(source) == target)
        {
            return;
        }

        _ownerCWT.Add(source, target);
    }

    public static Creature? GetOwner(UpdatableAndDeletable? source)
    {
        if (Config.FriendGrabbed.Value && GetGrabber(source) is { } grabber)
        {
            return grabber;
        }
        return GetThrower(source);
    }

    public static void ChainOwner(UpdatableAndDeletable? source, UpdatableAndDeletable? target)
    {
        SetThrower(target, GetOwner(source));
    }
}
