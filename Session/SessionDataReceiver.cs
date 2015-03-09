#region Header
// Author : Nicolas GAUTIER
// Solution : Foxyt.vs13
// Project : FoxytNetworking
// File : SessionDataReceiver.cs
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
using FoxytNetworking.Tool.Logger;

#endregion

namespace FoxytNetworking.Session
{
    internal static class SessionDataReceiver
    {


        internal static int? Receive(IAsyncResult asyn, AsyncCallback callback, ILogger logger = null)
        {
            //Recuperation du client
            Session dataSender = asyn.AsyncState as Session;

            //Sortie si null
            if (dataSender == null)
                return null;

            //Recuperation des données
            int sizeOfReceive = 0;

            lock (dataSender.OpenLocker) //Obtient le verrouillage de l'ecoute tant que la création complete n'est pas faite
            {
                lock (dataSender.CloserLocker) //Obtient le verrouillage de la fermeture du socket pendant la réception
                {
                    //Obtient la reception des données
                    lock (dataSender.ReceiverLocker)
                    {
                        if (dataSender.ClientSocket != null)
                        {
                            sizeOfReceive = dataSender.ClientSocket.EndReceive(asyn);
                        }
                        //else if (logger != null)
                        //{
                        //    logger.Warn("SessionDataReceive => sizeOfReceive = 0 car ClientSocket = null ");
                        //}

                        //else if (sizeOfReceive == dataSender.BufferSize) //TODO : Attente de la suite du packet
                        //{
                        //}
                        if (sizeOfReceive > 0)
                        {
                            //Copie des données dans un nouveau tableau
                            byte[] tabBuffer = new byte[sizeOfReceive];

                            //Copie des données dans un nouveau tableau
                            Buffer.BlockCopy(dataSender.BufferData, 0, tabBuffer, 0, sizeOfReceive);

                            //Mise en place d'un nouveau tableau pour la reception
                            dataSender.BufferData = new byte[dataSender.BufferSize];

                            if (dataSender.ClientSocket != null)
                            {
                                //Relance l'ecoute de data
                                dataSender.ClientSocket.BeginReceive(dataSender.BufferData, 0, dataSender.BufferSize,
                                    SocketFlags.None,
                                    callback, dataSender);
                            }

                            //Appelle fonction maitresse pour la reception
                            dataSender.DataProcessor.Receive(new ArraySegment<byte>(tabBuffer));

                            //TODO : Voir pour completer le buffer de data ici
                        }
                    }
                }
            }

            return sizeOfReceive;
        }
    }
}
