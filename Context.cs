using ImGuiNET;
using System.Diagnostics;
using OpenTK.Mathematics;
using HPEngine.Graphics;

namespace HPEngine;

public class Context
{
    internal ImGuiController ImGuiController;

    public Window Window;

    public Color ClearColor = Color.Black;
    public Renderer Renderer;

    public Input Input;

    public Context()
    {
        Window = new Window(WindowSettings.Default);

        Window.FramebufferResized += OnFramebufferSizeChanged;
        Window.MouseScrolled += OnMouseWheel;
        Window.TextInput += OnTextInput;

        Renderer = new Renderer(this);

        Input = new Input(Window);

        ImGuiController = new ImGuiController(this, Window.Size.X, Window.Size.Y);
    }

    private void OnFramebufferSizeChanged(Vec2i size)
    {
        ImGuiController.WindowResized(size.W, size.H);
    }

    private void OnMouseWheel(Vec2 by)
    {
        ImGuiController.MouseScroll(new Vector2(by.X, by.Y));
    }

    private void OnTextInput(char c)
    {
        ImGuiController.PressChar(c);
    }

    public void Run(App app)
    {
        app.OnStart();

        // FIXME: Should FPS counting and delta time be here?
        float fps = 0f;
        float framesRendered = 0f;
        var maxFps = 0f;

        var fpsSw = new Stopwatch();
        fpsSw.Start();

        var deltaSw = new Stopwatch();
        deltaSw.Start();

        var fpsRecord = new List<float>();

        TimeSpan dt = deltaSw.Elapsed;

        while (!Window.ShouldClose)
        {
            Window.PollEvents();

            dt = deltaSw.Elapsed;
            deltaSw.Restart();

            // Update
            app.OnUpdate((float)dt.TotalSeconds);
            ImGuiController.Update((float)dt.TotalSeconds);

            Renderer.SetRenderTarget(Window);

            // Draw
            Renderer.Clear(ClearColor);

#if DEBUG
            // Debug menu
            ImGui.Begin("Debug",
                    ImGuiWindowFlags.NoResize
                    | ImGuiWindowFlags.AlwaysAutoResize
                    | ImGuiWindowFlags.NoSavedSettings);
                ImGui.Text($"FPS: {MathF.Floor(fps)}");
                ImGui.Text($"Draw calls: {Renderer.GetDrawCalls()}");
            ImGui.End();
#endif

            app.OnDraw(Renderer);

#if DEBUG
            var plotHeight = 50f;
            var i = 0;
            foreach (var point in fpsRecord)
            {
                var percentage = point / maxFps;
                var height = plotHeight * percentage;
                var color = new Color(1f - percentage, percentage, 0f);
                Renderer.SetColor(color);
                Renderer.Rectangle(
                        new Vec2(i * 3, plotHeight - height),
                        new Vec2(2f, height));
                i++;
            }
#endif

            Renderer.PrepareNextFrame();
            ImGuiController.Render();
            Window.SwapBuffers();

            framesRendered++;
            if (fpsSw.Elapsed.TotalSeconds >= 1)
            {
                fps = framesRendered / (float)fpsSw.Elapsed.TotalSeconds;
                if (fpsRecord.Count > 30)
                    fpsRecord.RemoveAt(0);
                if (fps > maxFps)
                    maxFps = fps;
                fpsRecord.Add(fps);
                framesRendered = 0;
                fpsSw.Restart();
            }
        }
    }

    public void Close()
    {
        Window.Close();
    }
}
