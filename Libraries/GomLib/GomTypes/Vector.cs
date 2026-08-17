using System;
using System.Collections.Generic;

namespace GomLib.GomTypes {
  public class Vector : GomType {
    public Vector() : base(GomTypeId.Vec3) { }
    public override Object ReadData(DataObjectModel dom, GomBinaryReader reader) {
      List<Single> vec = new List<Single>(3);

      Single x = reader.ReadSingle();
      Single y = reader.ReadSingle();
      Single z = reader.ReadSingle();

      vec.Add(x);
      vec.Add(y);
      vec.Add(z);

      return vec;
    }
    public override System.String ToString() => "Vector3";
  }
}
