using UnityEngine;

public class ToolsProperties : MonoBehaviour
{
    [SerializeField] private Vector2Int _damages;
    [SerializeField] private int[] _rangeAmount;

    public Vector2Int Damages => _damages;
    public int[] RangeAmount => _rangeAmount;
}
