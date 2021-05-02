using UnityEngine;

[CreateAssetMenu(fileName = "New Edge", menuName = "Edge")]
public class Edge : ScriptableObject
{
    public Node[] connectedNodes = new Node[2];
    public int weight = 0;
}