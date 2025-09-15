using DefaultNamespace.InputManager;

namespace DefaultNamespace
{
    [System.Serializable]
    public class LevelData
    {
        public int levelWidth;
        public int levelHeight;
        public PieceType[][] level;
    }
}