using UnityEngine;

public class DropCube : MonoBehaviour
{

    public GameObject cube;

    // Update is called once per frame
    void Update()
    {
       if (Input.GetKeyDown(KeyCode.Space)){
           cube.SetActive(true);
       } 
    }
}
