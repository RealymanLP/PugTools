using System;

namespace GomLib.GomTypes {
  public class Float : GomType {
    public Float() : base(GomTypeId.Float) { }
    public override Object ReadData(DataObjectModel dom, GomBinaryReader reader)
      => reader.ReadSingle();

    public override System.String ToString() => "Float32";
  }
}
