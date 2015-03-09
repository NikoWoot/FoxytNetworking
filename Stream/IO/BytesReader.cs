#region Header
// Author : Nicolas GAUTIER
// Solution : Foxyt.vs13
// Project : FoxytNetworking
// File : BytesReader.cs
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
using FoxytNetworking.Exceptions;
using FoxytNetworking.Stream.Message;

#endregion

namespace FoxytNetworking.Stream.IO
{
    public class BytesReader
    {
        private const string BufferSizeExceptionMessage = "Impossible de lire des données dans un buffer entierement lu";

        private int position;   
        private readonly ArraySegment<byte> buffer;

        public BytesReader(ArraySegment<byte> data)
        {
            this.buffer = data;
        }

        public BytesReader(byte[] data)
        {
            this.buffer = new ArraySegment<byte>(data);
        }

        public BytesReader(MemoryStream stream)
            : this(stream.ToArray())
        {
        }

        public byte ReadByte()
        {
            const int size = sizeof(byte);
            if (this.Length < size)
                throw new BufferSizeException(BufferSizeExceptionMessage);

            byte[] lastData = new byte[size];
            Array.Copy(this.buffer.Array, this.position, lastData, 0, size);

            this.position += size;
            return lastData[0];
        }

        public int ReadInt()
        {
            const int size = sizeof(int);
            if (this.Length < size)
                throw new BufferSizeException(BufferSizeExceptionMessage);

            int value = BitConverter.ToInt32(this.buffer.Array, this.position);
            this.position += size;
            return value;
        }

        public bool ReadBool()
        {
            const int size = sizeof(bool);
            if (this.Length < size)
                throw new BufferSizeException(BufferSizeExceptionMessage);

            bool value = BitConverter.ToBoolean(this.buffer.Array, this.position);
            this.position += size;
            return value;
        }
        public char ReadChar()
        {
            const int size = sizeof(char);
            if (this.Length < size)
                throw new BufferSizeException(BufferSizeExceptionMessage);

            char value = BitConverter.ToChar(this.buffer.Array, this.position);
            this.position += size;
            return value;
        }

        public double ReadDouble()
        {
            const int size = sizeof(double);
            if (this.Length < size)
                throw new BufferSizeException(BufferSizeExceptionMessage);

            double value = BitConverter.ToDouble(this.buffer.Array, this.position);
            this.position += size;
            return value;
        }
        public float ReadFloat()
        {
            const int size = sizeof(float);
            if (this.Length < size)
                throw new BufferSizeException(BufferSizeExceptionMessage);

            float value = BitConverter.ToSingle(this.buffer.Array, this.position);
            this.position += size;
            return value;
        }

        public long ReadLong()
        {
            const int size = sizeof(long);
            if (this.Length < size)
                throw new BufferSizeException(BufferSizeExceptionMessage);

            long value = BitConverter.ToInt64(this.buffer.Array, this.position);
            this.position += size;
            return value;
        }

        public short ReadShort()
        {
            const int size = sizeof(short);
            if (this.Length < size)
                throw new BufferSizeException(BufferSizeExceptionMessage);

            short value = BitConverter.ToInt16(this.buffer.Array, this.position);
            this.position += size;
            return value;
        }

        public ushort ReadUShort()
        {
            const int size = sizeof(ushort);
            if (this.Length < size)
                throw new BufferSizeException(BufferSizeExceptionMessage);

            ushort value = BitConverter.ToUInt16(this.buffer.Array, this.position);
            this.position += size;
            return value;
        }
        public uint ReadUInt()
        {
            const int size = sizeof(uint);
            if (this.Length < size)
                throw new BufferSizeException(BufferSizeExceptionMessage);

            uint value = BitConverter.ToUInt32(this.buffer.Array, this.position);
            this.position += size;
            return value;
        }

        public ulong ReadULong()
        {
            const int size = sizeof(ulong);
            if (this.Length < size)
                throw new BufferSizeException(BufferSizeExceptionMessage);

            ulong value = BitConverter.ToUInt64(this.buffer.Array, this.position);
            this.position += size;
            return value;
        }

        public string ReadString()
        {
            int stringSize = this.ReadInt();
            if (this.Length < stringSize)
                throw new BufferSizeException(BufferSizeExceptionMessage);

            string value = Encoding.UTF8.GetString(this.buffer.Array, this.position, stringSize);
            this.position += stringSize;
            return value;
        }

        public T ReadMessage<T>()
            where T : IMessage, new()
        {
            T value = new T();
            int messageSize = this.ReadInt();
            if (this.Length < messageSize)
                throw new BufferSizeException(BufferSizeExceptionMessage);

            value.CreateMessage(this);

            return value;
        }

        public byte[] ToArray()
        {
            if (this.Length < 0)
                throw new BufferSizeException(BufferSizeExceptionMessage);

            byte[] lastData = new byte[this.Length];
            Array.Copy(this.buffer.Array, this.position, lastData, 0, this.Length);

            return lastData;
        }

        public int Length
        {
            get { return this.buffer.Array.Length - this.position; }
        }
    }


}
