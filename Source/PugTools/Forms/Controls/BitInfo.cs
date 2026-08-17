using System;

namespace Be.HexEditor {
  public class BitInfo {
    private Byte _value;

    public Int64 Position { get; set; }
    public Boolean this[Int32 index] {
      get => (_value & (1 << index)) != 0;
      set {
        if (value) _value |= (Byte)(1 << index); //set bit index 1
        else _value &= (Byte)~(1 << index); //set bit index 0
      }
    }
    public Byte Value {
      get => _value;
      set => _value = value;
    }

    public BitInfo(Byte value, Int64 position) {
      _value = value;
      Position = position;
    }
    public String GetBitAsString(Int32 index) {
      if (this[index]) return "1";
      else return "0";
    }

    public override String ToString() {
      String result =
        String.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
          GetBitAsString(7),
          GetBitAsString(6),
          GetBitAsString(5),
          GetBitAsString(4),
          GetBitAsString(3),
          GetBitAsString(2),
          GetBitAsString(1),
          GetBitAsString(0)
        );

      return result;
    }
  }
}
