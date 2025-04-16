﻿using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Lagrange.Proto.Primitives;
using Lagrange.Proto.Serialization.Converter;
using Lagrange.Proto.Serialization.Metadata;

namespace Lagrange.Proto.Serialization;

/// <summary>
/// Provides methods for serializing or deserializing objects from or to Protocol Buffers.
/// </summary>
public static partial class ProtoSerializer
{
    /// <summary>
    /// Serialize the ProtoPackable Object to the destination buffer, AOT Friendly
    /// </summary>
    /// <param name="dest">The destination buffer to write to</param>
    /// <param name="obj">The object to serialize</param>
    /// <typeparam name="T">The type of the object to serialize</typeparam>
    public static void SerializeProtoPackable<T>(IBufferWriter<byte> dest, T obj) where T : IProtoSerializable<T>
    {
        SerializeProtoPackableCore(dest, obj);
    }
    
    /// <summary>
    /// Serialize the ProtoPackable Object to a byte array, AOT Friendly
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <returns>The serialized object as a byte array</returns>
    public static byte[] SerializeProtoPackable<T>(T obj) where T : IProtoSerializable<T>
    {
        throw new NotImplementedException();
    }
    
    private static void SerializeProtoPackableCore<T>(IBufferWriter<byte> dest, T obj) where T : IProtoSerializable<T>
    {
        ProtoTypeResolver.Register(new ProtoSerializableConverter<T>());

        var writer = ProtoWriterCache.RentWriter(dest);
        T.SerializeHandler(obj, writer);
        writer.Flush();
        ProtoWriterCache.ReturnWriter(writer);
    }
    
    /// <summary>
    /// Serialize the object to the destination buffer, Reflection based
    /// </summary>
    /// <param name="dest">The destination buffer to write to</param>
    /// <param name="obj">The object to serialize</param>
    /// <typeparam name="T">The type of the object to serialize</typeparam>
    [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
    [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
    public static void Serialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IBufferWriter<byte> dest, T obj) 
    {
        var writer = ProtoWriterCache.RentWriter(dest);
        SerializeCore(writer, obj);
        ProtoWriterCache.ReturnWriter(writer);
    }
    
    /// <summary>
    /// Serialize the object to a byte array, Reflection based
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <returns>The serialized object as a byte array</returns>
    [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
    [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
    public static byte[] Serialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(T obj) 
    {
        var writer = ProtoWriterCache.RentWriterAndBuffer(512, out var buffer);
        SerializeCore(writer, obj);
        var written = buffer.ToArray();
        ProtoWriterCache.ReturnWriter(writer);
        
        return written;
    }

    [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
    [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
    private static void SerializeCore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(ProtoWriter writer, T obj)
    {       
        ProtoConverter<T> converter;
        if (ProtoTypeResolver.IsRegistered<T>())
        {
            converter = ProtoTypeResolver.GetConverter<T>();
        }
        else
        {
            converter = new ProtoObjectConverter<T>();
            ProtoTypeResolver.Register(converter);
        }
        
        converter.Write(0, WireType.VarInt, writer, obj); // the first two arguments are ignored
        writer.Flush();
    }
}