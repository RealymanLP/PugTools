using System;

namespace GomLib.GomTypes {
  public class EmbeddedClass : GomType {
    public DomClass DomClass { get; internal set; }
    public ulong DomClassId { get; internal set; }

    public EmbeddedClass() : base(GomTypeId.EmbeddedClass) { }
    internal override void Link(DataObjectModel dom) {
      _dom = dom;
      DomClass = _dom.Get<DomClass>(DomClassId);
    }
    public override Object ReadData(DataObjectModel dom, GomBinaryReader reader) {
      if (DomClass == null) Link(dom);
      return _dom.ScriptObjectReader.ReadObject(DomClass, reader, dom);
    }
    public override System.String ToString() {
      return System.String.Format("class {0}", DomClass);
    }
  }
}
