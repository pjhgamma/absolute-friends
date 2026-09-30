using AbsoluteFriends.Utils;

namespace AbsoluteFriends.Resonance;

internal class ResonanceBurst : UpdatableAndDeletable
{
    public ResonanceBurst(Room room, IEnumerable<AbstractCreature> abstractCreatures)
    {
        foreach (var abstractCreature in abstractCreatures)
        {
            if (abstractCreature?.realizedCreature is not { } creature || creature.room != room || creature.mainBodyChunk is not { } bodyChunk)
            {
                continue;
            }

            room.AddObject(new ShockWave(bodyChunk.pos, 300f, 0.2f, 15));
            room.AddObject(new MeltLights.MeltLight(1f, bodyChunk.pos, room, Palette.Secondary));
        }

        room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, 0f, 1f, 1f);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        Destroy();
    }
}
