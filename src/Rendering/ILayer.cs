namespace GameshowPro.BgRaster.Rendering;

interface ILayer
{
    void Render(RenderContext context, SKCanvas canvas);
}
