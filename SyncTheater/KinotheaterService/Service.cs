using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetCoreServer;
using Serilog;
using SyncTheater.Core;
using SyncTheater.KinotheaterService.Requests;
using SyncTheater.Utils;
using SyncTheater.Utils.DB;
using HttpClient = System.Net.Http.HttpClient;

namespace SyncTheater.KinotheaterService
{
    internal sealed class Service
    {
        private static readonly IPEndPoint ServiceGetIpv4EndPoint = new IPEndPoint(ServiceUtils.Ipv4, 7374);
        private static readonly IPEndPoint ServiceGetIpv6EndPoint = new IPEndPoint(ServiceUtils.Ipv6, 7376);
        private static readonly string BaseHttpApiUri = $"https://{ServiceUtils.Domain}:7372";
        private readonly Client _client = new Client(new IPEndPoint(ServiceUtils.Ipv4, 7373));
        private readonly UpdateRequest _requestData;
        private Task<string> _ipv4;
        private Task<string> _ipv6;

        public Service(ServerSettings settings, string id, string secret)
        {
            _ipv4 = GetIpv4Async();
            if (settings.UseIpv6)
            {
                _ipv6 = GetIpv6Async();
            }

            _requestData = new UpdateRequest
            {
                Secret = secret,
                Server = new Server
                {
                    Id = id,
                    PrivatePort = settings.Port,
                    PublicPort = settings.Nat.ForwardingResult?.PublicPort ?? 0,
                },
            };
            _client.OptionKeepAlive = true;

            _client.ConnectedEvent += CreateServerAsync;
        }

        public void Start()
        {
            Log.Verbose("Connecting to service...");
            
            _client.ConnectAsync();
        }

        public void Stop()
        {
            Log.Verbose("Sending stop signal to service...");

            _client.ForceDisconnect();
        }

        public static async Task<RegisterResponse> RegisterAsync(string id)
        {
            Log.Debug($"Registering server id `{id}`...");
            
            using var client = new HttpClient();
            var res = await client.PostAsync(new Uri($"{BaseHttpApiUri}/register"),
                new StringContent(new
                    {
                        Id = id,
                    }.ToJson(),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json
                )
            );

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (res.StatusCode)
            {
                case HttpStatusCode.Created:
                    var data = SerializationUtils.Deserialize<RegisterResponse>(await res.Content.ReadAsStringAsync());

                    Log.Information($"Successfully registered server. Server id: {data.Id}");

                    return data;

                case HttpStatusCode.Conflict:
                    Log.Error($"Id `{id}` is already in use.");

                    return null;

                default:
                    Log.Fatal(
                        $"Unknown error. Try restart server. Response body: {await res.Content.ReadAsStringAsync()}"
                    );

                    return null;
            }
        }

        private async void CreateServerAsync(object sender, EventArgs eventArgs)
        {
            await EnsureDataCreatedAsync();

            Log.Verbose("Sending server info to service...");

            using var client = new HttpClient();
            var res = await client.PostAsync(new Uri($"{BaseHttpApiUri}/update"),
                new StringContent(_requestData.ToJson(),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json
                )
            );

            var resBody = await res.Content.ReadAsStringAsync();
            var ok = false;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (res.StatusCode)
            {
                case HttpStatusCode.OK:
                    Log.Information("Successfully updated server info.");

                    ok = true;

                    _client.SendAsync($"secret:{_requestData.Secret}");

                    break;

                case HttpStatusCode.NotFound:
                    Log.Error(
                        $"Your server was not registered or was deleted. Server will be registered. Response body: {resBody}"
                    );

                    var registerResult = await RegisterAsync(_requestData.Server.Id);

                    if (registerResult == null)
                    {
                        break;
                    }

                    ok = true;
                    _requestData.Secret = registerResult.SuperSecretCode;
                    await Db.AddServerAsync(_requestData.Server.Id, _requestData.Secret);

                    _client.ReconnectAsync();

                    break;

                case HttpStatusCode.Conflict:
                    Log.Error($"Server with id `{_requestData.Server.Id}` already exists. Response body: {resBody}.");

                    break;

                case HttpStatusCode.BadRequest:
                    Log.Error(
                        $"Wrong secret `{_requestData.Secret}` for id `{_requestData.Server.Id}`. Response body: {resBody}."
                    );

                    break;

                default:
                    Log.Error($"Failed to add server. Try restart server. Response body: {resBody}.");

                    break;
            }

            if (!ok)
            {
                Log.Error("Your server was not updated on service. Fix the problems and restart server.");
            }
        }

        private static async Task<string> GetIpv4Async()
        {
            for (var i = 0; i < 5; ++i)
            {
                IPAddress ip;
                try
                {
                    ip = await Task.Run(Ipv4Getter,
                        new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token
                    );
                }
                catch (TaskCanceledException)
                {
                    continue;
                }

                if (ip != null && !ip.Equals(IPAddress.Any))
                {
                    return ip.ToString();
                }
            }

            Log.Error("Failed to get IPv4 address from service.");
            return null;
        }

        private static async Task<string> GetIpv6Async()
        {
            for (var i = 0; i < 5; ++i)
            {
                IPAddress ip;
                try
                {
                    ip = await Task.Run(Ipv6Getter,
                        new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token
                    );
                }
                catch (TaskCanceledException)
                {
                    continue;
                }

                if (ip != null && !ip.Equals(IPAddress.IPv6Any))
                {
                    return ip.ToString();
                }
            }

            Log.Error("Failed to get IPv6 address from service.");
            return null;
        }

        private static IPAddress Ipv4Getter()
        {
            using var ip4Client = new TcpClient(ServiceGetIpv4EndPoint);

            ip4Client.Connect();

            var ip = new byte[4];
            ip4Client.Receive(ip);
            ip4Client.Disconnect();

            try
            {
                return new IPAddress(ip);
            }
            catch (Exception)
            {
                Log.Fatal("Invalid response from service, wrong IPv4 address.");
                return null;
            }
        }

        private static IPAddress Ipv6Getter()
        {
            using var ip6Client = new TcpClient(ServiceGetIpv6EndPoint);

            ip6Client.Connect();

            var ip = new byte[16];
            ip6Client.Receive(ip);
            ip6Client.Disconnect();

            try
            {
                return new IPAddress(ip);
            }
            catch (Exception)
            {
                Log.Fatal("Invalid response from service, wrong IPv6 address.");
                return null;
            }
        }

        private async Task EnsureDataCreatedAsync()
        {
            while (true)
            {
                _requestData.Server.Ipv4 = await _ipv4;
                _requestData.Server.Ipv6 = await _ipv6 ?? "";

                if (string.IsNullOrEmpty(_requestData.Server.Ipv4))
                {
                    Log.Error("Failed to get IP addresses to create server.");

                    await Task.Delay(TimeSpan.FromSeconds(5));
                    _ipv4 = GetIpv4Async();
                    _ipv6 = GetIpv6Async();
                    continue;
                }

                break;
            }
        }
    }
}