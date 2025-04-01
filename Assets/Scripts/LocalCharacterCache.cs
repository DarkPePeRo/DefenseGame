using System.Collections.Generic;

public static class LocalCharacterCache
{
    public static List<CharacterSaveData> cachedProgress = new();
    public static List<PlacementData> cachedPlacements = new();

    public static void Clear()
    {
        cachedProgress.Clear();
        cachedPlacements.Clear();
    }
}