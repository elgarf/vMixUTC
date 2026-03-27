using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace vMixUTCStreamDeck
{
    internal class Program
    {
        private static string m_Path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\vMix\\streamdeckapiutc.txt";
        private static vMixStreamDeckLibrary.StreamDeckAPI device = null;
        static void Main(string[] args)
        {
            //while (!Debugger.IsAttached) { Thread.Sleep(100); }
            
            try
            {
                string text = "";
                string text2 = "";
                string text3 = "";
                int num = 0;
                int num2 = args.Length - 1;
                for (int i = num; i <= num2; i++)
                {
                    string text4 = args[i];
                    string left = text4;
                    if (left.ToLowerInvariant() == "-registerevent")
                    {
                        text3 = args[i + 1];
                    }
                    else if (left.ToLowerInvariant() == "-port")
                    {
                        text = args[i + 1];
                    }
                    else if (left.ToLowerInvariant() == "-pluginuuid")
                    {
                        text2 = args[i + 1];
                    }
                }
                if (!string.IsNullOrEmpty(text) & !string.IsNullOrEmpty(text2) & !string.IsNullOrEmpty(text3))
                {
                    
                    File.WriteAllText(m_Path, string.Concat(new string[]
                    {
                    text,
                    ",",
                    text2,
                    ",",
                    text3,
                    ",",
                    Process.GetCurrentProcess().Id.ToString()
                    }));
                }
                else
                {
                    Console.WriteLine("One or more properties are invalid.");
                }

                //Thread.Sleep(5000);
                Task.Run(() =>
                {
                    device = new vMixStreamDeckLibrary.StreamDeckAPI();
                    device.EventReceived += Device_EventReceived;
                });
                

                Process[] processesByName = Process.GetProcessesByName("StreamDeck");
                if (processesByName != null && processesByName.Length > 0)
                {
                    //MessageBox.Show(string.Format("{0}, {1}, {2}", text, text2, text3));
                    processesByName[0].WaitForExit();
                }
                else
                {
                    //MessageBox.Show(string.Format("RL{0}, {1}, {2}", text, text2, text3));
                    Console.ReadLine();
                }
                Console.WriteLine("Exiting...");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void Device_EventReceived(object sender, vMixStreamDeckLibrary.StreamDeckEventArgs e)
        {
            using (var s = new MemoryMessagePipe.MemoryMappedFileMessageSender("vMixUTCMMF"))
            {
                s.SendMessage(stream =>
                {
                    var buffer = Encoding.UTF8.GetBytes(e.Button.Context);
                    stream.WriteByte((byte)e.Type);

                    stream.Write(buffer, 0, buffer.Length);
                });
            }
        }
    }
}
