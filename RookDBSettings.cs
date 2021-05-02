using System;

namespace RookDB
{
    public sealed class RookDBSettings
    {
        public static readonly RookDBSettings MaximumReadPerformance = new RookDBSettings() 
        {
            writeCacheEnabled = true,
            writeCacheAutoFlush = true,

            readCacheEnabled = true,
            readCachePrefill = true,
            readCachePrecast = true,
        };

        public static readonly RookDBSettings DontCare = new RookDBSettings();
        
        public bool writeCacheEnabled = true;       //enables or disables write caching
        public bool writeCacheAutoFlush = false;    //if true any write will be immediately written to disk (async or not). Otherwise writes will accumulate in memory until manually flushed.

        public bool readCacheEnabled = true;        //enables or disables read caching
        public bool readCachePrefill = false;       //if true will populate all entries in memory at load time. Increases initial database startup time, but makes lookup a fixed cost.
        public bool readCachePrecast = false;       //if true will store casted versions fields in memory, otherwise it will defer until first read.
    }
}