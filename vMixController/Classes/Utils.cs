using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using vMixController.Classes;
using vMixController.ViewModel;
using vMixController.Widgets;

namespace vMixController.Classes
{

    public enum Status
    {
        Offline,
        Sync,
        Online
    }
    public enum MessageType
    {
        Button,
        Volume
    }
    public struct DocumentMessage
    {
        public MessageType Type;
        public XmlDocument Document;
        public DateTime Timestamp;
    }
    public static class Utils
    {

        public static string MAINPAGENAME = "MAIN";
        public static string DATAPAGENAME = "DATA";
        public static string PAGE1PAGENAME = "PAGE 1";
        public static string PAGE2PAGENAME = "PAGE 2";
        public static string PAGE3PAGENAME = "PAGE 3";
        public static string PAGE4PAGENAME = "PAGE 4";
        public static string PAGE5PAGENAME = "PAGE 5";

        public static bool GetBit(this byte byt, byte index)
        {
            if (index < 0 || index > 7)
                throw new ArgumentOutOfRangeException();

            return (byt & (1 << index)) >> index != 0;
        }

        public static byte SetBit(this byte byt, byte index, bool value)
        {
            if (index < 0 || index > 7)
                throw new ArgumentOutOfRangeException();
            return (byte)((byt & ~(1 << index)) + (value ? 1 << index : 0));
        }

        static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public static ObservableCollection<vMixControl> LoadController(string fileName, IList<vMixFunctionReference> functions, out MainWindowSettings windowSettings)
        {
            windowSettings = null;
            try
            {
                var _controls = new ObservableCollection<vMixControl>();
                using (var stream = File.OpenRead(fileName))
                {
                    return LoadController(stream, functions, out windowSettings);
                }
            }
            catch (Exception e)
            {
                //Emergency controller
                _logger.Error(e, "Error while loading controller!");
                windowSettings = new MainWindowSettings() { Width = 512 + 16 + 16 + 8, Height = 512 + 196 + 48 };
                var btn = new vMixControlButton() { IsColorized = true, Color = vMixWidgetSettingsViewModel.Colors[11].A, BorderColor = vMixWidgetSettingsViewModel.Colors[11].B, Name = "Report Bug", Top = 16 + 16 + 8 + 512, Left = 8, Width = 512, IsCaptionVisible = false, IsCaptionOn = false };
                btn.Commands.Add(new vMixControlButtonCommand() {
                    Action = new vMixFunctionReference() { Function = NativeFunctions.WIN, Native = true },
                    StringParameter = "https://forums.vmix.com/postmessage?t=6468&f=8" });
                return new ObservableCollection<vMixControl>() {
                    new vMixControlRegion() { Text = string.Format("{0}\n\n\nP.S. Don't be afraid, your controller is OK.\nReport about it on forum.", string.Join("", SecurityElement.Escape(e.ToString()).Select(x=>(XmlConvert.IsXmlChar(x)?x.ToString():"0x" + Convert.ToByte(x).ToString())).ToArray())), Width = 512, Height = 512, Top = 8, Left = 8, Color = Colors.Red, Name = "Error while loading controller!" },
                    btn
                };
            }

        }

        public static ObservableCollection<vMixControl> LoadController(Stream stream, IList<vMixFunctionReference> functions, out MainWindowSettings windowSettings)
        {
            var _controls = new ObservableCollection<vMixControl>();
            _logger.Info("Controller loading.");
            var reader = XmlReader.Create(stream);
            {
                reader.ReadStartElement();
                reader.ReadStartElement();
                _logger.Info("Loading widgets.");
                XmlSerializer s = new XmlSerializer(typeof(ObservableCollection<vMixControl>));
                var collection = (ObservableCollection<vMixControl>)s.Deserialize(reader);
                foreach (var item in collection)
                {
                    _controls.Add(item);
                    if (functions != null && item is vMixControlButton)
                    {
                        var btn = item as vMixControlButton;
                        foreach (var command in btn.Commands)
                        {
                            var newFunction = functions.Where(x => x.Function == command.Action.Function || (x.Aliases?.Split(',').Contains(command.Action.Function) ?? false)).FirstOrDefault();
                            if (newFunction != null)
                                command.Action = newFunction;
                        }
                    }
                }
                reader.ReadEndElement();
                reader.ReadStartElement();

                _logger.Info("Loading window settings.");
                s = new XmlSerializer(typeof(MainWindowSettings));

                var settings = (MainWindowSettings)s.Deserialize(reader);

                if (settings.Pages.Count != 7)
                {
                    settings.Pages.Clear();
                    settings.Pages.Add(MAINPAGENAME);
                    settings.Pages.Add(DATAPAGENAME);
                    settings.Pages.Add(PAGE1PAGENAME);
                    settings.Pages.Add(PAGE2PAGENAME);
                    settings.Pages.Add(PAGE3PAGENAME);
                    settings.Pages.Add(PAGE4PAGENAME);
                    settings.Pages.Add(PAGE5PAGENAME);
                }

                windowSettings = settings;

                reader.ReadEndElement();

                if (reader.IsStartElement())
                {
                    reader.ReadStartElement();

                    s = new XmlSerializer(typeof(ObservableCollection<Pair<string, string>>));
                    var globals = (ObservableCollection<Pair<string, string>>)s.Deserialize(reader);
                    //Add or update global variable, according to controller variables
                    foreach (var item in globals)
                    {
                        if (GlobalVariablesViewModel._variables.Count(x => x.A == item.A) == 0)
                            GlobalVariablesViewModel._variables.Add(item);
                        else
                            GlobalVariablesViewModel._variables.Where(x => x.A == item.A).First().B = item.B;
                    }
                    reader.ReadEndElement();
                }

                reader.ReadEndElement();

                _logger.Info("Configuring API.");
            }
            return _controls;
        }

        public static void SaveController(string fileName, ObservableCollection<vMixControl> _controls, MainWindowSettings _windowSettings)
        {
            using (var stream = new FileStream(fileName, FileMode.Create))
            {
                SaveController(stream, _controls, _windowSettings);
            }
        }

        public static void SaveController(Stream stream, ObservableCollection<vMixControl> _controls, MainWindowSettings _windowSettings)
        {
            _logger.Info("Saving controller.");
            var writer = XmlWriter.Create(stream);
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Root");
                writer.WriteStartElement("Controls");
                XmlSerializer s = new XmlSerializer(typeof(ObservableCollection<vMixControl>));
                _logger.Info("Writing widgets.");
                s.Serialize(writer, _controls);
                writer.WriteEndElement();
                writer.WriteStartElement("WindowSettings");
                s = new XmlSerializer(typeof(MainWindowSettings));
                _logger.Info("Writing window settings.");
                s.Serialize(writer, _windowSettings);
                writer.WriteEndElement();

                writer.WriteStartElement("GlobalVariables");
                s = new XmlSerializer(typeof(ObservableCollection<Pair<string, string>>));
                _logger.Info("Writing global variables.");
                s.Serialize(writer, GlobalVariablesViewModel._variables);
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
            }
        }

        public static string FindInputKeyByVariable(string varName, Dispatcher d = null)
        {
            if (varName != null)
                return (d ?? Dispatcher.CurrentDispatcher).Invoke(() =>
                {
                    var variable = GlobalVariablesViewModel._variables.Where(x => x.A == varName).FirstOrDefault();
                    var inputKey = varName;
                    if (variable != null)
                        inputKey = variable.B;
                    return inputKey;
                });
            return varName;
            
        }

        public static string SearchFile(string path, string cpath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path)) return "";

                var directories = Path.GetDirectoryName(path).Split(Path.DirectorySeparatorChar).Reverse().ToArray();
                var filename = Path.GetFileName(path);
                string dir = cpath;
                int i = 0;
                while (!File.Exists(Path.Combine(dir, filename)) && i < directories.Length)
                {
                    dir = Path.Combine(dir, directories[i]);
                    i++;
                }
                return Path.Combine(dir, filename);
            }
            catch (ArgumentException)
            {
                return "";
            }
        }

        public static T FindPropertyControl<T>(this UserControl[] controls, string key) where T : UserControl
        {
            var ctrl = controls.Where(x => {
                if (x.Tag is string k) return k == key;
                return false;
            }).FirstOrDefault();
            if (ctrl != null && ctrl is T)
                return (T)ctrl;
            return null;
        }
    }

    /// <summary>
    /// Static methods for transforming argb spaces and argb values.
    /// </summary>
    public static class SimpleColorTransforms
    {
        private static double tolerance
            => 0.000000000000001;


        /// <summary>
        /// Defines brightness levels.
        /// </summary>
        public enum Brightness
                : byte
        {
            Bright = 255,
            MediumBright = 210,
            Medium = 142,
            Dim = 98,
            XDim = 50
        }


        /// <summary>
        /// Defines alpha levels.
        /// </summary>
        public enum Alpha
                : byte
        {
            Opaque = 255,
            MediumHigh = 230,
            Medium = 175,
            MediumLow = 142,
            Low = 109,
            XLow = 45
        }


        /// <summary>
        /// Defines hint alpha levels.
        /// </summary>
        public enum HintAlpha
                : byte
        {
            Low = 64,
            XLow = 48,
            XxLow = 32,
            XxxLow = 16
        }


        /// <summary>
        /// Specifies a mode for argb transformations.
        /// </summary>
        public enum ColorTransformMode
                : byte
        {
            Hsl,
            Hsb
        }


        /// <summary>
        /// Converts RGB to HSL. Alpha is ignored.
        /// Output is: { H: [0, 360], S: [0, 1], L: [0, 1] }.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        public static double[] RgBtoHsl(Color color)
        {
            double h = 0D;
            double s = 0D;
            double l;

            // normalize red, green, blue values
            double r = color.R / 255D;
            double g = color.G / 255D;
            double b = color.B / 255D;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            // hue
            if (Math.Abs(max - min) < SimpleColorTransforms.tolerance)
                h = 0D; // undefined
            else if ((Math.Abs(max - r) < SimpleColorTransforms.tolerance)
                    && (g >= b))
                h = (60D * (g - b)) / (max - min);
            else if ((Math.Abs(max - r) < SimpleColorTransforms.tolerance)
                    && (g < b))
                h = ((60D * (g - b)) / (max - min)) + 360D;
            else if (Math.Abs(max - g) < SimpleColorTransforms.tolerance)
                h = ((60D * (b - r)) / (max - min)) + 120D;
            else if (Math.Abs(max - b) < SimpleColorTransforms.tolerance)
                h = ((60D * (r - g)) / (max - min)) + 240D;

            // luminance
            l = (max + min) / 2D;

            // saturation
            if ((Math.Abs(l) < SimpleColorTransforms.tolerance)
                    || (Math.Abs(max - min) < SimpleColorTransforms.tolerance))
                s = 0D;
            else if ((0D < l)
                    && (l <= .5D))
                s = (max - min) / (max + min);
            else if (l > .5D)
                s = (max - min) / (2D - (max + min)); //(max-min > 0)?

            return new[]
            {
                Math.Max(0D, Math.Min(360D, double.Parse($"{h:0.##}"))),
                Math.Max(0D, Math.Min(1D, double.Parse($"{s:0.##}"))),
                Math.Max(0D, Math.Min(1D, double.Parse($"{l:0.##}")))
            };
        }


        /// <summary>
        /// Converts HSL to RGB, with a specified output Alpha.
        /// Arguments are limited to the defined range:
        /// does not raise exceptions.
        /// </summary>
        /// <param name="h">Hue, must be in [0, 360].</param>
        /// <param name="s">Saturation, must be in [0, 1].</param>
        /// <param name="l">Luminance, must be in [0, 1].</param>
        /// <param name="a">Output Alpha, must be in [0, 255].</param>
        public static Color HsLtoRgb(double h, double s, double l, int a = 255)
        {
            h = Math.Max(0D, Math.Min(360D, h));
            s = Math.Max(0D, Math.Min(1D, s));
            l = Math.Max(0D, Math.Min(1D, l));
            a = Math.Max(0, Math.Min(255, a));

            // achromatic argb (gray scale)
            if (Math.Abs(s) < SimpleColorTransforms.tolerance)
            {
                return Color.FromArgb(
                        (byte)a,
                        (byte)Math.Max(0, Math.Min(255, Convert.ToInt32(double.Parse($"{l * 255D:0.00}")))),
                        (byte)Math.Max(0, Math.Min(255, Convert.ToInt32(double.Parse($"{l * 255D:0.00}")))),
                        (byte)Math.Max(0, Math.Min(255, Convert.ToInt32(double.Parse($"{l * 255D:0.00}")))));
            }

            double q = l < .5D
                    ? l * (1D + s)
                    : (l + s) - (l * s);
            double p = (2D * l) - q;

            double hk = h / 360D;
            double[] T = new double[3];
            T[0] = hk + (1D / 3D); // Tr
            T[1] = hk; // Tb
            T[2] = hk - (1D / 3D); // Tg

            for (int i = 0; i < 3; i++)
            {
                if (T[i] < 0D)
                    T[i] += 1D;
                if (T[i] > 1D)
                    T[i] -= 1D;

                if ((T[i] * 6D) < 1D)
                    T[i] = p + ((q - p) * 6D * T[i]);
                else if ((T[i] * 2D) < 1)
                    T[i] = q;
                else if ((T[i] * 3D) < 2)
                    T[i] = p + ((q - p) * ((2D / 3D) - T[i]) * 6D);
                else
                    T[i] = p;
            }

            return Color.FromArgb(
                    (byte)a,
                    (byte)Math.Max(0, Math.Min(255, Convert.ToInt32(double.Parse($"{T[0] * 255D:0.00}")))),
                    (byte)Math.Max(0, Math.Min(255, Convert.ToInt32(double.Parse($"{T[1] * 255D:0.00}")))),
                    (byte)Math.Max(0, Math.Min(255, Convert.ToInt32(double.Parse($"{T[2] * 255D:0.00}")))));
        }


        /// <summary>
        /// Converts RGB to HSB. Alpha is ignored.
        /// Output is: { H: [0, 360], S: [0, 1], B: [0, 1] }.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        public static double[] RgBtoHsb(Color color)
        {
            // normalize red, green and blue values
            double r = color.R / 255D;
            double g = color.G / 255D;
            double b = color.B / 255D;

            // conversion start
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            double h = 0D;
            if ((Math.Abs(max - r) < SimpleColorTransforms.tolerance)
                    && (g >= b))
                h = (60D * (g - b)) / (max - min);
            else if ((Math.Abs(max - r) < SimpleColorTransforms.tolerance)
                    && (g < b))
                h = ((60D * (g - b)) / (max - min)) + 360D;
            else if (Math.Abs(max - g) < SimpleColorTransforms.tolerance)
                h = ((60D * (b - r)) / (max - min)) + 120D;
            else if (Math.Abs(max - b) < SimpleColorTransforms.tolerance)
                h = ((60D * (r - g)) / (max - min)) + 240D;

            double s = Math.Abs(max) < SimpleColorTransforms.tolerance
                    ? 0D
                    : 1D - (min / max);

            return new[]
            {
                Math.Max(0D, Math.Min(360D, h)),
                Math.Max(0D, Math.Min(1D, s)),
                Math.Max(0D, Math.Min(1D, max))
            };
        }


        /// <summary>
        /// Converts HSB to RGB, with a specified output Alpha.
        /// Arguments are limited to the defined range:
        /// does not raise exceptions.
        /// </summary>
        /// <param name="h">Hue, must be in [0, 360].</param>
        /// <param name="s">Saturation, must be in [0, 1].</param>
        /// <param name="b">Brightness, must be in [0, 1].</param>
        /// <param name="a">Output Alpha, must be in [0, 255].</param>
        public static Color HsBtoRgb(double h, double s, double b, int a = 255)
        {
            h = Math.Max(0D, Math.Min(360D, h));
            s = Math.Max(0D, Math.Min(1D, s));
            b = Math.Max(0D, Math.Min(1D, b));
            a = Math.Max(0, Math.Min(255, a));

            double r = 0D;
            double g = 0D;
            double bl = 0D;

            if (Math.Abs(s) < SimpleColorTransforms.tolerance)
                r = g = bl = b;
            else
            {
                // the argb wheel consists of 6 sectors. Figure out which sector
                // you're in.
                double sectorPos = h / 60D;
                int sectorNumber = (int)Math.Floor(sectorPos);
                // get the fractional part of the sector
                double fractionalSector = sectorPos - sectorNumber;

                // calculate values for the three axes of the argb.
                double p = b * (1D - s);
                double q = b * (1D - (s * fractionalSector));
                double t = b * (1D - (s * (1D - fractionalSector)));

                // assign the fractional colors to r, g, and b based on the sector
                // the angle is in.
                switch (sectorNumber)
                {
                    case 0:
                        r = b;
                        g = t;
                        bl = p;
                        break;
                    case 1:
                        r = q;
                        g = b;
                        bl = p;
                        break;
                    case 2:
                        r = p;
                        g = b;
                        bl = t;
                        break;
                    case 3:
                        r = p;
                        g = q;
                        bl = b;
                        break;
                    case 4:
                        r = t;
                        g = p;
                        bl = b;
                        break;
                    case 5:
                        r = b;
                        g = p;
                        bl = q;
                        break;
                }
            }

            return Color.FromArgb(
                    (byte)a,
                    (byte)Math.Max(0, Math.Min(255, Convert.ToInt32(double.Parse($"{r * 255D:0.00}")))),
                    (byte)Math.Max(0, Math.Min(255, Convert.ToInt32(double.Parse($"{g * 255D:0.00}")))),
                    (byte)Math.Max(0, Math.Min(255, Convert.ToInt32(double.Parse($"{bl * 250D:0.00}")))));
        }


        /// <summary>
        /// Multiplies the Color's Luminance or Brightness by the argument;
        /// and optionally specifies the output Alpha.
        /// </summary>
        /// <param name="color">The color to transform.</param>
        /// <param name="colorTransformMode">Transform mode.</param>
        /// <param name="brightnessTransform">The transformation multiplier.</param>
        /// <param name="outputAlpha">Can optionally specify the Alpha to directly
        /// set on the output. If null, then the input <paramref name="color"/>
        /// Alpha is used.</param>
        public static Color TransformBrightness(
                Color color,
                ColorTransformMode colorTransformMode,
                double brightnessTransform,
                byte? outputAlpha = null)
        {
            double[] hsl = colorTransformMode == ColorTransformMode.Hsl
                    ? SimpleColorTransforms.RgBtoHsl(color)
                    : SimpleColorTransforms.RgBtoHsb(color);
            if ((Math.Abs(hsl[2]) < SimpleColorTransforms.tolerance)
                    && (brightnessTransform > 1D))
                hsl[2] = brightnessTransform - 1D;
            else
                hsl[2] *= brightnessTransform;
            return colorTransformMode == ColorTransformMode.Hsl
                    ? SimpleColorTransforms.HsLtoRgb(hsl[0], hsl[1], hsl[2], outputAlpha ?? color.A)
                    : SimpleColorTransforms.HsBtoRgb(hsl[0], hsl[1], hsl[2], outputAlpha ?? color.A);
        }


        /// <summary>
        /// Multiplies the Color's Saturation, and Luminance or Brightness by the argument;
        /// and optionally specifies the output Alpha.
        /// </summary>
        /// <param name="color">The color to transform.</param>
        /// <param name="colorTransformMode">Transform mode.</param>
        /// <param name="saturationTransform">The transformation multiplier.</param>
        /// <param name="brightnessTransform">The transformation multiplier.</param>
        /// <param name="outputAlpha">Can optionally specify the Alpha to directly
        /// set on the output. If null, then the input <paramref name="color"/>
        /// Alpha is used.</param>
        public static Color TransformSaturationAndBrightness(
                Color color,
                ColorTransformMode colorTransformMode,
                double saturationTransform,
                double brightnessTransform,
                byte? outputAlpha = null)
        {
            double[] hsl = colorTransformMode == ColorTransformMode.Hsl
                    ? SimpleColorTransforms.RgBtoHsl(color)
                    : SimpleColorTransforms.RgBtoHsb(color);
            if ((Math.Abs(hsl[1]) < SimpleColorTransforms.tolerance)
                    && (saturationTransform > 1D))
                hsl[1] = saturationTransform - 1D;
            else
                hsl[1] *= saturationTransform;
            if ((Math.Abs(hsl[2]) < SimpleColorTransforms.tolerance)
                    && (brightnessTransform > 1D))
                hsl[2] = brightnessTransform - 1D;
            else
                hsl[2] *= brightnessTransform;
            return colorTransformMode == ColorTransformMode.Hsl
                    ? SimpleColorTransforms.HsLtoRgb(hsl[0], hsl[1], hsl[2], outputAlpha ?? color.A)
                    : SimpleColorTransforms.HsBtoRgb(hsl[0], hsl[1], hsl[2], outputAlpha ?? color.A);
        }


        /// <summary>
        /// Creates a new Color by combining R, G, and B from each Color, scaled by the Color's Alpha.
        /// The R, G, B of each Color is scaled by the Color's Alpha. The R, G, B of both results is
        /// then added together and divided by 2. The valuea are limited to [0, 255].
        /// The Alpha of the output Color is specified; and is also limited to [0, 255]
        /// (does not raise exceptions).
        /// </summary>
        /// <param name="color1">Combined by scaling RGB by the A.</param>
        /// <param name="color2">Combined by scaling RGB by the A.</param>
        /// <param name="outputAlpha">The Alpha of the output Color.</param>
        public static Color AlphaCombine(Color color1, Color color2, byte outputAlpha)
        {
            double a1 = color1.A / 255D;
            double a2 = color2.A / 255D;
            return Color.FromArgb(
                    outputAlpha,
                    (byte)Math.Max(0D, Math.Min(255D, ((color1.R * a1) + (color2.R * a2)) * .5D)),
                    (byte)Math.Max(0D, Math.Min(255D, ((color1.G * a1) + (color2.G * a2)) * .5D)),
                    (byte)Math.Max(0D, Math.Min(255D, ((color1.B * a1) + (color2.B * a2)) * .5D)));
        }
    }
}
