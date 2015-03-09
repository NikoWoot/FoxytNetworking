#region Header
// Author : Nicolas GAUTIER
// Solution : Foxyt.vs13
// Project : FoxytNetworking
// File : DebugLogger.cs
// Created at : 19/09/2014 15:37
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

namespace FoxytNetworking.Tool.Logger
{
    public class DebugLogger : ILogger
    {
        private string name;

        public DebugLogger(string name)
        {
            this.name = name;
        }

        public void Debug(string message)
        {
            Console.WriteLine(name + " " + message);
        }

        public void Info(string message)
        {
            Console.WriteLine(name + " " + message);
        }

        public void Succes(string message)
        {
            Console.WriteLine(name + " " + message);
        }

        public void Error(string message)
        {
            Console.WriteLine(name + " " + message);
        }

        public void Fatal(string message)
        {
            Console.WriteLine(name + " " + message);
        }

        public void Trace(string message)
        {
            Console.WriteLine(name + " " + message);
        }

        public void Warn(string message)
        {
            Console.WriteLine(name + " " + message);
        }

        public void Log(string message)
        {
            Console.WriteLine(name + " " + message);
        }
    }
}
