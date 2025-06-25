using Photon.Pun;
using UnityEngine;

public class Bullet : MonoBehaviourPun
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == (int)EnumLayer.Player)
        {
            
        }
    }
}
