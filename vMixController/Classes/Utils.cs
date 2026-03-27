using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using vMixController.Classes.Scripting;
using vMixController.ViewModel;
using vMixController.Widgets;

namespace vMixController.Classes
{
    public class XorCryptoServiceProvider : ICryptoTransform
    {
        private readonly byte[] _key;
        private int _keyIndex;

        public XorCryptoServiceProvider(byte[] key)
        {
            if (key == null || key.Length == 0)
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }

            _key = (byte[])key.Clone();
            _keyIndex = 0;
        }

        public bool CanReuseTransform => true;
        public bool CanTransformMultipleBlocks => true;
        public int InputBlockSize => 1;
        public int OutputBlockSize => 1;

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            for (int i = 0; i < inputCount; i++)
            {
                outputBuffer[outputOffset + i] = (byte)(inputBuffer[inputOffset + i] ^ _key[_keyIndex]);
                _keyIndex = (_keyIndex + 1) % _key.Length;
            }

            return inputCount;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            byte[] outputBuffer = new byte[inputCount];
            TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);
            return outputBuffer;
        }

        public void Dispose()
        {
            // Clear the key from memory
            Array.Clear(_key, 0, _key.Length);
        }
    }

    public enum Status
    {
        Offline,
        Sync,
        Online,
        InputsChanged
    }

    public static class Utils
    {
        private static readonly ConcurrentDictionary<Type, XmlSerializer> XmlSerializerCache =
            new ConcurrentDictionary<Type, XmlSerializer>();

        public static XmlSerializer GetXmlSerializer(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return XmlSerializerCache.GetOrAdd(type, t => new XmlSerializer(t));
        }

        public static XmlSerializer GetXmlSerializer<T>()
        {
            return GetXmlSerializer(typeof(T));
        }

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

        public static bool GetBit(this short byt, byte index)
        {
            if (index < 0 || index > sizeof(short) * 8 - 1)
                throw new ArgumentOutOfRangeException();

            return (byt & (1 << index)) >> index != 0;
        }

        public static byte SetBit(this byte byt, byte index, bool value)
        {
            if (index < 0 || index > 7)
                throw new ArgumentOutOfRangeException();
            return (byte)((byt & ~(1 << index)) + (value ? 1 << index : 0));
        }

        public static short SetBit(this short byt, byte index, bool value)
        {
            if (index < 0 || index > sizeof(short) * 8 - 1)
                throw new ArgumentOutOfRangeException();
            return (short)((byt & ~(1 << index)) + (value ? 1 << index : 0));
        }

        static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static string PortableControllerPath { get; set; }

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
                btn.Commands.Add(new vMixControlButtonCommand()
                {
                    Action = new vMixFunctionReference() { Function = NativeFunctions.WIN, Native = true },
                    StringParameter = "https://forums.vmix.com/postmessage?t=6468&f=8"
                });
                return new ObservableCollection<vMixControl>() {
                    new vMixControlRegion() { Text = string.Format("{0}\n\n\nP.S. Don't be afraid, your controller is OK.\nReport about it on forum.", string.Join("", SecurityElement.Escape(e.ToString()).Select(x=>(XmlConvert.IsXmlChar(x)?x.ToString():"0x" + Convert.ToByte(x).ToString())).ToArray())), Width = 512, Height = 512, Top = 8, Left = 8, Color = Colors.Red, Name = "Error while loading controller!" },
                    btn
                };
            }

        }

        public static ObservableCollection<vMixControl> LoadController(Stream stream, IList<vMixFunctionReference> functions, out MainWindowSettings windowSettings)
        {

            byte[] signature = new byte[3];
            stream.Read(signature, 0, 3);
            Stream memstream = null;

            if (signature[0] == 0x44 && signature[1] == 0x77 && signature[2] == 0xCC)
            {
                memstream = new MemoryStream();
                byte[] buffer = new byte[8];
                using (var cs = new CryptoStream(stream, new XorCryptoServiceProvider(Constants.XORKEY), CryptoStreamMode.Read))
                {
                    int l = -1;
                    while ((l = cs.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        memstream.Write(buffer, 0, l);
                    }
                    memstream.Seek(0, SeekOrigin.Begin);
                }
            }
            else
            {
                stream.Seek(0, SeekOrigin.Begin);
                memstream = stream;
            }

            var _controls = new ObservableCollection<vMixControl>();
            _logger.Info("Controller loading.");
            var reader = XmlReader.Create(memstream);
            {
                reader.ReadStartElement();
                reader.ReadStartElement();
                _logger.Info("Loading widgets.");
                XmlSerializer s = GetXmlSerializer<ObservableCollection<vMixControl>>();
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
                s = GetXmlSerializer<MainWindowSettings>();

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

                    s = GetXmlSerializer<ObservableCollection<Pair<string, string>>>();
                    var globals = (ObservableCollection<Pair<string, string>>)s.Deserialize(reader);
                    //Add or update global variable, according to controller variables
                    foreach (var item in globals)
                    {
                        var globalSettings = AppServices.IsRegistered<GlobalVariablesViewModel>()
                            ? AppServices.GetRequiredService<GlobalVariablesViewModel>()
                            : null;
                        if (globalSettings == null)
                        {
                            continue;
                        }
                        if (globalSettings.Variables.Count(x => x.A == item.A) == 0)
                            globalSettings.Variables.Add(item);
                        else
                            globalSettings.Variables.Where(x => x.A == item.A).First().B = item.B;
                    }
                    reader.ReadEndElement();
                }

                reader.ReadEndElement();

                _logger.Info("Configuring API.");
            }

            //ReBind variables to viewers
            foreach (var item in _controls.OfType<vMixControlVariableViewer>())
            {
                string temp = item.Variable;
                item.Variable = null;
                item.Variable = temp;
            }

            //renumber z-index for old controllers
            var zeroIndexNonRegion = _controls.Where(x => !(x is vMixControlRegion) && x.ZIndex == 0).Count();
            var indexRegion = _controls.Where(x => (x is vMixControlRegion) && x.ZIndex == -1).Count();
            if (zeroIndexNonRegion + indexRegion == _controls.Count)
            {
                var index = 0;
                var mindex = -indexRegion;
                foreach (var item in _controls)
                    if (item is vMixControlRegion)
                        item.ZIndex = mindex++;
                    else
                        item.ZIndex = index++;
            }

            return _controls;
        }

        public static void SaveController(string fileName, ObservableCollection<vMixControl> _controls, MainWindowSettings _windowSettings)
        {
            try
            {
                using (var stream = new FileStream(fileName, FileMode.Create))
                {
                    SaveController(stream, _controls, _windowSettings);
                }
            }
            catch (Exception)
            {

            }
        }

        public static void SaveController(Stream stream, ObservableCollection<vMixControl> _controls, MainWindowSettings _windowSettings)
        {
            _logger.Info("Saving controller.");

            Stream memstream = new MemoryStream();
            Stream ms = null;
            if (!string.IsNullOrWhiteSpace(_windowSettings.Password))
                ms = new CryptoStream(memstream, new XorCryptoServiceProvider(Constants.XORKEY), CryptoStreamMode.Write);
            else
                ms = memstream;

            using (ms)
            {
                var writer = XmlWriter.Create(ms, new XmlWriterSettings() { Encoding = new UTF8Encoding(false) });
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Root");
                    writer.WriteStartElement("Controls");
                    XmlSerializer s = GetXmlSerializer<ObservableCollection<vMixControl>>();
                    _logger.Info("Writing widgets.");
                    s.Serialize(writer, _controls);
                    writer.WriteEndElement();
                    writer.WriteStartElement("WindowSettings");
                    s = GetXmlSerializer<MainWindowSettings>();
                    _logger.Info("Writing window settings.");
                    s.Serialize(writer, _windowSettings);
                    writer.WriteEndElement();


                    var globalSettings = AppServices.IsRegistered<GlobalVariablesViewModel>()
                        ? AppServices.GetRequiredService<GlobalVariablesViewModel>()
                        : null;
                    writer.WriteStartElement("GlobalVariables");
                    s = GetXmlSerializer<ObservableCollection<Pair<string, string>>>();
                    _logger.Info("Writing global variables.");
                    s.Serialize(writer, globalSettings?.Variables ?? new ObservableCollection<Pair<string, string>>());
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                }
                if (!string.IsNullOrWhiteSpace(_windowSettings.Password))
                {
                    stream.WriteByte(0x44);
                    stream.WriteByte(0x77);
                    stream.WriteByte(0xCC);
                }

                memstream.Seek(0, SeekOrigin.Begin);
                memstream.CopyTo(stream);
            }



        }

        public static string FindInputKeyByVariable(string varName, Dispatcher d = null)
        {
            if (varName != null)
                return (d ?? Dispatcher.CurrentDispatcher).Invoke(() =>
                {
                    // Virtual inputs (legacy): "-1" => [Active], "0" => [Preview]
                    // Resolve them to the currently active/preview input key.
                    var widgetSettings = AppServices.IsRegistered<vMixWidgetSettingsViewModel>()
                        ? AppServices.GetRequiredService<vMixWidgetSettingsViewModel>()
                        : null;
                    var model = widgetSettings?.Model;
                    if (model != null)
                    {
                        if (varName == "-1")
                        {
                            var activeInput = model.Inputs.FirstOrDefault(x => x.Number == model.Active);
                            if (activeInput != null)
                                return activeInput.Key;
                            return model.Active.ToString(CultureInfo.InvariantCulture);
                        }

                        if (varName == "0")
                        {
                            var previewInput = model.Inputs.FirstOrDefault(x => x.Number == model.Preview);
                            if (previewInput != null)
                                return previewInput.Key;
                            return model.Preview.ToString(CultureInfo.InvariantCulture);
                        }
                    }

                    var globalSettings = AppServices.IsRegistered<GlobalVariablesViewModel>()
                        ? AppServices.GetRequiredService<GlobalVariablesViewModel>()
                        : null;
                    if (globalSettings == null)
                    {
                        return varName;
                    }
                    var variable = globalSettings.Variables.Where(x => x.A == varName).FirstOrDefault();
                    var inputKey = varName;
                    if (variable != null)
                        inputKey = variable.B;
                    return inputKey;
                });
            return varName;

        }

        public static string SearchFile(string path, string cpath)
        {
            return ResolvePortablePath(path, cpath);
        }

        private static IEnumerable<string> EnumerateTrailingFolders(string originalPath, int maxDepth)
        {
            var directory = Path.GetDirectoryName(originalPath);
            if (string.IsNullOrWhiteSpace(directory))
                yield break;

            var parts = directory
                .Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x) && !x.EndsWith(":", StringComparison.Ordinal))
                .ToArray();

            var depthLimit = Math.Min(maxDepth, parts.Length);
            for (var depth = 1; depth <= depthLimit; depth++)
            {
                var tail = parts.Skip(parts.Length - depth).ToArray();
                yield return Path.Combine(tail);
            }
        }

        private static string ResolveInBaseDirectory(string originalPath, string baseDirectory, int maxDepth)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
                return null;

            if (!Directory.Exists(baseDirectory))
                return null;

            var fileName = Path.GetFileName(originalPath);
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            var inBase = Path.Combine(baseDirectory, fileName);
            if (File.Exists(inBase))
                return Path.GetFullPath(inBase);

            if (!Path.IsPathRooted(originalPath))
            {
                var directRelative = Path.Combine(baseDirectory, originalPath);
                if (File.Exists(directRelative))
                    return Path.GetFullPath(directRelative);
            }

            foreach (var tailDirectory in EnumerateTrailingFolders(originalPath, maxDepth))
            {
                var candidate = Path.Combine(baseDirectory, tailDirectory, fileName);
                if (File.Exists(candidate))
                    return Path.GetFullPath(candidate);
            }

            return null;
        }

        public static string ResolvePortablePath(string originalPath, string controllerPath = null, int maxDepth = 3)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(originalPath))
                    return string.Empty;

                var normalized = originalPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                if (File.Exists(normalized))
                    return Path.GetFullPath(normalized);

                var executableDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var resolvedFromExe = ResolveInBaseDirectory(normalized, executableDirectory, maxDepth);
                if (!string.IsNullOrWhiteSpace(resolvedFromExe))
                    return resolvedFromExe;

                var controllerFilePath = Utils.PortableControllerPath;

                string controllerDirectory = null;
                if (!string.IsNullOrWhiteSpace(controllerFilePath))
                {
                    if (Directory.Exists(controllerFilePath))
                        controllerDirectory = controllerFilePath;
                    else
                        controllerDirectory = Path.GetDirectoryName(controllerFilePath);
                }
                var resolvedFromController = ResolveInBaseDirectory(normalized, controllerDirectory, maxDepth);
                if (!string.IsNullOrWhiteSpace(resolvedFromController))
                    return resolvedFromController;

                return normalized;
            }
            catch (Exception)
            {
                return originalPath ?? string.Empty;
            }
        }

        public static T FindPropertyControl<T>(this UserControl[] controls, string key) where T : UserControl
        {
            var ctrl = controls.Where(x =>
            {
                if (x.Tag is string k) return k == key;
                return false;
            }).FirstOrDefault();
            if (ctrl != null && ctrl is T)
                return (T)ctrl;
            return null;
        }

        public static string GetNextCopyName(string originalName)
        {
            if (string.IsNullOrWhiteSpace(originalName))
                return "copy";

            // 1. Если в конце число → увеличиваем его
            var numberMatch = Regex.Match(originalName, @"^(.*?)(\d+)$");
            if (numberMatch.Success)
            {
                string prefix = numberMatch.Groups[1].Value;
                int number = int.Parse(numberMatch.Groups[2].Value);
                return $"{prefix}{number + 1}";
            }

            // 2. Если заканчивается на "copy" → добавляем "2"
            if (originalName.EndsWith("copy", StringComparison.OrdinalIgnoreCase))
            {
                return originalName + " 2";
            }

            // 3. Во всех остальных случаях → добавляем "copy"
            return originalName + " copy";
        }

        public static object NormalizeParameterValue(object value)
        {
            if (value is string str)
            {
                if (decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariant))
                    return invariant;

                if (decimal.TryParse(str, NumberStyles.Number, CultureInfo.CurrentCulture, out var current))
                    return current;
            }

            return value;
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
