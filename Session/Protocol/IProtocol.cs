#region Header
// Author : Nicolas GAUTIER
// Solution : Foxyt.vs13
// Project : FoxytNetworking
// File : IProtocol.cs
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

#endregion

namespace FoxytNetworking.Session.Protocol
{
    public interface IProtocol
    {
        Int32 MessageID { get;  set; }
        bool ServerSide { get; set; }
        byte[] Data { get;  set; }


        /// <summary>
        /// Renvoi la taille de l'entete actuel
        /// </summary>
        int GetHeaderLength();

        /// <summary>
        /// Conversion du Header du packet en bytes
        /// </summary>
        /// <returns></returns>
        byte[] HeaderToBytes();

        /// <summary>
        /// Methode utilisé pour via les données, crée le packet
        /// </summary>
        /// <param name="data">Données recu</param>
        void CreateHeader(byte[] data);

        /// <summary>
        /// Fonction indiquant à l'Event qu'il est fini de remplir et que je peux continuer le thread
        /// </summary>
        /// <param name="data"></param>
        void CompleteHeader(byte[] data);

    }
}
