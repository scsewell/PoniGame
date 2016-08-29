using UnityEngine;
using System.Collections;

public class CollisionNotifier : MonoBehaviour
{
    public delegate void OnCollisionHandler();
    public OnCollisionHandler OnCollision;
    
    private void OnCollisionEnter(Collision collision)
    {
        if (OnCollision != null)
        {
            OnCollision();
        }
    }
}
