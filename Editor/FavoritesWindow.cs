using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Favorites.Editor
{
    public class FavoritesWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset tree;
        [SerializeField] private VisualTreeAsset list;

        private FavoritesController _controller;

        [MenuItem("Window/Favorites")]
        public static void Open()
        {
            FavoritesWindow wnd = GetWindow<FavoritesWindow>();
            wnd.titleContent = new GUIContent("Favorites", EditorGUIUtility.IconContent("d_Favorite").image);
        }

        public void CreateGUI()
        {
            tree.CloneTree(rootVisualElement);

            _controller = new FavoritesController(rootVisualElement, list, LoadOrCreateData());
        }

        private static FavoritesSettings LoadOrCreateData()
        {
            string[] assets = AssetDatabase.FindAssets($"t: {typeof(FavoritesSettings)}");
            
            if (assets.Length != 0)
                return AssetDatabase.LoadAssetAtPath<FavoritesSettings>(AssetDatabase.GUIDToAssetPath(assets[0]));
            
            FavoritesSettings settings = CreateInstance<FavoritesSettings>();
            string assetName = AssetDatabase.GenerateUniqueAssetPath($"Assets/{nameof(FavoritesSettings)}.asset");
            AssetDatabase.CreateAsset(settings, assetName);
            AssetDatabase.SaveAssets();
            return settings;
        }
    }
}
