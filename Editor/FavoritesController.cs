using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Favorites.Editor.Manipulators;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Favorites.Editor
{
    public class FavoritesController
    {
        private static FavoritesCache Cache => FavoritesCache.instance;
        
        private readonly VisualElement _root;
        private readonly VisualElement _footer;
        private readonly VisualTreeAsset _listAsset;

        private readonly Dictionary<int, FavoriteItemManipulator> manipulators = new ();
        
        private ListView _listView;
        private VisualElement _preview;
        private Task scrollTask;

        public FavoritesController(VisualElement rootVisualElement, VisualTreeAsset listAsset, FavoritesSettings settings)
        {
            _root = rootVisualElement.Q("Root");
            _root.dataSource = settings;
            _listAsset = listAsset;

            _footer = _root.Q("Footer");
            _footer.RegisterCallback<WheelEvent>(OnFooterScrolled);
            
            BuildFooter();
            UpdateName();
            DelayedByFrame(() => BuildList("page-visible"));
        }

        private void OnClickAway(PointerUpEvent evt)
        {
            if (((VisualElement)evt.target).ClassListContains("unity-scroll-view__content-and-vertical-scroll-container"))
                _listView.ClearSelection();
        }

        private VisualElement ConstructPreview(VisualTreeAsset template)
        {
            TemplateContainer preview = template.Instantiate();
            preview.dataSource = _root.dataSource;
            preview.Q("Remove").style.display = DisplayStyle.None;

            _root.parent.Add(preview.AddClass("preview"));

            return preview;
        }

        private void OnFooterScrolled(WheelEvent wheelEvent)
        {
            switch (wheelEvent.delta.y)
            {
                case > 0:
                    DelayedByFrame(OnNext);
                    break;
                case < 0:
                    DelayedByFrame(OnPrevious);
                    break;
            }
        }
        
        private void OnBodyScrolled(WheelEvent wheelEvent)
        {
            if (_root.Q("unity-dragger").visible)
                return;
            
            switch (wheelEvent.delta.y)
            {
                case > 0:
                    DelayedByFrame(OnNext);
                    break;
                case < 0:
                    DelayedByFrame(OnPrevious);
                    break;
            }
        }

        private void OnReject() =>
            BuildFooter();

        private void OnAccept()
        {
            string value = _footer.Q<TextField>().value;
            
            if (string.IsNullOrEmpty(value))
            {
                OnReject();
                return;
            }
            
            Cache.CurrentList.name = value;
            BuildFooter();
        }

        private void BuildFooter()
        {
            _footer.Clear();
            Button backButton = new Button(() => DelayedByFrame(OnPrevious))
            {
                name = "Back_button",
                text = "<"
            }.AddClass("footer", "button-interactable");
            
            _footer.Add(backButton);
            
            backButton.SetEnabled(Cache.CurrentListIndex > 0);
            
            Label label = new Label(Cache.CurrentList.name)
            {
                name = "Category"
            }.AddClass("footer", "text-interactable");
            
            _footer.Add(label);
            
            label.RegisterCallback<PointerUpEvent>(OnCategoryClicked);
            
            _footer.Add(new Button(() => DelayedByFrame(OnNext))
            {
                text = ">"
            }.AddClass("footer", "button-interactable"));
        }

        private void OnCategoryClicked(PointerUpEvent evt)
        {
            _footer.Clear();
            _listView.RegisterCallbackOnce(new EventCallback<PointerUpEvent>(_ => OnReject()));
            
            _footer.Add(new Button(OnReject)
            {
                name = "Back_button",
                text = "x"
            }.AddClass("footer", "button-interactable"));
            
            TextField textField = new()
            {
                value = Cache.CurrentList.name,
                selectAllOnFocus = true
            };
            textField.Q("unity-text-input").AddToClassList("footer");
            textField.RegisterCallback(new EventCallback<KeyDownEvent>(e =>
            {
                switch (e.keyCode)
                {
                    case KeyCode.Return:
                        OnAccept();
                        break;
                    case KeyCode.Escape:
                        OnReject();
                        break;
                }
            }), TrickleDown.TrickleDown);

            _footer.Add(textField.AddClass("footer", "text-interactable"));
            _footer.Add(new Button(OnAccept)
            {
                text = "v"
            }.AddClass("footer", "button-interactable"));
            
            textField.Focus();
        }

        private void BuildList(string initialStateSelector, Action onBuild = default)
        {
            _listView = _listAsset.Instantiate().Q<ListView>().AddClass(initialStateSelector);
            _root.Add(_listView);
            
            _listView.RegisterCallbackOnce<GeometryChangedEvent>(_ => OnReady());
            
            
            void OnReady()
            {
                _preview ??= ConstructPreview(_listView.itemTemplate);

                _listView.makeNoneElement = () => new Label("Drop folders, assets \n or GameObjects").AddClass("non-element");
                _listView.AddManipulator(new FavoritesDragAndDropManipulator(_listView, _preview));
                _listView.RegisterCallback<PointerUpEvent>(OnClickAway);
                _listView.RegisterCallback<WheelEvent>(OnBodyScrolled);

                _listView.itemsSource = Cache.CurrentList.serializedIds;

                manipulators.Clear();
            
                _listView.bindItem = (item, index) =>
                {
                    Object currentObject = Cache.CurrentList.Get(index);
                
                    item.Q<Label>("Name").text = currentObject.name;
                    AssignIco(item.Q("Ico"), currentObject);
                
                    FavoriteItemManipulator manipulator = new (_listView, index);

                    if (!manipulators.ContainsKey(index))
                    {
                        item.AddManipulator(manipulator);
                        manipulators.Add(index, manipulator);
                    }
                };

                _listView.unbindItem = (item, index) =>
                {
                    if (manipulators.Count <= index)
                        return;
                    
                    item.RemoveManipulator(manipulators[index]);
                    manipulators.Remove(index);
                };
            
                _footer.BringToFront();
                onBuild?.Invoke();
            }
        }

        private void OnNext()
        {
            Cache.CurrentListIndex++;
            UpdateName();

            Transition("page-hidden-left", "page-hidden-right");
        }

        private void OnPrevious()
        {
            if (Cache.CurrentListIndex == 0)
                return;
            
            Cache.CurrentListIndex--;
            UpdateName();
            
            Transition("page-hidden-right", "page-hidden-left");

        }

        private void Transition(string from, string to)
        {
            _listView.RemoveClass("page-visible");
            _listView.AddClass(from);
            
            _listView.RegisterCallback<TransitionEndEvent>(e => ((VisualElement)e.target).RemoveFromHierarchy());
            
            BuildList(to, () =>
            {
                _listView.RemoveClass(to);
                _listView.AddClass("page-visible");
            });
        }

        private void UpdateName()
        {
            _footer.Q("Back_button").SetEnabled(Cache.CurrentListIndex > 0);
            _footer.Q<Label>("Category").text = Cache.CurrentList.name;
        }

        private void DelayedByFrame(Action action) =>
            EditorCoroutineUtility.StartCoroutine(DelayedByFrameEnumerator(action), this);

        private static IEnumerator DelayedByFrameEnumerator(Action action)
        {
            yield return null;
            action?.Invoke();
        }

        public static void AssignIco(VisualElement image, Object currentObject)
        {
            Texture2D preview = AssetPreview.GetAssetPreview(currentObject);

            if (preview != null)
                image.style.backgroundImage = new StyleBackground(preview);
            else
            {
                Texture2D miniIcon = AssetPreview.GetMiniThumbnail(currentObject);
                image.style.backgroundImage = new StyleBackground(miniIcon);
            }
        }
    }
}
