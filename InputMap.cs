namespace HPEngine;

internal enum InputMethod
{
    Key,
    MouseButton,
}

internal struct InputMapping
{
    public InputMethod Method;

    public MouseButtonId MouseButtonId;
    public KeyId KeyId;
}

public class InputMap
{
    Dictionary<string, List<InputMapping>> _mappings = new();
    Context _context;

    public InputMap(Context context)
    {
        _context = context;
    }

    public void CreateMapping(string name, KeyId keyId)
    {
        var action = new InputMapping() {
            Method = InputMethod.Key,
            KeyId = keyId,
        };

        if (!_mappings.ContainsKey(name))
            _mappings.Add(name, new List<InputMapping>());
        _mappings[name].Add(action);
    }

    public void CreateMapping(string name, MouseButtonId mbId)
    {
        var action = new InputMapping() {
            Method = InputMethod.MouseButton,
            MouseButtonId = mbId,
        };

        if (!_mappings.ContainsKey(name))
            _mappings.Add(name, new List<InputMapping>());
        _mappings[name].Add(action);
    }

    public bool IsDown(string name)
    {
        if (!_mappings.ContainsKey(name))
        {
            Console.Error.WriteLine($"Action '{name}' doesn't exist");
            return false;
        }

        var inputs = _mappings[name];

        foreach (var input in inputs)
        {
            switch (input.Method)
            {
                case InputMethod.Key:
                    if (_context.Input.IsKeyDown(input.KeyId))
                        return true;
                    break;
                case InputMethod.MouseButton:
                    if (_context.Input.IsMouseDown(input.MouseButtonId))
                        return true;
                    break;
            }
        }

        return false;
    }
}
