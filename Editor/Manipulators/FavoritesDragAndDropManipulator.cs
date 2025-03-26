using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Favorites.Editor.Manipulators
{
    public class FavoritesDragAndDropManipulator : PointerManipulator
    {
        private static FavoritesCache Cache => FavoritesCache.instance;
        
        private readonly ListView _listView;
        private readonly VisualElement _previewIco;
        private readonly Label _previewName;
        private readonly VisualElement _preview;
        
        private DragVisualMode _dragMode;

        public FavoritesDragAndDropManipulator(ListView listView, VisualElement preview)
        {
            _listView = listView;
            _preview = preview;
            _previewIco = _preview.Q("Ico");
            _previewName = _preview.Q<Label>("Name");
        }
        
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<DragEnterEvent>(OnDragEnter);
            target.RegisterCallback<DragLeaveEvent>(OnDragLeave);
            target.RegisterCallback<DragExitedEvent>(OnDragExit);
            
            _listView.canStartDrag += OnCanStartDrag;
            _listView.setupDragAndDrop += OnSetupDragAndDrop;
            _listView.dragAndDropUpdate += OnDragAndDropUpdate;
            _listView.handleDrop += OnDrop;
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<DragEnterEvent>(OnDragEnter);
            target.UnregisterCallback<DragLeaveEvent>(OnDragLeave);
            target.UnregisterCallback<DragExitedEvent>(OnDragExit);
            
            _listView.canStartDrag -= OnCanStartDrag;
            _listView.setupDragAndDrop -= OnSetupDragAndDrop;
            _listView.dragAndDropUpdate -= OnDragAndDropUpdate;
            _listView.handleDrop -= OnDrop;
        }

        private void OnDragEnter(DragEnterEvent evt)
        {
            if (DragAndDrop.objectReferences.Length == 0)
                return;
            
            Object currentObject = DragAndDrop.objectReferences.First();
            
            _previewName.text = currentObject.name;
            
            FavoritesController.AssignIco(_previewIco, currentObject);
            
            _preview.style.display = DisplayStyle.Flex;
            
            _dragMode = Cache.CurrentList.Contains(GlobalObjectId.GetGlobalObjectIdSlow(currentObject)) ? DragVisualMode.Move : DragVisualMode.Copy;
        }

        private void OnDragLeave(DragLeaveEvent evt) =>
            _preview.style.display = DisplayStyle.None;

        private void OnDragExit(DragExitedEvent evt) =>
            _preview.style.display = DisplayStyle.None;
        
        private bool OnCanStartDrag(CanStartDragArgs arg) => true;

        private StartDragArgs OnSetupDragAndDrop(SetupDragAndDropArgs arg)
        {
            StartDragArgs drag = new ("Favorites", DragVisualMode.Move);
            drag.SetUnityObjectReferences(Cache.CurrentList.Get(arg.selectedIds));
            
            Object currentObject = Cache.CurrentList.Get(arg.selectedIds.First());
            
            _previewName.text = currentObject.name;
            
            FavoritesController.AssignIco(_previewIco, currentObject);
            
            _preview.style.display = DisplayStyle.Flex;
            
            _dragMode = DragVisualMode.Move;
            
            return drag;
        }

        private DragVisualMode OnDragAndDropUpdate(HandleDragAndDropArgs data)
        {
            _preview.transform.position = data.position;

            return data.dropPosition == DragAndDropPosition.OverItem ? DragVisualMode.Rejected : _dragMode;
        }

        private DragVisualMode OnDrop(HandleDragAndDropArgs data)
        {
            if (!data.dragAndDropData.unityObjectReferences.Any())
                return DragVisualMode.Rejected;
            
            int newIndex = data.insertAtIndex;

            List<GlobalObjectId> newObjects = FilterNewObjects(data.dragAndDropData.unityObjectReferences);

            if (newObjects.Count > 0)
                Cache.CurrentList.InsertRange(newIndex, newObjects);
            else
            {
                if (newIndex > _listView.selectedIndex)
                    newIndex--;

                GlobalObjectId item = GlobalObjectId.GetGlobalObjectIdSlow(data.dragAndDropData.unityObjectReferences.First());
                
                Cache.CurrentList.Remove(item);
                Cache.CurrentList.Insert(newIndex, item);
            }
            _listView.RefreshItems();
            _listView.SetSelection(newIndex);
            
            return _dragMode;
        }
        
        private List<GlobalObjectId> FilterNewObjects(IEnumerable<Object> objects)
        {
            List<GlobalObjectId> objsToAdd = new ();
            foreach (Object iObject in objects)
            {
                GlobalObjectId obj = GlobalObjectId.GetGlobalObjectIdSlow(iObject);
               
                if (!Cache.CurrentList.Contains(obj))
                    objsToAdd.Add(obj);
            }
            return objsToAdd;
        }
    }
}
