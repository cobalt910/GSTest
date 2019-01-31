using UnityEditor;

namespace NetworkFramework.Attributes.EasyButtons.Editor
{
    /// <summary>
    /// Custom inspector for Object including derived classes.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class ObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            this.DrawEasyButtons();

            // Draw the rest of the inspector as usual
            DrawDefaultInspector();
        }
    }
}
