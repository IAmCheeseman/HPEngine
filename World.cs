using HPEngine.Graphics;

namespace HPEngine;

struct AddQueueInfo
{
    public GameObject Obj;
    public bool Update;
    public bool Draw;
}

struct TagQueueInfo
{
    public GameObject Obj;
    public string TagName;
}

public class World
{
    private HashSet<GameObject> _objects = new();
    private Dictionary<string, List<GameObject>> _tags = new();
    private List<GameObject> _updateList = new();
    private List<GameObject> _drawList = new();

    private Queue<AddQueueInfo> _addQueue = new();
    private Queue<GameObject> _removeQueue = new();
    private Queue<TagQueueInfo> _tagAddQueue = new();
    private Queue<TagQueueInfo> _tagRemoveQueue = new();

    private int _nextId = 0;

    public World()
    {
    }

    private T RemoveFromList<T>(List<T> list, int index)
    {
        var lastIndex = list.Count - 1;
        var last = list[lastIndex];
        list[index] = last;
        list.RemoveAt(lastIndex);
        return last;
    }

    private int GetNextId() => _nextId++;

    public void Update(float dt)
    {
        foreach (var obj in _updateList)
        {
            obj.OnUpdate(dt);
        }
    }

    public void Draw(Renderer renderer)
    {
        foreach (var obj in _drawList)
        {
            obj.OnDraw(renderer);
        }
    }

    private void AddTag(GameObject obj, string tagName)
    {
        if (!_tags.ContainsKey(tagName))
            _tags.Add(tagName, new List<GameObject>());

        var tagList = _tags[tagName];
        obj.TagIndices.Add(tagName, tagList.Count);
        tagList.Add(obj);

        Console.WriteLine($"Added '{tagName}' to {obj}");
    }

    private void RemoveTag(GameObject obj, string tagName)
    {
        if (!_tags.ContainsKey(tagName))
        {
            Console.Error.WriteLine($"No tag named '{tagName}' to remove");
            return;
        }

        // Already verified to exist at callsite
        var index = obj.TagIndices[tagName];
        obj.TagIndices.Remove(tagName);

        var tagList = _tags[tagName];
        if (tagList.Count == 0)
            return;

        var swapped = RemoveFromList(tagList, index);
        swapped.TagIndices[tagName] = index;

        Console.WriteLine($"Removed '{tagName}' from {obj}");
    }

    private void FlushAddQueue()
    {
        foreach (var info in _addQueue)
        {
            var obj = info.Obj;
            obj.World = this;
            obj.WorldId = GetNextId();

            if (info.Update)
            {
                _updateList.Add(obj);
                obj.UpdateIndex = _updateList.Count - 1;
            }
            
            if (info.Draw)
            {
                _drawList.Add(obj);
                obj.DrawIndex = _drawList.Count - 1;
            }

            obj.OnAdd();
        }
        _addQueue.Clear();
    }

    private void FlushRemoveQueue()
    {
        foreach (var obj in _removeQueue)
        {
            obj.OnRemove();

            if (obj.UpdateIndex != -1)
            {
                var swapped = RemoveFromList(_updateList, obj.UpdateIndex);
                swapped.UpdateIndex = obj.UpdateIndex;
            }

            if (obj.DrawIndex != -1)
            {
                var swapped = RemoveFromList(_drawList, obj.DrawIndex);
                swapped.DrawIndex = obj.DrawIndex;
            }

            foreach (var tag in obj.TagIndices)
            {
                RemoveTag(obj, tag.Key);
            }

            _objects.Remove(obj);
        }
        _removeQueue.Clear();
    }

    public void FlushQueues()
    {
        foreach (var tag in _tagAddQueue)
            AddTag(tag.Obj, tag.TagName);
        _tagAddQueue.Clear();

        foreach (var tag in _tagRemoveQueue)
            RemoveTag(tag.Obj, tag.TagName);
        _tagRemoveQueue.Clear();

        FlushAddQueue();
        FlushRemoveQueue();
    }

    public void AddObject(GameObject obj, bool update = true, bool draw = true)
    {
        _addQueue.Enqueue(new AddQueueInfo {
                Obj = obj,
                Update = update,
                Draw = draw,
            });
    }

    public void RemoveObject(GameObject obj)
    {
        _removeQueue.Enqueue(obj);
    }

    public void Tag(GameObject obj, string tagName)
    {
        _tagAddQueue.Enqueue(new TagQueueInfo {
                Obj = obj,
                TagName = tagName,
            });
    }

    public void Untag(GameObject obj, string tagName)
    {
        if (!obj.TagIndices.ContainsKey(tagName))
            return;

        _tagRemoveQueue.Enqueue(new TagQueueInfo {
                Obj = obj,
                TagName = tagName,
            });
    }

    public IReadOnlyList<GameObject> GetTagged(string tagName)
    {
        if (!_tags.ContainsKey(tagName))
            return new GameObject[] {};
        return _tags[tagName];
    }

    public T GrabFirstTagged<T>(string tagName)
    {
        throw new NotImplementedException();
    }

    public void SetProcess(GameObject obj, bool process)
    {
    }

    public void SetDraw(GameObject obj, bool draw)
    {
    }
}

