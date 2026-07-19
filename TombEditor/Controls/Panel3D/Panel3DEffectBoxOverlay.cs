using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TombLib.LevelData;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        private EffectBoxOverlayWindow _effectBoxOverlayWindow;

        /// <summary>
        /// Installs a post-present outline overlay which distinguishes Effect Boxes from
        /// ordinary volumes without drawing a second surface over the volume geometry.
        /// </summary>
        internal void InitializeEffectBoxOverlay()
        {
            if (_effectBoxOverlayWindow != null || IsDisposed ||
                System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Runtime)
                return;

            _effectBoxOverlayWindow = new EffectBoxOverlayWindow(this);
            Disposed += Panel3DEffectBoxOverlay_Disposed;
        }

        private void Panel3DEffectBoxOverlay_Disposed(object sender, EventArgs e)
        {
            Disposed -= Panel3DEffectBoxOverlay_Disposed;
            _effectBoxOverlayWindow?.Dispose();
            _effectBoxOverlayWindow = null;
        }

        private sealed class EffectBoxOverlayWindow : NativeWindow, IDisposable
        {
            private const int WmPaint = 0x000F;

            private static readonly (int A, int B)[] Edges =
            {
                (0, 1), (1, 3), (3, 2), (2, 0),
                (4, 5), (5, 7), (7, 6), (6, 4),
                (0, 4), (1, 5), (2, 6), (3, 7)
            };

            private readonly Panel3D _owner;
            private bool _drawing;
            private bool _disposed;

            public EffectBoxOverlayWindow(Panel3D owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
                _owner.HandleCreated += Owner_HandleCreated;
                _owner.HandleDestroyed += Owner_HandleDestroyed;

                if (_owner.IsHandleCreated)
                    AssignHandle(_owner.Handle);
            }

            protected override void WndProc(ref Message message)
            {
                base.WndProc(ref message);

                if (message.Msg == WmPaint && !_drawing && !_disposed)
                    DrawEffectBoxes();
            }

            private void Owner_HandleCreated(object sender, EventArgs e)
            {
                if (!_disposed && Handle == IntPtr.Zero)
                    AssignHandle(_owner.Handle);
            }

            private void Owner_HandleDestroyed(object sender, EventArgs e)
            {
                if (Handle != IntPtr.Zero)
                    ReleaseHandle();
            }

            private void DrawEffectBoxes()
            {
                var editor = _owner._editor;
                if (editor?.Level == null || !editor.Level.IsTombEngine ||
                    !_owner.ShowVolumes || !_owner.Visible ||
                    _owner.ClientSize.Width <= 0 || _owner.ClientSize.Height <= 0)
                    return;

                var effectBoxes = _owner.CollectRoomsToDraw()
                    .Where(room => _owner._frustum.Contains(room.WorldBoundingBox))
                    .SelectMany(room => room.Objects)
                    .OfType<BoxVolumeInstance>()
                    .Where(volume => volume.IsEffectBox())
                    .ToArray();

                if (effectBoxes.Length == 0)
                    return;

                _drawing = true;
                try
                {
                    using (var graphics = Graphics.FromHwnd(_owner.Handle))
                    {
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.CompositingMode = CompositingMode.SourceOver;

                        foreach (var box in effectBoxes)
                            DrawEffectBox(graphics, box);
                    }
                }
                catch (ExternalException)
                {
                    // The next paint pass redraws after handle or swap-chain recreation.
                }
                finally
                {
                    _drawing = false;
                }
            }

            private void DrawEffectBox(Graphics graphics, BoxVolumeInstance box)
            {
                var half = box.Size * 0.5f;
                var corners = new[]
                {
                    new Vector3(-half.X, -half.Y, -half.Z),
                    new Vector3( half.X, -half.Y, -half.Z),
                    new Vector3(-half.X,  half.Y, -half.Z),
                    new Vector3( half.X,  half.Y, -half.Z),
                    new Vector3(-half.X, -half.Y,  half.Z),
                    new Vector3( half.X, -half.Y,  half.Z),
                    new Vector3(-half.X,  half.Y,  half.Z),
                    new Vector3( half.X,  half.Y,  half.Z)
                };

                var matrix = box.RotationPositionMatrix * _owner._viewProjection;
                var projected = new PointF[corners.Length];
                for (int i = 0; i < corners.Length; i++)
                {
                    if (!TryProject(corners[i], matrix, _owner.ClientSize, out projected[i]))
                        return;
                }

                bool selected = ReferenceEquals(_owner._editor.SelectedObject, box);
                var outlineColor = box.Enabled
                    ? Color.FromArgb(245, 255, 207, 32)
                    : Color.FromArgb(210, 188, 165, 63);

                using (var pen = new Pen(outlineColor, selected ? 3.0f : 2.0f))
                {
                    pen.LineJoin = LineJoin.Round;
                    if (!box.Enabled)
                        pen.DashStyle = DashStyle.Dash;

                    foreach (var edge in Edges)
                        graphics.DrawLine(pen, projected[edge.A], projected[edge.B]);
                }

                var center = new PointF(
                    projected.Average(point => point.X),
                    projected.Average(point => point.Y));

                using (var font = new Font(SystemFonts.MessageBoxFont.FontFamily, 7.5f, FontStyle.Bold))
                using (var textBrush = new SolidBrush(Color.FromArgb(235, 255, 230, 105)))
                using (var shadowBrush = new SolidBrush(Color.FromArgb(180, 20, 20, 20)))
                {
                    const string label = "FX";
                    var textSize = graphics.MeasureString(label, font);
                    var position = new PointF(center.X - textSize.Width * 0.5f, center.Y - textSize.Height * 0.5f);
                    graphics.DrawString(label, font, shadowBrush, position.X + 1.0f, position.Y + 1.0f);
                    graphics.DrawString(label, font, textBrush, position);
                }
            }

            private static bool TryProject(Vector3 position, Matrix4x4 matrix, Size viewport,
                out PointF screenPosition)
            {
                var clip = Vector4.Transform(new Vector4(position, 1.0f), matrix);
                if (clip.W <= 0.001f || float.IsNaN(clip.W) || float.IsInfinity(clip.W))
                {
                    screenPosition = PointF.Empty;
                    return false;
                }

                var inverseW = 1.0f / clip.W;
                screenPosition = new PointF(
                    (clip.X * inverseW + 1.0f) * 0.5f * viewport.Width,
                    (1.0f - clip.Y * inverseW) * 0.5f * viewport.Height);

                return !float.IsNaN(screenPosition.X) && !float.IsNaN(screenPosition.Y) &&
                       !float.IsInfinity(screenPosition.X) && !float.IsInfinity(screenPosition.Y);
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                _owner.HandleCreated -= Owner_HandleCreated;
                _owner.HandleDestroyed -= Owner_HandleDestroyed;

                if (Handle != IntPtr.Zero)
                    ReleaseHandle();
            }
        }
    }
}
