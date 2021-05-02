using System;

namespace RookDB
{
    public sealed class RookPath
    {
        public readonly string path;
        internal ulong readUid;

        internal RookPath(string path, ulong? readUid = null)
        {
            this.path = path;
            this.readUid = readUid.HasValue ? readUid.Value : 0;
        }

        public static implicit operator RookPath(string str)
        {
            return new RookPath(str);
        }
    }
}