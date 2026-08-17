using System;
using System.Collections.Generic;

namespace GomLib.GomTypes {
  public class Map : GomType {
    public GomType KeyType { get; internal set; }
    public GomType ValueType { get; internal set; }

    public Map() : base(GomTypeId.Map) { }
    public override System.Boolean ConfirmType(GomBinaryReader reader)
      => base.ConfirmType(reader);
    internal override void Link(DataObjectModel dom) {
      _dom = dom;
      KeyType.Link(dom);
      ValueType.Link(dom);
    }
    public override Object ReadData(DataObjectModel dom, GomBinaryReader reader) {
      GomType keyType = dom.GomTypeLoader.Load(reader, dom, false);

      // Type ID 0 means that the inline type is omitted.  Fall back to the
      // type declared by the Map definition.
      if (keyType == null)
        keyType = KeyType;
      else if (KeyType != null && keyType.TypeId == KeyType.TypeId)
        keyType = KeyType;

      GomType valType = dom.GomTypeLoader.Load(reader, dom, false);

      if (valType == null)
        valType = ValueType;
      else if (ValueType != null && valType.TypeId == ValueType.TypeId)
        valType = ValueType;

      if (keyType == null || valType == null)
        throw new InvalidOperationException("Map has no key/value type.");

      Int32 len = (Int32)reader.ReadNumber();
      Int32 len2 = (Int32)reader.ReadNumber();

      if (len != len2)
        throw new InvalidOperationException("Map length values aren't the same?!");

      Dictionary<Object, Object> result = new Dictionary<Object, Object>(len);

      for (Int32 i = 0; i < len; i++) {
        Object key = keyType.ReadItem(dom, reader);
        Object val = valType.ReadItem(dom, reader);

        result.Add(key, val);
      }

      return result;
    }
    public override System.String ToString()
      => System.String.Format("Map<{0}, {1}>", KeyType, ValueType);
  }
}
