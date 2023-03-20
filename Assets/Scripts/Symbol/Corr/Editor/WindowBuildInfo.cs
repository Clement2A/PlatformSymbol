using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class WindowBuildInfo : EditorWindow
{
    private List<BuildInfo> buildInfos = new List<BuildInfo>();
    private BuildTargetGroup targetGroup = BuildTargetGroup.Standalone;
    private BuildInfo currentBuildInfo = null;

    [MenuItem("Tool/Scripting Tool")]
    static void OpenWindow()
    {
        WindowBuildInfo _window = CreateWindow<WindowBuildInfo>("Scripting Tool");
        _window.Show();

    }

    private void OnEnable()
    {
        buildInfos = FindAssetByType<BuildInfo>();
        currentBuildInfo = buildInfos.FirstOrDefault(_asset => _asset.IsActive);
    }

    protected virtual void  OnGUI()
    {
        DrawButtons();
        DrawAllBuildInfo();
        DrawLine(Color.gray);
        DrawAllScriptingSymbol();
    }

    void DrawButtons()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reload all scripts"))
            SyncSolutionUtilities.Sync(true);
        if(GUILayout.Button("Apply"))
        {
            RemoveScriptingSymbols();
            AddScriptingSymbols();
        }
        if(GUILayout.Button("Refresh"))
            buildInfos = FindAssetByType<BuildInfo>();
        EditorGUILayout.EndHorizontal();
    }

    void RemoveScriptingSymbols()
    {
        if (currentBuildInfo == null) return;
        PlayerSettings.GetScriptingDefineSymbolsForGroup(currentBuildInfo.TargetGroup, out string[] _symbols);
        List<string> _symbolList = _symbols.ToList();
        foreach(BuildData _symbol in currentBuildInfo.Datas.Where(d => _symbolList.Contains(d.ScriptingSymbol.Symbol)))
            _symbolList.Remove(_symbol.ScriptingSymbol.Symbol);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(currentBuildInfo.TargetGroup, _symbolList.ToArray());
    }

    void AddScriptingSymbols()
    {
        if (currentBuildInfo == null) return;
        PlayerSettings.GetScriptingDefineSymbolsForGroup(currentBuildInfo.TargetGroup, out string[] _symbols);
        List<string> _symbolList = _symbols.ToList();
        foreach (BuildData _symbol in currentBuildInfo.Datas.Where(d => !_symbolList.Contains(d.ScriptingSymbol.Symbol) && d.IsUsed))
            _symbolList.Add(_symbol.ScriptingSymbol.Symbol);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(currentBuildInfo.TargetGroup, _symbolList.ToArray());
    }

    void DrawAllBuildInfo()
    {
        Space();
        int _length = buildInfos.Count;
        bool _isBegin = false;

        ResetAllBuildInfo();
        for (int i =0; i < _length; i++)
        {
            BuildInfo buildInfo = buildInfos[i];

            GetIsBegin(i, ref _isBegin);
            Color _color = buildInfo.IsActive ? new Color(0, 0.5f, 0) : new Color(0.5f, 0, 0);
            if(GUILayout.Button(buildInfo.TargetName, SetBackgroundColor(_color, Color.white), GUILayout.Width(150))) //SetCOlor
            {
                buildInfo.IsActive = !buildInfo.IsActive;
                currentBuildInfo = buildInfo;
                if (!buildInfos.Any(b => b.IsActive))
                    currentBuildInfo = null;
                EditorUserBuildSettings.SwitchActiveBuildTarget(buildInfo.TargetGroup, buildInfo.Target);
            }
        }
        if (_isBegin) EditorGUILayout.EndHorizontal();
    }

    void DrawAllScriptingSymbol()
    {
        if (currentBuildInfo == null) return;
        List<BuildData> _datas = currentBuildInfo.Datas;
        int _length = _datas.Count;
        bool _isBegin = false;
        for(int i =0; i < _length; i++)
        {
            BuildData _data = _datas[i];
            GetIsBegin(i, ref _isBegin);
            Color _color = _data.IsUsed ? new Color() : new Color();
            if (GUILayout.Button(_data.ScriptingSymbol.Label, SetBackgroundColor(_color, Color.white), GUILayout.Width(150)))
                _data.IsUsed = !_data.IsUsed;
        }
        if (_isBegin) EditorGUILayout.EndHorizontal();
    }


    void ResetAllBuildInfo() => buildInfos.ForEach(b => b.IsActive = false);

    void GetIsBegin(int _index, ref bool _isBegin)
    {
        if (_index % 2 == 0)
        {
            if (_index != 0)
            {
                EditorGUILayout.EndHorizontal();
                _isBegin = false;
            }
            _isBegin = true;
            EditorGUILayout.BeginHorizontal();
        }
    }

    GUIStyle SetBackgroundColor(Color _backgroundColor, Color _textColor)
    {
        GUIStyle _result = new GUIStyle(GUI.skin.button)
        {
            normal =
            {
                background = CreateTexture(10,10, _backgroundColor),
                textColor = _textColor
            }
        };
        return _result;
    }

    Texture2D CreateTexture(int _width, int _height, Color _color)
    {
        Color[] _pixels = new Color[_width * _height];
        for(int i = 0; i < _pixels.Length; i++)
            _pixels[i] = _color;
        Texture2D _texture = new Texture2D(_width, _height);
        _texture.SetPixels(_pixels);
        _texture.Apply();
        return _texture;
    }

    void Space(int _number = 1)
    {
        for (int i = 0; i < _number; i++)
            EditorGUILayout.Space();
    }

    List<T> FindAssetByType<T>() where T : Object
    {
        string[] _guids = AssetDatabase.FindAssets($"t:{typeof(T)}");
        return _guids
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<T>)
            .Where(_asset => _asset != null)
            .ToList();
    }

    void DrawLine(Color _color, int _thickness = 2, int _padding = 10)
    {
        Rect _rect = EditorGUILayout.GetControlRect(GUILayout.Height(_padding + _thickness));
        _rect.height = _thickness;
        _rect.y += _padding / 2;
        _rect.x -= 2;
        _rect.width += 6;
        EditorGUI.DrawRect(_rect, _color);
    }
}
