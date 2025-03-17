using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Favorites.Editor.Manipulators
{
    public class FavoriteItemManipulator : PointerManipulator
    {
        private readonly ListView _listView;
        private readonly int _index;
        private readonly FavoritesData _data;

        private Button _removeButton;
        
        private Object lastObjectSelected;
        private double lastObjectSelectedAt;

        public FavoriteItemManipulator(ListView listView, int index, FavoritesData data)
        {
            _listView = listView;
            _data = data;
            _index = index;
        }
        
        protected override void RegisterCallbacksOnTarget()
        {
            _removeButton = target.Q<Button>("Remove");
            
            target.RegisterCallback<PointerOverEvent>(OnPointerEnter);
            target.RegisterCallback<PointerOutEvent>(OnPointerOutEvent);
            target.RegisterCallback<PointerUpEvent>(OnPointerUpPingEvent);
            
            _removeButton.clicked += OnRemoveEvent;
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerOverEvent>(OnPointerEnter);
            target.UnregisterCallback<PointerOutEvent>(OnPointerOutEvent);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUpPingEvent);
            
            _removeButton.clicked -= OnRemoveEvent;
        }

        private void OnPointerEnter(PointerOverEvent pointerOverEvent)
        {
            ShowButton();
            pointerOverEvent.StopPropagation();
        }

        private void OnPointerOutEvent(PointerOutEvent pointerOutEvent)
        {
            HideButton();
            pointerOutEvent.StopPropagation();
        }

        private void OnPointerUpPingEvent(PointerUpEvent pointerUpEvent)
        {
            if (_listView.selectedIndex != _index)
                return;
            
            Object currentObject = _data.CurrentList.Get(_index);
            Selection.activeObject = currentObject;
            if (lastObjectSelected == currentObject)
            {
                if (lastObjectSelectedAt + _data.doubleClickTime > EditorApplication.timeSinceStartup)
                    AssetDatabase.OpenAsset(currentObject);
                else if (lastObjectSelectedAt + _data.pingTime > EditorApplication.timeSinceStartup)
                    EditorGUIUtility.PingObject(currentObject);
            }
            lastObjectSelected = currentObject;
            lastObjectSelectedAt = EditorApplication.timeSinceStartup;
        }

        private void OnRemoveEvent()
        {
            
            _data.CurrentList.RemoveAt(_index);
            _listView.RefreshItems();
        }

        private void ShowButton()
        {
            if (DragAndDrop.visualMode != DragAndDropVisualMode.None)
                return;
            
            _removeButton.style.opacity = new StyleFloat(0.65f);
        }

        private void HideButton() =>
            _removeButton.style.opacity = new StyleFloat(0f);
    }
}
