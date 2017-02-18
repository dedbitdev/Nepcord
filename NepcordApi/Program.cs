using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using NepcordApi.connections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using System.Timers;
using RestSharp;
using NepcordApi.rest;
using NepcordApi.auth;
using NepcordApi.serializers;
using RestSharp.Serializers;
using System.Threading.Tasks;
using NepcordApi.log;
using System.Text.RegularExpressions;
using System.Globalization;
using NepcordApi;

namespace NepcordApi.log
{
    public static class NepLogger
    {
        public static void Log(string LogContent, string Misc = null)
        {
            StringBuilder b = new StringBuilder();
            if (Misc != null)
            {
                b.Append($"[Nepcord API] [{DateTime.Now.ToShortTimeString()}] [{Misc}] ");
            }
            else
            {
                b.Append($"[Nepcord API] [{DateTime.Now.ToShortTimeString()}] ");
            }
            Console.WriteLine(b.Append(LogContent).ToString());
        }
    }
}
namespace NepcordApi.serializers
{
    /// <summary>
    /// Custom serializer (Newtonsoft.Json) for RestSharp. Must be applied on every RestRequest.
    /// </summary>
    public class RemcordJsonSerializer : ISerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:NepcordApi.serializers.RemcordJsonSerializer"/> class.
        /// </summary>
        public RemcordJsonSerializer()
        {
            ContentType = "application/json";
        }

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>A serialized stirng.</returns>
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public string RootElement { get; set; }

        public string Namespace { get; set; }

        public string DateFormat { get; set; }

        public string ContentType { get; set; }
    }
}

namespace NepcordApi
{
    /// <summary>
    /// Experimental - a attempt at creating a user-friendly DiscordClient class.
    /// </summary>
    public class DiscordClient
    {
        public RemcordRest RestClient;
        public RemcordAuthentication Authentication { get; private set; }
        /// <summary>
        /// Updates the current authentication set.
        /// </summary>
        /// <param name="_Authentication">Authentication to replace the current one with.</param>
        public void UpdateAuthentication(RemcordAuthentication _Authentication)
        {
            Authentication = _Authentication;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:NepcordApi.DiscordClient"/> class.
        /// </summary>
        public DiscordClient()
        {
            RestClient = new RemcordRest();
        }
    }
}

namespace NepcordApi.rest
{
    /// <summary>
    /// Base class for user messages.
    /// </summary>
    public class Message
    {

    }
    /// <summary>
    /// Current not used, just the same as MessagePayload.
    /// </summary>
    public class RestMessage : Message
    {
        [JsonProperty("content")]
        public string Content;
    }
    /// <summary>
    /// Remcord REST API manager.
    /// </summary>
    public class RemcordRest
    {
        /// <summary>
        /// Sends a POST request using SendRequest.
        /// </summary>
        /// <returns>The response from Discord.</returns>
        /// <param name="EndpointTarget">Discord API endpoint to send to.</param>
        /// <param name="Payload">Payload to send.</param>
        /// <param name="Authentication">Most endpoints require authentication.</param>
        public IRestResponse SendPostRequest(string EndpointTarget, MessagePayload Payload, RemcordAuthentication Authentication) => SendRequest(EndpointTarget, Payload, Method.POST, Authentication);
        /// <summary>
        /// Sends a request using the specified method through REST.
        /// </summary>
        /// <returns>The response from Discord.</returns>
        /// <param name="EndpointTarget">Discord API endpoint to send to.</param>
        /// <param name="Payload">Payload to send.</param>
        /// <param name="Method">Method.</param>
        /// <param name="Authentication">Most endpoints require authentication.</param>
        public IRestResponse SendRequest(string EndpointTarget, MessagePayload Payload, Method Method, RemcordAuthentication Authentication)
        {
            RestClient RequestClient = new RestClient("https://discordapp.com/api");
            RestRequest Request = new RestRequest(EndpointTarget, Method);
            Request.JsonSerializer = new RemcordJsonSerializer();
            Request.AddJsonBody(Payload);
            if (Authentication != null) Request.AddHeader("Authorization", "Bot " + Authentication.Token);
            IRestResponse Response = RequestClient.Execute(Request);
            return Response;
        }
    }
    /// <summary>
    /// A message payload sent from Discord, usually with a MESSAGE_CREATE event.
    /// </summary>
    public class MessagePayload
    {
        [JsonProperty("content")]
        public string Content { get; set; }
    }
    /// <summary>
    /// A static class for sending messages to a channel.
    /// </summary>
    public static class MessageInterface
    {
        /// <summary>
        /// Sends a message to a specified channel.
        /// </summary>
        /// <param name="_Content">String to send</param>
        /// <param name="ChannelID">Channel ID to send message to.</param>
        public static void SendMessage(string _Content, string ChannelID)
        {
            RemcordRest rc = new RemcordRest();
            RemcordAuthentication a = new RemcordAuthentication();
            a.Token = tokenx.GetToken();
            MessagePayload p = new MessagePayload();
            p.Content = _Content;
            rc.SendPostRequest(EndpointRoutes.POST_SendMessage(ChannelID), p, a);
        }
        public static async Task SendMessageAsync(string _Content, string ChannelID)
        {
            RemcordRest client = new RemcordRest();
            RemcordAuthentication authx = new RemcordAuthentication();
            authx.Token = tokenx.GetToken();
            MessagePayload payload = new MessagePayload();
            payload.Content = _Content;
            await Task.Run(() =>
            {
                client.SendPostRequest(EndpointRoutes.POST_SendMessage(ChannelID), payload, authx);
            });
        }
    }
}

namespace NepcordApi.auth
{
    /// <summary>
    /// A class for storing Discord authentication data.
    /// </summary>
    public class RemcordAuthentication
    {
        public string Token;
    }
}

namespace NepcordApi
{
    /// <summary>
    /// Temporary token storage.
    /// </summary>
    public static class tokenx
    {
        /// <summary>
        /// The token.
        /// </summary>
        public static string GetToken()
        {
            using (StreamReader Reader = new StreamReader("secret.json"))
            {
                string jsoninput = Reader.ReadToEnd();
                dynamic ds = JsonConvert.DeserializeObject(jsoninput);
                return ds.token;
            }
        }
    }
    /// <summary>
    /// All operation codes for Discord.
    /// </summary>
    enum OpCodes
    {
        Dispatch = 0,
        Heartbeat = 1,
        Identify = 2,
        StatusUpdate = 3,
        VoiceStatusUpdate = 4,
        VoiceServerPing = 5,
        Resume = 6,
        Reconnect = 7,
        RequestGuildMembers = 8,
        InvalidSession = 9,
        Hello = 10,
        HeartbeatACK = 11
    }
    /// <summary>
    /// A list of Discord API endpoints.
    /// </summary>
    public static class EndpointRoutes
    {
        public static string POST_SendMessage(string ID) { return $"channels/{ID}/messages"; }
    }
    /// <summary>
    /// A normal payload.
    /// </summary>
    public class RemPayload
    {
        public int? op;
        public object d;
        public int? s;
        public string t;
    }
    /// <summary>
    /// A user message sent to a channel.
    /// </summary>
    public class DiscordUserMessage
    {
        [JsonProperty("id")]
        public string ID { get; private set; }
        [JsonProperty("channel_id")]
        public string ChannelID { get; private set; }
        [JsonProperty("author")]
        public object MessageAuthor { get; private set; }
        [JsonProperty("content")]
        public string Content { get; private set; }
    }
    /// <summary>
    /// Rem Identify Payload Properties object class.
    /// </summary>
    class RIPPContent
    {
        [JsonProperty("$os")]
        public string OS;
        [JsonProperty("$browser")]
        public string Lib = "Remcord";
        [JsonProperty("$device")]
        public string Device = "Remcord";
        [JsonProperty("$referrer")]
        public string Referrer = "";
        [JsonProperty("$referring_domain")]
        public string RefferingDomain = "";
    }
    /// <summary>
    /// Rem Identify Payload content class.
    /// </summary>
    class RIPContent
    {
        //public bool compress;
        //public int large_threshold;
        public string token;
        //public int[] shard;
        public object properties;
    }
    /// <summary>
    /// The main program.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// A list of all OpCodes and descriptions.
        /// </summary>
        static Dictionary<OpCodes, string> OpCodeDetails = new Dictionary<OpCodes, string> {
                        {OpCodes.Dispatch, "Dispatch"},
                        {OpCodes.Heartbeat, "Heartbeat"},
                        {OpCodes.HeartbeatACK, "Heartbeat Acknowledgement"},
                        {OpCodes.Hello, "Hello"},
                        {OpCodes.Identify, "Identify"},
                        {OpCodes.InvalidSession, "Invalid Session"},
                        {OpCodes.Reconnect, "Reconnect"},
                        {OpCodes.RequestGuildMembers, "Request Guild Members"},
                        {OpCodes.Resume, "Resume"},
                        {OpCodes.StatusUpdate, "Status Update"},
                        {OpCodes.VoiceServerPing, "Voice Server Ping"},
                        {OpCodes.VoiceStatusUpdate, "Voice Status Update"}
                    };
        /// <summary>
        /// The current heartbeat sequence.
        /// </summary>
        public static int? hb_seq;
        /// <summary>
        /// The timer, to send out a heartbeat with a defined interval.
        /// </summary>
        public static Timer HeartbeatTimer = new Timer();
        /// <summary>
        /// The active WebSocket.
        /// </summary>
        public static WebSocket s;
        /// <summary>
        /// Are we running in verbose mode?
        /// </summary>
        public static bool VerboseMode;
        /// <summary>
        /// The active SocketManager.
        /// </summary>
        public static SocketManager m;
        /// <summary>
        /// Trying to parse the heartbeat interval sent from Discord.
        /// </summary>
        public static int result;
        /// <summary>
        /// Have recieved the Hello message from Discord.
        /// </summary>
        public static bool HaveRecievedHello;
        /// <summary>
        /// The time the last heartbeat was sent.
        /// </summary>
        public static DateTime LastHeartbeatTime;
        /// <summary>
        /// Have we sent the first heartbeat?
        /// </summary>
        public static bool SentFirstHeartbeat;
        /// <summary>
        /// Sends a heartbeat through the default WebSocket.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args given by the heartbeat timer (as Discord requires a heartbeat sent every x milliseconds)</param>
        static void SendHeartbeat(object sender, ElapsedEventArgs e)
        {
            RemPayload hbP = new RemPayload();
            hbP.op = (int)OpCodes.Heartbeat;
            hbP.d = hb_seq;
            s.SendPayload(hbP);
            StringBuilder b = new StringBuilder();
            b.Append("Sending Heartbeat.");
            if (SentFirstHeartbeat)
            {
                b.Append(" " + DateTime.Now.Subtract(LastHeartbeatTime).Seconds + " seconds since the last heartbeat.");
            }
            NepLogger.Log(b.ToString(), "Heartbeat");
            LastHeartbeatTime = DateTime.Now;
            SentFirstHeartbeat = true;
        }
        /// <summary>
        /// The response payload from Discord. (Reused)
        /// </summary>
        public static RemPayload resp;
        /// <summary>
        /// Called when the Discord socket is closed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="args">Event arguments.</param>
        public static void OnSocketClose(object sender, CloseEventArgs args)
        {
            NepLogger.Log("CRITICAL - Discord socket closed.");
            NepLogger.Log("Reason: " + args.Reason);
            NepLogger.Log("Code: " + args.Code);
        }

        public static string GetBuildInfo()
        {
            return $"This build uses .NET Framework and is {(VerboseMode ? "running in verbose mode." : "Not running in verbose mode.")}";
        }
        /// <summary>
        /// All legitimate verbose mode arguments.
        /// </summary>
        public static List<string> VerboseModeArguments = new List<string>()
        {
            "-v", "-verbose", "--v", "--verbose"
        };
        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static void Main(string[] args)
        {
            try
            {
                if (VerboseModeArguments.Contains(args[0]))
                {
                    VerboseMode = true;
                    NepLogger.Log("Running Remcord 1.3 in verbose mode.");
                }
            }
            catch (IndexOutOfRangeException)
            {
                NepLogger.Log("Running Remcord 1.3 with no arguments.");
            }
            Connect();
        }
        /// <summary>
        /// Handles an incoming socket message from Discord.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">The arguments of the socket message.</param>
        public static void HandleSocketMessage(object sender, MessageEventArgs e)
        {
            if (e.IsText)
            {
                resp = JsonConvert.DeserializeObject<RemPayload>(e.Data);
                var respd = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(resp.d));
                hb_seq = resp.s;
                if (resp.op == (int)OpCodes.Hello && HaveRecievedHello == false)
                {
                    int.TryParse((respd.First(x => x.Key == "heartbeat_interval").Value).ToString(), out result);
                    HeartbeatTimer.Interval = result;
                    HeartbeatTimer.Enabled = true;
                    HaveRecievedHello = true;
                    if (VerboseMode) NepLogger.Log("Recieved Hello payload. Heartbeat interval: " + result, "Setup");
                }
                if (resp.op == (int)OpCodes.HeartbeatACK && VerboseMode)
                {
                    NepLogger.Log("Recieved Heartbeat acknowledgement.", "Heartbeat");
                }
                if (!(resp.op == (int)OpCodes.Dispatch) && !(resp.op == (int)OpCodes.HeartbeatACK) && !(resp.op == (int)OpCodes.Hello) && VerboseMode)
                {
                    NepLogger.Log($"Recieved message from Discord with OP Code {resp.op} ({OpCodeDetails[(OpCodes)resp.op]}), and data {resp.d}");
                }
                if (resp.op == (int)OpCodes.Dispatch && resp.t == "MESSAGE_CREATE")
                {
                    //messagecreate
                    var message = JsonConvert.DeserializeObject<DiscordUserMessage>(JsonConvert.SerializeObject(resp.d));
                    if (message.Content == ".remcord")
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("**== Remcord - a Discord API for .NET ==**");
                        sb.AppendLine("Remcord is an upcoming alpha API and bot platform for .NET.");
                        sb.AppendLine("Current version: 0.12 *Drunk Elephant*");
                        sb.AppendLine("Build info: " + GetBuildInfo());
                        sb.AppendLine("Current gateway version: **Gateway v5**");
                        sb.AppendLine("Using WSS or WSX? **WSS**");
                        sb.AppendLine("Using:");
                        sb.AppendLine("**WebSocketSharp** as the WebSocket library. [Regrets!]");
                        sb.AppendLine("**RestSharp** for REST communication with Discord. [More regrets]");
                        MessageInterface.SendMessage(sb.ToString(), message.ChannelID);
                    }
                    if (message.Content == ".socket")
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("**Remcord Socket Information**");
                        sb.AppendLine("Currently using **WebSocketSharp** as our WebSocket library.");
                        sb.AppendLine("Connected to Discord gateway " + ConnectionManager.GetGateway().url);
                        sb.AppendLine("Internal gateway " + ConnectionManager.GetGateway());
                        sb.AppendLine("Remcord version: 0.12");
                        var time = DateTime.Now.Subtract(LastHeartbeatTime).Seconds;
                        if (SentFirstHeartbeat) sb.AppendLine($"The last heartbeat was sent " + time + $" {(time == 1 ? "second" : "seconds")} ago.");
                        MessageInterface.SendMessage(sb.ToString(), message.ChannelID);
                    }
                    if (message.Content.Contains(".rr ") || message.Content.Contains(".russianroulette "))
                    {
                        Random gen = new Random();
                        var args = message.Content.Split(' ')[1];
                        int ndx;
                        if (int.TryParse(args, out ndx))
                        {
                            MessageInterface.SendMessage(gen.Next(1, ndx) == 1 ? ":skull_crossbones: You have died!" : ":sunny: You are alive.", message.ChannelID);
                        }
                        else
                        {
                            MessageInterface.SendMessage("invalid argument", message.ChannelID);
                        }

                    }
                    if (message.Content == ".rr" || message.Content == ".russianroulette")
                    {
                        Random gen = new Random();
                        int nx = gen.Next(1, 6);
                        MessageInterface.SendMessage((nx == 1 ? ":skull_crossbones: You have died!" : ":sunny: You are alive."), message.ChannelID);
                    }
                }
                else if (resp.op == (int)OpCodes.Dispatch && VerboseMode)
                {
                    NepLogger.Log((resp.t).ToTitleCase().Replace('_', ' '), "Incoming Event");
                }
            }
            else
            {
                NepLogger.Log(e.RawData + e.Data, "Unknown Event Data");
            }
        }
        /// <summary>
        /// Converts the string to sentence case.
        /// </summary>
        /// <param name="Input">The string to convert.</param>
        public static string ToTitleCase(this string Input)
        {
            /*var lc = Input.ToLower();
            var r = new Regex(@"(^[a-z])|\.\s+(.)", RegexOptions.ExplicitCapture);
            return r.Replace(lc, s => s.Value.ToUpper());*/
            var r = Input.ToLower().Replace("_", " ");
            TextInfo info = CultureInfo.CurrentCulture.TextInfo;
            return info.ToTitleCase(r);
        }
        /// <summary>
        /// Connects to Discord and sets up the heartbeat manager and payload manager.
        /// </summary>
        public static void Connect()
        {
            if (VerboseMode) NepLogger.Log("Using Discord gateway " + (ConnectionManager.GetGateway()).url, "Setup");
            m = new SocketManager();
            s = m.CreateSocket();
            HeartbeatTimer.Elapsed += SendHeartbeat;
            s.OnMessage += HandleSocketMessage;
            s.OnClose += OnSocketClose;
            NepLogger.Log("Attempting to connect.", "Setup");
            s.Connect();
            RemPayload p = new RemPayload();
            RIPPContent prop = new RIPPContent();
            RIPContent n = new RIPContent();
            prop.OS = "win";
            n.properties = JObject.FromObject(prop);
            n.token = tokenx.GetToken();
            p.d = JObject.FromObject(n);
            p.op = 2;
            s.SendPayload(p, false);
            if (VerboseMode) NepLogger.Log("Sending " + OpCodeDetails[OpCodes.Identify] + " to Discord and triggering handshake.", "Setup");
            NepLogger.Log("Connected.", "Setup");
            Console.ReadLine();
        }
    }
    /// <summary>
    /// WebSocket extensions for sending payloads.
    /// </summary>
    static class SocketExtensions
    {
        /// <summary>
        /// Sends the payload.
        /// </summary>
        /// <param name="Socket">Socket to send through.</param>
        /// <param name="payload">Payload to send.</param>
        public static void SendPayload(this WebSocket Socket, RemPayload payload, bool Log = true)
        {
            payload.s = Program.hb_seq;
            string Data = JsonConvert.SerializeObject(payload);
            if (payload.op != (int)OpCodes.Heartbeat && Log && Program.VerboseMode)
            {
                NepLogger.Log("Sending " + Data + " over the WebSocket.");
            }
            Socket.Send(Data);
        }
    }
}
namespace NepcordApi.connections
{
    /// <summary>
    /// Discord API info such as the API base URL.
    /// </summary>
    static class APIInfo
    {
        /// <summary>
        /// Discord's base URL for REST API connections.
        /// </summary>
        public static string APIBaseUrl = "https://discordapp.com/api/";
    }
    /// <summary>
    /// Represents a Discord gateway.
    /// </summary>
    class Gateway
    {
        /// <summary>
        /// Gateway URL.
        /// </summary>
        public string url;
    }
    /// <summary>
    /// A connection manager, for getting a gateway through WebRequest.
    /// </summary>
    class ConnectionManager
    {
        /// <summary>
        /// Gets a new Gateway from Discord.
        /// </summary>
        /// <returns>The gateway.</returns>
        public static Gateway GetGateway()
        {
            WebRequest GatewayRequest = WebRequest.Create(APIInfo.APIBaseUrl + "gateway");
            StreamReader Reader = new StreamReader(GatewayRequest.GetResponse().GetResponseStream());
            Gateway EstablishedGateway = new Gateway();
            var respx = JObject.Parse(Reader.ReadLine());
            EstablishedGateway.url = (string)respx["url"];
            return EstablishedGateway;
        }
    }
    /// <summary>
    /// A socket manager to create WebSockets with the base URL.
    /// </summary>
    class SocketManager
    {
        /// <summary>
        /// The socket.
        /// </summary>
        WebSocket Socket;
        /// <summary>
        /// Creates a Socket.
        /// </summary>
        /// <returns>A configured WebSocket using GetGateway.</returns>
        public WebSocket CreateSocket()
        {
            Socket = new WebSocket(ConnectionManager.GetGateway().url + "?v=5&encoding=json");
            return Socket;
        }
    }
}
