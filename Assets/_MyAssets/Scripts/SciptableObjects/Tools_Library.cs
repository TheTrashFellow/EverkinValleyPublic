using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ToolLibrary", menuName = "Game/Tools Library")]
public class Tools_Library : ScriptableObject
{
    [System.Serializable]
    public class Tool
    {
        public int _toolId;
        public string _toolName;
        public GameObject _toolPrefabs;
    }

    public GameObject GetToolById(int toolId)
    {
        foreach (Tool tool in _tools)
        {
            if (tool._toolId == toolId)
            {
                return tool._toolPrefabs;
            }
        }
        return null;
    }

    public GameObject GetToolByNAme(string name)
    {
        foreach (Tool tool in _tools)
        {
            if (tool._toolName == name)
            {
                return tool._toolPrefabs;
            }
        }
        return null;
    }

    public List<Tool> _tools;
}