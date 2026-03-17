namespace SandboxNetwork
{
    public class ObjectHashTable<T1, T2> where T2 : class
    {
        protected System.Collections.Hashtable objectTable = new System.Collections.Hashtable();
        public bool Add(T1 index, T2 type)
        {
            if (objectTable.ContainsKey(index))
                return false;

            objectTable.Add(index, type);
            return true;
        }
        public bool Add(string index, T2 type)
        {
            if (objectTable.ContainsKey(index))
                return false;

            objectTable.Add(index, type);
            return true;
        }
        public T2 Get(T1 type)
        {
            if (objectTable.ContainsKey(type))
            {
                if (objectTable[type] is T2)
                {
                    var @object = objectTable[type] as T2;
                    return @object;
                }
            }
            return null;
        }
        public T2 Get(string type)
        {
            if (objectTable.ContainsKey(type))
            {
                if (objectTable[type] is T2)
                {
                    var @object = objectTable[type] as T2;
                    return @object;
                }
            }
            return null;
        }
        public bool Remove(T1 type)
        {
            if (objectTable.ContainsKey(type))
            {
                objectTable.Remove(type);
                return true;
            }
            return false;
        }
        public bool Remove(string type)
        {
            if (objectTable.ContainsKey(type))
            {
                objectTable.Remove(type);
                return true;
            }
            return false;
        }
        public void Clear()
        {
            objectTable.Clear();
        }
    }
}