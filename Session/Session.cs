#region Header
// Author : Nicolas GAUTIER
// Solution : Foxyt.vs13
// Project : FoxytNetworking
// File : Session.cs
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
using System.Net.Sockets;
using System.Text;
using FoxytNetworking.Event;
using FoxytNetworking.Session.Processor;
using FoxytNetworking.Tool.Logger;

#endregion

namespace FoxytNetworking.Session
{
    public class Session
    {
        private const int DEFAULT_BUFFERSIZE = 3000;

        #region Events

        public event EventHandler<SessionDisconnectArgs> OnDisconnect;

        #endregion

        #region Fields

        protected IDataProcessor _DataProcessor;
        protected int _BufferSize;

        #endregion

        #region Properties

        protected object SenderLocker { get; private set; }
        internal object ReceiverLocker { get; private set; }
        internal object CloserLocker { get; private set; }
        internal object OpenLocker { get; private set; }

        public int BufferSize
        {
            get { return this._BufferSize; }
            set
            {
                if (value == this._BufferSize)
                    return;

                this._BufferSize = value;
                this.BufferData = new byte[value];

                if (this.ClientSocket != null)
                {
                    this.ClientSocket.SendBufferSize = value;
                    this.ClientSocket.ReceiveBufferSize = value;
                }
            }
        }

        internal DataSession SessionData { get; private set; }

        protected ILogger Logger { get; set; }


        public byte[] BufferData { get; set; }

        public int DisconnectTimeout { get; set; }

        internal Socket ClientSocket { get; set; }

        public IDataProcessor DataProcessor
        {
            get { return this._DataProcessor; }
        }

        public T GetTypedDataProcessor<T>()
            where T : class, IDataProcessor
        {
            return this.DataProcessor as T;
        }

        public int ClientID
        {
            get { return this.SessionData.UniqueID; }
        }

        public virtual string IP
        {
            get { return this.SessionData.RemoteIP; }
        }

        public int Port
        {
            get { return this.SessionData.RemotePort; }
        }

        public virtual bool Connected
        {
            get { return this.ClientSocket != null && this.ClientSocket.Connected; }
        }

        #endregion

        #region Constructeur

        public Session()
        {
            this.SessionData = new DataSession();
            this.BufferSize = DEFAULT_BUFFERSIZE;
            this._DataProcessor = new SimpleDataProcessor(this);

            this.ReceiverLocker = new object();
            this.SenderLocker = new object();
            this.CloserLocker = new object();
            this.OpenLocker = new object();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Fonction permettant la deconnection
        /// </summary>
        public virtual void Disconnect(bool silent = false)
        {
            if (this.ClientSocket == null)
                return;

            lock (this.CloserLocker)
            {
                if (this.ClientSocket == null)
                    return;

                this.ClientSocket.Close(this.DisconnectTimeout);
                this.ClientSocket = null;

                if (!silent && this.OnDisconnect != null)
                    this.OnDisconnect(this, new SessionDisconnectArgs(this));
            }
        }

        #endregion

        #region Send Message
        /// <summary>
        /// Fonction d'envoi d'un message sous forme de byte[]
        /// </summary>
        /// <param name="messageToSend"></param>
        public void SendMessage(byte[] messageToSend)
        {
            try
            {
                int size = 0;

                lock (this.SenderLocker)
                {
                    if (this.ClientSocket != null && this.ClientSocket.Connected)
                    {
                        size = this.ClientSocket.Send(messageToSend);
                    }
                }

                if (size == 0)
                {
                    if (this.Logger != null)
                        this.Logger.Trace("SendMessage => Deconnexion");

                    this.Disconnect();
                }
            }
            catch (SocketException ex)
            {
                if (this.Logger != null)
                    this.Logger.Error("SendMessage => " + ex.ToString());

                this.Disconnect();
            }
        }

        public void SendMessage(string mes)
        {
            this.SendMessage(Encoding.UTF8.GetBytes(mes));
        }
        #endregion

    }
}
