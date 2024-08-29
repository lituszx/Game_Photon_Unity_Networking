using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseControl : MonoBehaviour
{
    public GameObject dead, waterSplash, ice;
    private GameObject mutemusic;
    private void Start()
    {        
        GameObject tempIce = Instantiate(ice);
        mutemusic = tempIce;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {   
            if(collision.gameObject.GetComponent<PhotonView>().photonView.isMine)
            {
            mutemusic.GetComponent<AudioSource>().mute = true;
            GameObject tempDead = Instantiate(dead);
            Destroy(tempDead, 7);
            GameObject tempWater = Instantiate(waterSplash);
            Destroy(tempWater, 2);
            Invoke("Respawn", 4f);
            }
        }
    }
    private void Respawn()
    {
        mutemusic.GetComponent<AudioSource>().mute = false;
    }
}
