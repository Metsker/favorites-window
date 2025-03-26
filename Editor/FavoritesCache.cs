using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Favorites.Editor
{
    [FilePath("Library/Favorites Cache.asset", FilePathAttribute.Location.ProjectFolder)]
    public class FavoritesCache : ScriptableSingleton<FavoritesCache>
    {
        [SerializeField, Min(0)] private int currentListIndex;
        [SerializeField] private List<FavoritesList> favoriteLists = new ();

        public int CurrentListIndex 
        {
            get => currentListIndex;
            set
            {
                if (value < 0)
                    return;
                
                if (value > favoriteLists.Count - 1)
                    AddList();
                else if (value == favoriteLists.Count - 2 && favoriteLists[value + 1].serializedIds.Count == 0)
                    RemoveList(value + 1);

                currentListIndex = value;
            }
        }
        public int FavoriteListsCount => favoriteLists.Count;

        public FavoritesList CurrentList {
            get
            {
                if (favoriteLists.Count == 0)
                    AddList();
                
                if (currentListIndex >= favoriteLists.Count)
                    currentListIndex = favoriteLists.Count - 1;
                
                return favoriteLists[CurrentListIndex];
            }
        }

        private void AddList()
        {
            favoriteLists.Add(new FavoritesList("Page " + (favoriteLists.Count + 1)));
            
            CurrentListIndex = favoriteLists.Count - 1;
        }

        private void RemoveList(int index)
        {
            favoriteLists.RemoveAt(index);
            
            if (CurrentListIndex >= favoriteLists.Count)
                CurrentListIndex = favoriteLists.Count - 1;
        }

        private string[] ListNames() =>
            favoriteLists.Select(l => l.name).ToArray();

        public static void Save() =>
            instance.Save(false);
    }
}
