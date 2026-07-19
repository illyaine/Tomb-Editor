using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.LevelData;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        private EffectBoxOverlay _effectBoxOverlay;

        internal void InitializeEffectBoxOverlay()
        {
            if (_effectBoxOverlay != null ||
                System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Runtime)
                return;

            _effectBoxOverlay = new EffectBoxOverlay(this)
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(_effectBoxOverlay);
            _effectBoxOverlay.BringToFront();
        }

        private sealed class EffectBoxOverlay : Control
        {
            private const int WmNcHitTest = 0x0084;
            private static readonly IntPtr HtTransparent = new IntPtr(-1);

            private static readonly Vector3[] Corners =
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3( 0.5f, -0.5f, -0.5f),
                new Vector3( 0.5f,  0.5f, -0.5f),
                new Vector3(-0.5f,  0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f,  0.5f,  0.5f),
                new Vector3(-0.5f,  0.5f,  0.5f)
            };

            private static readonly int[,] Edges =
            {
                { 0, 1 }, { 1, 2 }, { 2, 3 }, { 3, 0 },
                { 4, 5 }, { 5, 6 }, { 6, 7 }, { 7, 4 },
                { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 }
            };

            private static readonly int[,] Faces =
            {
                { 0, 1, 2, 3 },
                { 4, 5, 6, 7 },
                { 0, 1, 5, 4 },
                { 2, 3, 7, 6 },
                { 1, 2, 6, 5 },
                { 3, 0, 4, 7 }
            };

            private readonly Panel3D _owner;
            private readonly Timer _refreshTimer;

            public EffectBoxOverlay(Panel3D owner)
            {
                _owner = owner;
                SetStyle(ControlStyles.UserPaint |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
                TabStop = false;

                _refreshTimer = new Timer { Interval = 33 };
                _refreshTimer.Tick += RefreshTimer_Tick;
                _refreshTimer.Start();
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    var parameters = base.CreateParams;
                    parameters.ExStyle |= 0x20; // WS_EX_TRANSPARENT
                    return parameters;
                }
            }

            protected override void WndProc(ref Message message)
            {
                if (message.Msg == WmNcHitTest)
                {
                    message.Result = HtTransparent;
                    return;
                }

                base.WndProc(ref message);
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                // Preserve the DirectX surface below this transparent overlay.
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                if (!_owner.ShowVolumes ||
                    _owner._editor?.Level == null ||
                    !_owner._editor.Level.IsTombEngine ||
                    Width <= 0 || Height <= 0)
                    return;

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                var selectedObject = _owner._editor.SelectedObject;
                var boxes = _owner._editor.Level.GetAllObjects()
                    .OfType<BoxVolumeInstance>()
                    .Where(box => box.Room != null && box.IsEffectBox());

                foreach (var box in boxes)
                {
                    bool selected = ReferenceEquals(selectedObject, box);
                    if (!_owner.ShowAllRooms && !selected && box.Room != _owner._editor.SelectedRoom)
                        continue;

                    DrawEffectBox(e.Graphics, box, selected);
                }
            }

            private void DrawEffectBox(Graphics graphics, BoxVolumeInstance box, bool selected)
            {
                var matrix = Matrix4x4.CreateScale(box.Size) *
                             box.RotationPositionMatrix *
                             _owner._viewProjection;

                var points = new PointF[Corners.Length];
                var visible = new bool[Corners.Length];
                for (int index = 0; index < Corners.Length; index++)
                    visible[index] = TryProject(Corners[index], matrix, out points[index]);

                var baseColor = selected
                    ? Color.FromArgb(255, 242, 96)
                    : box.Enabled
                        ? Color.FromArgb(238, 190, 25)
                        : Color.FromArgb(165, 143, 66);

                using (var fillBrush = new SolidBrush(Color.FromArgb(selected ? 34 : 20, baseColor)))
                {
                    for (int face = 0; face < Faces.GetLength(0); face++)
                    {
                        var polygon = new PointF[4];
                        bool drawFace = true;
                        for (int point = 0; point < 4; point++)
                        {
                            int cornerIndex = Faces[face, point];
                            if (!visible[cornerIndex])
                            {
                                drawFace = false;
                                break;
                            }
                            polygon[point] = points[cornerIndex];
                        }

                        if (drawFace)
                            graphics.FillPolygon(fillBrush, polygon);
                    }
                }

                using (var pen = new Pen(baseColor, selected ? 2.2f : 1.35f))
                {
                    for (int edge = 0; edge < Edges.GetLength(0); edge++)
                    {
                        int first = Edges[edge, 0];
                        int second = Edges[edge, 1];
                        if (visible[first] && visible[second])
                            graphics.DrawLine(pen, points[first], points[second]);
                    }
                }

                if (selected && TryProject(Vector3.Zero, matrix, out var center))
                {
                    using (var brush = new SolidBrush(Color.FromArgb(255, 246, 151)))
                        graphics.FillEllipse(brush, center.X - 3.0f, center.Y - 3.0f, 6.0f, 6.0f);

                    graphics.DrawLine(Pens.White, center.X - 5.0f, center.Y, center.X + 5.0f, center.Y);
                    graphics.DrawLine(Pens.White, center.X, center.Y - 5.0f, center.X, center.Y + 5.0f);
                }
            }

            private bool TryProject(Vector3 point, Matrix4x4 matrix, out PointF screenPoint)
            {
                var clip = Vector4.Transform(new Vector4(point, 1.0f), matrix);
                if (clip.W <= 0.001f)
                {
                    screenPoint = PointF.Empty;
                    return false;
                }

                float inverseW = 1.0f / clip.W;
                float x = clip.X * inverseW;
                float y = clip.Y * inverseW;
                float z = clip.Z * inverseW;
                if (z < -0.2f || z > 1.2f)
                {
                    screenPoint = PointF.Empty;
                    return false;
                }

                screenPoint = new PointF(
                    (x + 1.0f) * 0.5f * Width,
                    (1.0f - y) * 0.5f * Height);
                return true;
            }

            private void RefreshTimer_Tick(object sender, EventArgs e)
            {
                if (Visible && _owner.Visible)
                    Invalidate();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _refreshTimer.Stop();
                    _refreshTimer.Tick -= RefreshTimer_Tick;
                    _refreshTimer.Dispose();
                }

                base.Dispose(disposing);
            }
        }
    }
}
