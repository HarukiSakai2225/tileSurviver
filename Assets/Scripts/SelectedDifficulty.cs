public static class SelectedDifficulty
{
    public static EnemyDifficultySettings CurrentSettings { get; private set; }

    public static void Set(EnemyDifficultySettings settings)
    {
        CurrentSettings = settings;
    }

    public static void Clear()
    {
        CurrentSettings = null;
    }
}