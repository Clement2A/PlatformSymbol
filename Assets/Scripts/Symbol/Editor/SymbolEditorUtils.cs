using UnityEditor;

public class SymbolEditorUtils
{
    [MenuItem("Symbols/Edit...")]
    public static void CallSymbolEditor()
    {
        EditorWindow.GetWindow<SymbolEditorWindow>(true, "Symbol Editor");
    }
}
