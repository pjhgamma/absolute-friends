using AbsoluteFriends.Options;
using RWCustom;
using UnityEngine;
using VoidSea;

namespace AbsoluteFriends.Pilgrimage;

internal class WormThreadOverlay(VoidSeaScene scene, VoidWorm worm, VoidWorm.Arm arm, Creature friend) : CosmeticSprite
{
    public override void Update(bool eu)
    {
        base.Update(eu);

        if (!Config.Ascension.IsActive || friend.room != room || !scene.worms.Contains(worm))
        {
            Destroy();
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = [new FSprite("pixel", true)
        {
            anchorY = 0f,
            scaleX = 1f,
            isVisible = false
        }];

        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Midground"));
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        FSprite line = sLeaser.sprites[0];

        if (slatedForDeletetion || friend.room != room || arm.thread == null || arm.threadStatus <= 0)
        {
            line.isVisible = false;

            return;
        }

        BodyPart? head = friend.graphicsModule switch
        {
            LizardGraphics lizardGraphics => lizardGraphics.head,
            PlayerGraphics playerGraphics => playerGraphics.head,
            _ => null
        };
        Vector2 start = Vector2.Lerp(arm.thread[0, 1], arm.thread[0, 0], timeStacker);
        Vector2 end = head == null
            ? Vector2.Lerp(friend.mainBodyChunk.lastPos, friend.mainBodyChunk.pos, timeStacker)
            : Vector2.Lerp(head.lastPos, head.pos, timeStacker);

        line.isVisible = true;
        line.color = Color.Lerp(Color.white, Color.black, worm.dark);
        line.x = start.x - camPos.x;
        line.y = start.y - camPos.y;
        line.scaleY = Vector2.Distance(start, end);
        line.rotation = Custom.AimFromOneVectorToAnother(start, end);
    }
}
