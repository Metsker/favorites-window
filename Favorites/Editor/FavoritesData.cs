using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Favorites.Editor
{
    public class FavoritesData : ScriptableObject
    {
        [Range(1, 100)] public int itemHeight = 40;
        [Range(1, 50)] public int itemFontSize = 15;
        [Range(0.01f, 1)] public float doubleClickTime = 0.5f;
        [Range(0.01f, 5)] public float pingTime = 2f;
        public AlternatingRowBackground backgroundStyle = AlternatingRowBackground.ContentOnly;
        [Space]
        [SerializeField, Min(0)] private int currentListIndex;
        [SerializeField] private List<FavoritesList> favoriteLists;

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
                if (currentListIndex >= favoriteLists.Count)
                    currentListIndex = favoriteLists.Count - 1;
                
                return favoriteLists[CurrentListIndex];
            }
        }
        
        public void Reset()
        {
            favoriteLists = new List<FavoritesList>();
            AddList();
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
    }
}
