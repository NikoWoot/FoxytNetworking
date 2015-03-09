#region Header
// Author : Nicolas GAUTIER
// Solution : Foxyt.vs13
// Project : FoxytNetworking
// File : SyncProtocol.cs
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
using System.Threading;
using FoxytNetworking.Stream.IO;

#endregion

namespace FoxytNetworking.Session.Protocol.SyncProtocol
{
    /// <summary>
    /// Classe utilisé pour taggé un packet de facon simple à travers un reseau
    /// Ce type de message ne contient que le strict necessaire
    /// </summary>
    public class SyncProtocol : IProtocol
    {
        public const int DEFAULT_TIMEOUT = 1000;

        #region Properties
        public Int32 MessageID { get; set; }
        public bool ServerSide { get; set; }
            

        public byte[] Data { get; set; }

        private ManualResetEvent Event { get; set; }
        private int TimeoutMessage { get; set; }
        #endregion

        #region Constructors
        public SyncProtocol()
        {
            this.TimeoutMessage = DEFAULT_TIMEOUT;
            this.Event = new ManualResetEvent(false);
        }
        #endregion

        #region IProtocol Membres

        /// <summary>
        /// Fonction permettant de convertir mon entete sous forme de tableau de byte[] pour etre envoyer a travers le reseau
        /// </summary>
        /// <returns></returns>
        public byte[] HeaderToBytes()
        {
            //Ecriture des données
            BytesWriter writer = new BytesWriter();

            writer.Write(this.MessageID);
            writer.Write(this.ServerSide);

            return writer.ToArray();
        }


        /// <summary>
        /// Fonction permettant d'extraire les données de ma classe à partir d'infos en bytes
        /// </summary>
        /// <param name="data"></param>
        public void CreateHeader(byte[] data)
        {
            //Decoupage des données
            BytesReader reader = new BytesReader(data);
            
            this.MessageID = reader.ReadInt();
            this.ServerSide = reader.ReadBool();
            this.Data = reader.ToArray();
        }


        /// <summary>
        /// Renvoi la taille de mon Header pour le decouper dans les data de mon packet
        /// </summary>
        /// <returns></returns>
        public int GetHeaderLength()
        {
            return sizeof(Int32) + sizeof(bool);
        }


        /// <summary>
        /// Fonction utilisé pour stopé l'execution du Thread pendant {TIMEOUT} ms
        /// </summary>
        public bool WaitPacket()
        {
            return this.Event.WaitOne(this.TimeoutMessage, true);
        }

        /// <summary>
        /// Fonction indiquant à l'Event qu'il est fini de remplir et que je peux continuer le thread
        /// </summary>
        /// <param name="data"></param>
        public void CompleteHeader(byte[] data)
        {
            this.Data = data;

            //Indique que le packet est complet
            this.Event.Set();
        }
        #endregion
    }
}
