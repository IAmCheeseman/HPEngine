using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace HPEngine;
public class Shader : IDisposable
{
    private bool _disposed = false;
    private Dictionary<string, int> _uniforms;
    private Dictionary<string, int> _attributes;
    private int _handle;

    private string _vertPath;
    private string _fragPath;

    public Shader(string vertPath, string fragPath)
    {
        _vertPath = vertPath;
        _fragPath = fragPath;

        var vert = LoadShader(vertPath, ShaderType.VertexShader);
        var frag = LoadShader(fragPath, ShaderType.FragmentShader);

        _handle = GL.CreateProgram();
        GL.AttachShader(_handle, vert);
        GL.AttachShader(_handle, frag);
        GL.LinkProgram(_handle);

        GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string info = GL.GetProgramInfoLog(_handle);
            Error(info);
        }

        GL.DetachShader(_handle, vert);
        GL.DeleteShader(vert);
        GL.DetachShader(_handle, frag);
        GL.DeleteShader(frag);

        GL.GetProgram(
                _handle,
                GetProgramParameterName.ActiveUniforms, out var uniformCount);
        _uniforms = new Dictionary<string, int>();

        for (var i = 0; i < uniformCount; i++)
        {
            var key = GL.GetActiveUniform(_handle, i, out _, out _);
            var location = GL.GetUniformLocation(_handle, key);
            _uniforms.Add(key, location);
        }

        GL.GetProgram(
                _handle,
                GetProgramParameterName.ActiveAttributes, out var attributeCount);
        _attributes = new Dictionary<string, int>();

        for (var i = 0; i < attributeCount; i++)
        {
            var key = GL.GetActiveAttrib(_handle, i, out _, out _);
            var location = GL.GetAttribLocation(_handle, key);
            _attributes.Add(key, location);
        }
    }

    ~Shader()
    {
        Dispose(false);
    }

    private void Error(string msg)
    {
        Console.Error.WriteLine(
                $"Error in '{_vertPath}' + '{_fragPath}'\n\t{msg}");
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        GL.DeleteProgram(_handle);
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private int LoadShader(string path, ShaderType type)
    {
        string source; 
        try
        {
            source = File.ReadAllText(path);
        }
        catch
        {
            Error($"Error: could not open '${path}'");
            return 0;
        }

        var shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string info = GL.GetShaderInfoLog(shader);
            Error(info);
            GL.DeleteShader(shader);
            return 0;
        }
        return shader;
    }

    internal void Use()
    {
        GL.UseProgram(_handle);
    }

    internal int GetAttributeLocation(string attribute)
    {
        if (!_attributes.ContainsKey(attribute))
        {
            Error(
                $"Attribute '{attribute}' does not exist (Did you forget to define it? Or are you not using it?)");
            // Fill in the uniform slot with -1 so that this error message
            // is not printed more than once
            _attributes.Add(attribute, -1);
        }
        return _attributes[attribute];

    }

    private int GetUniformLocation(string uniform)
    {
        if (!_uniforms.ContainsKey(uniform))
        {
            Error($"Uniform '{uniform}' does not exist (Did you forget to define it? Or are you not using it?)");
            // Fill in the uniform slot with -1 so that this error message
            // is not printed more than once
            _uniforms.Add(uniform, -1);
        }
        return _uniforms[uniform];
    }

    public void Send(string uniform, Matrix4 mat)
    {
        Use();
        GL.UniformMatrix4(GetUniformLocation(uniform), false, ref mat);
    }

    public void Send(string uniform, float f)
    {
        Use();
        GL.Uniform1(GetUniformLocation(uniform), f);
    }

    public void Send(string uniform, double f)
    {
        Use();
        GL.Uniform1(GetUniformLocation(uniform), (float)f);
    }

    public void Send(string uniform, int f)
    {
        Use();
        GL.Uniform1(GetUniformLocation(uniform), f);
    }

    public void Send(string uniform, uint f)
    {
        Use();
        GL.Uniform1(GetUniformLocation(uniform), f);
    }

    public void Send(string uniform, Vec2 v)
    {
        Use();
        GL.Uniform2(GetUniformLocation(uniform), v.X, v.Y);
    }

    public void Send(string uniform, Vec2i v)
    {
        Use();
        GL.Uniform2(GetUniformLocation(uniform), v.X, v.Y);
    }

    public void Send<T>(string uniform, T v)
    {
        Console.Error.WriteLine(
                $"Type '{typeof(T).ToString()}' not supported with shaders");
    }
}
