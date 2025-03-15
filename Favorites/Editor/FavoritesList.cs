using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Favorites.Editor
{
    [Serializable]
    public class FavoritesList
    {
        public string name;
        public List<string> serializedIds;
        
        public FavoritesList(string name)
        {
            this.name = name;
            serializedIds = new List<string>();
        }

        public Object Get(int index)
        {
            if (serializedIds.Count < index)
                return null;
            
            return GlobalObjectId.TryParse(serializedIds[index], out GlobalObjectId obj) ?
                GlobalObjectId.GlobalObjectIdentifierToObjectSlow(obj) : null;
        }
        
        public IEnumerable<Object> Get(IEnumerable<int> indexes) =>
            indexes.Select(Get);

        public bool Contains(GlobalObjectId objectId) =>
            serializedIds.Contains(objectId.ToString());

        public void Add(IEnumerable<GlobalObjectId> objectIds) =>
            serializedIds.AddRange(objectIds.Select(o => o.ToString()));

        public void Insert(int index, GlobalObjectId id) =>
            serializedIds.Insert(index, id.ToString());

        public void InsertRange(int index, IEnumerable<GlobalObjectId> objectIds) =>
            serializedIds.InsertRange(index, objectIds.Where(o => !serializedIds.Contains(o.ToString())).Select(o => o.ToString()));

        public void Add(GlobalObjectId objectId) =>
            serializedIds.Add(objectId.ToString());

        public void Remove(List<GlobalObjectId> objectIds)
        {
            foreach (GlobalObjectId iObject in objectIds)
                serializedIds.Remove(iObject.ToString());
        }

        public void Remove(GlobalObjectId objectId) =>
            serializedIds.Remove(objectId.ToString());

        public void RemoveAt(int index) =>
            serializedIds.RemoveAt(index);

        public void Clear() =>
            serializedIds.Clear();

        public int IndexOf(GlobalObjectId globalObjectId) =>
            serializedIds.IndexOf(globalObjectId.ToString());
    }
}
