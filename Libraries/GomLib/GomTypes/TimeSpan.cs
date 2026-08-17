using System;

namespace GomLib.GomTypes {
  public class TimeSpan : GomType {
    public TimeSpan() : base(GomTypeId.TimeSpan) { }
    public override Object ReadData(DataObjectModel dom, GomBinaryReader reader)
      => reader.ReadNumber();

    public override System.String ToString() => "TimeSpan";
  }
}
