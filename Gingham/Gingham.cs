using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Collections.Generic;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;

namespace GinghamEffect
{
    public class PluginSupportInfo : IPluginSupportInfo
    {
        public string Author => base.GetType().Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
        public string Copyright => base.GetType().Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
        public string DisplayName => base.GetType().Assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
        public Version Version => base.GetType().Assembly.GetName().Version;
        public Uri WebsiteUri => new Uri("https://forums.getpaint.net/index.php?showtopic=32371");
    }

    [PluginSupportInfo(typeof(PluginSupportInfo), DisplayName = "Gingham")]
    public class GinghamEffectPlugin : PropertyBasedEffect
    {
        private static readonly Image StaticIcon = new Bitmap(typeof(GinghamEffectPlugin), "Gingham.png");

        public GinghamEffectPlugin()
            : base("Gingham", StaticIcon, SubmenuNames.Render, EffectFlags.Configurable)
        {
        }

        private Surface ginghamSurface;

        private enum PropertyNames
        {
            LineWidth,
            Color,
            HorStyle,
            VerStyle
        }

        private enum LineStyle
        {
            Solid33Opacity,
            Solid66Opacity,
            DiagonalLinesUp,
            DiagonalLinesDown,
            Dots5050
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>
            {
                new Int32Property(PropertyNames.LineWidth, 20, 2, 100),
                new Int32Property(PropertyNames.Color, ColorBgra.ToOpaqueInt32(ColorBgra.FromBgra(EnvironmentParameters.PrimaryColor.B, EnvironmentParameters.PrimaryColor.G, EnvironmentParameters.PrimaryColor.R, 255)), 0, 0xffffff),
                StaticListChoiceProperty.CreateForEnum<LineStyle>(PropertyNames.HorStyle, LineStyle.DiagonalLinesUp, false),
                StaticListChoiceProperty.CreateForEnum<LineStyle>(PropertyNames.VerStyle, LineStyle.Dots5050, false)
            };

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.LineWidth, ControlInfoPropertyNames.DisplayName, "Line Width");
            configUI.SetPropertyControlValue(PropertyNames.Color, ControlInfoPropertyNames.DisplayName, "Color");
            configUI.SetPropertyControlType(PropertyNames.Color, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(PropertyNames.HorStyle, ControlInfoPropertyNames.DisplayName, "Horizontal Style");
            PropertyControlInfo Amount3Control = configUI.FindControlForPropertyName(PropertyNames.HorStyle);
            Amount3Control.SetValueDisplayName(LineStyle.Solid33Opacity, "Solid - 33% Opacity");
            Amount3Control.SetValueDisplayName(LineStyle.Solid66Opacity, "Solid - 66% Opacity");
            Amount3Control.SetValueDisplayName(LineStyle.DiagonalLinesUp, "Diagonal Lines - Up");
            Amount3Control.SetValueDisplayName(LineStyle.DiagonalLinesDown, "Diagonal Lines - Down");
            Amount3Control.SetValueDisplayName(LineStyle.Dots5050, "Dots - 50/50");
            configUI.SetPropertyControlValue(PropertyNames.VerStyle, ControlInfoPropertyNames.DisplayName, "Vertical Style");
            PropertyControlInfo Amount4Control = configUI.FindControlForPropertyName(PropertyNames.VerStyle);
            Amount4Control.SetValueDisplayName(LineStyle.Solid33Opacity, "Solid - 33% Opacity");
            Amount4Control.SetValueDisplayName(LineStyle.Solid66Opacity, "Solid - 66% Opacity");
            Amount4Control.SetValueDisplayName(LineStyle.DiagonalLinesUp, "Diagonal Lines - Up");
            Amount4Control.SetValueDisplayName(LineStyle.DiagonalLinesDown, "Diagonal Lines - Down");
            Amount4Control.SetValueDisplayName(LineStyle.Dots5050, "Dots - 50/50");

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            int lineWidth = newToken.GetProperty<Int32Property>(PropertyNames.LineWidth).Value;
            ColorBgra color = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.Color).Value);
            LineStyle horStyle = (LineStyle)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.HorStyle).Value;
            LineStyle verStyle = (LineStyle)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.VerStyle).Value;

            Rectangle selection = EnvironmentParameters.GetSelection(srcArgs.Bounds).GetBoundsInt();

            // Calculate the number of lines will fit in the selection
            int horLines = (int)Math.Ceiling((double)selection.Height / lineWidth / 2);
            int verLines = (int)Math.Ceiling((double)selection.Width / lineWidth / 2);

            if (ginghamSurface == null)
            {
                ginghamSurface = new Surface(dstArgs.Size);
            }

            using (Graphics g = new RenderArgs(ginghamSurface).Graphics)
            {
                using (SolidBrush backColor = new SolidBrush(Color.White))
                {
                    g.FillRectangle(backColor, selection);
                }

                // Draw Horizontal Lines
                using (Pen horPen = BuildPen(horStyle, lineWidth, color))
                {
                    for (int i = 0; i < horLines; i++)
                    {
                        Point leftPt = new Point(selection.Left, selection.Top + lineWidth / 2 + lineWidth * i * 2);
                        Point rightPt = new Point(selection.Right, selection.Top + lineWidth / 2 + lineWidth * i * 2);

                        g.DrawLine(horPen, leftPt, rightPt);
                    }
                }

                // Draw Vertical Lines
                using (Pen verPen = BuildPen(verStyle, lineWidth, color))
                {
                    for (int i = 0; i < verLines; i++)
                    {
                        Point topPt = new Point(selection.Left + lineWidth / 2 + lineWidth * i * 2, selection.Top);
                        Point bottomPt = new Point(selection.Left + lineWidth / 2 + lineWidth * i * 2, selection.Bottom);

                        g.DrawLine(verPen, topPt, bottomPt);
                    }
                }

                // Draw Horizontal & Vertical intersections
                using (Pen xyPen = new Pen(color, lineWidth))
                {
                    for (int x = 0; x < horLines; x++)
                    {
                        for (int y = 0; y < verLines; y++)
                        {
                            Point point1 = new Point(selection.Left + lineWidth * 2 * y, selection.Top + lineWidth / 2 + lineWidth * x * 2);
                            Point point2 = new Point(selection.Left + lineWidth * 2 * y + lineWidth, selection.Top + lineWidth / 2 + lineWidth * x * 2);

                            g.DrawLine(xyPen, point1, point2);
                        }
                    }
                }
            }

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            if (length == 0) return;

            DstArgs.Surface.CopySurface(ginghamSurface, renderRects, startIndex, length);
        }

        private static Pen BuildPen(LineStyle style, int width, ColorBgra color)
        {
            using (Brush brush = GetBrush(style))
            {
                return new Pen(brush, width);
            }

            Brush GetBrush(LineStyle lineStyle)
            {
                switch (lineStyle)
                {
                    case LineStyle.Solid66Opacity:
                        return new SolidBrush(Color.FromArgb(170, color));
                    case LineStyle.DiagonalLinesUp:
                        return new HatchBrush(HatchStyle.DarkUpwardDiagonal, color, Color.White);
                    case LineStyle.DiagonalLinesDown:
                        return new HatchBrush(HatchStyle.DarkDownwardDiagonal, color, Color.White);
                    case LineStyle.Dots5050:
                        return new HatchBrush(HatchStyle.Percent50, color, Color.White);
                    case LineStyle.Solid33Opacity:
                    default:
                        return new SolidBrush(Color.FromArgb(85, color));
                }
            }
        }
    }
}
