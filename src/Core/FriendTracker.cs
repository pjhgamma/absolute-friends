using static RippleFriends.Core.OwnerTracker;
using RippleFriends.Options;
using RWCustom;

namespace RippleFriends.Core;

internal static class FriendTracker
{
    public static bool IsFriendCreature(Creature? source, Creature? target)
    {
        if (target == null || source == null)
        {
            return false;
        }

        if (!Config.FriendArena.Value && target.room.game.IsArenaSession)
        {
            return false;
        }

        if (target is Player)
        {
            if (source is Player)
            {
                return Config.FriendPlayer.Value && source != target;
            }

            var abstractSource = source.abstractCreature;
            var aiSource = abstractSource?.abstractAI?.RealAI;
            var friend = aiSource?.friendTracker?.friend;

            if ((aiSource is LizardAI && Custom.rainWorld.options.friendlyLizards && friend != null) || friend == target)
            {
                return Config.FriendCreature.Value;
            }

            if (aiSource != null)
            {
                var abstractTarget = target.abstractCreature;
                var representation = aiSource.tracker?.RepresentationForCreature(abstractTarget, false);

                var relationship = representation != null
                    ? aiSource.DynamicRelationship(representation)
                    : aiSource.StaticRelationship(abstractTarget);

                if (relationship.type == CreatureTemplate.Relationship.Type.Pack)
                {
                    return Config.FriendCreature.Value;
                }
                if (relationship.type == CreatureTemplate.Relationship.Type.Ignores)
                {
                    return Config.FriendNeutralCreature.Value;
                }
            }
        }

        return false;
    }

    public static bool IsFriendIterator(UpdatableAndDeletable? source, UpdatableAndDeletable? target)
    {
        if (Config.FriendIterator.Value)
        {
            var player = target is Player targetPlayer ? targetPlayer : (GetOwner(target) is Player targetOwner ? targetOwner : null);

            if (player == null)
            {
                return false;
            }
            if (source is Oracle)
            {
                return !player.monkAscension;
            }
            if (source is SLOracleSwarmer)
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsDirectedFriend(UpdatableAndDeletable? source, UpdatableAndDeletable? target)
    {
        Creature? sourceCreature = source as Creature;
        Creature? targetCreature = target as Creature;
        Creature? sourceOwner = GetOwner(source);
        Creature? targetOwner = GetOwner(target);

        if (sourceCreature != null && targetCreature != null && IsFriendCreature(sourceCreature, targetCreature))
        {
            return true;
        }
        if (sourceCreature != null && targetOwner != null && IsFriendCreature(sourceCreature, targetOwner))
        {
            return true;
        }
        if (sourceOwner != null && targetCreature != null && IsFriendCreature(sourceOwner, targetCreature))
        {
            return true;
        }
        if (sourceOwner != null && targetOwner != null && IsFriendCreature(sourceOwner, targetOwner))
        {
            return true;
        }
        return IsFriendIterator(source, target);
    }

    public static bool IsFriend(UpdatableAndDeletable? source, UpdatableAndDeletable? target)
    {
        return IsDirectedFriend(source, target) || IsDirectedFriend(target, source);
    }
}
