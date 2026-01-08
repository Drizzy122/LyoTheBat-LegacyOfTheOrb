using System.Collections;
using System.Collections.Generic;
using Platformer;
using UnityEngine;

public class FractureObject : MonoBehaviour
{
    [field: Header("Refs")]
    [field:SerializeField] GameObject originalObject;
    [field: SerializeField] MeshRenderer meshRenderer;
    [field:SerializeField] GameObject fracturedObject;
    [field:SerializeField] GameObject explosionVFX;
    
    [field: Header("Explosion")]
    [field:SerializeField] float epxlosionMinForce = 5;
    [field:SerializeField] float explosionMaxForce = 100;
    [field:SerializeField] float explosionForceRadius = 10;
    [field:SerializeField] float fragScaleFactor = 1;

    private GameObject fractObj;
    public void Explode()
    {
        if (originalObject != null)
        {
            meshRenderer.enabled = false;
            if (fracturedObject != null)
            {
                fractObj = Instantiate(fracturedObject, transform.position , Quaternion.identity) as GameObject;

                foreach (Transform t in fractObj.transform)
                {
                    var rb = t.GetComponent<Rigidbody>();

                    if (rb != null)
                        rb.AddExplosionForce(Random.Range(epxlosionMinForce, explosionMaxForce), originalObject.transform.position, explosionForceRadius);

                    StartCoroutine(Shrink(t, 2));
                }
                AudioManager.instance.PlayOneShot(FMODEvents.instance.explode, this.transform.position);
                Destroy(fractObj, 5f);
                Destroy(originalObject, 5f);
                
                if (explosionVFX != null)
                {
                    // Pass the current object's position and rotation
                    GameObject exploVFX = Instantiate(explosionVFX, transform.position, Quaternion.identity) as GameObject;
                    Destroy(exploVFX, 7);
                }
            }
        }
    }
    IEnumerator Shrink (Transform t, float delay)
    {
        yield return new WaitForSeconds(delay);

        Vector3 newScale = t.localScale;

        while(newScale.x >= 0)
        {
            newScale -= new Vector3(fragScaleFactor, fragScaleFactor, fragScaleFactor);

            t.localScale = newScale;
            yield return new WaitForSeconds (0.05f);
        }
    }
}