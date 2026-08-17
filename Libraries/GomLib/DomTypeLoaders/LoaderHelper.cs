using System;

namespace GomLib.DomTypeLoaders
{
    static class LoaderHelper
    {
        internal static void ParseShared(GomBinaryReader reader, DomType dom)
        {
            Int64 offset = reader.BaseStream.Position;

            //reader.BaseStream.Position = 0x8;
            //dom.Id = reader.ReadUInt64();
            reader.BaseStream.Position = 0x14;
            Int16 nameOffset = reader.ReadInt16();
            Int16 descOffset = reader.ReadInt16();

            reader.BaseStream.Position = nameOffset;
            dom.Name = reader.ReadNullTerminatedString();

            reader.BaseStream.Position = descOffset;
            dom.Description = reader.ReadNullTerminatedString();

            reader.BaseStream.Position = offset;
        }
    }
}
