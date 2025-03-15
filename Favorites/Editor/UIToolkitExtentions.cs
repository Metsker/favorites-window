using UnityEngine.UIElements;

namespace Favorites.Editor
{
    //source: https://github.com/adammyhre/Unity-Utils
    public static class UIToolkitExtentions
    {
        public static VisualElement CreateChild(this VisualElement parent, params string[] classes) {
            var child = new VisualElement();
            child.AddClass(classes).AddTo(parent);
            return child;
        }

        public static T CreateChild<T>(this VisualElement parent, params string[] classes)
            where T : VisualElement, new() {
            var child = new T();
            child.AddClass(classes).AddTo(parent);
            return child;
        }

        public static T AddTo<T>(this T child, VisualElement parent) where T : VisualElement {
            parent.Add(child);
            return child;
        }
        
        public static void RemoveFrom<T>(this T child, VisualElement parent)
            where T : VisualElement => parent.Remove(child);

        public static T AddClass<T>(this T visualElement, params string[] classes) where T : VisualElement {
            foreach (string cls in classes) {
                if (!string.IsNullOrEmpty(cls)) {
                    visualElement.AddToClassList(cls);
                }
            }
            return visualElement;
        }
        
        public static void RemoveClass<T>(this T visualElement, params string[] classes) where T : VisualElement {
            foreach (string cls in classes) {
                if (!string.IsNullOrEmpty(cls)) {
                    visualElement.RemoveFromClassList(cls);
                }
            }
        }

        public static T WithManipulator<T>(this T visualElement, IManipulator manipulator) where T : VisualElement {
            visualElement.AddManipulator(manipulator);
            return visualElement;
        }
    }
}
