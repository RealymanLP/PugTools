using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TorArchive {
  public enum Endianness {
    LittleEndian,
    BigEndian
  }

  public class ExtendedBinaryReader : BinaryReader {

    #region Constructors
    public ExtendedBinaryReader(Stream str) : base(str) { }
    public ExtendedBinaryReader(Stream str, Encoding encoding) : base(str, encoding) { }

    #endregion Constructors

    #region Methods
    public String ReadFixedLengthString(Int32 length) {
      return ReadFixedLengthString(length, Encoding.UTF8);
    }
    public String ReadFixedLengthString(Int32 length, Encoding encoding) {
      // Byte[] buff = ReadBytes(length);
      // return encoding.GetString(buff);
      return encoding.GetString(ReadBytes(length));
    }
    public Int16 ReadInt16(Endianness endianness) {
      Int16 val = base.ReadInt16();

      if (endianness == Endianness.LittleEndian) {
        return val;
      } else {
        return System.Net.IPAddress.NetworkToHostOrder(val);
      }
    }
    public Int32 ReadInt32(Endianness endianness) {
      Int32 val = base.ReadInt32();

      if (endianness == Endianness.LittleEndian) {
        return val;
      } else {
        return System.Net.IPAddress.NetworkToHostOrder(val);
      }
    }
    public String ReadNullTerminatedString() {
      return ReadNullTerminatedString(Encoding.UTF8);
    }

    public String ReadNullTerminatedString(Encoding encoding) {
      List<Byte> byteBuffer = new List<Byte>();
      Byte b = ReadByte();

      // Read until we encounter a null byte
      while (b != 0) {
        byteBuffer.Add(b);
        b = ReadByte();
      }

      return encoding.GetString(byteBuffer.ToArray());
    }
    public Single ReadSingle(Endianness endianness) {
      if (endianness == Endianness.LittleEndian) {
        return base.ReadSingle();
      }

      // Byte[] b = base.ReadBytes(4);
      // return BitConverter.ToSingle(b.Reverse().ToArray(), 0);
      return BitConverter.ToSingle(base.ReadBytes(4).Reverse().ToArray(), 0);
    }
    public UInt16 ReadUInt16(Endianness endianness) {
      if (endianness == Endianness.LittleEndian) {
        return base.ReadUInt16();
      }

      // Byte[] b = base.ReadBytes(2);
      // return BitConverter.ToUInt16(b.Reverse().ToArray(), 0);
      return BitConverter.ToUInt16(base.ReadBytes(2).Reverse().ToArray(), 0);
    }
    public UInt32 ReadUInt32(Endianness endianness) {
      if (endianness == Endianness.LittleEndian) {
        return base.ReadUInt32();
      }

      // Byte[] b = base.ReadBytes(4);
      // return BitConverter.ToUInt32(b.Reverse().ToArray(), 0);
      return BitConverter.ToUInt32(base.ReadBytes(4).Reverse().ToArray(), 0);
    }

    #endregion Methos
  }
}
