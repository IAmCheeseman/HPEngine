namespace HPEngine;

public struct Animation
{
    public int Start;
    public int End;
    public int Fps;

    public Animation(int start, int end, int fps = 10)
    {
        Start = start;
        End = end;
        Fps = fps;
    }
}

public struct AnimationConfig
{
    public int HFrames;
    public int VFrames;
    public Dictionary<string, Animation> Animations;
}

public class AnimatedTexture : Texture
{
    public Texture Texture;
    public AnimationConfig Config;
    public int Frame { get; private set; } = 2;
    private Vec2i _frameSize;
    private float _timer;

    public AnimatedTexture(Texture texture, AnimationConfig config)
    {
        Texture = texture;
        Config = config;
        _frameSize = Texture.Size / new Vec2i(config.HFrames, config.VFrames);
    }

    // TODO: Add support for multiple types of animation 'progression'
    // - Forwards
    // - Backwards
    // - Ping-Pong
    public void Animate(string animationName, float dt, float speedModifier = 1f)
    {
        var animation = Config.Animations[animationName];

        var frameDuration = 1f / animation.Fps;
        _timer += dt;

        if (_timer > frameDuration)
        {
            Frame++;
            _timer -= frameDuration;
        }

        if (Frame >= animation.End || Frame < animation.Start)
            Frame = animation.Start;
    }

    public Rect GetRect()
    {
        var start = new Vec2i(
                    Frame % Config.HFrames,
                    Frame / Config.HFrames) * _frameSize;
        return new Rect(start, _frameSize);
    }

    public override void Use() => Texture.Use();
    public override Texture GetTexture() => Texture.GetTexture();
    public override void SetWrapMode(WrapMode mode) => Texture.SetWrapMode(mode);
    public override void SetFilterMode(FilterMode mode) => Texture.SetFilterMode(mode);
    public override int GetWidth() => _frameSize.W;
    public override int GetHeight() => _frameSize.H;
}
