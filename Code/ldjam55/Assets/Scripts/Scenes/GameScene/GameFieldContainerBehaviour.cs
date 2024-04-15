using UnityEngine;

public class GameFieldContainerBehaviour : MonoBehaviour
{
    public object ContainedObject { get; set; }
    [SerializeField]
    public string ObjectType;

    public int ObjectId;
}
