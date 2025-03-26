using UnityEngine;
using UnityEngine.UIElements;

namespace Favorites.Editor
{
    public class FavoritesSettings : ScriptableObject
    {
        [Range(1, 100)] public int itemHeight = 40;
        [Range(1, 50)] public int itemFontSize = 15;
        public AlternatingRowBackground backgroundStyle = AlternatingRowBackground.ContentOnly;
    }
}
