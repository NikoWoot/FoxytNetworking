#region Header
// Author : Nicolas GAUTIER
// Solution : Foxyt.vs13
// Project : FoxytNetworking
// File : Server.cs
// Created at : 29/09/2014 16:00
// 
// CC BY-NC-ND 4.0
// http://creativecommons.org/licenses/by-nc-nd/4.0/deed.fr
// 
// Cette oeuvre, création, site ou texte est sous 
// licence Creative Commons  Attribution - Pas d'Utilisation Commerciale - Pas de Modification 4.0 International.
// Pour accéder à une copie de cette licence, 
// merci de vous rendre à l'adresse suivante http://creativecommons.org/licenses/by-nc-nd/4.0/deed.fr 
// ou envoyez un courrier à Creative Commons, 444 Castro Street, Suite 900, Mountain View, California, 94041, USA.
#endregion
#region Directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using FoxytNetworking.Event;
using FoxytNetworking.Server.Factory;
using FoxytNetworking.Server.Filter;
using FoxytNetworking.Session;
using FoxytNetworking.Tool.Logger;

#endregion

namespace FoxytNetworking.Server
{
    public class Server
    {
        #region Events
        public event EventHandler<SessionConnectArgs> OnSessionConnect;
        public event EventHandler<SessionDisconnectArgs> OnSessionDisconnect;
        #endregion

        #region Fields

        protected Socket _Socket;

        #endregion

        #region Properties
        internal DataServer Data { get; set; }

        protected ISessionFactory SessionFactory { get; set; }

        public ILogger Logger { get; set; }
        public IList<IConnectivityFilter> ConnectivityFilters { get; private set; }

        public IEnumerable<Session.Session> Clients
        {
            get
            {
                return this.Data.ServerClients.Values.AsEnumerable();
            }
        }

        #endregion

        #region Constructors

        public Server(ISessionFactory sessionFactory)
        {
            this.Data = new DataServer
            {
                PendingQueueLimit = ConstantsServer.DEFAULT_PENDINGQUEUE
            };

            this.ConnectivityFilters = new List<IConnectivityFilter>();
            this.SessionFactory = sessionFactory;
        }

        public Server()
            : this(new GenericSessionFactory<Session.Session>())
        {
        }

        #endregion

        #region Methods Start-Stop
        /// <summary>
        /// Mise en ecoute du serveur
        /// </summary>
        public void Start(string listenIP, int listenPort)
        {
            this.Start(listenIP == IPAddress.None.ToString()
                    ? new IPEndPoint(IPAddress.Any, listenPort)
                    : new IPEndPoint(IPAddress.Parse(listenIP), listenPort));
        }

        /// <summary>
        /// Mise en ecoute du serveur
        /// </summary>
        public void Start(IPEndPoint endPoint)
        {
            try
            {
                this.Data.ListenIP = endPoint.Address.ToString();
                this.Data.ListenPort = endPoint.Port;

                this._Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this._Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                this._Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);

                this._Socket.Bind(endPoint);

                this._Socket.Listen(this.Data.PendingQueueLimit);

                this._Socket.BeginAccept(this.OnSessionConnectCallback, null);
            }
            catch (SocketException e)
            {
                string msgError = "Impossible de demarrer le serveur " + e.ToString();
                if (this.Logger != null)
                    this.Logger.Error(msgError);

                throw new Exception(msgError, e);
            }
        }

        /// <summary>
        /// Method d'arret du serveur
        /// </summary>
        public void Stop()
        {
            if (this._Socket != null)
            {
                this._Socket.Close();
                this._Socket.Dispose();
                this._Socket = null;
            }

            this.CloseAllSockets();
        }
        #endregion

        #region Methods - Socket Management

        private void CloseSocket(Session.Session dataSender, bool silent = false)
        {
            int cliID = -1;

            if (dataSender == null)
                return;

            lock (dataSender.CloserLocker)
            {
                cliID = dataSender.SessionData.UniqueID;

                if (dataSender.ClientSocket == null)
                {
                    if (Logger != null)
                        Logger.Warn("Fermeture d'un socket avec ClientSocket == null");
                    return;
                }


                if (Logger != null)
                    Logger.Trace("CloseSocket => Fermeture d'un socket");

                try
                {
                    if (!silent && this.OnSessionDisconnect != null)
                        this.OnSessionDisconnect(this, new SessionDisconnectArgs(dataSender));
                }
                catch (Exception ex)
                {
                    if (Logger != null)
                        Logger.Trace("CloseSocket => Event - Exception" + ex.ToString());
                }

                this.SessionFactory.DestroySession(dataSender);
            }


            if (cliID != -1)
            {
                this.Data.ServerClients.Remove(cliID);
            }
        }

        /// <summary>
        /// Fermeture du serveur avec la methode de fermeture de toute les sockets
        /// </summary>
        public void CloseAllSockets(bool silent = false)
        {
            IEnumerable<Session.Session> clients = this.Clients.ToList();
            foreach (Session.Session client in clients)
            {
                this.CloseSocket(client, silent);
            }
        }

        #endregion

        #region Callbacks Methods

        private void OnSessionConnectCallback(IAsyncResult asyn)
        {
            Socket newSock = null;
            Session.Session dataSender = null;
            bool silentClose = true;

            try
            {
                newSock = this._Socket.EndAccept(asyn);
                newSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);

                //TODO : Relance si serveur est encore en cours (normalement exception avant)
                //Redemarrage de l'ecoute des connexions (SEULEMENT ICI car sinon probleme de MultiThread possible)
                this._Socket.BeginAccept(this.OnSessionConnectCallback, null);

                //Si je peux pas accepter, je le kick direct
                if (!this.ConnectivityFilters.All(c => c.CanCreateSession(this, newSock)))
                {
                    newSock.Disconnect(true);
                    return;
                }

                //Création du client
                dataSender = this.SessionFactory.CreateSession(newSock);

                lock (dataSender.OpenLocker)
                {                
                    //Ajout dans la collection de client
                    this.Data.ServerClients.Add(dataSender.SessionData.UniqueID, dataSender);

                    // En cas d'erreur sur l'ecoute, sur l'event de connexion, redeclenche l'event de deconnexion
                    silentClose = false;

                    //Lance l'ecoute de la session
                    dataSender.ClientSocket.BeginReceive(dataSender.BufferData, 0, dataSender.BufferSize, SocketFlags.None,
                        this.OnReceiveDataCallback, dataSender);

                    //Lancement de l'event de connexion
                    if (this.OnSessionConnect != null)
                        this.OnSessionConnect(this, new SessionConnectArgs(dataSender));
                }
            }
            catch (ObjectDisposedException ode)
            {
                if (this.Logger != null)
                    this.Logger.Trace("OnConnect => Fermeture du socket par le serveur" + ode.ToString());

                this.CloseSocket(dataSender, silentClose);
            }
            catch (Exception ex)
            {
                if (this.Logger != null)
                    this.Logger.Error("OnConnect => Fermeture du socket par le client" + ex.ToString());

                this.CloseSocket(dataSender, silentClose);
            }
        }


        private void OnReceiveDataCallback(IAsyncResult asyn)
        {
            //Recuperation du client
            Session.Session dataSender = asyn.AsyncState as Session.Session;

            try
            {
                //Recuperation des données
                int? sizeOfReceive = SessionDataReceiver.Receive(asyn, this.OnReceiveDataCallback, this.Logger);

                //Sortie si null
                if (sizeOfReceive == null)
                {
                    if (this.Logger != null)
                        this.Logger.Trace("Datasender est null");
                }
                else if (sizeOfReceive == 0)
                    this.CloseSocket(dataSender);
            }
            catch (SocketException sockEx)
            {
                switch (sockEx.SocketErrorCode)
                {
                    case SocketError.TimedOut:
                    case SocketError.WouldBlock:
                    case SocketError.ConnectionReset:
                    case SocketError.ConnectionAborted:
                        this.CloseSocket(dataSender);
                        break;

                    default:
                        if (Logger != null)
                            Logger.Trace(String.Format("OnReceive => Erreur socket non traité (NativeErrorCode : {0})",
                                sockEx.NativeErrorCode));
                        break;
                }
            }
            catch (ObjectDisposedException odException)
            {
                if (this.Logger != null)
                    this.Logger.Trace("OnReceive => Fermeture du socket Ode");

                this.CloseSocket(dataSender);
            }
            catch (Exception ex)
            {
                if (this.Logger != null)
                    this.Logger.Trace("OnReceive => Fermeture du socket Exception : " + ex.ToString());

                this.CloseSocket(dataSender);
            }

        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            this.Stop();
        }
        #endregion

    }
}
