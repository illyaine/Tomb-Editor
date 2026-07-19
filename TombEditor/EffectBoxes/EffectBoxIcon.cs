using System.Drawing;
using System.Drawing.Drawing2D;

namespace TombEditor
{
    /// <summary>
    /// Creates the editor icon without adding another generated resource entry.
    /// The yellow volume with a sparkle distinguishes effect boxes from trigger volumes.
    /// </summary>
    internal static class EffectBoxIcon
    {
        public static Image Image16 { get; } = CreateImage(16);

        private static Bitmap CreateImage(int size)
        {
            var bitmap = new Bitmap(size, size);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                var outline = Color.FromArgb(255, 166, 28);
                var front = Color.FromArgb(205, 154, 15);
                var left = Color.FromArgb(236, 187, 32);
                var top = Color.FromArgb(255, 218, 73);

                var topFace = new[]
                {
                    new PointF(3.0f, 5.0f),
                    new PointF(7.5f, 2.2f),
                    new PointF(12.0f, 5.0f),
                    new PointF(7.5f, 7.8f)
                };
                var leftFace = new[]
                {
                    new PointF(3.0f, 5.0f),
                    new PointF(7.5f, 7.8f),
                    new PointF(7.5f, 13.1f),
                    new PointF(3.0f, 10.2f)
                };
                var rightFace = new[]
                {
                    new PointF(7.5f, 7.8f),
                    new PointF(12.0f, 5.0f),
                    new PointF(12.0f, 10.2f),
                    new PointF(7.5f, 13.1f)
                };

                using (var brush = new SolidBrush(top))
                    graphics.FillPolygon(brush, topFace);
                using (var brush = new SolidBrush(left))
                    graphics.FillPolygon(brush, leftFace);
                using (var brush = new SolidBrush(front))
                    graphics.FillPolygon(brush, rightFace);
                using (var pen = new Pen(outline, 1.0f))
                {
                    graphics.DrawPolygon(pen, topFace);
                    graphics.DrawPolygon(pen, leftFace);
                    graphics.DrawPolygon(pen, rightFace);
                }

                // Small effect sparkle.
                using (var pen = new Pen(Color.FromArgb(255, 246, 151), 1.25f))
                {
                    graphics.DrawLine(pen, 12.6f, 1.0f, 12.6f, 4.2f);
                    graphics.DrawLine(pen, 11.0f, 2.6f, 14.2f, 2.6f);
                    graphics.DrawLine(pen, 11.5f, 1.5f, 13.7f, 3.7f);
                    graphics.DrawLine(pen, 13.7f, 1.5f, 11.5f, 3.7f);
                }
            }

            return bitmap;
        }
    }
}
