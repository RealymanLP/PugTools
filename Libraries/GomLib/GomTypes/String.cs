using System;
using System.Text;

namespace GomLib.GomTypes {
  public class String : GomType {
    public String() : base(GomTypeId.String) { }
    public override Object ReadData(DataObjectModel dom, GomBinaryReader reader)
      => reader.ReadLengthPrefixString(Encoding.UTF8);

    public override System.String ToString() => "String";
  }
}
