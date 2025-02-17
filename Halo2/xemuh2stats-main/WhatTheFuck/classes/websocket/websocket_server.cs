using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WhatTheFuck.classes.websocket
{
    /// <summary>
    /// Handler for when a message was received
    /// </summary>
    public class OnMessageReceivedHandler : EventArgs
    {
        /// <summary>The websocketClient that send the message</summary>
        private websocket_client _websocketClient;

        /// <summary>The message the websocketClient sent</summary>
        private string _message;

        /// <summary>Create a new message received event handler</summary>
        /// <param name="websocketClient">The websocketClient that sent the message</param>
        /// <param name="Message">The message the websocketClient sent</param>
        public OnMessageReceivedHandler(websocket_client websocketClient, string Message)
        {
            this._websocketClient = websocketClient;
            this._message = Message;
        }

        /// <summary>Get the websocketClient that sent the received message</summary>
        /// <returns>The websocketClient that sent the message</returns>
        public websocket_client GetClient()
        {
            return _websocketClient;
        }

        /// <summary>The message that was received from the websocketClient</summary>
        /// <returns>The received message</returns>
        public string GetMessage()
        {
            return _message;
        }

    }

    /// <summary>
    /// Handler for when a message was send to a websocketClient
    /// </summary>
    public class OnSendMessageHandler : EventArgs
    {
        /// <summary>The websocketClient the message was sent to</summary>
        private websocket_client _websocketClient;

        /// <summary>The message that was sent to the websocketClient</summary>
        private string _message;

        /// <summary>Create a new handler for when a message was sent</summary>
        /// <param name="websocketClient">The websocketClient the message was sent to</param>
        /// <param name="Message">The message that was sent to the websocketClient</param>
        public OnSendMessageHandler(websocket_client websocketClient, string Message)
        {
            this._websocketClient = websocketClient;
            this._message = Message;
        }

        /// <summary>The websocketClient the message was sent to</summary>
        /// <returns>The websocketClient receiver</returns>
        public websocket_client GetClient()
        {
            return _websocketClient;
        }

        /// <summary>The message that was send to the websocketClient</summary>
        /// <returns>The sent message</returns>
        public string GetMessage()
        {
            return _message;
        }
    }

    /// <summary>
    /// Handler for when a websocketClient connected
    /// </summary>
    public class OnClientConnectedHandler : EventArgs
    {
        /// <summary>The websocketClient that connected to the server</summary>
        private websocket_client _websocketClient;

        /// <summary>Create a new event handler for when a websocketClient connected</summary>
        /// <param name="websocketClient">The websocketClient that connected</param>
        public OnClientConnectedHandler(websocket_client websocketClient)
        {
            this._websocketClient = websocketClient;
        }

        /// <summary>Get the websocketClient that was connected</summary>
        /// <returns>The websocketClient that connected </returns>
        public websocket_client GetClient()
        {
            return _websocketClient;
        }
    }

    /// <summary>
    /// Handler for when a websocketClient disconnects
    /// </summary>
    public class OnClientDisconnectedHandler : EventArgs
    {
        /// <summary>The websocketClient that diconnected</summary>
        private websocket_client _websocketClient;

        /// <summary>Create a new handler for when a websocketClient disconnects</summary>
        /// <param name="websocketClient">The disconnected websocketClient</param>
        public OnClientDisconnectedHandler(websocket_client websocketClient)
        {
            this._websocketClient = websocketClient;
        }

        /// <summary>Gets the websocketClient that disconnected</summary>
        /// <returns>The disconnected websocketClient</returns>
        public websocket_client GetClient()
        {
            return _websocketClient;
        }
    }

    ///<summary>
    /// Object for all listen servers
    ///</summary>
    public partial class websocket_server
    {
        #region Fields

        /// <summary>The listen socket (server socket)</summary>
        private Socket _socket;

        /// <summary>The listen ip end point of the server</summary>
        private IPEndPoint _endPoint;

        /// <summary>The connected clients to the server </summary>
        public List<websocket_client> _clients = new List<websocket_client>();

        #endregion

        #region Class Events

        /// <summary>Create and start a new listen socket server</summary>
        /// <param name="EndPoint">The listen endpoint of the server</param>
        public websocket_server(IPEndPoint EndPoint)
        {
            // Set the endpoint if the input is valid
            if (EndPoint == null) return;
            this._endPoint = EndPoint;

            // Create a new listen socket
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Start the server
            start();
        }

        #endregion

        #region Field Getters

        /// <summary>Gets the listen socket</summary>
        /// <returns>The listen socket</returns>
        public Socket GetSocket()
        {
            return _socket;
        }

        /// <summary>Get the listen socket endpoint</summary>
        /// <returns>The listen socket endpoint</returns>
        public IPEndPoint GetEndPoint()
        {
            return _endPoint;
        }

        /// <summary>Gets a connected websocketClient at the given index</summary>
        /// <param name="Index">The connected websocketClient array index</param>
        /// <returns>The connected websocketClient at the index, returns null if the index is out of bounds</returns>
        public websocket_client GetConnectedClient(int Index)
        {
            if (Index < 0 || Index >= _clients.Count) return null;
            return _clients[Index];
        }

        /// <summary>Gets a connected websocketClient with the given guid</summary>
        /// <param name="Guid">The Guid of the websocketClient to get</param>
        /// <returns>The websocketClient with the given id, return null if no websocketClient with the guid could be found</returns>
        public websocket_client GetConnectedClient(string Guid)
        {
            foreach (websocket_client client in _clients)
            {
                if (client.GetGuid() == Guid) return client;
            }
            return null;
        }

        /// <summary>Gets a connected websocketClient with the given socket</summary>
        /// <param name="Socket">The socket of the websocketClient </param>
        /// <returns>The connected websocketClient with the given socket, returns null if no websocketClient with the socket was found</returns>
        public websocket_client GetConnectedClient(Socket Socket)
        {
            foreach (websocket_client client in _clients)
            {
                if (client.GetSocket() == Socket) return client;
            }
            return null;
        }

        /// <summary>Get the number of clients that are connected to the server</summary>
        /// <returns>The number of connected clients</returns>
        public int GetConnectedClientCount()
        {
            return _clients.Count;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the listen server when a server object is created
        /// </summary>
        private void start()
        {
            // Bind the socket and start listending
            GetSocket().Bind(GetEndPoint());
            GetSocket().Listen(0);

            // Start to accept clients and accept incomming connections 
            GetSocket().BeginAccept(connectionCallback, null);
        }

        /// <summary>
        /// Stops the listen server 
        /// </summary>
        public void Stop()
        {
            GetSocket().Close();
            GetSocket().Dispose();
        }

        /// <summary>Called when the socket is trying to accept an incomming connection</summary>
        /// <param name="AsyncResult">The async operation state</param>
        private void connectionCallback(IAsyncResult AsyncResult)
        {
            try
            {
                // Gets the websocketClient thats trying to connect to the server
                Socket clientSocket = GetSocket().EndAccept(AsyncResult);

                // Read the handshake updgrade request
                byte[] handshakeBuffer = new byte[1024];
                int handshakeReceived = clientSocket.Receive(handshakeBuffer);

                // Get the hanshake request key and get the hanshake response
                string requestKey = Helpers.GetHandshakeRequestKey(Encoding.Default.GetString(handshakeBuffer));
                string hanshakeResponse = Helpers.GetHandshakeResponse(Helpers.HashKey(requestKey));

                // Send the handshake updgrade response to the connecting websocketClient 
                clientSocket.Send(Encoding.Default.GetBytes(hanshakeResponse));

                // Create a new websocketClient object and add 
                // it to the list of connected clients
                websocket_client websocketClient = new websocket_client(this, clientSocket);
                _clients.Add(websocketClient);

                // Call the event when a websocketClient has connected to the listen server 
                if (OnClientConnected == null) throw new Exception("websocket_server error: event OnClientConnected is not bound!");
                OnClientConnected(this, new OnClientConnectedHandler(websocketClient));

                // Start to accept incomming connections again 
                GetSocket().BeginAccept(connectionCallback, null);

            }
            catch (Exception Exception)
            {
                Debug.WriteLine("An error has occured while trying to accept a connecting websocketClient.\n\n{0}", Exception.Message);
            }
        }

        /// <summary>Called when a message was recived, calls the OnMessageReceived event</summary>
        /// <param name="websocketClient">The websocketClient that sent the message</param>
        /// <param name="Message">The message that the websocketClient sent</param>
        public void ReceiveMessage(websocket_client websocketClient, string Message)
        {
            if (OnMessageReceived == null) throw new Exception("websocket_server error: event OnMessageReceived is not bound!");
            OnMessageReceived(this, new OnMessageReceivedHandler(websocketClient, Message));
        }

        /// <summary>Called when a websocketClient disconnectes, calls event OnClientDisconnected</summary>
        /// <param name="websocketClient">The websocketClient that disconnected</param>
        public void ClientDisconnect(websocket_client websocketClient)
        {
            // Remove the websocketClient from the connected clients list
            _clients.Remove(websocketClient);

            // Call the OnClientDisconnected event
            if (OnClientDisconnected == null) throw new Exception("websocket_server error: OnClientDisconnected is not bound!");
            OnClientDisconnected(this, new OnClientDisconnectedHandler(websocketClient));
        }

        #endregion

        #region websocket_server Events

        /// <summary>Send a message to a connected websocketClient</summary>
        /// <param name="websocketClient">The websocketClient to send the data to</param>
        /// <param name="Data">The data to send the websocketClient</param>
        public void SendMessage(websocket_client websocketClient, string Data)
        {
            // Create a websocket frame around the data to send
            byte[] frameMessage = Helpers.GetFrameFromString(Data);

            // Send the framed message to the in websocketClient
            websocketClient.GetSocket().Send(frameMessage);

            // Call the on send message callback event 
            if (OnSendMessage == null) throw new Exception("websocket_server error: event OnSendMessage is not bound!");
            OnSendMessage(this, new OnSendMessageHandler(websocketClient, Data));
        }

        /// <summary>Called after a message was sent</summary>
        public event EventHandler<OnSendMessageHandler> OnSendMessage;

        /// <summary>Called when a websocketClient was connected to the server (after handshake)</summary>
        public event EventHandler<OnClientConnectedHandler> OnClientConnected;

        /// <summary>Called when a message was received from a connected websocketClient</summary>
        public event EventHandler<OnMessageReceivedHandler> OnMessageReceived;

        /// <summary>Called when a websocketClient disconnected</summary>
        public event EventHandler<OnClientDisconnectedHandler> OnClientDisconnected;

        #endregion
    }
}
