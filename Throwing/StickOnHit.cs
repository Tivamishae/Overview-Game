using UnityEngine;

public class StickOnHit : MonoBehaviour
{
    public int itemID; // Must match InteractableObject.ItemID
    public Quaternion rotationOffset = Quaternion.identity;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity.normalized) * rotationOffset;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("MainCamera"))
            return;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }

        GameObject prefab = ItemDatabase.Instance.Get3DPrefabByID(itemID);
        GameObject newObj = Instantiate(prefab, transform.position, transform.rotation);
        newObj.transform.SetParent(collision.transform, true);

        Destroy(gameObject);
    }
}