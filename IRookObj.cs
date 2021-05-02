using System;

namespace RookDB
{
    public interface IRookObj
    {
        void FlushWriteCache();
        string GetFullPath();
    }
}