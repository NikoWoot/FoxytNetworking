#region Header
// Author : Nicolas GAUTIER
// Solution : Foxyt.vs13
// Project : FoxytNetworking
// File : BytesWriter.cs
// Created at : 29/09/2014 16:33
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
using System.IO;
using System.Text;
using FoxytNetworking.Stream.Message;

#endregion

namespace FoxytNetworking.Stream.IO
{
    public class BytesWriter
    {
        private readonly MemoryStream stream;

        public BytesWriter()
        {
            this.stream = new MemoryStream();
        }

        public void Write(byte value) { this.stream.Write(BitConverter.GetBytes(value), 0, sizeof(byte)); }
        public void Write(byte[] value) { this.stream.Write(value, 0, value.Length); }
        public void Write(bool value) { this.stream.Write(BitConverter.GetBytes(value), 0, sizeof(bool)); }
        public void Write(char value) { this.stream.Write(BitConverter.GetBytes(value), 0, sizeof(char)); }
        public void Write(double value) { this.stream.Write(BitConverter.GetBytes(value), 0, sizeof(double)); }
        public void Write(float value) { this.stream.Write(BitConverter.GetBytes(value), 0, sizeof(float)); }
        public void Write(int value) { this.stream.Write(BitConverter.GetBytes(value), 0, sizeof(int)); }
        public void Write(long value) { this.stream.Write(BitConverter.GetBytes(value), 0, sizeof(long)); }
        public void Write(short value) { this.stream.Write(BitConverter.GetBytes(value), 0, sizeof(short)); }
        public void Write(uint value) { this.stream.Write(BitConverter.GetBytes(value), 0, sizeof(uint)); }
        public void Write(ulong value) { this.stream.Write(BitConverter.GetBytes(value), 0, sizeof(ulong)); }
        public void Write(ushort value) { this.stream.Write(BitConverter.GetBytes(value), 0, sizeof(ushort)); }

        public void Write(string value)
        {
            byte[] array = Encoding.UTF8.GetBytes(value);
            this.Write(array.Length); //Ecriture de la taille de la chaine
            this.stream.Write(array, 0, array.Length); //Ecriture de la chaine
        }

        public void Write(IMessage message)
        {
            byte[] array = message.MessageToBytes();
            this.Write(array.Length);
            this.stream.Write(array, 0, array.Length); //Ecriture du message
        }

        public byte[] ToArray()
        {
            return this.stream.ToArray();
        }

        public long Length
        {
            get { return this.stream.Length; }
        }   
    }
}
