using RippleFriends.Core;

namespace RippleFriends.Creatures;

internal static class LizardUtils
{
    extension(Lizard? lizard)
    {
        public bool IsFocusFriend => lizard.IsFriend(lizard?.AI?.focusCreature?.representedCreature);
    }
}
