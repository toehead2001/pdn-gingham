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

        public enum PropertyNames
        {
            Amount1,
            Amount2,
            Amount3,
            Amount4
        }

        public enum Amount3Options
        {
            Amount3Option1,
            Amount3Option2,
            Amount3Option3,
            Amount3Option4,
            Amount3Option5
        }

        public enum Amount4Options
        {
            Amount4Option1,
            Amount4Option2,
            Amount4Option3,
            Amount4Option4,
            Amount4Option5
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>
            {
                new Int32Property(PropertyNames.Amount1, 20, 2, 100),
                new Int32Property(PropertyNames.Amount2, ColorBgra.ToOpaqueInt32(ColorBgra.FromBgra(EnvironmentParameters.PrimaryColor.B, EnvironmentParameters.PrimaryColor.G, EnvironmentParameters.PrimaryColor.R, 255)), 0, 0xffffff),
                StaticListChoiceProperty.CreateForEnum<Amount3Options>(PropertyNames.Amount3, Amount3Options.Amount3Option3, false),
                StaticListChoiceProperty.CreateForEnum<Amount4Options>(PropertyNames.Amount4, Amount4Options.Amount4Option5, false)
            };

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Amount1, ControlInfoPropertyNames.DisplayName, "Line Width");
            configUI.SetPropertyControlValue(PropertyNames.Amount2, ControlInfoPropertyNames.DisplayName, "Color");
            configUI.SetPropertyControlType(PropertyNames.Amount2, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(PropertyNames.Amount3, ControlInfoPropertyNames.DisplayName, "Horizontal Style");
            PropertyControlInfo Amount3Control = configUI.FindControlForPropertyName(PropertyNames.Amount3);
            Amount3Control.SetValueDisplayName(Amount3Options.Amount3Option1, "Solid - 33% Opacity");
            Amount3Control.SetValueDisplayName(Amount3Options.Amount3Option2, "Solid - 66% Opacity");
            Amount3Control.SetValueDisplayName(Amount3Options.Amount3Option3, "Diagonal Lines - Up");
            Amount3Control.SetValueDisplayName(Amount3Options.Amount3Option4, "Diagonal Lines - Down");
            Amount3Control.SetValueDisplayName(Amount3Options.Amount3Option5, "Dots - 50/50");
            configUI.SetPropertyControlValue(PropertyNames.Amount4, ControlInfoPropertyNames.DisplayName, "Vertical Style");
            PropertyControlInfo Amount4Control = configUI.FindControlForPropertyName(PropertyNames.Amount4);
            Amount4Control.SetValueDisplayName(Amount4Options.Amount4Option1, "Solid - 33% Opacity");
            Amount4Control.SetValueDisplayName(Amount4Options.Amount4Option2, "Solid - 66% Opacity");
            Amount4Control.SetValueDisplayName(Amount4Options.Amount4Option3, "Diagonal Lines - Up");
            Amount4Control.SetValueDisplayName(Amount4Options.Amount4Option4, "Diagonal Lines - Down");
            Amount4Control.SetValueDisplayName(Amount4Options.Amount4Option5, "Dots - 50/50");

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            Amount1 = newToken.GetProperty<Int32Property>(PropertyNames.Amount1).Value;
            Amount2 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.Amount2).Value);
            Amount3 = (byte)((int)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.Amount3).Value);
            Amount4 = (byte)((int)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.Amount4).Value);

            Rectangle selection = EnvironmentParameters.GetSelection(srcArgs.Bounds).GetBoundsInt();

            Bitmap ginghamBitmap = new Bitmap(selection.Width, selection.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics ginghamGraphics = Graphics.FromImage(ginghamBitmap);

            // Fill with white
            Rectangle backgroundRect = new Rectangle(0, 0, selection.Width, selection.Height);
            using (SolidBrush backColor = new SolidBrush(Color.White))
                ginghamGraphics.FillRectangle(backColor, backgroundRect);

            // Set Brush Styles
            Brush xBrush;
            switch (Amount3)
            {
                case 0: // Solid 33% Opacity
                    xBrush = new SolidBrush(Color.FromArgb(85, Amount2));
                    break;
                case 1: // Solid 66% Opacity
                    xBrush = new SolidBrush(Color.FromArgb(170, Amount2));
                    break;
                case 2: // Diagonal Lines Up
                    xBrush = new HatchBrush(HatchStyle.DarkUpwardDiagonal, Amount2, Color.White);
                    break;
                case 3: // Diagonal Lines Down
                    xBrush = new HatchBrush(HatchStyle.DarkDownwardDiagonal, Amount2, Color.White);
                    break;
                case 4: // 50/50 Dots
                    xBrush = new HatchBrush(HatchStyle.Percent50, Amount2, Color.White);
                    break;
                default:
                    xBrush = new SolidBrush(Color.FromArgb(85, Amount2));
                    break;
            }
            Brush yBrush;
            switch (Amount4)
            {
                case 0: // Solid 33% Opacity
                    yBrush = new SolidBrush(Color.FromArgb(85, Amount2));
                    break;
                case 1: // Solid 66% Opacity
                    yBrush = new SolidBrush(Color.FromArgb(170, Amount2));
                    break;
                case 2: // Diagonal Lines Up
                    yBrush = new HatchBrush(HatchStyle.DarkUpwardDiagonal, Amount2, Color.White);
                    break;
                case 3: // Diagonal Lines Down
                    yBrush = new HatchBrush(HatchStyle.DarkDownwardDiagonal, Amount2, Color.White);
                    break;
                case 4: // 50/50 Dots
                    yBrush = new HatchBrush(HatchStyle.Percent50, Amount2, Color.White);
                    break;
                default:
                    yBrush = new SolidBrush(Color.FromArgb(85, Amount2));
                    break;
            }

            // Set Pens
            Pen xPen = new Pen(xBrush, Amount1);
            xBrush.Dispose();
            Pen yPen = new Pen(yBrush, Amount1);
            yBrush.Dispose();
            Pen xyPen = new Pen(Amount2, Amount1);

            // Calculate the number of lines will fit in the selection
            int xLines = (int)Math.Ceiling((double)selection.Height / Amount1 / 2);
            int yLines = (int)Math.Ceiling((double)selection.Width / Amount1 / 2);

            // Draw Horizontal Lines
            for (int i = 0; i < xLines; i++)
            {
                // Create points that define line.
                Point point1 = new Point(0, Amount1 / 2 + Amount1 * i * 2);
                Point point2 = new Point(selection.Width, Amount1 / 2 + Amount1 * i * 2);

                // Draw line to screen.
                ginghamGraphics.DrawLine(xPen, point1, point2);
            }
            xPen.Dispose();

            // Draw Vertical Lines
            for (int i = 0; i < yLines; i++)
            {
                // Create points that define line.
                Point point1 = new Point(Amount1 / 2 + Amount1 * i * 2, 0);
                Point point2 = new Point(Amount1 / 2 + Amount1 * i * 2, selection.Height);

                // Draw line to screen.
                ginghamGraphics.DrawLine(yPen, point1, point2);
            }
            yPen.Dispose();

            // Draw Horizontal & Vertical intersections
            for (int x = 0; x < xLines; x++)
            {
                for (int y = 0; y < yLines; y++)
                {
                    // Create points that define line.
                    Point point1 = new Point(Amount1 * 2 * y, Amount1 / 2 + Amount1 * x * 2);
                    Point point2 = new Point(Amount1 * 2 * y + Amount1, Amount1 / 2 + Amount1 * x * 2);

                    // Draw line to screen.
                    ginghamGraphics.DrawLine(xyPen, point1, point2);
                }
            }
            xyPen.Dispose();

            ginghamSurface = Surface.CopyFromBitmap(ginghamBitmap);
            ginghamBitmap.Dispose();

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            if (length == 0) return;
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Render(DstArgs.Surface, SrcArgs.Surface, renderRects[i]);
            }
        }

        private int Amount1 = 20; // [2,100] Line Width
        private ColorBgra Amount2 = ColorBgra.FromBgr(0, 0, 0); // Color
        private byte Amount3 = 2; // Horizontal Pattern|Solid - 33% Opacity|Solid - 66% Opacity|Diagonal Lines - Up|Diagonal Lines - Down|50/50 Dots
        private byte Amount4 = 4; // Vertical Pattern|Solid - 33% Opacity|Solid - 66% Opacity|Diagonal Lines - Up|Diagonal Lines - Down|50/50 Dots

        private Surface ginghamSurface;

        private void Render(Surface dst, Surface src, Rectangle rect)
        {
            Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();

            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                if (IsCancelRequested) return;
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    dst[x, y] = ginghamSurface.GetBilinearSample(x - selection.Left, y - selection.Top);
                }
            }
        }
    }
}
