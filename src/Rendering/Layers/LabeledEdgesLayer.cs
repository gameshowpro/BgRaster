namespace GameshowPro.BgRaster.Rendering.Layers;

using System.Globalization;
using GameshowPro.BgRaster.Rendering;

sealed class LabeledEdgesLayer : ILayer
{
    // Temporary anchor debug overlay. Keep for future geometry tuning sessions.
    // static bool ShowLabelAnchorDebugDot => true;

    public void Render(RenderContext context, SKCanvas canvas)
    {
        if (context.Options.LabeledEdgesSides.Length == 0 || context.Options.LabeledEdgesThicknessPx <= 0f)
            return;

        float textSizePx = context.Options.LabeledEdgesTextSizePx;
        if (textSizePx <= 0f)
            return;

        foreach (LabeledEdgeSide side in context.Options.LabeledEdgesSides)
        {
            string label = BuildLabelTextForContext(side, context);
            if (string.IsNullOrWhiteSpace(label))
                continue;

            SKPoint target = GetTargetPoint(side, context.CanvasOffsetX, context.CanvasOffsetY, context.ViewportWidth, context.ViewportHeight);
            SKPoint direction = GetDirectionVector(side);
            SKPoint anchor = GetTailStartPoint(target, direction, context.Options.LabeledEdgesTailLengthPx, context.Options.LabeledEdgesThicknessPx, context.Options.LabeledEdgesHeadScale);
            SKPoint labelAnchor = GetLabelAnchorPoint(side, anchor, textSizePx);
            TextAnchor textAnchor = GetTextAnchor(side);

            DrawArrow(context, canvas, anchor, target);
            DrawAnchoredLabel(canvas, label, labelAnchor, textAnchor, textSizePx);
            // DrawLabelAnchorDebugDot(canvas, labelAnchor);
        }
    }

    static void DrawLabelAnchorDebugDot(SKCanvas canvas, SKPoint anchor)
    {
        // if (!ShowLabelAnchorDebugDot)
        //     return;

        int pixelX = (int)MathF.Round(anchor.X, MidpointRounding.AwayFromZero);
        int pixelY = (int)MathF.Round(anchor.Y, MidpointRounding.AwayFromZero);

        using SKPaint dotPaint = new()
        {
            Color = SKColors.Red,
            IsAntialias = false,
            Style = SKPaintStyle.Fill,
        };
        canvas.DrawRect(pixelX, pixelY, 1f, 1f, dotPaint);
    }

    internal static SKPoint GetTargetPoint(LabeledEdgeSide side, int canvasOffsetX, int canvasOffsetY, int viewportWidth, int viewportHeight)
    {
        float left = canvasOffsetX;
        float top = canvasOffsetY;
        float right = canvasOffsetX + viewportWidth - 1f;
        float bottom = canvasOffsetY + viewportHeight - 1f;
        float centerX = canvasOffsetX + viewportWidth / 2f;
        float centerY = canvasOffsetY + viewportHeight / 2f;

        return side switch
        {
            LabeledEdgeSide.TL => new SKPoint(left, top),
            LabeledEdgeSide.T => new SKPoint(centerX, top),
            LabeledEdgeSide.TR => new SKPoint(right, top),
            LabeledEdgeSide.R => new SKPoint(right, centerY),
            LabeledEdgeSide.BR => new SKPoint(right, bottom),
            LabeledEdgeSide.B => new SKPoint(centerX, bottom),
            LabeledEdgeSide.BL => new SKPoint(left, bottom),
            LabeledEdgeSide.L => new SKPoint(left, centerY),
            _ => new SKPoint(centerX, centerY),
        };
    }

    internal static string BuildLabelText(LabeledEdgeSide side, int viewportWidth, int viewportHeight)
    {
        int width = Math.Max(0, viewportWidth);
        int height = Math.Max(0, viewportHeight);

        return BuildLabelText(
            side,
            left: 0,
            top: 0,
            right: width,
            bottom: height,
            centerX: width / 2,
            centerY: height / 2);
    }

    static string BuildLabelTextForContext(LabeledEdgeSide side, RenderContext context)
    {
        int viewportWidth = Math.Max(0, context.ViewportWidth);
        int viewportHeight = Math.Max(0, context.ViewportHeight);

        int localLeft = context.CanvasOffsetX;
        int localTop = context.CanvasOffsetY;
        int localRight = context.CanvasOffsetX + viewportWidth;
        int localBottom = context.CanvasOffsetY + viewportHeight;
        int localCenterX = context.CanvasOffsetX + viewportWidth / 2;
        int localCenterY = context.CanvasOffsetY + viewportHeight / 2;

        return context.Options.LabeledEdgesScope switch
        {
            LabeledEdgesScope.Slice => BuildLabelText(
                side,
                left: 0,
                top: 0,
                right: viewportWidth,
                bottom: viewportHeight,
                centerX: viewportWidth / 2,
                centerY: viewportHeight / 2),
            LabeledEdgesScope.Desktop => BuildLabelText(
                side,
                left: context.OutputRecord.DesktopX + localLeft,
                top: context.OutputRecord.DesktopY + localTop,
                right: context.OutputRecord.DesktopX + localRight,
                bottom: context.OutputRecord.DesktopY + localBottom,
                centerX: context.OutputRecord.DesktopX + localCenterX,
                centerY: context.OutputRecord.DesktopY + localCenterY),
            _ => BuildLabelText(
                side,
                left: localLeft,
                top: localTop,
                right: localRight,
                bottom: localBottom,
                centerX: localCenterX,
                centerY: localCenterY),
        };
    }

    static string BuildLabelText(LabeledEdgeSide side, int left, int top, int right, int bottom, int centerX, int centerY)
    {
        int safeLeft = left;
        int safeTop = top;
        int safeRight = Math.Max(left, right);
        int safeBottom = Math.Max(top, bottom);
        int safeCenterX = Math.Clamp(centerX, safeLeft, safeRight);
        int safeCenterY = Math.Clamp(centerY, safeTop, safeBottom);

        return side switch
        {
            LabeledEdgeSide.TL => $"{safeLeft},{safeTop}",
            LabeledEdgeSide.T => safeTop.ToString(CultureInfo.InvariantCulture),
            LabeledEdgeSide.TR => $"{safeRight},{safeTop}",
            LabeledEdgeSide.R => safeRight.ToString(CultureInfo.InvariantCulture),
            LabeledEdgeSide.BR => $"{safeRight},{safeBottom}",
            LabeledEdgeSide.B => safeBottom.ToString(CultureInfo.InvariantCulture),
            LabeledEdgeSide.BL => $"{safeLeft},{safeBottom}",
            LabeledEdgeSide.L => safeLeft.ToString(CultureInfo.InvariantCulture),
            _ => string.Empty,
        };
    }

    static SKPoint GetTailStartPoint(
        SKPoint target,
        SKPoint direction,
        float tailLengthPx,
        float thicknessPx,
        float headScale)
    {
        float gap = tailLengthPx + Math.Max(1f, thicknessPx * Math.Max(1f, headScale) * 4f);

        return new SKPoint(target.X - direction.X * gap, target.Y - direction.Y * gap);
    }

    static SKPoint GetDirectionVector(LabeledEdgeSide side)
    {
        const float invSqrt2 = 0.70710677f;

        return side switch
        {
            LabeledEdgeSide.TL => new SKPoint(-invSqrt2, -invSqrt2),
            LabeledEdgeSide.T => new SKPoint(0f, -1f),
            LabeledEdgeSide.TR => new SKPoint(invSqrt2, -invSqrt2),
            LabeledEdgeSide.R => new SKPoint(1f, 0f),
            LabeledEdgeSide.BR => new SKPoint(invSqrt2, invSqrt2),
            LabeledEdgeSide.B => new SKPoint(0f, 1f),
            LabeledEdgeSide.BL => new SKPoint(-invSqrt2, invSqrt2),
            LabeledEdgeSide.L => new SKPoint(-1f, 0f),
            _ => new SKPoint(0f, -1f),
        };
    }

    static SKPoint GetLabelAnchorPoint(LabeledEdgeSide side, SKPoint tailStart, float textSizePx)
    {
        float baseX = MathF.Round(tailStart.X, MidpointRounding.AwayFromZero);
        float baseY = MathF.Round(tailStart.Y, MidpointRounding.AwayFromZero);
        float offset = Math.Max(0f, MathF.Round(textSizePx * 0.2f, MidpointRounding.AwayFromZero));
        SKPoint direction = GetLabelOffsetDirection(side);
        float anchorX = baseX + direction.X * offset;
        float anchorY = baseY + direction.Y * offset;

        // Preserved correction math from anchor calibration pass:
        // Raster-grid correction for negative axis direction and top-left pixel addressing.
        // if (direction.X < 0f)
        //     anchorX -= 1f;
        //
        // if (direction.Y < 0f)
        //     anchorY -= 1f;

        return new SKPoint(anchorX, anchorY);
    }

    static SKPoint GetLabelOffsetDirection(LabeledEdgeSide side)
    {
        const float invSqrt2 = 0.70710677f;

        return side switch
        {
            LabeledEdgeSide.TL => new SKPoint(invSqrt2, invSqrt2),
            LabeledEdgeSide.T => new SKPoint(0f, 1f),
            LabeledEdgeSide.TR => new SKPoint(-invSqrt2, invSqrt2),
            LabeledEdgeSide.R => new SKPoint(-1f, 0f),
            LabeledEdgeSide.BR => new SKPoint(-invSqrt2, -invSqrt2),
            LabeledEdgeSide.B => new SKPoint(0f, -1f),
            LabeledEdgeSide.BL => new SKPoint(invSqrt2, -invSqrt2),
            LabeledEdgeSide.L => new SKPoint(1f, 0f),
            _ => new SKPoint(0f, 0f),
        };
    }

    static TextAnchor GetTextAnchor(LabeledEdgeSide side) => side switch
    {
        LabeledEdgeSide.TL => TextAnchor.TopLeft,
        LabeledEdgeSide.T => TextAnchor.TopCenter,
        LabeledEdgeSide.TR => TextAnchor.TopRight,
        LabeledEdgeSide.R => TextAnchor.MiddleRight,
        LabeledEdgeSide.BR => TextAnchor.BaselineRight,
        LabeledEdgeSide.B => TextAnchor.BaselineCenter,
        LabeledEdgeSide.BL => TextAnchor.BaselineLeft,
        LabeledEdgeSide.L => TextAnchor.MiddleLeft,
        _ => TextAnchor.TopLeft,
    };

    static void DrawArrow(RenderContext context, SKCanvas canvas, SKPoint start, SKPoint target)
    {
        float dx = target.X - start.X;
        float dy = target.Y - start.Y;
        float length = MathF.Sqrt(dx * dx + dy * dy);
        if (length <= 0f)
            return;

        float directionX = dx / length;
        float directionY = dy / length;

        float thicknessPx = context.Options.LabeledEdgesThicknessPx;
        float headScale = Math.Max(0f, context.Options.LabeledEdgesHeadScale);
        float headLength = Math.Max(1f, thicknessPx * headScale * 4f);
        float headWidth = Math.Max(1f, thicknessPx * headScale * 3f);
        float stemLength = Math.Max(0f, context.Options.LabeledEdgesTailLengthPx);

        SKPoint stemEnd = new(start.X + directionX * stemLength, start.Y + directionY * stemLength);
        SKPoint headBase = new(target.X - directionX * headLength, target.Y - directionY * headLength);
        SKPoint normal = new(-directionY, directionX);
        float halfThickness = thicknessPx / 2f;
        SKPoint tailStartLeft = new(start.X + normal.X * halfThickness, start.Y + normal.Y * halfThickness);
        SKPoint tailStartRight = new(start.X - normal.X * halfThickness, start.Y - normal.Y * halfThickness);
        SKPoint tailEndLeft = new(stemEnd.X + normal.X * halfThickness, stemEnd.Y + normal.Y * halfThickness);
        SKPoint tailEndRight = new(stemEnd.X - normal.X * halfThickness, stemEnd.Y - normal.Y * halfThickness);
        SKPoint headLeft = new(headBase.X + normal.X * (headWidth / 2f), headBase.Y + normal.Y * (headWidth / 2f));
        SKPoint headRight = new(headBase.X - normal.X * (headWidth / 2f), headBase.Y - normal.Y * (headWidth / 2f));

        using SKPaint tailPaint = new()
        {
            Color = SKColors.White,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };

        using SKPaint headPaint = new()
        {
            Color = SKColors.White,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };

        using (SKPath tail = new())
        {
            tail.MoveTo(tailStartLeft);
            tail.LineTo(tailStartRight);
            tail.LineTo(tailEndRight);
            tail.LineTo(tailEndLeft);
            tail.Close();
            canvas.DrawPath(tail, tailPaint);
        }

        using (SKPath head = new())
        {
            head.MoveTo(target);
            head.LineTo(headLeft);
            head.LineTo(headRight);
            head.Close();
            canvas.DrawPath(head, headPaint);
        }
    }

    static void DrawAnchoredLabel(SKCanvas canvas, string label, SKPoint anchor, TextAnchor textAnchor, float textSizePx)
    {
        using SKPaint textPaint = new()
        {
            Color = SKColors.White,
            IsAntialias = true,
            Typeface = FontManager.Typeface,
            TextSize = textSizePx,
        };

        SKRect bounds = default;
        float advanceWidth = textPaint.MeasureText(label, ref bounds);

        DrawTextAt(canvas, label, textPaint, anchor.X, anchor.Y, textAnchor, bounds, advanceWidth);
    }

    static void DrawTextAt(SKCanvas canvas, string text, SKPaint paint, float anchorX, float anchorY, TextAnchor textAnchor, SKRect bounds, float advanceWidth)
    {
        float x = anchorX;
        float baselineY = anchorY;

        switch (textAnchor)
        {
            case TextAnchor.TopLeft:
                x -= bounds.Left;
                baselineY -= bounds.Top;
                break;
            case TextAnchor.TopCenter:
                x -= advanceWidth / 2f;
                baselineY -= bounds.Top;
                break;
            case TextAnchor.TopRight:
                // Preserved pixel-edge compensation from text calibration pass:
                // x += 1f;
                x -= advanceWidth;
                baselineY -= bounds.Top;
                break;
            case TextAnchor.MiddleLeft:
                x -= bounds.Left;
                baselineY -= bounds.MidY;
                break;
            case TextAnchor.MiddleCenter:
                x -= advanceWidth / 2f;
                baselineY -= bounds.MidY;
                break;
            case TextAnchor.MiddleRight:
                // x += 1f;
                x -= advanceWidth;
                baselineY -= bounds.MidY;
                break;
            case TextAnchor.BottomLeft:
                x -= bounds.Left;
                // baselineY += 1f;
                baselineY -= bounds.Bottom;
                break;
            case TextAnchor.BottomCenter:
                x -= advanceWidth / 2f;
                // baselineY += 1f;
                baselineY -= bounds.Bottom;
                break;
            case TextAnchor.BottomRight:
                // x += 1f;
                x -= advanceWidth;
                // baselineY += 1f;
                baselineY -= bounds.Bottom;
                break;
            case TextAnchor.BaselineLeft:
                x -= bounds.Left;
                // baselineY += 1f;
                break;
            case TextAnchor.BaselineCenter:
                x -= advanceWidth / 2f;
                // baselineY += 1f;
                break;
            case TextAnchor.BaselineRight:
                // x += 1f;
                x -= advanceWidth;
                // baselineY += 1f;
                break;
        }

        canvas.DrawText(text, x, baselineY, paint);
    }

    enum TextAnchor
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
        BaselineLeft,
        BaselineCenter,
        BaselineRight,
    }
}
