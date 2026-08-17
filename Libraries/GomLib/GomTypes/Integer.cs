using System;

namespace GomLib.GomTypes {
  class Integer : GomType {
    public Integer() : base(GomTypeId.Int64) { }
    public override Object ReadData(DataObjectModel dom, GomBinaryReader reader)
      => reader.ReadSignedNumber();

    public override System.String ToString() => "Int64";
  }
}
