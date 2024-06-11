using UnityEditor;

public class SymbolEditorUtils
{
    [MenuItem("Edit/Edit symbols")]
    public static void CallSymbolEditor()
    {
        EditorWindow.GetWindow<SymbolEditorWindow>(true, "Symbol Editor");
    }
}
