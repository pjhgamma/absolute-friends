using RWCustom;

namespace AbsoluteFriends.Utils;

public static class RainWorldUtils
{
    public const int Second = 40;

    public static RainWorldGame? CurrentGame => Custom.rainWorld?.processManager?.currentMainLoop as RainWorldGame;

    public static RoomCamera? MainCamera(RainWorldGame? game) => game?.cameras is { Length: > 0 } cameras ? cameras[0] : null;

    public static FShader? Shader(string name) => Custom.rainWorld?.Shaders is { } shaders && shaders.TryGetValue(name, out FShader shader) ? shader : null;

    extension(RoomCamera.SpriteLeaser? sLeaser)
    {
        public PhysicalObject? DrawnObject => sLeaser?.drawableObject switch
        {
            GraphicsModule graphicsModule => graphicsModule.owner,
            PhysicalObject physicalObject => physicalObject,
            _ => null
        };
    }
}
