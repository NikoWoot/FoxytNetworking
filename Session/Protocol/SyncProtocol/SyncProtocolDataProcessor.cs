#region Header
// Author : Nicolas GAUTIER
// Solution : Foxyt.vs13
// Project : FoxytNetworking
// File : SyncProtocolDataProcessor.cs
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
using System.Linq;
using FoxytNetworking.Session.Processor;

#endregion

namespace FoxytNetworking.Session.Protocol.SyncProtocol
{
    public class SyncProtocolDataProcessor : DefaultDataProcessor<SyncProtocol>
    {
        protected ClientSync SessionSync { get { return this.session as ClientSync; } }

        public SyncProtocolDataProcessor(ClientSync session)
            : base(session)
        {
        }

        /// <summary>
        /// Fonction de traitement des données en fonction de leurs arrivés, leurs types et leurs comportements
        /// </summary>
        public override void Receive(ArraySegment<byte> data)
        {
            //Recréation d'un message
            SyncProtocol ihp = new SyncProtocol();
            ihp.CreateHeader(data.Array);

            SyncProtocol message = null;

            // -1 Code Asynchrone
            if (ihp.MessageID != -1)
            {
                //Recherche dans la liste si il y avait une demande
                message = this.SessionSync.StandByMessages.FirstOrDefault(p => p.MessageID == ihp.MessageID && p.ServerSide == ihp.ServerSide);
            }

            if (message != null) //Si le message est attendu et synchrone
                message.CompleteHeader(ihp.Data);
            else
                this.RaiseDataReceived(ihp);
        }


    }
}
