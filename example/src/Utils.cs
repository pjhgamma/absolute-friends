using MoreSlugcats;

namespace RippleFriendsExample;

internal static class Utils
{
    extension(AbstractCreature? abstractCreature)
    {
        internal bool IsHunterDaddy => abstractCreature?.creatureTemplate?.type == MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy;

        internal bool IsRed => abstractCreature?.state is PlayerState { slugcatCharacter: SlugcatStats.Name name } && name == SlugcatStats.Name.Red;
    }
}
