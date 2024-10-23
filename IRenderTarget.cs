namespace HPEngine;

public interface IRenderTarget
{
    public Framebuffer BindFramebuffer(Renderer renderer);
    public void UnbindFramebuffer(Renderer renderer) {}
}
