using UnityEngine;

namespace DefaultNamespace.InputManager
{
    public class PieceModel
    {
        public PieceType Type { get; set; }
        public bool IsMatched { get; set; }
        public bool IsFalling { get; set; }
        public bool IsSwapping { get; set; }
        public bool IsDestroyed { get; set; }
        
        public int FallingInFieldProcess;

        public Vector2Int PlaceForFall { get; set; }

        public PieceModel(PieceType type)
        {
            Type = type;
            IsMatched = false;
            IsFalling = false;
            IsSwapping = false;
            IsDestroyed = false;
        }

        public bool CanMove() =>
            IsMatched == false && IsFalling == false && IsSwapping == false;
    }
}