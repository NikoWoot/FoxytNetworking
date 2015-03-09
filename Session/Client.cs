#region Header
// Author : Nicolas GAUTIER
// Solution : Foxyt.vs13
// Project : FoxytNetworking
// File : Client.cs
// Created at : 29/09/2014 16:37
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
using System.Net;
using System.Net.Sockets;
using FoxytNetworking.Event;

#endregion

namespace FoxytNetworking.Session
{
    public class Client : Session
    {
        #region Events

        public event EventHandler<SessionConnectArgs> OnConnect;

        #endregion

        #region Methods

        /// <summary>
        /// Fonction permettant la connection du client à un serveur
        /// </summary>
        /// <param name="ip">IP du serveur</param>
        /// <param name="port">Port du serveur</param>
        public virtual bool Connect(string ip, int port)
        {
            return this.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        /// <summary>
        /// Fonction permettant la connection du client à un serveur
        /// </summary>
        public virtual bool Connect(IPEndPoint remoteEndPoint)
        {
            try
            {
                lock (this.OpenLocker)
                {
                    this.SessionData.RemoteIP = remoteEndPoint.Address.ToString();
                    this.SessionData.RemotePort = remoteEndPoint.Port;

                    this.ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    this.ClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    this.ClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);

                    this.ClientSocket.Connect(remoteEndPoint);

                    this.ClientSocket.BeginReceive(this.BufferData, 0, this.BufferSize, SocketFlags.None, this.OnReceiveDataCallback, this);

                    if (this.OnConnect != null)
                        this.OnConnect(this, new SessionConnectArgs(this));

                    return true;
                }
            }
            catch (Exception ex)
            {
                if (this.Logger != null)
                    this.Logger.Error("ClientConnect error " + ex.ToString());

                return false;
            }
        }
        #endregion

        #region Callbacks
        //TODO : Refactoring avec la même méthode disponible dans AbstractServer
        private void OnReceiveDataCallback(IAsyncResult asyn)
        {
            //Recuperation du client
            Session dataSender = asyn.AsyncState as Session;
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
                    this.Disconnect();
            }

            catch (SocketException sockEx)
            {
                switch (sockEx.SocketErrorCode)
                {
                    case SocketError.TimedOut:
                    case SocketError.WouldBlock:
                    case SocketError.ConnectionReset:
                    case SocketError.ConnectionAborted:
                        this.Disconnect();
                        break;

                    default:
                        if (this.Logger != null)
                            this.Logger.Trace(String.Format("OnReceive => Erreur socket non traité (NativeErrorCode : {0})",
                                sockEx.NativeErrorCode));
                        break;
                }
            }
            catch (ObjectDisposedException odException)
            {
                if (this.Logger != null)
                    this.Logger.Trace("OnReceive => Fermeture du socket Ode");

                this.Disconnect();
            }
            catch (Exception ex)
            {
                if (this.Logger != null)
                    this.Logger.Trace("OnReceive => Fermeture du socket Exception : " + ex.ToString());

                this.Disconnect();
            }

        }
        #endregion
    }
}
