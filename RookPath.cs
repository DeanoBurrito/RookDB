using System;

namespace RookDB
{
    public sealed class RookPath
    {
        public readonly string path;
        internal ulong? readUid;

        internal RookPath(string path, ulong? readUid = null)
        {
            this.path = path;
            this.readUid = readUid;
        }

        public static implicit operator RookPath(string str)
        {
            return new RookPath(str);
        }
    }
}