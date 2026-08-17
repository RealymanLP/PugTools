using System;
using System.Collections.Generic;

namespace GomLib.GomTypes {
  public class List : GomType {
    public GomType ContainedType { get; internal set; }

    public List() : base(GomTypeId.List) { }
    internal override void Link(DataObjectModel dom) {
      _dom = dom;
      ContainedType.Link(dom);
    }
    public override Object ReadData(DataObjectModel dom, GomBinaryReader reader) {
      GomType itemType = dom.GomTypeLoader.Load(reader, dom, false);

      // Type ID 0 means that the inline type is omitted.  Use the type
      // declared by the List definition in that case.
      if (itemType == null)
        itemType = ContainedType;
      else if ((ContainedType != null) && (itemType.TypeId == ContainedType.TypeId))
        itemType = ContainedType;

      if (itemType == null)
        throw new InvalidOperationException("List has no element type.");

      Int32 len = (Int32)reader.ReadNumber();
      Int32 len2 = (Int32)reader.ReadNumber();

      if (len != len2)
        throw new InvalidOperationException("List length values aren't the same?!");

      List<Object> result = new List<Object>(len);

      for (Int32 i = 0; i < len; i++) {
        _ = reader.ReadNumber();
        Object val = itemType.ReadItem(dom, reader);
        result.Add(val);
      }

      return result;
    }
    public override System.String ToString() => System.String.Format("List<{0}>", ContainedType);
  }
}
