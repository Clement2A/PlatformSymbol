using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SYMB_", menuName = "Global Tool/Scripting Symbol")]
public class ScriptingSymbol : ScriptableObject
{
    [SerializeField] string label = "";
    [SerializeField] string symbol = "";

    public string Symbol => symbol;
    public string Label => label;
}
