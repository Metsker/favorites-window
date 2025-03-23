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

        private FavoritesData LoadOrCreateData()
        {
            string[] assets = AssetDatabase.FindAssets($"t: {typeof(FavoritesData)}");
            if (assets.Length == 0)
            {
                var data = CreateInstance<FavoritesData>();
                string assetName = AssetDatabase.GenerateUniqueAssetPath("Assets/FavoritesData.asset");
                AssetDatabase.CreateAsset(data, assetName);
                AssetDatabase.SaveAssets();
                return data;
            }
            else
                return AssetDatabase.LoadAssetAtPath<FavoritesData>(AssetDatabase.GUIDToAssetPath(assets[0]));
        }
    }
}
