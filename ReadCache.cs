using System;
using System.Collections.Generic;

namespace RookDB
{
    internal class ReadCache
    {
        internal Dictionary<ulong, IRookObj> cachedObjects = new Dictionary<ulong, IRookObj>();
        
        public ReadCache()
        {
            
        }

        public IRookObj GetObj(ulong uid)
        {
            throw new NotImplementedException();
        }
    }
}