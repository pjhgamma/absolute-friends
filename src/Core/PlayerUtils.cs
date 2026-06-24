namespace RippleFriends.Core;

internal static class PlayerUtils
{
    public static IEnumerable<Player> GetOtherPlayers(Player owner)
    {
        foreach (var abstractPlayer in owner.room?.game?.Players ?? [])
        {
            if (abstractPlayer?.realizedCreature is Player player && player != owner)
            {
                yield return player;
            }
        }
    }

    public static bool IsIdlePlayer(Player player, float seconds)
    {
        return player.touchedNoInputCounter >= seconds * 40f;
    }
}
