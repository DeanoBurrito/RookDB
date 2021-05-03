using System;
using System.Collections.Generic;

namespace RookDB
{
    internal class ReadCache
    {
        internal Dictionary<ulong, IRookObj> cachedObjects = new Dictionary<ulong, IRookObj>();
        
        public ReadCache()
        {
            //everything just gets mapped as a flat array, with the uint as the key (big jump table.)
            //saves traversing the tree-like structure of the database at runtime.
        }

        public bool ObjCached(ulong uid)
        {
            throw new NotImplementedException();
        }

        public IRookObj GetObj(ulong uid)
        {
            throw new NotImplementedException();
        }
    }
}