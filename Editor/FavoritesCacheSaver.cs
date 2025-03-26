using UnityEditor;

namespace Favorites.Editor
{
    public static class FavoritesCacheSaver
    {
        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.quitting -= FavoritesCache.Save;
            EditorApplication.quitting += FavoritesCache.Save;
        }
    }
}
