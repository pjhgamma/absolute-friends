using AbsoluteFriends.Core;
using AbsoluteFriends.Diagnostics;
using AbsoluteFriends.Hooks;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace AbsoluteFriends.Items;

internal class MushroomHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.Mushroom];

    [HookPatch(typeof(IL.Player), nameof(IL.Player.Update))]
    [HookTest([1702], ["callvirt List`1::GetEnumerator"])]
    private static void IL_Player_Update(ILContext il)
    {
        ILCursor cursor = new(il);

        if (cursor.TryGotoNext(i => i.MatchCallvirt<RainWorldGame>("get_AlivePlayers")))
        {
            cursor.Remove();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitGuarded((RainWorldGame game, Player source) =>
            {
                List<AbstractCreature> friends = [];

                if (!source.IsTracked || source.inShortcut)
                {
                    return friends;
                }

                foreach (var abstractCreature in FriendUtils.TrackedFriendsIncludingPlayers)
                {
                    if (abstractCreature.world?.game == game && abstractCreature.realizedCreature is Player player && !player.dead && !player.inShortcut && source.IsFriend(player))
                    {
                        friends.Add(abstractCreature);
                    }
                }

                return friends;
            }, (game, _) => game.AlivePlayers);
        }
    }

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.Update))]
    private static void On_RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);

        List<Player> players = [];
        Dictionary<Player, (int Counter, float Effect)> active = [];

        foreach (var abstractCreature in FriendUtils.TrackedFriendsIncludingPlayers)
        {
            if (abstractCreature.world?.game == self && abstractCreature.realizedCreature is Player { dead: false } player)
            {
                players.Add(player);

                if (!player.inShortcut)
                {
                    active[player] = (player.mushroomCounter, player.mushroomEffect);
                }
            }
        }

        if (active.Count == 0)
        {
            return;
        }

        foreach (var player in players)
        {
            int counter = 0;
            float effect = 0f;
            bool hasSource = false;

            foreach (var source in active)
            {
                if (source.Key == player || player.IsFriend(source.Key))
                {
                    counter = hasSource ? Math.Max(counter, source.Value.Counter) : source.Value.Counter;
                    effect = hasSource ? Math.Max(effect, source.Value.Effect) : source.Value.Effect;
                    hasSource = true;
                }
            }

            if (hasSource)
            {
                player.mushroomCounter = counter;
                player.mushroomEffect = effect;
            }
        }
    }
}
