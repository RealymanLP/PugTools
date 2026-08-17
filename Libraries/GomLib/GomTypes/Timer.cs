using System;

namespace GomLib.GomTypes {
  public class Timer : GomType {
    public Timer() : base(GomTypeId.Timer) { }
    public override Object ReadData(DataObjectModel dom, GomBinaryReader reader) {
      reader.ReadBytes(0x22);
      return null;
    }
    public override System.String ToString() => "Timer";
  }
}
