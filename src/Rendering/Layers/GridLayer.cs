// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Rendering.Layers;

using System.Globalization;

sealed class GridLayer : ILayer
{
    public void Render(RenderContext context, SKCanvas canvas)
    {
        float cellSize = context.Options.GridSizePx;
        if (cellSize <= 0f) return;

        int ox = context.CanvasOffsetX;
        int oy = context.CanvasOffsetY;
        int vw = context.ViewportWidth;
        int vh = context.ViewportHeight;

        float offsetX = context.Options.GridOffsetXPx;
        float offsetY = context.Options.GridOffsetYPx;
        float strokePx = context.Options.GridStrokePx;

        SKColor oddColor = context.Options.GridOddColor;
        SKColor evenColor = context.Options.GridEvenColor;

        using SKPaint oddPaint = new() { Color = oddColor, IsAntialias = false };
        using SKPaint evenPaint = new() { Color = evenColor, IsAntialias = false };

        if (strokePx > 0f)
        {
            oddPaint.Style = SKPaintStyle.Stroke;
            oddPaint.StrokeWidth = strokePx;
            evenPaint.Style = SKPaintStyle.Stroke;
            evenPaint.StrokeWidth = strokePx;
        }

        float startX = ox + ((offsetX % cellSize) - cellSize) % cellSize;
        float startY = oy + ((offsetY % cellSize) - cellSize) % cellSize;

        bool drawCoords = context.Options.GridCoordinates;
        SKFont? coordFont = null;
        SKPaint? darkText = null;
        SKPaint? lightText = null;
        SKPaint? darkTri = null;
        SKPaint? lightTri = null;
        float fontSize = 0f;

        if (drawCoords)
        {
            fontSize = MathF.Max(6f, MathF.Min(cellSize * 0.22f, 18f));
            coordFont = new SKFont(FontManager.Typeface, fontSize);
            darkText = new SKPaint { Color = SKColors.Black, IsAntialias = true };
            lightText = new SKPaint { Color = SKColors.White, IsAntialias = true };
            darkTri = new SKPaint { Color = SKColors.Black, IsAntialias = true, Style = SKPaintStyle.Fill };
            lightTri = new SKPaint { Color = SKColors.White, IsAntialias = true, Style = SKPaintStyle.Fill };
        }

        try
        {
            int col = 0;
            for (float x = startX; x < ox + vw; x += cellSize, col++)
            {
                int row = 0;
                for (float y = startY; y < oy + vh; y += cellSize, row++)
                {
                    bool even = (col + row) % 2 == 0;
                    SKRect cellRect = SKRect.Create(x, y, cellSize, cellSize);
                    SKPaint paint = even ? evenPaint : oddPaint;
                    canvas.DrawRect(cellRect, paint);

                    if (drawCoords && coordFont is not null && cellSize >= 12f)
                    {
                        SKColor refColor = even ? evenColor : oddColor;
                        bool useLight = IsDark(refColor);
                        SKPaint textPaint = useLight ? lightText! : darkText!;
                        SKPaint triPaint = useLight ? lightTri! : darkTri!;

                        float triSize = MathF.Min(cellSize * 0.12f, 8f);
                        using (SKPath tri = new())
                        {
                            tri.MoveTo(x, y);
                            tri.LineTo(x + triSize, y);
                            tri.LineTo(x, y + triSize);
                            tri.Close();
                            canvas.DrawPath(tri, triPaint);
                        }

                        string colStr = col.ToString(CultureInfo.InvariantCulture);
                        string rowStr = row.ToString(CultureInfo.InvariantCulture);
                        float pad = MathF.Max(2f, cellSize * 0.05f);

                        canvas.DrawText(colStr, x + triSize + pad, y + fontSize, coordFont, textPaint);
                        canvas.DrawText("x", x + cellSize / 2f - fontSize / 4f, y + cellSize / 2f + fontSize / 3f, coordFont, textPaint);

                        float rowWidth = fontSize * 0.6f * rowStr.Length;
                        canvas.DrawText(rowStr, x + cellSize - rowWidth - pad, y + cellSize - pad, coordFont, textPaint);
                    }
                }
            }
        }
        finally
        {
            coordFont?.Dispose();
            darkText?.Dispose();
            lightText?.Dispose();
            darkTri?.Dispose();
            lightTri?.Dispose();
        }
    }

    static bool IsDark(SKColor c)
    {
        float lum = (0.299f * c.Red + 0.587f * c.Green + 0.114f * c.Blue) / 255f;
        float effective = lum * (c.Alpha / 255f) + (1f - c.Alpha / 255f) * 0f;
        return effective < 0.5f;
    }
}
