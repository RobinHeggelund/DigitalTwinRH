using UnityEngine;

public class MeshCycler : MonoBehaviour
{
    public GameObject[] meshes; // Array to hold the different mesh GameObjects
    private int currentMeshIndex = 0; // Index to track the currently active mesh

    void Start()
    {
        // Ensure that only the first mesh is active at the start
        ShowOnlyCurrentMesh();
    }

    void Update()
    {
        // Cycle to the next mesh when 'E' is pressed
        if (Input.GetKeyDown(KeyCode.E))
        {
            currentMeshIndex++;
            if (currentMeshIndex >= meshes.Length)
            {
                currentMeshIndex = 0; // Loop back to the first mesh
            }
            ShowOnlyCurrentMesh();
        }

        // Cycle to the previous mesh when 'Q' is pressed
        if (Input.GetKeyDown(KeyCode.Q))
        {
            currentMeshIndex--;
            if (currentMeshIndex < 0)
            {
                currentMeshIndex = meshes.Length - 1; // Loop back to the last mesh
            }
            ShowOnlyCurrentMesh();
        }
    }

    // Enable only the current mesh and disable all others
    void ShowOnlyCurrentMesh()
    {
        for (int i = 0; i < meshes.Length; i++)
        {
            if (i == currentMeshIndex)
            {
                meshes[i].SetActive(true);  // Show the current mesh
            }
            else
            {
                meshes[i].SetActive(false); // Hide all other meshes
            }
        }
    }
}
