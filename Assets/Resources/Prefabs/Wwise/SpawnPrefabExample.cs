using UnityEngine;

public class SpawnPrefabExample : MonoBehaviour
{
    public GameObject prefabToSpawn;
    private GameObject spawnedObject;
    public float destroyDelay = 1.0f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            spawnedObject = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
            Invoke("DestroyObject", destroyDelay);
        }
    }

    void DestroyObject()
    {
        Destroy(spawnedObject);
    }
}
