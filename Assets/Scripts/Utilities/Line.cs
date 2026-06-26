using UnityEngine;

public class Line : MonoBehaviour
{
    [SerializeField] private Transform player;
    void Update()
    {
        transform.position = player.position;
    }
}
