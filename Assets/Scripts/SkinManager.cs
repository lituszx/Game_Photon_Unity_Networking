using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinManager : Photon.MonoBehaviour
{
    [System.Serializable]
    public class skinProperties
    {
        public string idPlayer;
        public int skin;
        public int changeSkin = -1;
        public skinProperties(string _id, int _skin)
        {
            idPlayer = _id; skin = _skin;
        }
    }
    public List<skinProperties> skins = new List<skinProperties>();
    public GameObject[] maps;
    public int randomMap;
    public GameObject actualMap;
    public void AddPlayer(string _id, int _skin)
    {
        if (!HasPlayer(_id))
        {
            skins.Add(new skinProperties(_id, _skin));
        }
    }
    private bool HasPlayer(string _id)
    {
        for (int i = 0; i < skins.Count; i++)
        {
            if (skins[i].idPlayer == _id) return true;
        }
        return false;
    }

    public void SetSkins()
    {
        for (int i = skins.Count - 1; i >= 0; i--)
        {
            if (HasSkin(skins[i].idPlayer, skins[i].skin))
            {
                skins[i].skin = Random.Range(0, 4);
                while (HasSkin(skins[i].idPlayer, skins[i].skin))
                {
                    skins[i].skin = Random.Range(0, 4);
                }
            }
        }
        for (int j = 0; j < skins.Count; j++)
        {
            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
            for (int h = 0; h < allPlayers.Length; h++)
            {
                if (allPlayers[h].GetComponent<PhotonView>().owner.UserId == skins[j].idPlayer)
                {
                    allPlayers[h].GetComponent<PhotonView>().RPC("ChangeSkin", PhotonTargets.All, skins[j].skin);

                }
            }
        }
    }
    public void SetMap()
    {
        randomMap = Random.Range(0, maps.Length);
        maps[randomMap].SetActive(true);
        actualMap = maps[randomMap];
    }
    public bool HasSkin(string _id, int _skin)
    {
        for (int i = 0; i < skins.Count; i++)
        {
            if (skins[i].idPlayer != _id)
            {
                if (skins[i].skin == _skin) return true;
            }
        }
        return false;
    }
}
