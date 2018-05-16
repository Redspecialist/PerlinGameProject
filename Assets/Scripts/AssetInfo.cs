using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AssetInfo {

    private static int characterNumber;

    public static void setCharacterNumber(int num)
    {
        characterNumber = num;
    }
    public static int getCharacterNumber()
    {
        return characterNumber;
    }
}
