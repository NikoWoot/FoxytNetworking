#region Header
// Author : Nicolas GAUTIER
// Solution : Foxyt.vs13
// Project : FoxytNetworking
// File : DefaultDataProcessor.cs
// Created at : 29/09/2014 16:36
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
using FoxytNetworking.Event;

#endregion

namespace FoxytNetworking.Session.Processor
{
    public abstract class DefaultDataProcessor<T> : IDataProcessor
    {
        #region Events

        public event EventHandler<SessionDataReceivedArgs<T>> OnDataReceived;

        #endregion

        #region Fields

        protected readonly Session session;

        #endregion

        #region Constructor

        public DefaultDataProcessor(Session session)
        {
            this.session = session;
        }

        #endregion

        #region Abstract Methods
        public abstract void Receive(ArraySegment<byte> data);

        #endregion

        #region Methods

        protected void RaiseDataReceived(T data)
        {
            if (this.OnDataReceived != null)
                this.OnDataReceived(this, new SessionDataReceivedArgs<T>(this.session, data));
        }

        #endregion

    }
}
