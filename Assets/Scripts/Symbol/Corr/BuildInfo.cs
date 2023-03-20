using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public struct BuildData
{
    [SerializeField] private ScriptableObject scriptingSymbol;
    [SerializeField, HideInInspector] private bool isUsed;

    public ScriptingSymbol ScriptingSymbol => ScriptingSymbol;
    public bool IsUsed {get => isUsed; set => isUsed = value;}
}

[CreateAssetMenu(fileName = "BUILD_", menuName = "Global Tool/Build Info")]
public class BuildInfo : ScriptableObject
{
    [SerializeField] private BuildTarget target = BuildTarget.StandaloneWindows;
    [SerializeField] private List<BuildData> datas = new List<BuildData>();
    [SerializeField, HideInInspector] private bool isActive = false;

    public BuildTarget Target => target;
    public BuildTargetGroup TargetGroup => BuildPipeline.GetBuildTargetGroup(Target);
    public List<BuildData> Datas => datas;
    public bool IsActive { get => isActive; set => isActive = value; }
    public string TargetName => $"{target}";

    public void AddSymbol() => datas.Add(new BuildData());
    public void RemoveAt(int _index) => datas.RemoveAt(_index);
    public void Clear() => datas.Clear();
}
