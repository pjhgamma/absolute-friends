using MoreSlugcats;

namespace AbsoluteFriends.Utils;

public static class PlayerUtils
{
    extension(AbstractPhysicalObject? abstractPhysicalObject)
    {
        private CreatureTemplate.Type? Template => (abstractPhysicalObject as AbstractCreature)?.creatureTemplate?.type;

        public bool IsPlayer => abstractPhysicalObject.Template == CreatureTemplate.Type.Slugcat;

        public bool IsNPC => abstractPhysicalObject.Template == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC;

        public bool IsSlugcat => abstractPhysicalObject.IsPlayer || abstractPhysicalObject.IsNPC;
    }

    extension(PhysicalObject? physicalObject)
    {
        public bool IsPlayer => physicalObject?.abstractPhysicalObject.IsPlayer == true;

        public bool IsNPC => physicalObject?.abstractPhysicalObject.IsNPC == true;

        public bool IsSlugcat => physicalObject?.abstractPhysicalObject.IsSlugcat == true;
    }

    extension(RainWorldGame? game)
    {
        public IEnumerable<Player> RealizedPlayers
        {
            get
            {
                foreach (var abstractPlayer in game?.Players ?? [])
                {
                    if (abstractPlayer?.realizedCreature is Player player)
                    {
                        yield return player;
                    }
                }
            }
        }
    }
}
