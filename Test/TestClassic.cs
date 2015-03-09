#region Header
// Author : Nicolas GAUTIER
// Solution : Foxyt.vs13
// Project : FoxytNetworking
// File : TestClassic.cs
// Created at : 19/09/2014 14:09
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
using System.Text;
using System.Threading.Tasks;
using FoxytNetworking.Server.Factory;
using FoxytNetworking.Session;
using FoxytNetworking.Session.Processor;
using FoxytNetworking.Session.Protocol.SyncProtocol;
using FoxytNetworking.Tool.Logger;

#endregion

namespace FoxytNetworking.Test
{
    public class TestClassic
    {
        public static void Main()
        {
            Test1();
            Console.WriteLine("===========================================================");
            Test2();

            Console.ReadLine();
        }



        public static void Test1()
        {
            Server.Server server = new Server.Server(new GenericSessionFactory<Client>());

            server.Logger = new DebugLogger("Server1");
            server.OnSessionConnect += (sender, args) =>
            {
                (args.Session.DataProcessor as SimpleDataProcessor).OnDataReceived +=
                    (sessionSender, sessionArgs) =>
                    {
                        server.Logger.Trace("Recu :" + Encoding.UTF8.GetString(sessionArgs.Data.Array).Replace("\0", ""));
                    };
            };

            server.OnSessionDisconnect += (sender, args) => server.Logger.Info("OnDisconnect ");

            server.Start("127.0.0.1", 7777);

            Client connectClient = new Client();
            connectClient.OnDisconnect += (sender, args) => Console.WriteLine("Session | Disconnect");

            if (connectClient.Connect("127.0.0.1", 7777))
            {
                connectClient.SendMessage("Bonjour Test1");
            }

            System.Threading.Thread.Sleep(20);

            connectClient.Disconnect();

            server.Stop();
        }


        public static void Test2()
        {
            Server.Server server = new Server.Server(new GenericSessionFactory<ClientSync>());
            server.Logger = new DebugLogger("Server2");

            server.OnSessionConnect += (sender, args) =>
            {
                 (args.Session.DataProcessor as SyncProtocolDataProcessor).OnDataReceived +=
                                        (sessionSender, sessionArgs) =>
                                        {
                                            server.Logger.Trace("Recu :" + Encoding.UTF8.GetString(sessionArgs.Data.Data).Replace("\0", ""));

                                            ((ClientSync)sessionSender).SendAsynchroneMessage(
                                                Encoding.UTF8.GetBytes("Ceci est une reponse"), sessionArgs.Data);
                                        };
            };
            server.OnSessionDisconnect += (sender, args) => server.Logger.Info("OnDisconnect");

            server.Start("127.0.0.1", 7777);

            ClientSync connectClient = new ClientSync();
            connectClient.OnDisconnect += (sender, args) => Console.WriteLine("Session | Disconnect");

            if (connectClient.Connect("127.0.0.1", 7777))
            {
                Task<SyncProtocol> responseMsg = connectClient.SendSynchroneMessage(Encoding.UTF8.GetBytes("Ceci est un message"));
                responseMsg.Wait();

                if (responseMsg.Result.Data != null)
                {
                    Console.WriteLine(Encoding.UTF8.GetString(responseMsg.Result.Data).Replace("\0", ""));
                }
            }

            System.Threading.Thread.Sleep(20);

            connectClient.Disconnect();

            server.Stop();
        }
    }
}
