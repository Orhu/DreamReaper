using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressManager : MonoBehaviour {
    // This script tracks what levels have been unlocked and the best moves score for each level, will be displayed on level select screen
    public static bool[] unlockedLevels = new bool[]{true, false, false, false, false, false, false, false, false, false, false, false};
    public static int[] bestMoves = new int[]{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1};

    public static void UnlockLevel(int levelNum) {
        if (levelNum < unlockedLevels.Length) {
            unlockedLevels[levelNum] = true;
        }
    }

    public static void UpdateBestMoves(int levelNum, int numMoves) {
        if (levelNum >= unlockedLevels.Length)
            return;
        
        if (bestMoves[levelNum] == -1 || bestMoves[levelNum] > numMoves) {
            bestMoves[levelNum] = numMoves;
        }
    }

    public static void ClearLevel(int levelNum, int numMoves) {
        if (levelNum < unlockedLevels.Length) {
            UnlockLevel(levelNum + 1);
        }

        UpdateBestMoves(levelNum, numMoves);
    }
}
