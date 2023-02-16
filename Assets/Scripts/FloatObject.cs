using UnityEngine;

public class FloatObject : MonoBehaviour
{
    [SerializeField] private float floatStrength = 1;
    [SerializeField] private float floatSpeed = 1;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time * floatSpeed) * floatStrength, 0);
    }
}
