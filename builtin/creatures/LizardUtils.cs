using AbsoluteFriends.Core;

namespace AbsoluteFriends.Creatures;

internal static class LizardUtils
{
    extension(Lizard? lizard)
    {
        public bool IsFocusFriend => lizard.IsFriend(lizard?.AI?.focusCreature?.representedCreature);
    }
}
