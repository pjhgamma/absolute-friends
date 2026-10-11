using AbsoluteFriends.Utils;
using Menu;
using UnityEngine;
using RelationshipType = CreatureTemplate.Relationship.Type;

namespace AbsoluteFriends.FriendshipEditor;

internal static class RelationshipTypes
{
    internal static RelationshipType[] Editable => field ??=
    [
        RelationshipType.Ignores,
        RelationshipType.Pack,
        RelationshipType.Eats,
        RelationshipType.Attacks,
        RelationshipType.AgressiveRival,
        RelationshipType.Antagonizes,
        RelationshipType.Afraid,
        RelationshipType.StayOutOfWay,
        RelationshipType.Uncomfortable,
    ];

    internal static RelationshipType? Find(string? name) => name == null ? null : Array.Find(Editable, type => type.value == name);

    internal static Color ColorOf(RelationshipType type) => type.value switch
    {
        "Pack" or "PlaysWith" or "SocialDependent" => Palette.Friendly,
        "Eats" or "Attacks" or "AgressiveRival" or "Antagonizes" => Palette.Unfriendly,
        "Afraid" or "StayOutOfWay" or "Uncomfortable" => Palette.Warning,
        "DoesntTrack" => MenuColorEffect.rgbMediumGrey,
        _ => Palette.Neutral,
    };
}
