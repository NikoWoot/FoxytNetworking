#region Header
// Author : Nicolas GAUTIER
// Solution : Foxyt.vs13
// Project : FoxytNetworking
// File : ClientSync.cs
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

using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using FoxytNetworking.Exceptions;
using FoxytNetworking.Tool.UID;

#endregion

namespace FoxytNetworking.Session.Protocol.SyncProtocol
{
    public class ClientSync : Client
    {
        private const int DEFAULT_SYNCLIENT_BUFFERSIZE = 6000;

        #region Fields

        protected bool serverSide = true;
        protected BinaryFormatter binFormat;
        private readonly List<SyncProtocol> standByMessages;

        #endregion

        #region Properties

        public UniqueIDGenerator UIdGenerator { get; protected set; }

        public IList<SyncProtocol> StandByMessages
        {
            get
            {
                return this.standByMessages;
            }
        }

        #endregion

        #region Constructors
        public ClientSync()
        {
            this.BufferSize = DEFAULT_SYNCLIENT_BUFFERSIZE;

            this.standByMessages = new List<SyncProtocol>();

            //Création du generateur de nombre unique pour les messages de protocol
            this.UIdGenerator = new UniqueIDGenerator();
            this.binFormat = new BinaryFormatter();
            this._DataProcessor = new SyncProtocolDataProcessor(this);
        }

        #endregion

        #region Methods

        public override bool Connect(string ip, int port)
        {
            this.serverSide = false;
            return base.Connect(ip, port);
        }

        private void SendData(byte[] messageToSend, SyncProtocol ihp)
        {
            //Recuperation de l'entete sous forme de Byte[]
            byte[] bufferHeader = ihp.HeaderToBytes();

            //Copie dans le tableau final de l'entete puis du message
            byte[] buffer = new byte[messageToSend.Length + ihp.GetHeaderLength()];
            if (messageToSend.Length > this.BufferSize)
                throw new BufferSizeException("Taille du message > au buffer ( {0} )", messageToSend.Length);

            bufferHeader.CopyTo(buffer, 0);
            messageToSend.CopyTo(buffer, bufferHeader.Length);

            //Envoi du message dans le tunnel TCP
            this.SendMessage(buffer);
        }

        #region SendSynchroneMessage


        public Task<SyncProtocol> SendSynchroneMessage(byte[] data, SyncProtocol ihp = null)
        {
            return Task<SyncProtocol>.Factory.StartNew(() =>
            {
                if (ihp == null)
                    ihp = new SyncProtocol { MessageID = this.UIdGenerator.GetUniqueID(), ServerSide = this.serverSide };

                //Ajout du packet à la liste des packets en attente
                this.StandByMessages.Add(ihp);

                //Envoi du message via le protocol definit
                this.SendData(data, ihp);

                //Interruption du Thread pour attendre la reponse
                ihp.WaitPacket();

                //Suppresion dans la liste
                this.StandByMessages.Remove(ihp);

                //Liberation de l'ID unique
                this.UIdGenerator.FreeUniqueID(ihp.MessageID);

                return ihp;
            });
        }

        public Task<Y> SendSynchroneMessage<Y>(byte[] data, SyncProtocol ihp = null)
            where Y : class
        {
            if (!typeof(Y).IsSerializable)
                throw new System.Exception("Type non serializable pour Y");

            // -> Retourne le flux sous la forme d'un tableau d'octets
            return this.SendSynchroneMessage(data, ihp).ContinueWith(task =>
            {
                if (task.Result == null)
                    return null;

                // -> Création d'un flux memoire pour contenir l'objet
                MemoryStream ms = new MemoryStream(task.Result.Data);

                object obj = this.binFormat.Deserialize(ms);
                Y objForReturn = (obj as Y);

                return objForReturn;
            });
        }

        public Task<SyncProtocol> SendSynchroneMessage<V>(V data, SyncProtocol ihp = null)
            where V : class
        {
            if (!typeof(V).IsSerializable)
                throw new System.Exception("Type non serializable pour V");

            // -> Création d'un flux memoire pour contenir l'objet
            MemoryStream ms = new MemoryStream();

            // -> Serialize l'objet dans le flux memoire
            this.binFormat.Serialize(ms, data);

            return this.SendSynchroneMessage(ms.ToArray(), ihp);
        }

        public Task<Y> SendSynchroneMessage<V, Y>(V data, SyncProtocol ihp = null)
            where V : class
            where Y : class
        {
            if (!typeof(V).IsSerializable || !typeof(Y).IsSerializable)
                throw new System.Exception("Type non serializable pour V ou Y");

            // -> Retourne le flux sous la forme d'un tableau d'octets
            return this.SendSynchroneMessage(data, ihp).ContinueWith(task =>
            {
                if (task.Result == null)
                    return null;

                // -> Création d'un flux memoire pour contenir l'objet
                MemoryStream ms = new MemoryStream(task.Result.Data);

                object obj = this.binFormat.Deserialize(ms);
                Y objForReturn = (obj as Y);

                return objForReturn;
            });
        }

        #endregion


        public void SendAsynchroneMessage(byte[] data, SyncProtocol ihp = null)
        {
            if (ihp == null)
                ihp = new SyncProtocol { MessageID = -1, ServerSide = this.serverSide };

            //Envoi du message via le protocol definit
            this.SendData(data, ihp);
        }

        #endregion

    }
}
