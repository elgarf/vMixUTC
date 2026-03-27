using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Xml;
using Microsoft.VisualBasic.CompilerServices;

namespace vMixStreamDeckLibrary
{
    // Token: 0x0200000B RID: 11
    public class StreamDeckAPI : IDisposable
    {
        // Token: 0x1700000A RID: 10
        // (get) Token: 0x0600001A RID: 26 RVA: 0x00002480 File Offset: 0x00000680
        // (set) Token: 0x0600001B RID: 27 RVA: 0x00002494 File Offset: 0x00000694
        public virtual WebSocket m_Socket
        {
            get
            {
                return this._m_Socket;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                WebSocket.TextReceivedEventHandler obj = new WebSocket.TextReceivedEventHandler(this.m_Socket_TextReceived);
                WebSocket.SocketReadyEventHandler obj2 = new WebSocket.SocketReadyEventHandler(this.m_Socket_SocketReady);
                WebSocket.SocketClosedEventHandler obj3 = new WebSocket.SocketClosedEventHandler(this.m_Socket_SocketClosed);
                if (this._m_Socket != null)
                {
                    this._m_Socket.TextReceived -= obj;
                    this._m_Socket.SocketReady -= obj2;
                    this._m_Socket.SocketClosed -= obj3;
                }
                this._m_Socket = value;
                if (this._m_Socket != null)
                {
                    this._m_Socket.TextReceived += obj;
                    this._m_Socket.SocketReady += obj2;
                    this._m_Socket.SocketClosed += obj3;
                }
            }
        }

        private void m_Socket_TextReceived(object sender, WebSocketEventArgs e)
        {
            try
            {
                string data = e.Message;
                if (!string.IsNullOrEmpty(data))
                {
                    this.WriteInfo("Message: " + data);
                    if (data.Contains("{"))
                    {
                        XmlDocument xmlDocument = JSON.ConvertToXML(data);
                        XmlNode xmlNode = xmlDocument.SelectSingleNode("//event");
                        if (xmlNode != null)
                        {
                            string innerText = xmlNode.InnerText;
                            if (Operators.CompareString(innerText, "willAppear", false) == 0)
                            {
                                XmlNode xmlNode2 = xmlDocument.SelectSingleNode("//device");
                                if (xmlNode2 != null)
                                {
                                    XmlNode xmlNode3 = xmlDocument.SelectSingleNode("//context");
                                    if (xmlNode3 != null)
                                    {
                                        XmlNode xmlNode4 = xmlDocument.SelectSingleNode("//row");
                                        if (xmlNode4 != null)
                                        {
                                            XmlNode xmlNode5 = xmlDocument.SelectSingleNode("//column");
                                            if (xmlNode5 != null)
                                            {
                                                int rw = 0;
                                                int col = 0;
                                                if (int.TryParse(xmlNode4.InnerText, out rw) && int.TryParse(xmlNode5.InnerText, out col))
                                                {
                                                    StreamDeckAPI.RegisterSession session = this.m_Session;
                                                    if (session != null)
                                                    {
                                                        StreamDeckAPI.RegisterContext registerContext = session.GetContext(xmlNode3.InnerText);
                                                        if (registerContext != null)
                                                        {
                                                            registerContext.Button.Context = xmlNode3.InnerText;
                                                            registerContext.Button.Device = xmlNode2.InnerText;
                                                            this.WriteInfo("UpdatedContextAndDevice");
                                                        }
                                                        else
                                                        {
                                                            string controller = "Keypad";
                                                            XmlNode xmlNode6 = xmlDocument.SelectSingleNode("//controller");
                                                            if (xmlNode6 != null)
                                                            {
                                                                controller = xmlNode6.InnerText;
                                                            }
                                                            registerContext = new StreamDeckAPI.RegisterContext(xmlNode2.InnerText, xmlNode3.InnerText, rw, col, controller);
                                                            session.Contexts.Add(registerContext);
                                                            this.WriteInfo("AddedContext");
                                                        }
                                                        this.EventReceived?.Invoke(this, new StreamDeckEventArgs(StreamDeckEvent.ButtonRegistered, "", registerContext.Button, 0));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (Operators.CompareString(innerText, "keyDown", false) != 0)
                                {
                                    if (Operators.CompareString(innerText, "keyUp", false) != 0)
                                    {
                                        if (Operators.CompareString(innerText, "touchTap", false) != 0)
                                        {
                                            if (Operators.CompareString(innerText, "dialDown", false) != 0)
                                            {
                                                if (Operators.CompareString(innerText, "dialUp", false) != 0)
                                                {
                                                    if (Operators.CompareString(innerText, "dialRotate", false) != 0)
                                                    {
                                                        goto IL_40A;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }



                                this.WriteInfo("keyEvent");
                                XmlNode xmlNode7 = xmlDocument.SelectSingleNode("//context");
                                if (xmlNode7 != null)
                                {
                                    StreamDeckAPI.RegisterSession session2 = this.m_Session;
                                    if (session2 != null)
                                    {
                                        StreamDeckAPI.RegisterContext context = session2.GetContext(xmlNode7.InnerText);
                                        if (context != null)
                                        {
                                            string innerText2 = xmlNode.InnerText;
                                            if (Operators.CompareString(innerText2, "keyDown", false) == 0)
                                            {
                                                //StreamDeckAPI.EventReceivedEventHandler eventReceivedEvent = this.EventReceivedEvent;
                                                this.EventReceived?.Invoke(this, new StreamDeckEventArgs(StreamDeckEvent.KeyDown, "", context.Button, 127));
                                            }
                                            else if (Operators.CompareString(innerText2, "keyUp", false) == 0)
                                            {
                                                //StreamDeckAPI.EventReceivedEventHandler eventReceivedEvent = this.EventReceivedEvent;
                                                this.EventReceived?.Invoke(this, new StreamDeckEventArgs(StreamDeckEvent.KeyUp, "", context.Button, 0));
                                            }
                                            else if (Operators.CompareString(innerText2, "touchTap", false) == 0)
                                            {
                                                //StreamDeckAPI.EventReceivedEventHandler eventReceivedEvent = this.EventReceivedEvent;
                                                this.EventReceived?.Invoke(this, new StreamDeckEventArgs(StreamDeckEvent.TouchTap, "", context.Button, 127));
                                            }
                                            else if (Operators.CompareString(innerText2, "dialDown", false) == 0)
                                            {
                                                //StreamDeckAPI.EventReceivedEventHandler eventReceivedEvent = this.EventReceivedEvent;
                                                this.EventReceived?.Invoke(this, new StreamDeckEventArgs(StreamDeckEvent.DialDown, "", context.Button, 127));
                                            }
                                            else if (Operators.CompareString(innerText2, "dialUp", false) == 0)
                                            {
                                                //StreamDeckAPI.EventReceivedEventHandler eventReceivedEvent = this.EventReceivedEvent;
                                                this.EventReceived?.Invoke(this, new StreamDeckEventArgs(StreamDeckEvent.DialUp, "", context.Button, 0));
                                            }
                                            else if (Operators.CompareString(innerText2, "dialRotate", false) == 0)
                                            {
                                                XmlNode xmlNode8 = xmlDocument.SelectSingleNode("//ticks");
                                                if (xmlNode8 != null)
                                                {
                                                    int v = 0;
                                                    if (int.TryParse(xmlNode8.InnerText, out v))
                                                    {
                                                        //StreamDeckAPI.EventReceivedEventHandler eventReceivedEvent = this.EventReceivedEvent;
                                                        this.EventReceived?.Invoke(this, new StreamDeckEventArgs(StreamDeckEvent.DialRotate, "", context.Button, v));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            IL_40A:;
            }
            catch (Exception ex)
            {
                this.WriteError("TextReceived: " + ex.ToString());
                this._HandleFailure();
            }
        }


        // Token: 0x14000001 RID: 1
        // (add) Token: 0x0600001C RID: 28 RVA: 0x000020CB File Offset: 0x000002CB
        // (remove) Token: 0x0600001D RID: 29 RVA: 0x000020E4 File Offset: 0x000002E4
        public event StreamDeckAPI.EventReceivedEventHandler EventReceived;

        // Token: 0x0600001E RID: 30 RVA: 0x0000252C File Offset: 0x0000072C
        public StreamDeckAPI()
        {
            //this.m_Socket = new WatsonWebsocket.WatsonWsServer();
            this.m_Path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\vMix\\streamdeckapiutc.txt";
            this.m_Sync = RuntimeHelpers.GetObjectValue(new object());
            this.m_Task = new ThreadTask(new ThreadStart(this._Task));
            this.EventReceived += StreamDeckAPI_EventReceived;
        }

        private void StreamDeckAPI_EventReceived(object sender, StreamDeckEventArgs e)
        {
            
        }

        // Token: 0x0600001F RID: 31 RVA: 0x00002580 File Offset: 0x00000780
        public StreamDeckButton[] GetButtons()
        {
            List<StreamDeckButton> list = new List<StreamDeckButton>();
            StreamDeckAPI.RegisterSession session = this.m_Session;
            if (session != null)
            {

                foreach (StreamDeckAPI.RegisterContext registerContext in session.Contexts)
                {
                    if (registerContext.Button != null)
                    {
                        list.Add(registerContext.Button);
                    }
                }

            }
            return list.ToArray();
        }

        // Token: 0x06000020 RID: 32 RVA: 0x000025F8 File Offset: 0x000007F8
        public void SetImage(string context, string mimeType, byte[] data)
        {
            StreamDeckAPI.RegisterSession session = this.m_Session;
            if (session != null)
            {
                StreamDeckAPI.RegisterContext context2 = session.GetContext(context);
                if (context2 != null)
                {
                    string szText;
                    if (data != null)
                    {
                        string text = "data:" + mimeType + ";base64," + Convert.ToBase64String(data);
                        szText = string.Concat(new string[]
                        {
                            "{\"event\":\"setImage\",\"context\":\"",
                            context2.Button.Context,
                            "\",\"payload\":{\"image\":\"",
                            text,
                            "\",\"target\": 1}}"
                        });
                    }
                    else
                    {
                        szText = "{\"event\":\"setImage\",\"context\":\"" + context2.Button.Context + "\",\"payload\":{\"target\": 1}}";
                    }
                    this.SendTextFrame(szText);
                }
            }
        }

        // Token: 0x06000021 RID: 33 RVA: 0x000026A4 File Offset: 0x000008A4
        private void SendTextFrame(string szText)
        {
            object sync = this.m_Sync;
            ObjectFlowControl.CheckForSyncLockOnValueType(sync);
            lock (sync)
            {
                if (this.m_Socket != null)
                {
                    //m_Socket.SendAsync(this.m_APIClientMetadata.Guid, Encoding.UTF8.GetBytes(szText), System.Net.WebSockets.WebSocketMessageType.Text).RunSynchronously();
                    this.m_Socket.SendTextFrame(szText);
                }
            }
        }

        // Token: 0x06000022 RID: 34 RVA: 0x000026F4 File Offset: 0x000008F4
        public static string CreateImageData(string mimeType, byte[] data)
        {
            return "data:" + mimeType + ";base64," + Convert.ToBase64String(data);
        }

        // Token: 0x06000023 RID: 35 RVA: 0x00002718 File Offset: 0x00000918
        public void SetFeedback(string context, NameValueCollection items)
        {
            StreamDeckAPI.RegisterSession session = this.m_Session;
            checked
            {
                if (session != null)
                {
                    StreamDeckAPI.RegisterContext context2 = session.GetContext(context);
                    if (context2 != null)
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        int num = 0;
                        int num2 = items.Count - 1;
                        for (int i = num; i <= num2; i++)
                        {
                            string text = items.Keys[i];
                            string text2 = items[i];
                            stringBuilder.Append(string.Concat(new string[]
                            {
                                "\"",
                                text,
                                "\": \"",
                                text2,
                                "\""
                            }));
                            if (i != items.Count - 1)
                            {
                                stringBuilder.AppendLine(",");
                            }
                        }
                        string text3 = string.Concat(new string[]
                        {
                            "{\"event\":\"setFeedback\",\"context\":\"",
                            context2.Button.Context,
                            "\",\"payload\":{",
                            stringBuilder.ToString(),
                            "}}"
                        });
                        this.WriteInfo(text3);
                        this.SendTextFrame(text3);
                    }
                }
            }
        }

        // Token: 0x06000024 RID: 36 RVA: 0x00002824 File Offset: 0x00000A24
        public void SetFeedbackLayout(string context, string layout)
        {
            StreamDeckAPI.RegisterSession session = this.m_Session;
            if (session != null)
            {
                StreamDeckAPI.RegisterContext context2 = session.GetContext(context);
                if (context2 != null)
                {
                    string text = string.Concat(new string[]
                    {
                        "{\"event\":\"setFeedbackLayout\",\"context\":\"",
                        context2.Button.Context,
                        "\",\"payload\":{\"layout\": \"",
                        layout,
                        "\"}}"
                    });
                    this.WriteInfo(text);
                    this.SendTextFrame(text);
                }
            }
        }

        // Token: 0x06000025 RID: 37 RVA: 0x0000288C File Offset: 0x00000A8C
        private Process GetProcess(int id)
        {
            Process result;
            try
            {
                result = Process.GetProcessById(id);
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }

        // Token: 0x06000026 RID: 38 RVA: 0x000028C4 File Offset: 0x00000AC4
        private StreamDeckAPI.RegisterSession GetRegisterDetails()
        {
            if (File.Exists(this.m_Path))
            {
                string text = File.ReadAllText(this.m_Path);
                if (!string.IsNullOrEmpty(text))
                {
                    string[] array = text.Split(new char[]
                    {
                        ','
                    });
                    if (array.Length == 4)
                    {
                        StreamDeckAPI.RegisterSession registerSession = new StreamDeckAPI.RegisterSession();
                        if (int.TryParse(array[3], out registerSession.PID))
                        {
                            Process process = this.GetProcess(registerSession.PID);
                            if (process != null)
                            {
                                if (Operators.CompareString(process.ProcessName, "vMixUTCStreamDeck", false) == 0)
                                {
                                    if (int.TryParse(array[0], out registerSession.Port))
                                    {
                                        registerSession.UUID = array[1];
                                        registerSession.RegisterEvent = array[2];
                                        return registerSession;
                                    }
                                }
                                else
                                {
                                    this.WriteInfo("Register.ProcessNameDoesNotMatch");
                                }
                            }
                            else
                            {
                                this.WriteInfo("Register.ProcessNotFound");
                            }
                        }
                    }
                }
            }
            return null;
        }

        // Token: 0x06000027 RID: 39 RVA: 0x00002990 File Offset: 0x00000B90
        private void WriteInfo(string message)
        {
            try
            {
                this.EventReceived?.Invoke(this, new StreamDeckEventArgs(StreamDeckEvent.Info, message, null, 0));
            }
            catch (Exception)
            {
            }
        }

        // Token: 0x06000028 RID: 40 RVA: 0x000029D8 File Offset: 0x00000BD8
        private void WriteError(string message)
        {
            try
            {
                this.EventReceived?.Invoke(this, new StreamDeckEventArgs(StreamDeckEvent.Error, message, null, 0));
            }
            catch (Exception)
            {
            }
        }

        // Token: 0x06000029 RID: 41 RVA: 0x00002A20 File Offset: 0x00000C20
        private void _Task()
        {
            if (this.m_Socket == null)
            {
                try
                {
                    StreamDeckAPI.RegisterSession registerDetails = this.GetRegisterDetails();
                    if (registerDetails != null)
                    {
                        this.m_Session = registerDetails;
                        this.m_Socket = new WebSocket(new Uri("ws://localhost:" + Conversions.ToString(registerDetails.Port)), "", null);

                        this.WriteInfo("Connected");
                        return;
                    }
                    Thread.Sleep(1000);
                    return;
                }
                catch (Exception ex)
                {
                    this.WriteError("Connect: " + ex.ToString());
                    Thread.Sleep(1000);
                    return;
                }
            }
            Thread.Sleep(1000);
        }


        private void m_Socket_SocketClosed(object sender, EventArgs e)
        {
            this.WriteError("SocketClosed");
            this._HandleFailure();
        }

        private void m_Socket_SocketReady(object sender, EventArgs e)
        {
            try
            {
                StreamDeckAPI.RegisterSession session = this.m_Session;
                if (session != null)
                {
                    string szText = string.Concat(new string[]
                    {
                        "{\"event\":\"",
                        session.RegisterEvent,
                        "\",\"uuid\":\"",
                        session.UUID,
                        "\"}"
                    });
                    //this.m_Socket.SendAsync(e.Client.Guid, Encoding.UTF8.GetBytes(szText), System.Net.WebSockets.WebSocketMessageType.Text).RunSynchronously();
                    this.SendTextFrame(szText);
                    this.WriteInfo("Registered");
                }
            }
            catch (Exception ex)
            {
                this.WriteError("SocketReady: " + ex.ToString());
                this._HandleFailure();
            }
        }

        // Token: 0x0600002A RID: 42 RVA: 0x00002AD8 File Offset: 0x00000CD8
        private void _HandleFailure()
        {
            object sync = this.m_Sync;
            ObjectFlowControl.CheckForSyncLockOnValueType(sync);
            lock (sync)
            {
                if (this.m_Socket != null)
                {
                    try
                    {
                        this.m_Socket.Dispose();
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        this.m_Session = null;
                        this.m_Socket = null;
                    }
                }
            }
        }

        // Token: 0x0600002E RID: 46 RVA: 0x00003060 File Offset: 0x00001260
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (this.m_Task != null)
                {
                    this.m_Task.Dispose();
                    this.m_Task = null;
                }
                if (this.m_Socket != null)
                {
                    this.m_Socket.Dispose();
                    this.m_Socket = null;
                }
            }
            this.disposedValue = true;
        }

        // Token: 0x06000030 RID: 48 RVA: 0x0000211F File Offset: 0x0000031F
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Token: 0x04000009 RID: 9
        private ThreadTask m_Task;


        // Token: 0x0400000B RID: 11
        private StreamDeckAPI.RegisterSession m_Session;

        // Token: 0x0400000C RID: 12
        private string m_Path;

        // Token: 0x0400000F RID: 15
        private object m_Sync;

        // Token: 0x04000010 RID: 16
        private bool disposedValue;
        private WebSocket _m_Socket;

        // Token: 0x0200000C RID: 12
        // (Invoke) Token: 0x06000034 RID: 52
        public delegate void EventReceivedEventHandler(object sender, StreamDeckEventArgs e);

        // Token: 0x0200000D RID: 13
        private class RegisterSession
        {
            // Token: 0x06000035 RID: 53 RVA: 0x0000212E File Offset: 0x0000032E
            public RegisterSession()
            {
                this.Contexts = new List<StreamDeckAPI.RegisterContext>();
            }

            // Token: 0x06000036 RID: 54 RVA: 0x000030B0 File Offset: 0x000012B0
            public StreamDeckAPI.RegisterContext GetContext(string ctx)
            {

                foreach (StreamDeckAPI.RegisterContext registerContext in this.Contexts)
                {
                    if (Operators.CompareString(registerContext.Button.Context, ctx, false) == 0)
                    {
                        return registerContext;
                    }
                }

                return null;
            }

            // Token: 0x04000011 RID: 17
            public int PID;

            // Token: 0x04000012 RID: 18
            public int Port;

            // Token: 0x04000013 RID: 19
            public string UUID;

            // Token: 0x04000014 RID: 20
            public string RegisterEvent;

            // Token: 0x04000015 RID: 21
            public List<StreamDeckAPI.RegisterContext> Contexts;
        }

        // Token: 0x0200000E RID: 14
        private class RegisterContext
        {
            // Token: 0x06000037 RID: 55 RVA: 0x00003118 File Offset: 0x00001318
            public RegisterContext(string dev, string ctx, int rw, int col, string controller)
            {
                this.Button = new StreamDeckButton
                {
                    Device = dev,
                    Row = rw,
                    Column = col,
                    Context = ctx,
                    Controller = controller
                };
            }

            // Token: 0x04000016 RID: 22
            public StreamDeckButton Button;
        }
    }
}
