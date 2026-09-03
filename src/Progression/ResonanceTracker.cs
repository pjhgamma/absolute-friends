using UnityEngine;

namespace RippleFriends.Progression;

internal class ResonanceTracker
{
    public int Death;

    public int Aftershock;

    public int Reflection;

    public int Duration;

    public float Progress => Duration.GetProgress(Reflection);

    public float Severity(Room? room)
    {
        int cycleLength = room?.world?.rainCycle?.cycleLength ?? 0;

        return cycleLength > 0 ? Mathf.InverseLerp(0f, cycleLength, Aftershock) : 0f;
    }
}
