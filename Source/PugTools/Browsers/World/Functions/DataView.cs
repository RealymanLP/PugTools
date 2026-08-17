using System;
using System.IO;
using System.Linq;

namespace DataView
{
    public class DataView
    {
        private readonly BinaryReader br;

        public DataView(Stream stream)
        {
            br = new BinaryReader(stream);
        }

        public sbyte GetInt8(int offset)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            return br.ReadSByte();
        }

        public short GetInt16(int offset, bool littleEndian = false)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            if (littleEndian) return br.ReadInt16();
            else
            {
                var data = br.ReadBytes(2);
                Array.Reverse(data);
                return BitConverter.ToInt16(data, 0);
            }
        }

        public int GetInt32(int offset, bool littleEndian = false)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            if (littleEndian) return br.ReadInt32();
            else
            {
                var data = br.ReadBytes(4);
                Array.Reverse(data);
                return BitConverter.ToInt32(data, 0);
            }
        }

        public long GetInt64(int offset, bool littleEndian = false)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            if (littleEndian) return br.ReadInt64();
            else
            {
                var data = br.ReadBytes(8);
                Array.Reverse(data);
                return BitConverter.ToInt64(data, 0);
            }
        }

        public byte GetUint8(int offset)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            return br.ReadByte();
        }

        public ushort GetUint16(int offset, bool littleEndian = false)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            if (littleEndian) return br.ReadUInt16();
            else
            {
                var data = br.ReadBytes(2);
                Array.Reverse(data);
                return BitConverter.ToUInt16(data, 0);
            }
        }

        public uint GetUint32(int offset, bool littleEndian = false)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            if (littleEndian) return br.ReadUInt32();
            else
            {
                var data = br.ReadBytes(4);
                Array.Reverse(data);
                return BitConverter.ToUInt32(data, 0);
            }
        }

        public ulong GetUint64(int offset, bool littleEndian = false)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            if (littleEndian) return br.ReadUInt64();
            else
            {
                var data = br.ReadBytes(8);
                Array.Reverse(data);
                return BitConverter.ToUInt64(data, 0);
            }
        }

        public float GetFloat32(int offset, bool littleEndian = false)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            if (littleEndian) return br.ReadSingle();
            else
            {
                var data = br.ReadBytes(4);
                Array.Reverse(data);
                return BitConverter.ToSingle(data, 0);
            }
        }

        public double GetFloat64(int offset, bool littleEndian = false)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            if (littleEndian) return br.ReadDouble();
            else
            {
                var data = br.ReadBytes(8);
                Array.Reverse(data);
                return BitConverter.ToDouble(data, 0);
            }
        }

        public string GetString(int offset, int length)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            byte curChar;
            string outName = "";

            while (length > 0 && (curChar = br.ReadByte()) != 0x00)
            {
                outName += curChar;
                length--;
            }

            return outName;
        }

        public string GetWString(int offset, int length)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            ushort curChar;
            string outName = "";

            while (length > 0 && (curChar = br.ReadUInt16()) != 0x00)
            {
                outName += BitConverter.GetBytes(curChar).First();
                length--;
            }

            return outName;
        }
    }
}