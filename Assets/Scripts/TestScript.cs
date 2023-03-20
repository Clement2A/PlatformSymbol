using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    void Update()
    {
#if ThisSymbolDoesNotExist
        Debug.Log("ThisSymbolDoesNotExist actually exists");
#endif
#if TestSymbol
        Debug.Log("TestSymbol exists");
#endif
#if OtherTestSymbol
        Debug.Log("OtherTestSymbol exists");
#endif
    }
}
