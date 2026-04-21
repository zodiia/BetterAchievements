using InteropGenerator.Runtime;

namespace BetterAchievements.Helpers;

public static class ByteArrayExtension
{
    /**
     * Quick hashing of the bit array for comparison later.
     *
     * See: https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
     */
    public static unsafe ulong ComputeHash(this BitArray bitArray)
    {
        const ulong prime = 0x100000001B3; // FNV prime
        var hash = 0xCBF29CE484222325; // FNV offset basis

        var ptr = (ulong*)bitArray.Pointer;
        var count = bitArray.ByteLength / 8;
        var remainingBytes = bitArray.ByteLength % 8;

        for (var i = 0; i < count; i++)
        {
            hash ^= ptr[i];
            hash *= prime;
        }

        if (remainingBytes > 0)
        {
            var lastChunk = 0ul;
            var remainder = bitArray.Pointer + (count * 8);
            for (var i = 0; i < remainingBytes; i++)
            {
                lastChunk |= (ulong)remainder[i] << (i * 8);
            }
            hash ^= lastChunk;
            hash *= prime;
        }

        return hash;
    }
}
