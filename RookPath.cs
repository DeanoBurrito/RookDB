using System;

namespace RookDB
{
    public sealed class RookPath
    {
        public readonly string path;
        internal readonly string[] segments;
        internal uint? id = null;
        internal bool isMeta = false;

        public RookPath(string path)
        {
            this.path = path;
            segments = path.Split('/');
        }

        internal RookPath(string path, uint id)
        {
            this.path = path;
            this.id = id;
            segments = path.Split('/');
        }

        public static implicit operator RookPath(string path)
        {
            return new RookPath(path);
        }

        public override string ToString()
        {
            string idStr = id.HasValue ? " " + id.Value : "";
            return "[RookPath] " + path + idStr;
        }
    }
}