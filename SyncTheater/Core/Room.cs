using System;
using System.Collections.Generic;
using System.Net;
using Serilog;
using SyncTheater.Core.Models;
using SyncTheater.Core.Servers;
using SyncTheater.KinotheaterService;
using SyncTheater.Types.Exceptions;
using SyncTheater.Utils.NAT;

namespace SyncTheater.Core
{
    internal sealed class Room
    {
        private readonly RoomState _state = new RoomState();
        private bool _initialized;
        private Server _server;
        private Service _service;

        private Room()
        {
        }

        public static Room GetInstance { get; } = new Room();
        public static RoomState GetState => GetInstance._state;

        public bool IsOpen => _server?.IsStarted ?? false;

        public ServerSettings ServerSettings { get; private set; }
        public RoomSettings RoomSettings { get; private set; }

        public Room Initialize(ServerSettings serverSettings, RoomSettings roomSettings)
        {
            if (_initialized)
            {
                throw new RoomAlreadyInitializedException();
            }

            ServerSettings = serverSettings;
            RoomSettings = roomSettings;

            _server = ServerSettings.UseIpv6 ? new Server(IPAddress.IPv6Any, ServerSettings.Port) :
                new Server(IPAddress.Any, ServerSettings.Port);

            if (ServerSettings.UseService)
            {
                _service = new Service(ServerSettings, roomSettings.ServerId, roomSettings.Secret);
            }

            _initialized = true;
            return GetInstance;
        }

        public void Open()
        {
            if (!_initialized)
            {
                throw new RoomNotInitializedException();
            }

            if (_server.IsStarted)
            {
                throw new RoomAlreadyOpenException();
            }

            Log.Verbose("Starting server...");
            _server.Start();
            Log.Information($"Server started on port {ServerSettings.Port}.");

            if (ServerSettings.UseService)
            {
                _service.Start();
            }

            MainLoop();
        }

        public void Close()
        {
            if (!_server.IsStarted)
            {
                throw new RoomNotOpenException();
            }

            if (ServerSettings.Nat.Traverse)
            {
                NatTraversal.ClearAsync(ServerSettings.Nat.ForwardingResult.PublicPort).GetAwaiter();
            }

            _service.Stop();

            Log.Verbose("Stopping server...");
            _server.Stop();
            Log.Information("Server stopped.");
        }

        public void Send(byte[] data, IEnumerable<User> sendTo)
        {
            foreach (var user in sendTo)
            {
                _server.FindSession(user.SessionId).SendAsync(data);
            }
        }

        private void MainLoop()
        {
            while (_server.IsStarted)
            {
                Console.Read();
            }
        }
    }
}