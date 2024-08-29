using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GranadeControl : Photon.MonoBehaviour
{
    private float count;

    void Start()
    {
        
    }
    void Update()
    {
        count += Time.deltaTime;
        if(count >= 4)
        {
            Collider[] allPlayers = Physics.OverlapSphere(transform.position, 4);
            for (int i = 0; i < allPlayers.Length; i++)
            {
                if(allPlayers[i].GetComponent<PlayerControl>() != null)
                {
                    int finalDamage = (int)(120 / Vector3.Distance(transform.position, allPlayers[i].transform.position));
                    allPlayers[i].GetComponent<PhotonView>().RPC("GetDamage", PhotonTargets.All, finalDamage);
                }
            }
            PhotonNetwork.Destroy(gameObject);
        }
    }
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            
        }
        else
        {
            
        }
    }
}
