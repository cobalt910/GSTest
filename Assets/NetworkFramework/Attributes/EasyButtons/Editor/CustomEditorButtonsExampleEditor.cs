using UnityEditor;

namespace NetworkFramework.Attributes.EasyButtons.Editor
{
    [CustomEditor(typeof(CustomEditorButtonsExample))]
    public class CustomEditorButtonsExampleEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            this.DrawEasyButtons();
            base.OnInspectorGUI();
        }
    }
}
