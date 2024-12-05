using UnityEngine;

public class handfollow : MonoBehaviour
{
    public Transform target; // The object to follow (assign in the Inspector)

    // Optional offset to maintain a specific position relative to the target
    public Vector3 positionOffset;

    // Optional rotation offset
    public Vector3 rotationOffset;

    void LateUpdate()
    {
        if (target != null)
        {
            // Match position with optional offset
            transform.position = target.position + positionOffset;

            // Match rotation with optional offset
            //transform.rotation = target.rotation * Quaternion.Euler(rotationOffset);
        }
    }
}
