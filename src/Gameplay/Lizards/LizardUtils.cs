using RippleFriends.Friends;

namespace RippleFriends.Gameplay.Lizards;

internal static class LizardUtils
{
    extension(Lizard? lizard)
    {
        public bool IsFocusFriend => lizard.IsFriend(lizard?.AI?.focusCreature?.representedCreature);
    }
}
