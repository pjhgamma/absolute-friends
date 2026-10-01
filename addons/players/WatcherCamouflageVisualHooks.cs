using System.Runtime.CompilerServices;
using AbsoluteFriends.Core;
using AbsoluteFriends.Hooks;
using AbsoluteFriends.Utils;
using UnityEngine;
using Watcher;

namespace AbsoluteFriends.Players;

internal class WatcherCamouflageVisualHooks : WatcherHooks
{
    private const int FirstBodySpriteIndex = 0;

    private const int LastBodySpriteIndex = 6;

    private const int FaceSpriteIndex = 9;

    private const int CamoMaskSpriteIndex = 12;

    private static readonly ConditionalWeakTable<Room, StrongBox<bool>> _sharedRippleRooms = new();

    protected override Configurable<bool>[] Options => [Config.WatcherCamouflage];

    private static void SynchronizeCameras(RainWorldGame game)
    {
        List<Player> watchers = [];

        foreach (var player in game.RealizedPlayers)
        {
            if (player.IsTrackedWatcher)
            {
                watchers.Add(player);
            }
        }

        if (!watchers.Any(player => player.rippleLevel >= 5f))
        {
            return;
        }

        foreach (var camera in game.cameras ?? [])
        {
            SynchronizeRippleCamera(camera, watchers);
        }
    }

    private static Player? FindCameraWatcher(RoomCamera camera, Room room, IReadOnlyList<Player> watchers)
    {
        return watchers.FirstOrDefault(player => player.abstractCreature == camera.followAbstractCreature && player.room == room)
            ?? watchers.FirstOrDefault(player => player.room == room)
            ?? watchers.FirstOrDefault(player => player.abstractCreature == camera.followAbstractCreature);
    }

    private static bool SynchronizeRippleRoom(Room room, IReadOnlyList<Player> watchers, bool active, float progress)
    {
        bool transitioning = watchers.Any(player => player.room == room && player.transitionRipple != null && player.camoProgress < 1f);

        if (active && !transitioning && room.fsRipple == null)
        {
            room.fsRipple = new RippleFullScreen();
            room.AddObject(room.fsRipple);
            _sharedRippleRooms.GetOrCreateValue(room).Value = true;

            return true;
        }

        if (!active && progress < 1f && _sharedRippleRooms.TryGetValue(room, out StrongBox<bool> shared) && shared.Value)
        {
            room.fsRipple?.Destroy();
            room.fsRipple = null;
            _sharedRippleRooms.Remove(room);

            return true;
        }

        return false;
    }

    private static void SynchronizeRippleCamera(RoomCamera? camera, IReadOnlyList<Player> watchers)
    {
        if (camera?.room is not { } room || camera.loadingRoom != null || FindCameraWatcher(camera, room, watchers) is not { } target)
        {
            return;
        }

        bool hasMaxFriend = false;
        bool sharedActive = false;
        float sharedProgress = 0f;

        foreach (var player in watchers)
        {
            if (player.rippleLevel >= 5f && (player == target || target.IsFriend(player)))
            {
                hasMaxFriend = true;
                sharedActive |= player.isCamo;
                sharedProgress = Mathf.Max(sharedProgress, player.camoProgress);
            }
        }

        if (!hasMaxFriend)
        {
            return;
        }

        bool active = target.rippleLevel >= 5f ? target.isCamo : sharedActive;
        float progress = target.rippleLevel >= 5f ? target.camoProgress : sharedProgress;

        if (camera.rippleData == null)
        {
            camera.UpdateRippleData(room, camera.currentCameraPosition);
        }

        if (camera.rippleData is not { } data)
        {
            return;
        }

        bool changed = data.gameplayRippleActive != active;

        data.gameplayRippleActive = active;
        data.gameplayRippleAnimation = progress;
        camera.lastRippleState = active;

        bool roomChanged = SynchronizeRippleRoom(room, watchers, active, progress);

        if (changed || roomChanged)
        {
            camera.RefreshRippleMask();
        }
    }

    private static bool HasCamoMask(RoomCamera.SpriteLeaser? sLeaser)
    {
        return sLeaser?.sprites is { Length: > CamoMaskSpriteIndex }
            && sLeaser.drawableObject is PlayerGraphics { useSimpleCamo: false }
            && (sLeaser.DrawnObject as Player).IsWatcher;
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

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.Update))]
    private static void On_RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);

        SynchronizeCameras(self);
    }

    [HookPatch(typeof(On.Player), nameof(On.Player.SpawnWatcherMechanicRipple))]
    private static CosmeticRipple On_Player_SpawnWatcherMechanicRipple(On.Player.orig_SpawnWatcherMechanicRipple orig, Player self)
    {
        Room? room = self.room;
        RoomCamera? mainCamera = RainWorldUtils.MainCamera(room?.game);

        foreach (var camera in room?.game?.cameras ?? [])
        {
            if (room != null && camera?.room == room && camera.rippleData == null)
            {
                camera.UpdateRippleData(room, camera.currentCameraPosition);
            }
        }

        try
        {
            return orig(self);
        }
        finally
        {
            if (mainCamera?.room is { } mainRoom && mainRoom != room)
            {
                mainCamera.UpdateRippleData(mainRoom, mainCamera.currentCameraPosition);
            }
        }
    }

    [HookPatch(typeof(On.RoomCamera), nameof(On.RoomCamera.DrawUpdate))]
    private static void On_RoomCamera_DrawUpdate(On.RoomCamera.orig_DrawUpdate orig, RoomCamera self, float timeStacker, float timeSpeed)
    {
        if (self.room != null && self.rippleData is { } data && (data.gameplayRippleActive || data.gameplayRippleAnimation > 0f))
        {
            data.SetGlobals();
        }

        orig(self, timeStacker, timeSpeed);
    }

    [HookPatch(typeof(On.PlayerGraphics), nameof(On.PlayerGraphics.InitiateSprites))]
    private static void On_PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        if (self.owner is Player { room: not null } player && player.IsWatcher)
        {
            player.room.watcherCamoTaken = false;
        }

        orig(self, sLeaser, rCam);
    }

    [HookPatch(typeof(On.PlayerGraphics), nameof(On.PlayerGraphics.DrawSprites))]
    private static void On_PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!(self.owner as Player).IsWatcher || sLeaser.sprites is not { Length: > CamoMaskSpriteIndex } sprites)
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
}
