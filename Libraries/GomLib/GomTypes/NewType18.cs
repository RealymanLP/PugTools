using System;

namespace GomLib.GomTypes
{
    /// <summary>
    /// Placeholder for GomTypeId 0x18 (24), introduced with the 64-bit client.
    /// Confirmed to consume exactly 4 bytes right after its own type-id byte (verified via
    /// byte-boundary analysis of real 64-bit .tor schema data). The semantic meaning of those
    /// 4 bytes is not yet known, so they are stored raw rather than interpreted/guessed at.
    /// Rename/replace this class once the real structure and a proper name are confirmed.
    /// </summary>
    public class NewType18 : GomType
    {
        public byte[] RawBytes { get; set; }

        public NewType18() : base(GomTypeId.NewType18) { }

        public override object ReadData(DataObjectModel dom, GomBinaryReader reader)
        {
            // We don't yet know how instance *data* of this type is encoded (as opposed to
            // the type *definition*, which we've confirmed is 4 raw bytes). Rather than
            // silently misreading and desyncing the stream further, fail loudly here so we
            // can capture a sample and figure this part out too, the same way we did for
            // the type definition itself.
            throw new NotSupportedException(
                "Reading instance data for GomType 0x18 (NewType18) is not implemented yet - " +
                "the type definition's 4-byte payload is understood, but not yet how actual " +
                "field values of this type are laid out.");
        }

        public override string ToString() => string.Format("NewType18 (raw: {0})",
            RawBytes != null ? BitConverter.ToString(RawBytes).Replace("-", " ") : "null");
    }
}
