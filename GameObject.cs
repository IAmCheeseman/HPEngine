using HPEngine.Graphics;

namespace HPEngine;

public class GameObject
{
    internal int UpdateIndex = -1;
    internal int DrawIndex = -1;
    internal Dictionary<string, int> TagIndices = new();

    public World? World;
    public int WorldId;

    public virtual void OnAdd()
    {
    }

    public virtual void OnRemove()
    {
    }

    public virtual void OnUpdate(float dt)
    {
    }

    public virtual void OnDraw(Renderer renderer)
    {
    }

    public void Tag(string tagName)
    {
        World?.Tag(this, tagName);
    }

    public void Untag(string tagName)
    {
        World?.Untag(this, tagName);
    }

    public void Remove()
    {
        World?.RemoveObject(this);
    }

    public override string ToString()
    {
        return $"{base.ToString()} #{WorldId}";
    }
}
