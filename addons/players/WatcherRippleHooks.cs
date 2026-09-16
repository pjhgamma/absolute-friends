using Mono.Cecil.Cil;
using MonoMod.Cil;
using RippleFriends.Core;
using RippleFriends.Diagnostics;
using RippleFriends.Hooks;
using RippleFriends.Utils;
using System.Runtime.CompilerServices;
using UnityEngine;
using Watcher;

namespace RippleFriends.Players;

internal class WatcherRippleHooks : WatcherHooks
{
    private const int FirstBodySpriteIndex = 0;

    private const int LastBodySpriteIndex = 6;

    private const int FaceSpriteIndex = 9;

    private const int CamoMaskSpriteIndex = 12;

    private static readonly ConditionalWeakTable<Player, StrongBox<bool>> _camoStates = new();

    private static int _syncedClock = -1;

    protected override Configurable<bool>[] Options => [Config.WatcherRipple];

    private static bool IsWatcher(Player? player) => player?.SlugCatClass == WatcherEnums.SlugcatStatsName.Watcher;

    private static bool IsCamoPlayer(Player? player) => player is { dead: false } && IsWatcher(player);

    private static bool CanToggleCamo(Player player) => player.room != null && player.Consious && player.warpExhausionTime <= 0 && player.timeInVoidSeaRoom < RainWorldUtils.Second;

    private static bool CanEnterCamo(Player player) => CanToggleCamo(player) && player.camoRechargePenalty <= 0;

    private static bool IsActivatingCamo(Player player) => player.activateCamoTimer > 0 || player.performingActivationTimer > 0;

    private static IEnumerable<Player> FriendPlayers(Player? player)
    {
        foreach (var friendPlayer in (player?.abstractCreature?.world?.game).RealizedPlayers)
        {
            if (friendPlayer != player && player.IsFriend(friendPlayer))
            {
                yield return friendPlayer;
            }
        }
    }

    private static bool IsSynchronizingCamo(Player player)
    {
        if (!IsCamoPlayer(player) || (player.isCamo ? !CanToggleCamo(player) : !CanEnterCamo(player)))
        {
            return false;
        }

        foreach (var friendPlayer in FriendPlayers(player))
        {
            if (IsCamoPlayer(friendPlayer) && CanToggleCamo(friendPlayer) && friendPlayer.RippleAbilityActivationButtonCondition)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSynchronizedActivation(Player player) => IsSynchronizingCamo(player) && !player.RippleAbilityActivationButtonCondition;

    private static void SynchronizeCamoCharge(RainWorldGame game)
    {
        Dictionary<Player, float> charges = [];

        foreach (var player in game.RealizedPlayers)
        {
            if (IsWatcher(player) && player.camoRechargePenalty <= 0)
            {
                charges[player] = player.camoCharge;
            }
        }

        foreach (var player in charges.Keys)
        {
            foreach (var friendPlayer in FriendPlayers(player))
            {
                if (IsCamoPlayer(friendPlayer) && charges.TryGetValue(friendPlayer, out float charge))
                {
                    player.camoCharge = player.dead ? charge : Mathf.Max(player.camoCharge, charge);
                }
            }
        }
    }

    private static void SynchronizeCamo(Player player)
    {
        if (!IsCamoPlayer(player) || !CanToggleCamo(player) || IsActivatingCamo(player))
        {
            return;
        }

        StrongBox<bool> camoState = _camoStates.GetOrCreateValue(player);

        if (player.isCamo != camoState.Value && (!camoState.Value || CanEnterCamo(player)))
        {
            foreach (var friendPlayer in FriendPlayers(player))
            {
                if (IsCamoPlayer(friendPlayer) && friendPlayer.isCamo == camoState.Value)
                {
                    player.ToggleCamo();

                    break;
                }
            }
        }

        camoState.Value = player.isCamo;
    }

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

    private static bool HasCamoMask(RoomCamera.SpriteLeaser? sLeaser)
    {
        return sLeaser?.sprites is { Length: > CamoMaskSpriteIndex }
            && sLeaser.drawableObject is PlayerGraphics { useSimpleCamo: false }
            && IsWatcher(sLeaser.DrawnObject as Player);
    }

    private static bool ShareCamoMask(RoomCamera camera)
    {
        int masked = 0;

        foreach (var sLeaser in camera.spriteLeasers ?? [])
        {
            if (HasCamoMask(sLeaser) && ++masked > 1)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsCamoVisible(RoomCamera camera)
    {
        foreach (var sLeaser in camera.spriteLeasers ?? [])
        {
            if (HasCamoMask(sLeaser) && sLeaser.DrawnObject is Player { camoProgress: > 0f })
            {
                return true;
            }
        }

        return false;
    }

    private static FSprite? LastSprite(RoomCamera camera, FContainer container, int first, int last)
    {
        FSprite? anchor = null;
        int lastIndex = -1;

        foreach (var sLeaser in camera.spriteLeasers ?? [])
        {
            if (!HasCamoMask(sLeaser))
            {
                continue;
            }

            for (int index = first; index <= last; index++)
            {
                FSprite sprite = sLeaser.sprites[index];

                if (sprite == null || sprite.container != container)
                {
                    continue;
                }

                int childIndex = container.GetChildIndex(sprite);

                if (childIndex > lastIndex)
                {
                    lastIndex = childIndex;
                    anchor = sprite;
                }
            }
        }

        return anchor;
    }

    private static void MoveAfter(FSprite sprite, RoomCamera camera, int first, int last)
    {
        if (sprite.container is not { } container || LastSprite(camera, container, first, last) is not { } anchor || container.GetChildIndex(anchor) <= container.GetChildIndex(sprite))
        {
            return;
        }

        sprite.MoveInFrontOfOtherNode(anchor);
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

    [HookPatch(typeof(IL.Player), nameof(IL.Player.WatcherUpdate))]
    [HookTest([642], ["brfalse; ldarg.0; ldfld Player::rippleRingDelay"])]
    private static void IL_Player_WatcherUpdate(ILContext il)
    {
        ILCursor c = new(il);

        if (c.TryGotoNext(
            MoveType.After,
            i => i.MatchCall<Player>("get_RippleAbilityActivationButtonCondition")
        ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitGuarded((bool condition, Player player) => condition || IsSynchronizingCamo(player), (condition, _) => condition);
        }
    }

    [HookPatch(typeof(On.Player), nameof(On.Player.SpawnRippleRing))]
    private static void On_Player_SpawnRippleRing(On.Player.orig_SpawnRippleRing orig, Player self)
    {
        if (!IsSynchronizedActivation(self))
        {
            orig(self);
        }
    }

    [HookPatch(typeof(On.Player), nameof(On.Player.SpawnPersistentRipple))]
    private static void On_Player_SpawnPersistentRipple(On.Player.orig_SpawnPersistentRipple orig, Player self, float minRadius, float maxRadius, int cycleExpiry)
    {
        if (!IsSynchronizedActivation(self))
        {
            orig(self, minRadius, maxRadius, cycleExpiry);
        }
    }

    [HookPatch(typeof(On.Player), nameof(On.Player.ToggleCamo))]
    private static void On_Player_ToggleCamo(On.Player.orig_ToggleCamo orig, Player self)
    {
        bool wasCamo = self.isCamo;

        orig(self);

        if (self.activateCamoTimer <= 0 || self.reachedCamoToggle || !CanToggleCamo(self) || self.isCamo == wasCamo)
        {
            return;
        }

        _camoStates.GetOrCreateValue(self).Value = self.isCamo;

        foreach (var player in FriendPlayers(self))
        {
            if (!IsWatcher(player))
            {
                continue;
            }

            _camoStates.GetOrCreateValue(player).Value = self.isCamo;

            if (player.isCamo != self.isCamo && !IsActivatingCamo(player) && (self.isCamo ? CanEnterCamo(player) : CanToggleCamo(player)))
            {
                player.ToggleCamo();
            }
        }
    }

    [HookPatch(typeof(On.Player), nameof(On.Player.CamoUpdate))]
    private static void On_Player_CamoUpdate(On.Player.orig_CamoUpdate orig, Player self)
    {
        if (self.room?.game is { } game && IsWatcher(self) && game.clock != _syncedClock)
        {
            _syncedClock = game.clock;

            SynchronizeCamoCharge(game);
        }

        orig(self);
    }

    [HookPatch(typeof(On.Creature), nameof(On.Creature.Update))]
    private static void On_Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        orig(self, eu);

        if (self.room == null || self.abstractCreature == null)
        {
            return;
        }

        if (self is Player player)
        {
            SynchronizeCamo(player);

            return;
        }

        SynchronizeRippleLayer(self);
    }

    [HookPatch(typeof(On.PlayerGraphics), nameof(On.PlayerGraphics.InitiateSprites))]
    private static void On_PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        if (self.owner is Player { room: not null } player && IsWatcher(player))
        {
            player.room.watcherCamoTaken = false;
        }

        orig(self, sLeaser, rCam);
    }

    [HookPatch(typeof(On.PlayerGraphics), nameof(On.PlayerGraphics.DrawSprites))]
    private static void On_PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!IsWatcher(self.owner as Player) || sLeaser.sprites is not { Length: > CamoMaskSpriteIndex } sprites)
        {
            return;
        }

        FSprite mask = sprites[CamoMaskSpriteIndex];
        FSprite face = sprites[FaceSpriteIndex];

        if (mask?.container is not { } container || face == null || face.container != container || !ShareCamoMask(rCam))
        {
            return;
        }

        MoveAfter(mask, rCam, FirstBodySpriteIndex, LastBodySpriteIndex);

        if (IsCamoVisible(rCam))
        {
            MoveAfter(face, rCam, CamoMaskSpriteIndex, CamoMaskSpriteIndex);
        }
        else
        {
            face.MoveInFrontOfOtherNode(sprites[LastBodySpriteIndex]);
        }

        self.faceSpriteIndex = container.GetChildIndex(face);
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
