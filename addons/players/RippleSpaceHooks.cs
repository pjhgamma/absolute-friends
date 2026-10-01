using AbsoluteFriends.Core;
using AbsoluteFriends.Hooks;
using AbsoluteFriends.Utils;

namespace AbsoluteFriends.Players;

internal class RippleSpaceHooks : WatcherHooks
{
    protected override Configurable<bool>[] Options => [Config.RippleSpace];

    private static void SynchronizeRippleLayer(Creature creature)
    {
        if (!creature.IsTracked)
        {
            return;
        }

        foreach (var abstractPlayer in creature.room.game?.Players ?? [])
        {
            if (abstractPlayer?.realizedCreature is Player friendPlayer && abstractPlayer.rippleLayer != creature.abstractCreature.rippleLayer && creature.IsFriend(friendPlayer))
            {
                creature.ChangeRippleLayer(abstractPlayer.rippleLayer);

                RevealInRippleSpace(creature.room.game, creature);

                return;
            }
        }
    }

    private static void RevealInRippleSpace(RainWorldGame? game, PhysicalObject target)
    {
        foreach (var camera in game?.cameras ?? [])
        {
            foreach (var sLeaser in camera?.spriteLeasers ?? [])
            {
                RevealInRippleSpace(sLeaser, target);
            }
        }
    }

    private static void RevealInRippleSpace(RoomCamera.SpriteLeaser sLeaser, PhysicalObject? target = null)
    {
        PhysicalObject? owner = sLeaser.DrawnObject;

        if (target == null ? !owner.IsSlugcat && owner.IsFriendOfPlayer && owner.IsTracked : owner == target)
        {
            foreach (var sprite in sLeaser?.sprites ?? [])
            {
                if (sprite?.shader is { } shader)
                {
                    if (RainWorldUtils.Shader(shader.name + "BothSides") is { } bothSides)
                    {
                        sprite.shader = bothSides;
                    }
                    else if (shader == FShader.defaultShader && RainWorldUtils.Shader("RippleBasicBothSides") is { } rippleBasicBothSides)
                    {
                        sprite.shader = rippleBasicBothSides;
                    }
                }
            }
        }
    }

    [HookPatch(typeof(On.Creature), nameof(On.Creature.Update))]
    private static void On_Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        orig(self, eu);

        if (self is Player || self.room == null || self.abstractCreature == null)
        {
            return;
        }

        SynchronizeRippleLayer(self);
    }

    [HookPatch(typeof(On.RoomCamera.SpriteLeaser), nameof(On.RoomCamera.SpriteLeaser.ctor))]
    private static void On_SpriteLeaser_ctor(On.RoomCamera.SpriteLeaser.orig_ctor orig, RoomCamera.SpriteLeaser self, IDrawable obj, RoomCamera rCam)
    {
        orig(self, obj, rCam);

        RevealInRippleSpace(self);
    }

    [HookPatch(typeof(On.RoomCamera.SpriteLeaser), nameof(On.RoomCamera.SpriteLeaser.UpdatePalette))]
    private static void On_SpriteLeaser_UpdatePalette(On.RoomCamera.SpriteLeaser.orig_UpdatePalette orig, RoomCamera.SpriteLeaser self, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, rCam, palette);

        RevealInRippleSpace(self);
    }
}
