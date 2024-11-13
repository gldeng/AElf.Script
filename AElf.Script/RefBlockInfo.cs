using Google.Protobuf;

namespace AElf.Script;

public sealed class RefBlockInfo
{
    public RefBlockInfo(long height, ByteString prefix)
    {
        Height = height;
        Prefix = prefix;
    }

    public long Height { get; }
    public ByteString Prefix { get; }
}