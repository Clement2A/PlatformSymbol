using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using System;

public class SymbolEditorWindow : EditorWindow
{
    [SerializeField] Dictionary<string,bool> symbolsState = new();
    List<string> offSymbols = new();
    List<string> onSymbols = new();
    List<string> removedSymbols = new();
    const string RSP_FILE = "csc.rsp";
    const string JSON_FILE = "csc.json";
    string newSymbol = "";
    const int MARGIN = 5;
    readonly string[] KNOWN_SYMBOLS = {"TestSymbol","OtherTestSymbol","UNITY_SYMBOL","VeryLongSymbolNameToTestStuff" };
    Rect leftPanel, rightPanel, middlePanel, controlPanel;
    GUIStyle elementPanelStyle, elementNormalStyle, elementSwitchedStyle, removeButtonStyle, restoreButtonStyle;
    Vector2 scrollPositionLeft, scrollPositionMiddle, scrollPositionRight;
    Texture2D normalTexture, hoverTexture, switchTexture, removeNormalTexture, removeHoverTexture, restoreNormalTexture, restoreHoverTexture;

    void OnEnable()
    {
        LoadDatas();
        SetNormalTexture();
        SetHoverTexture();
        SetSwitchTexture();
        SetRemoveNormalTexture();
        SetRemoveHoverTexture();
        SetRestoreNormalTexture();
        SetRestoreHoverTexture();
        minSize = new Vector2(370,250);

    }

    void OnGUI()
    {
        SetElementPanelStyle();
        SetNormalElementStyle();
        SetSwitchedElementStyle();
        SetRemoveButtonStyle();
        SetRestoreButtonStyle();
        float _elementPanelWidth = (position.width - 20) / 3;
        float _elementPanelHeight = position.height - MARGIN * 2;
        leftPanel = new Rect(MARGIN, MARGIN, _elementPanelWidth, _elementPanelHeight);
        middlePanel = new Rect(MARGIN * 2 + _elementPanelWidth, MARGIN + _elementPanelHeight / 2, _elementPanelWidth, _elementPanelHeight / 2);
        controlPanel = new Rect(MARGIN * 2 + _elementPanelWidth, MARGIN, _elementPanelWidth, _elementPanelHeight / 2);
        rightPanel = new Rect(MARGIN*3 + _elementPanelWidth * 2, MARGIN, _elementPanelWidth, _elementPanelHeight);
        GUILayout.BeginArea(leftPanel, elementPanelStyle);
            GUILayout.BeginVertical();
                scrollPositionLeft = EditorGUILayout.BeginScrollView(scrollPositionLeft, GUILayout.Width(_elementPanelWidth), GUILayout.Height(_elementPanelHeight));
                    for(int i = 0; i<offSymbols.Count;i++)
                    {
                        GUILayout.BeginHorizontal();
                            if(GUILayout.Button("X", removeButtonStyle))
                            {
                                RemoveSymbol(offSymbols[i]);
                                i--;
                                continue;
                            }
                            if (GUILayout.Button(offSymbols[i], !symbolsState[offSymbols[i]] ? elementNormalStyle : elementSwitchedStyle))
                            {
                                SwitchOnSymbol(i);
                            }
                        GUILayout.EndHorizontal();
                    }
                GUILayout.EndScrollView();
            GUILayout.EndVertical();
        GUILayout.EndArea();
        GUILayout.BeginArea(controlPanel);
            GUILayout.BeginVertical();
                if(GUILayout.Button("Refresh symbols"))
                    ConfirmChanges();GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                    newSymbol = GUILayout.TextField(newSymbol);
                    if(GUILayout.Button("Create"))
                        AddSymbol(newSymbol);
                GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        GUILayout.EndArea();
        GUILayout.BeginArea(middlePanel, elementPanelStyle);
            GUILayout.BeginVertical();
                scrollPositionMiddle = EditorGUILayout.BeginScrollView(scrollPositionMiddle, GUILayout.Width(_elementPanelWidth), GUILayout.Height(_elementPanelHeight/2));
                    for(int i = 0; i< removedSymbols.Count;i++)
                    {
                        GUILayout.BeginHorizontal();
                        if(GUILayout.Button("O", restoreButtonStyle))
                        {
                            RestoreSymbol(removedSymbols[i]);
                        }
                        if(!(i< removedSymbols.Count)) break;
                        if (GUILayout.Button(removedSymbols[i], elementNormalStyle))
                        {
                            SwitchOffSymbol(i);
                        }
                        GUILayout.EndHorizontal();
                    }
                GUILayout.EndScrollView();
            GUILayout.EndVertical();
        GUILayout.EndArea();
        GUILayout.BeginArea(rightPanel, elementPanelStyle);
            GUILayout.BeginVertical();
                scrollPositionRight = EditorGUILayout.BeginScrollView(scrollPositionRight, GUILayout.Width(_elementPanelWidth), GUILayout.Height(_elementPanelHeight));
                    for(int i = 0; i<onSymbols.Count;i++)
                    {
                        GUILayout.BeginHorizontal();
                        if(GUILayout.Button("X", removeButtonStyle))
                        {
                            RemoveSymbol(onSymbols[i]);
                            i--;
                            continue;
                        }
                        if(!(i<onSymbols.Count)) break;
                        if (GUILayout.Button(onSymbols[i], symbolsState[onSymbols[i]] ? elementNormalStyle : elementSwitchedStyle))
                        {
                            SwitchOffSymbol(i);
                        }
                        GUILayout.EndHorizontal();
                    }
                GUILayout.EndScrollView();
            GUILayout.EndVertical();
        GUILayout.EndArea();
        Repaint();
    }

    void SetNormalTexture()
    {
        normalTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
        normalTexture.SetPixel(0, 0, new Color(0, 0, 0, 0));
        normalTexture.Apply();
    }

    void SetHoverTexture()
    {
        hoverTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
        hoverTexture.SetPixel(0, 0, new Color(0, 0, 0, .2f));
        hoverTexture.Apply();
    }

    void SetSwitchTexture()
    {
        switchTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
        switchTexture.SetPixel(0, 0, new Color(0, .2f, 0, .2f));
        switchTexture.Apply();
    }

    void SetRemoveNormalTexture()
    {
        removeNormalTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
        removeNormalTexture.SetPixel(0, 0, new Color(.5f, .01f,.01f, 1));
        removeNormalTexture.Apply();
    }

    void SetRemoveHoverTexture()
    {
        removeHoverTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
        removeHoverTexture.SetPixel(0, 0, new Color(.7f, .05f, .05f, 1));
        removeHoverTexture.Apply();
    }

    void SetRestoreNormalTexture()
    {
        restoreNormalTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
        restoreNormalTexture.SetPixel(0, 0, new Color(.01f, .4f,.01f, 1));
        restoreNormalTexture.Apply();
    }

    void SetRestoreHoverTexture()
    {
        restoreHoverTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
        restoreHoverTexture.SetPixel(0, 0, new Color(.05f, .6f, .05f, 1));
        restoreHoverTexture.Apply();
    }

    void SwitchOnSymbol(int _index)
    {
        onSymbols.Add(offSymbols[_index]);
        offSymbols.RemoveAt(_index);
    }

    void SwitchOffSymbol(int _index)
    {
        offSymbols.Add(onSymbols[_index]);
        onSymbols.RemoveAt(_index);
    }

    void SetElementPanelStyle()
    {
        Texture2D _bgColor = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
        _bgColor.SetPixel(0, 0, new Color(.03f,.03f,.03f));
        _bgColor.Apply();
        elementPanelStyle = new GUIStyle();
        elementPanelStyle.margin = new RectOffset(3,3,3,3);
        elementPanelStyle.normal.background = _bgColor;
    }

    void SetNormalElementStyle()
    {
        elementNormalStyle = new GUIStyle(GUI.skin.button);
        elementNormalStyle.normal.textColor = Color.white;
        elementNormalStyle.hover.textColor = Color.cyan;
        elementNormalStyle.alignment = TextAnchor.MiddleLeft;
        elementNormalStyle.normal.background = normalTexture;
        elementNormalStyle.hover.background = normalTexture;
    }

    void SetSwitchedElementStyle()
    {
        elementSwitchedStyle = new GUIStyle(GUI.skin.button);
        elementSwitchedStyle.normal.textColor = Color.white;
        elementSwitchedStyle.hover.textColor = Color.cyan;
        elementSwitchedStyle.alignment = TextAnchor.MiddleLeft;
        elementSwitchedStyle.normal.background = switchTexture;
        elementSwitchedStyle.hover.background = switchTexture;
    }

    void SetRemoveButtonStyle()
    {
        removeButtonStyle = new GUIStyle(GUI.skin.button);
        removeButtonStyle.fixedWidth = 20;
        removeButtonStyle.normal.background = removeNormalTexture;
        removeButtonStyle.hover.background = removeHoverTexture;
        removeButtonStyle.padding = new RectOffset(2, 0, 2, 0);
    }

    void SetRestoreButtonStyle()
    {
        restoreButtonStyle = new GUIStyle(GUI.skin.button);
        restoreButtonStyle.fixedWidth = 20;
        restoreButtonStyle.normal.background = restoreNormalTexture;
        restoreButtonStyle.hover.background = restoreHoverTexture;
        restoreButtonStyle.padding = new RectOffset(2, 0, 2, 0);
    }


    bool LoadDatas()
    {
        symbolsState = LoadSymbols();
        if (symbolsState == null)
            return false;
        if (symbolsState.Count == 0)
            return true;
        onSymbols.Clear();
        offSymbols.Clear();
        foreach (KeyValuePair<string, bool> _symbol in symbolsState)
            (_symbol.Value ? onSymbols : offSymbols).Add(_symbol.Key);
        return true;
    }

    void RemoveSymbol(string _symbol)
    {
        removedSymbols.Add(_symbol);
        (symbolsState[_symbol] ? onSymbols : offSymbols).Remove(_symbol);
    }

    void RestoreSymbol(string _symbol)
    {
        if (!symbolsState.ContainsKey(_symbol))
            return;
        (symbolsState[_symbol] ? onSymbols : offSymbols).Add(_symbol);
        removedSymbols.Remove(_symbol);
    }

    bool AddSymbol(string _symbol)
    {
        if (symbolsState.ContainsKey(_symbol))
            return false;
        symbolsState.Add(_symbol, false);
        onSymbols.Add(_symbol);
        return true;
    }

    Dictionary<string, bool> LoadSymbols()
    {
        string _path = Path.Combine(Application.dataPath, JSON_FILE);
        if (!File.Exists(_path))
            return new();
        return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, bool>>(File.ReadAllText(_path));
    }

    void ConfirmChanges()
    {
        foreach (string _symbol in removedSymbols)
            symbolsState.Remove(_symbol);
        removedSymbols.Clear();
        foreach (string _symbol in onSymbols)
            symbolsState[_symbol] = true;
        foreach (string _symbol in offSymbols)
            symbolsState[_symbol] = false;
        PopulateRspFile();
    }

    void PopulateRspFile()
    {
        string _path = Path.Combine(Application.dataPath, RSP_FILE);
        if (!File.Exists(_path))
            File.Create(_path).Dispose();
        string _rspFileContent = "";
        foreach(KeyValuePair<string, bool> _symbol in symbolsState)
        {
            if(!_symbol.Value)
                continue;
            _rspFileContent += $"-define:{_symbol.Key}\n";
        }
        File.WriteAllText(_path, string.Empty);
        File.WriteAllText(_path, _rspFileContent);
        UpdateJsonFile();
        AssetDatabase.Refresh();
    }

    void UpdateJsonFile()
    {
        string _path = Path.Combine(Application.dataPath, JSON_FILE);
        if (!File.Exists(_path))
            File.Create(_path).Dispose();
        string _result = Newtonsoft.Json.JsonConvert.SerializeObject(symbolsState);
        File.WriteAllText(_path, string.Empty);
        File.WriteAllText(_path, _result);//
    }
}

[Serializable]
public class SymbolBoolPair
{
    public string symbol = "";
    public bool value = false;

    public SymbolBoolPair(string symbol, bool value)
    {
        this.symbol = symbol;
        this.value = value;
    }

    public override string ToString()
    {
        return $"{symbol} - {value}";
    }
}