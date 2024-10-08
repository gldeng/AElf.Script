using AElf.Types;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace AElf.Scripts;

public class AddressConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(Address);

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var scalar = parser.Consume<Scalar>();
        return Address.FromBase58(scalar.Value);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (value is Address address)
        {
            emitter.Emit(new Scalar(address.ToBase58()));
        }
        else
        {
            throw new InvalidOperationException("Expected AElf.Types.Address object.");
        }
    }
}

public class HashConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(Hash);

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var scalar = parser.Consume<Scalar>();
        return Hash.LoadFromHex(scalar.Value);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (value is Hash hash)
        {
            emitter.Emit(new Scalar(hash.ToHex()));
        }
        else
        {
            throw new InvalidOperationException("Expected AElf.Types.Hash object.");
        }
    }
}