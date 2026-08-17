using System;

namespace GomLib.GomTypeLoaders
{
    class NewType18Loader : IGomTypeLoader
    {
        public GomTypeId SupportedType { get { return GomTypeId.NewType18; } }

        public GomType Load(GomBinaryReader reader, bool fromGom, DataObjectModel dom)
        {
            GomTypes.NewType18 t = new GomTypes.NewType18();

            // Confirmed via byte-boundary analysis: exactly 4 bytes follow the type-id byte
            // in the schema/type definition. We keep them raw since their meaning is unknown.
            t.RawBytes = reader.ReadBytes(4);

            return t;
        }
    }
}
