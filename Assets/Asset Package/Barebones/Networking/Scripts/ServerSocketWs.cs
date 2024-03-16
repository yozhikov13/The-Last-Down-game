using arebones.Networking;
using Barebones.Logging;
using Barebones.MasterServer;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using WebSocketSharp.Server;

namespace Barebones.Networking
{
    /// <summary>
    /// Server socket, which accepts websocket connections
    /// </summary>
    public partial class ServerSocketWs : IServerSocket, IUpdatable
    {
        private WebSocketServer server;
        private Queue<Action> executeOnUpdate;
        private float initialSendMessageDelayTime = 0.2f;

        public event Action OnUpdateEvent;

        /// <summary>
        /// Invoked, when a client connects to this socket
        /// </summary>
        public event PeerActionHandler OnClientConnectedEvent;

        /// <summary>
        /// Invoked, when client disconnects from this socket
        /// </summary>
        public event PeerActionHandler OnClientDisconnectedEvent;

        public ServerSocketWs()
        {
            executeOnUpdate = new Queue<Action>();
        }

        /// <summary>
        /// Opens the socket and starts listening to a given port. IP is 127.0.0.1
        /// </summary>
        /// <param name="port"></param>
        public void Listen(int port)
        {
            Listen("127.0.0.1", port);
        }

        /// <summary>
        /// Opens the socket and starts listening to a given port and IP
        /// </summary>
        /// <param name="port"></param>
        public void Listen(string ip, int port)
        {
            // Stop listening when application closes
            MsfTimer.Instance.OnApplicationQuitEvent += Stop;

            if(ip == "127.0.0.1" | ip == "localhost")
            {
                server = new WebSocketServer(port);
            }
            else
            {
                server = new WebSocketServer(IPAddress.Parse(ip), port);
            }

            SetupService(server);

            server.Stop();
            server.Start();

            MsfUpdateRunner.Instance.Add(this);
        }

        /// <summary>
        /// Stops listening
        /// </summary>
        public void Stop()
        {
            MsfUpdateRunner.Instance.Remove(this);
            server.Stop();
        }

        public void ExecuteOnUpdate(Action action)
        {
            lock (executeOnUpdate)
            {
                executeOnUpdate.Enqueue(action);
            }
        }

        private void SetupService(WebSocketServer server)
        {
            server.AddWebSocketService<WsService>("/msf", (service) =>
            {
                service.IgnoreExtensions = true;
                service.SetServerSocket(this);
                var peer = new PeerWsServer(service);

                service.OnMessageEvent += (data) =>
                {
                    peer.HandleDataReceived(data, 0);
                };

                ExecuteOnUpdate(() =>
                {
                    MsfTimer.Instance.StartCoroutine(peer.SendDelayedMessages(initialSendMessageDelayTime));
                    OnClientConnectedEvent?.Invoke(peer);
                });

                peer.OnPeerDisconnectedEvent += OnClientDisconnectedEvent;

                service.OnCloseEvent += reason =>
                {
                    peer.NotifyDisconnectEvent();
                };

                service.OnErrorEvent += reason =>
                {
                    Logs.Error(reason);
                    peer.NotifyDisconnectEvent();
                };
            });

            server.AddWebSocketService<EchoService>("/echo");
        }

        public void Update()
        {
            OnUpdateEvent?.Invoke();

            lock (executeOnUpdate)
            {
                while (executeOnUpdate.Count > 0)
                {
                    executeOnUpdate.Dequeue()?.Invoke();
                }
            }
        }
    }
}