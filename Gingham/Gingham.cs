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
        public string Author
        {
            get
            {
                return ((AssemblyCopyrightAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright;
            }
        }
        public string Copyright
        {
            get
            {
                return ((AssemblyDescriptionAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0]).Description;
            }
        }

        public string DisplayName
        {
            get
            {
                return ((AssemblyProductAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0]).Product;
            }
        }

        public Version Version
        {
            get
            {
                return base.GetType().Assembly.GetName().Version;
            }
        }

        public Uri WebsiteUri
        {
            get
            {
                return new Uri("http://www.getpaint.net/redirect/plugins.html");
            }
        }
    }

    [PluginSupportInfo(typeof(PluginSupportInfo), DisplayName = "Gingham")]
    public class GinghamEffectPlugin : PropertyBasedEffect
    {
        public static string StaticName
        {
            get
            {
                return "Gingham";
            }
        }

        public static Image StaticIcon
        {
            get
            {
                return new Bitmap(typeof(GinghamEffectPlugin), "Gingham.png");
            }
        }

        public static string SubmenuName
        {
            get
            {
                return SubmenuNames.Render;  // Programmer's chosen default
            }
        }

        public GinghamEffectPlugin()
            : base(StaticName, StaticIcon, SubmenuName, EffectFlags.Configurable)
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
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.Amount1, 20, 2, 100));
            props.Add(new Int32Property(PropertyNames.Amount2, ColorBgra.ToOpaqueInt32(ColorBgra.FromBgra(EnvironmentParameters.PrimaryColor.B, EnvironmentParameters.PrimaryColor.G, EnvironmentParameters.PrimaryColor.R, 255)), 0, 0xffffff));
            props.Add(StaticListChoiceProperty.CreateForEnum<Amount3Options>(PropertyNames.Amount3, Amount3Options.Amount3Option3, false));
            props.Add(StaticListChoiceProperty.CreateForEnum<Amount4Options>(PropertyNames.Amount4, Amount4Options.Amount4Option5, false));

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

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);


            Rectangle selection = EnvironmentParameters.GetSelection(srcArgs.Bounds).GetBoundsInt();

            Bitmap ginghamBitmap = new Bitmap(selection.Width, selection.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(ginghamBitmap);

            // Fill with white
            Rectangle backgroundRect = new Rectangle(0, 0, selection.Width, selection.Height);
            g.FillRectangle(new SolidBrush(Color.White), backgroundRect);

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
            Brush solidBrush = new SolidBrush(Amount2);

            // Set Pens
            Pen xPen = new Pen(xBrush, Amount1);
            xBrush.Dispose();
            Pen yPen = new Pen(yBrush, Amount1);
            yBrush.Dispose();
            Pen xyPen = new Pen(solidBrush, Amount1);
            solidBrush.Dispose();

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
                g.DrawLine(xPen, point1, point2);
            }
            xPen.Dispose();
            // Draw Vertical Lines
            for (int i = 0; i < yLines; i++)
            {
                // Create points that define line.
                Point point1 = new Point(Amount1 / 2 + Amount1 * i * 2, 0);
                Point point2 = new Point(Amount1 / 2 + Amount1 * i * 2, selection.Height);

                // Draw line to screen.
                g.DrawLine(yPen, point1, point2);
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
                    g.DrawLine(xyPen, point1, point2);
                }
            }
            xyPen.Dispose();

            ginghamSurface = Surface.CopyFromBitmap(ginghamBitmap);
            ginghamBitmap.Dispose();
        }

        protected override unsafe void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            if (length == 0) return;
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Render(DstArgs.Surface, SrcArgs.Surface, rois[i]);
            }
        }

        #region User Entered Code
        #region UICode
        int Amount1 = 20; // [2,100] Line Width
        ColorBgra Amount2 = ColorBgra.FromBgr(0, 0, 0); // Color
        byte Amount3 = 2; // Horizontal Pattern|Solid - 33% Opacity|Solid - 66% Opacity|Diagonal Lines - Up|Diagonal Lines - Down|50/50 Dots
        byte Amount4 = 4; // Vertical Pattern|Solid - 33% Opacity|Solid - 66% Opacity|Diagonal Lines - Up|Diagonal Lines - Down|50/50 Dots
        #endregion

        private Surface ginghamSurface;

        void Render(Surface dst, Surface src, Rectangle rect)
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

        #endregion
    }
}