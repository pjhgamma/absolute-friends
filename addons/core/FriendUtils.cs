using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using RWCustom;

namespace AbsoluteFriends.Core;

public static partial class FriendUtils
{
    public const float FriendLikeThreshold = 0.5f;

    public const float FriendReputationThreshold = 0f;

    private static readonly List<Func<AbstractCreature, AbstractCreature, bool?>> _friendshipRules = [];

    public static bool IsFriendSession
    {
        get
        {
            if (RainWorldUtils.CurrentGame is not { } game)
            {
                return false;
            }

            if (Custom.rainWorld?.safariMode == true)
            {
                return Config.FriendSafari.IsActive;
            }

            if (game.IsArenaSession)
            {
                return Config.FriendArena.IsActive;
            }

            if (!game.IsStorySession)
            {
                return false;
            }

            return Custom.rainWorld?.ExpeditionMode == true ? Config.FriendExpedition.IsActive : Config.FriendStory.IsActive;
        }
    }

    public static void RegisterFriendshipRule(Func<AbstractCreature, AbstractCreature, bool?> rule)
    {
        if (rule != null && !_friendshipRules.Contains(rule))
        {
            _friendshipRules.Add(rule);
        }
    }

    public static void UnregisterFriendshipRule(Func<AbstractCreature, AbstractCreature, bool?> rule) => _friendshipRules.Remove(rule);

    extension(AbstractPhysicalObject? source)
    {
        public bool IsTracked => source is AbstractCreature { IsTracked: true };

        public bool IsFriendOfPlayer
        {
            get
            {
                foreach (var abstractPlayer in source?.world?.game?.Players ?? [])
                {
                    if (source.IsFriend(abstractPlayer))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public FriendshipType FriendshipOfPlayer
        {
            get
            {
                FriendshipType result = FriendshipType.None;

                foreach (var abstractPlayer in source?.world?.game?.Players ?? [])
                {
                    FriendshipType type = source.FriendshipWith(abstractPlayer);

                    if (type > result)
                    {
                        result = type;
                    }
                }

                return result;
            }
        }

        public FriendshipType FriendshipWith(AbstractPhysicalObject? target, bool direct = false)
        {
            FriendshipType type = source.GetFriendshipType(target, direct, true);

            return type != FriendshipType.None && (source.IsTrackedWith(target) || target.IsTrackedWith(source)) ? FriendshipType.Tracked : type;
        }

        public bool IsFriend(AbstractPhysicalObject? target, bool direct = false) => source.IsFriend(target, direct, true);

        public bool IsFriend(PhysicalObject? target, bool direct = false) => source.IsFriend(target?.abstractPhysicalObject, direct);

        private bool IsFriend(AbstractPhysicalObject? target, bool direct, bool chaining) =>
            source.GetFriendshipType(target, direct, chaining) != FriendshipType.None;

        private bool IsTrackedWith(AbstractPhysicalObject? target) => target is AbstractCreature { IsPlayer: true } abstractPlayer && source is AbstractCreature { IsPlayer: false } abstractCreature && abstractCreature.IsTrackedFor(abstractPlayer);

        private FriendshipType GetFriendshipType(AbstractPhysicalObject? target, bool direct, bool chaining)
        {
            AbstractCreature? sourceSelf = source as AbstractCreature;
            AbstractCreature? targetSelf = target as AbstractCreature;
            FriendshipType type = sourceSelf.GetFriendshipType(targetSelf, chaining);

            if (type != FriendshipType.None)
            {
                return type;
            }

            if (direct || sourceSelf.IsDenied || targetSelf.IsDenied)
            {
                return FriendshipType.None;
            }

            AbstractCreature? sourceOwner = source.Owner;
            AbstractCreature? targetOwner = target.Owner;

            return sourceSelf.GetFriendshipType(targetOwner, chaining) != FriendshipType.None
                || sourceOwner.GetFriendshipType(targetSelf, chaining) != FriendshipType.None
                || sourceOwner.GetFriendshipType(targetOwner, chaining) != FriendshipType.None ? FriendshipType.Indirect : FriendshipType.None;
        }
    }

    extension(PhysicalObject? source)
    {
        public bool IsTracked => (source?.abstractPhysicalObject).IsTracked;

        public bool IsFriendOfPlayer => (source?.abstractPhysicalObject).IsFriendOfPlayer;

        public bool IsFriend(AbstractPhysicalObject? target, bool direct = false) => (source?.abstractPhysicalObject).IsFriend(target, direct);

        public bool IsFriend(PhysicalObject? target, bool direct = false) => (source?.abstractPhysicalObject).IsFriend(target?.abstractPhysicalObject, direct);
    }

    extension(Room? room)
    {
        public IEnumerable<PhysicalObject> Objects
        {
            get
            {
                foreach (var physicalObjects in room?.physicalObjects ?? [])
                {
                    foreach (var physicalObject in physicalObjects)
                    {
                        if (physicalObject != null)
                        {
                            yield return physicalObject;
                        }
                    }
                }
            }
        }

        public IEnumerable<Creature> Creatures
        {
            get
            {
                foreach (var physicalObject in room.Objects)
                {
                    if (physicalObject is Creature creature)
                    {
                        yield return creature;
                    }
                }
            }
        }

        public IEnumerable<Creature> FriendsOf(PhysicalObject? source)
        {
            foreach (var creature in room.Creatures)
            {
                if (source.IsFriend(creature))
                {
                    yield return creature;
                }
            }
        }

        public IEnumerable<Creature> PlayerFriends
        {
            get
            {
                foreach (var creature in room.Creatures)
                {
                    if (creature.IsFriendOfPlayer)
                    {
                        yield return creature;
                    }
                }
            }
        }
    }
}
