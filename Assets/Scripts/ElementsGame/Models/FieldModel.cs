namespace DefaultNamespace.InputManager
{
    public class FieldModel
    {
        public FieldData FieldData;
        public int FieldProcesses = 0;
        public int FieldActiveProcesses = 0;
        public bool FieldCalm => FieldActiveProcesses == 0;
        public int PiecesLeft;
    }
}