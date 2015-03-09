#region Header
// Author : Nicolas GAUTIER
// Solution : Foxyt.vs13
// Project : FoxytNetworking
// File : IUIDGenerator.cs
// Created at : 19/09/2014 12:10
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
namespace FoxytNetworking.Tool.UID
{
    public interface IUIDGenerator
    {
        int GetUniqueID();
        void FreeUniqueID(int id);
    }
}
