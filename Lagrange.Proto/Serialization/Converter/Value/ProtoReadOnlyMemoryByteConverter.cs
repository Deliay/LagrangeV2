using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization.Converter;

public class ProtoReadOnlyMemoryByteConverter : ProtoConverter<ReadOnlyMemory<byte>>
{
    public override void Write(int field, WireType wireType, ProtoWriter writer, ReadOnlyMemory<byte> value)
    {
        writer.EncodeVarInt(value.Length);
        writer.WriteRawBytes(value.Span);
    }

    public override ReadOnlyMemory<byte> Read(int field, WireType wireType, ref ProtoReader reader)
    {
        int length = reader.DecodeVarInt<int>();
        if (length == 0) return ReadOnlyMemory<byte>.Empty;
        
        var buffer = GC.AllocateUninitializedArray<byte>(length);
        var span = reader.CreateSpan(length);
        span.CopyTo(buffer);
        return new ReadOnlyMemory<byte>(buffer, 0, length);
    }
}