using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class GrabbableObject : MonoBehaviour
{
    private Rigidbody objectRigidbody;
    private Transform objectGrabPoint;

    private float lerpSpeed = 10f;

    private void Awake()
    {
        objectRigidbody = GetComponent<Rigidbody>();
    }

    public void Grab(Transform _objectGrabPoint)
    {
        this.objectGrabPoint = _objectGrabPoint;
        objectRigidbody.useGravity = false;
        objectRigidbody.linearVelocity = Vector3.zero;
    }

    public void Drop()
    {
        if (objectGrabPoint != null)
        {
        this.objectGrabPoint = null;
        objectRigidbody.useGravity = true;
        }
    }

    private void FixedUpdate()
    {
        if (objectGrabPoint != null)
        {
            Vector3 newPosition = Vector3.Lerp(transform.position, objectGrabPoint.position, lerpSpeed * Time.deltaTime);
            objectRigidbody.MovePosition(newPosition);
        }
    }
}
