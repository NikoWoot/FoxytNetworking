#region Header
// Author : Nicolas GAUTIER
// Solution : Foxyt.vs13
// Project : FoxytNetworking
// File : GenericSessionFactory.cs
// Created at : 29/09/2014 16:30
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
using FoxytNetworking.Tool.UID;

#endregion

namespace FoxytNetworking.Server.Factory
{
    public class GenericSessionFactory<T> : ISessionFactory
        where T : Session.Session, new()
    {
        private const int DEFAULT_BUFFERSIZE = 3000;

        protected IUIDGenerator UidGenerator { get; set; }

        public GenericSessionFactory()
        {
           this.UidGenerator = new UniqueIDGenerator(); 
        }

        public Session.Session CreateSession(Socket newSock)
        {
            T typedSession = new T();
            typedSession.SessionData.UniqueID = this.UidGenerator.GetUniqueID();
            typedSession.ClientSocket = newSock;
            typedSession.BufferSize = DEFAULT_BUFFERSIZE;

            IPEndPoint remoteEndPoint = (newSock.RemoteEndPoint as IPEndPoint);
            typedSession.SessionData.RemoteIP = remoteEndPoint.Address.ToString();
            typedSession.SessionData.RemotePort = remoteEndPoint.Port;

            return typedSession;
        }

        public void DestroySession(Session.Session session)
        {
            T typedSession = session as T;

            try
            {
                typedSession.ClientSocket.Close();
                typedSession.ClientSocket.Dispose();
                typedSession.ClientSocket = null;
            }
            catch (Exception ex)
            {
                throw new Exception("CloseSocket => ClientSocket-Exception ", ex);
            }

            if (typedSession.ClientID != -1)
                this.UidGenerator.FreeUniqueID(typedSession.ClientID);  
        }
    }
}
