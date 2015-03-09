﻿#region Header
// Author : Nicolas GAUTIER
// Solution : Foxyt.vs13
// Project : FoxytNetworking
// File : NumberLimitConnectivityFilter.cs
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
namespace FoxytNetworking.Server.Filter
{
    public class NumberLimitConnectivityFilter : IConnectivityFilter
    {
        private readonly int connectionLimit;

        public NumberLimitConnectivityFilter(int connectionLimit)
        {
            //TODO : Mise en place d'un logger
            this.connectionLimit = connectionLimit;
        }

        public bool CanCreateSession(FoxytNetworking.Server.Server server, System.Net.Sockets.Socket sock)
        {
            return server.Data.ServerClients.Count + 1 < this.connectionLimit;
        }
    }
}
