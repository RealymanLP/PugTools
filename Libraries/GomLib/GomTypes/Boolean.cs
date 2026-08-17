using System;

namespace GomLib.GomTypes {
  public class Boolean : GomType {
    public Boolean() : base(GomTypeId.Boolean) { }
    public override Object ReadData(DataObjectModel dom, GomBinaryReader reader)
      => reader.ReadByte() != 0;

    public override System.String ToString() => "Boolean";
  }
}
