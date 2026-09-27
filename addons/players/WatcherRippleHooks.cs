using System.Runtime.CompilerServices;
using AbsoluteFriends.Core;
using AbsoluteFriends.Diagnostics;
using AbsoluteFriends.Hooks;
using AbsoluteFriends.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using Watcher;

namespace AbsoluteFriends.Players;

internal class WatcherRippleCamoHooks : WatcherHooks
{
    private static readonly ConditionalWeakTable<Player, StrongBox<bool>> _camoStates = new();

    private static readonly ConditionalWeakTable<Player, StrongBox<(Player Source, bool InitialCamo)>> _sharedCamoActivations = new();

    private static readonly ConditionalWeakTable<Player, StrongBox<int>> _rippleFoodClocks = new();

    protected override Configurable<bool>[] Options => [Config.WatcherRipple];

    private static bool IsWatcher(Player? player) => player?.SlugCatClass == WatcherEnums.SlugcatStatsName.Watcher;

    private static bool IsCamoPlayer(Player? player) => player is { dead: false } && player.IsPlayer && IsWatcher(player);

    private static bool CanToggleCamo(Player player) => player.room != null && !player.inShortcut && player.Consious && player.warpExhausionTime <= 0 && player.timeInVoidSeaRoom < RainWorldUtils.Second;

    private static bool CanEnterCamo(Player player) => CanToggleCamo(player) && player.camoRechargePenalty <= 0;

    private static bool IsActivatingCamo(Player player) => player.activateCamoTimer > 0 || player.performingActivationTimer > 0;

    private static Player? SharedCamoSource(Player player)
    {
        if (!IsCamoPlayer(player) || !player.IsTracked || (player.isCamo ? !CanToggleCamo(player) : !CanEnterCamo(player)))
        {
            return null;
        }

        foreach (var friendPlayer in FriendPlayers(player))
        {
            bool alreadyToggled = friendPlayer.startingCamoStateOnActivate >= 0
                && friendPlayer.isCamo != (friendPlayer.startingCamoStateOnActivate != 0);

            if (!alreadyToggled && friendPlayer.isCamo == player.isCamo && CanToggleCamo(friendPlayer) && friendPlayer.RippleAbilityActivationButtonCondition)
            {
                return friendPlayer;
            }
        }

        return null;
    }

    private static bool IsSynchronizedActivation(Player player) => !player.RippleAbilityActivationButtonCondition && _sharedCamoActivations.TryGetValue(player, out _);

    private static bool CanContinueSharedCamo(Player player, (Player Source, bool InitialCamo) activation)
    {
        return IsActivatingCamo(player)
            && player.isCamo == activation.InitialCamo
            && (activation.Source.RippleAbilityActivationButtonCondition || activation.Source.isCamo != activation.InitialCamo);
    }

    private static bool ShouldActivateCamo(bool ownInput, Player player)
    {
        if (ownInput)
        {
            _sharedCamoActivations.Remove(player);

            return true;
        }

        if (_sharedCamoActivations.TryGetValue(player, out StrongBox<(Player Source, bool InitialCamo)> shared)
            && CanContinueSharedCamo(player, shared.Value))
        {
            return true;
        }

        if (SharedCamoSource(player) is not { } source)
        {
            _sharedCamoActivations.Remove(player);

            return false;
        }

        _sharedCamoActivations.GetOrCreateValue(player).Value = (source, player.isCamo);

        return true;
    }

    private static int LevitationActivationTimer(int timer, Player player)
    {
        if (player.RippleAbilityActivationButtonCondition)
        {
            _sharedCamoActivations.Remove(player);

            return timer;
        }

        return _sharedCamoActivations.TryGetValue(player, out _) ? 0 : timer;
    }

    private static IEnumerable<Player> FriendPlayers(Player? player)
    {
        if (!IsCamoPlayer(player) || !player.IsTracked)
        {
            yield break;
        }

        foreach (var friendPlayer in (player?.abstractCreature?.world?.game).RealizedPlayers)
        {
            if (friendPlayer != player && IsCamoPlayer(friendPlayer) && friendPlayer.IsTracked && player.IsFriend(friendPlayer))
            {
                yield return friendPlayer;
            }
        }
    }

    private static void SynchronizeCamoCharge(RainWorldGame game)
    {
        List<Player> watchers = [];
        Dictionary<Player, (float Charge, int Penalty, bool AteRippleFood)> activeCharges = [];

        foreach (var player in game.RealizedPlayers)
        {
            if (IsCamoPlayer(player) && player.IsTracked)
            {
                watchers.Add(player);

                if (!player.inShortcut)
                {
                    bool ateRippleFood = _rippleFoodClocks.TryGetValue(player, out StrongBox<int> foodClock) && foodClock.Value == game.clock;

                    activeCharges[player] = (player.camoCharge, player.camoRechargePenalty, ateRippleFood);
                }
            }
        }

        if (activeCharges.Count == 0)
        {
            return;
        }

        foreach (var player in watchers)
        {
            float minCharge = 0f;
            float maxCharge = 0f;
            int penalty = 0;
            bool ateRippleFood = false;
            bool hasSource = false;

            foreach (var source in activeCharges)
            {
                if (source.Key != player && !player.IsFriend(source.Key))
                {
                    continue;
                }

                minCharge = hasSource ? Mathf.Min(minCharge, source.Value.Charge) : source.Value.Charge;
                maxCharge = hasSource ? Mathf.Max(maxCharge, source.Value.Charge) : source.Value.Charge;
                penalty = Math.Max(penalty, source.Value.Penalty);
                ateRippleFood |= source.Value.AteRippleFood;
                hasSource = true;
            }

            if (!hasSource)
            {
                continue;
            }

            player.camoCharge = ateRippleFood ? minCharge : maxCharge;
            player.camoRechargePenalty = penalty;

            if (player.isCamo && penalty > 0)
            {
                if (player.room != null && !player.inShortcut)
                {
                    player.ToggleCamo();
                }

                player.isCamo = false;
                _camoStates.GetOrCreateValue(player).Value = false;
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
                if (friendPlayer.isCamo == camoState.Value)
                {
                    player.ToggleCamo();

                    break;
                }
            }
        }

        camoState.Value = player.isCamo;
    }

    [HookPatch(typeof(IL.Player), nameof(IL.Player.WatcherUpdate))]
    [HookTest([642], ["brfalse; ldarg.0; ldfld Player::rippleRingDelay"])]
    private static void IL_Player_WatcherUpdate(ILContext il)
    {
        ILCursor c = new(il);

        if (c.TryGotoNext(MoveType.After, i => i.MatchCall<Player>("get_RippleAbilityActivationButtonCondition")))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitGuarded(ShouldActivateCamo, (condition, _) => condition);
        }
    }

    [HookPatch(typeof(IL.Player), nameof(IL.Player.TickLevitation_bool_int_float))]
    [HookTest([2], ["ldc.i4.0; bgt; ldarg.0"])]
    private static void IL_Player_TickLevitation(ILContext il)
    {
        ILCursor c = new(il);

        if (c.TryGotoNext(MoveType.After, i => i.MatchLdfld<Player>("performingActivationTimer")))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitGuarded(LevitationActivationTimer, (timer, _) => timer);
        }
    }

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.JollyGameUpdate))]
    private static void On_RainWorldGame_JollyGameUpdate(On.RainWorldGame.orig_JollyGameUpdate orig, RainWorldGame self)
    {
        if (!self.RealizedPlayers.Any(player => IsCamoPlayer(player) && player.IsTracked))
        {
            orig(self);
        }
    }

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.Update))]
    private static void On_RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);

        SynchronizeCamoCharge(self);
        WatcherRippleVisualHooks.SynchronizeCameras(self);
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
        RainWorldGame? game = self.abstractCreature?.world?.game;

        if (game != null && self.consumedRippleFood > 0)
        {
            _rippleFoodClocks.GetOrCreateValue(self).Value = game.clock;
        }

        orig(self);
    }

    [HookPatch(typeof(On.Creature), nameof(On.Creature.Update))]
    private static void On_Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        orig(self, eu);

        if (self is not Player player || player.room == null || player.abstractCreature == null)
        {
            return;
        }

        if (!IsActivatingCamo(player))
        {
            _sharedCamoActivations.Remove(player);
        }

        SynchronizeCamo(player);
    }
}

internal class WatcherRippleVisualHooks : WatcherHooks
{
    private const int FirstBodySpriteIndex = 0;

    private const int LastBodySpriteIndex = 6;

    private const int FaceSpriteIndex = 9;

    private const int CamoMaskSpriteIndex = 12;

    private static readonly ConditionalWeakTable<Room, StrongBox<bool>> _sharedRippleRooms = new();

    protected override Configurable<bool>[] Options => [Config.WatcherRipple];

    private static bool IsWatcher(Player? player) => player?.SlugCatClass == WatcherEnums.SlugcatStatsName.Watcher;

    private static bool IsTrackedWatcher(Player? player) => player is { dead: false } && player.IsPlayer && player.IsTracked && IsWatcher(player);

    internal static void SynchronizeCameras(RainWorldGame game)
    {
        List<Player> watchers = [];

        foreach (var player in game.RealizedPlayers)
        {
            if (IsTrackedWatcher(player))
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
