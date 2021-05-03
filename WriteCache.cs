using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace RookDB
{
    internal sealed class WriteCache
    {
        private readonly RookDB parent;

        private readonly Dictionary<RookPath, object> overrides = new Dictionary<RookPath, object>();

        public WriteCache(RookDB parent)
        {
            this.parent = parent;
        }

        public bool FlushToMemory()
        {
            try
            {
                foreach (object o in overrides)
                {
                    if (o is RookField field)
                    {
                        continue;
                    }
                    else if (o is RookRecord record)
                    {
                        continue;
                    }
                    else if (o is RookColumn column)
                    {
                        continue;
                    }
                    else if (o is RookSheet sheet)
                    {
                        continue;
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error occured during cache flush to memory: " + e.ToString());
                return false;
            }

            DiscardCache();
            return true;
        }

        public bool FlushToStorage()
        {
            if (parent.filename == null)
            {
                Console.Error.WriteLine("RookDB has no filename, cannot flush WriteCache to disk! Aborting write operation.");
                return false;
            }

            try
            {
                JsonWriterOptions jsonWriterOptions = new JsonWriterOptions();
                jsonWriterOptions.Indented = true;
                jsonWriterOptions.SkipValidation = false;

                using (FileStream file = File.OpenWrite(parent.filename))
                using (Utf8JsonWriter jsonWriter = new Utf8JsonWriter(file, jsonWriterOptions))
                {
                    //TODO: actually do the writing
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error occured during cache flush to disk: " + e.ToString());
                return false;
            }

            return true;
        }

        private void WriteSheet(Utf8JsonWriter writer, RookSheet sheet)
        {

        }

        private void WriteRecord(Utf8JsonWriter writer, RookSheet sheet)
        {

        }

        public void DiscardCache()
        {
            overrides.Clear();
        }

        public object GetOverride(RookPath path)
        {
            if (overrides.ContainsKey(path))
                return overrides[path];
            return null;
        }

        public T GetOverride<T>(RookPath path)
        {
            object inside = GetOverride(path);
            if (inside.GetType() != typeof(T))
                return default(T);
            return (T)inside;
        }

        public void SetOverride(RookPath path, object value)
        {
            if (!overrides.ContainsKey(path))
                overrides.Add(path, value);
            else
                overrides[path] = value;
        }

        public void ClearOverride(RookPath path)
        {
            if (overrides.ContainsKey(path))
                overrides.Remove(path);
        }
    }
}