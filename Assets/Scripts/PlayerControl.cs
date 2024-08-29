using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerControl : Photon.MonoBehaviour
{
    private Vector3 moveDir;
    public float speed, jumpSpeed, gravity, rotSpeed, hitforce, gobackforce, timerQ, timerE, EndR;
    private float currentR, currentPower, rotX, rotY;
    private Vector3 currentPos;
    private Quaternion currentRot;
    private RaycastHit hitInfo;
    private Ray rayInfo;
    public GameObject playerCach, Pulser, Oponent;
    public Text nickName, lifeText;
    private int life = 100;
    public int skin = -1;
    private bool hasSkin = false, currentE;
    public GameObject uiMine;
    public List<GameObject> uiPlayer;
    public int ultiPower, lineDistance;
    private Rigidbody rigid;
    public bool isGrounded, catched, caching = false;
    private Vector3 dir, firstposE;
    public enum PlayerState { NORMAL, CACHED, ATTACK_E }
    public PlayerState state;
    private SkinManager manager;
    static public float gameStartTime = 10;
    private bool startGame, reciveSkin, firstTake;
    public Image pulserBar, UltiBar, ultiSkin;
    public List<Sprite> UIskin;
    public List<Sprite> OtherPlayers;
    public Animator anim;
    public GameObject damage, splashpoint, splash;
    private GameObject loseplayer;
    private Vector3 originalPos;
    private void Awake()
    {
        splashpoint = Camera.main.transform.GetChild(0).gameObject;
        ultiPower = 6000;
    }
    [PunRPC]
    public void EspectadorPos()
    {
        transform.position = new Vector3(-2000, 0, 2000);
    }
    [PunRPC]
    public void ChangeSkin(int _skin)
    {
        skin = _skin;
        GameObject mySkin = Resources.Load<GameObject>(skin.ToString());
        if (mySkin != null)
        {
            if (transform.Find("Skin") != null) Destroy(transform.Find("Skin").gameObject);
            GameObject newSkin = Instantiate(mySkin, transform);
            newSkin.name = "Skin";
            newSkin.transform.localPosition = new Vector3(0f, -0.85f, 0f);
            anim = newSkin.GetComponent<Animator>();
        }
        //Cambiar UI
        ultiSkin.sprite = UIskin[skin];
        DelayUI();
    }
    void Start()
    {
        if (photonView.owner.IsMasterClient)
        {
            gameStartTime = 10;
            startGame = false;
            manager = GameObject.Find("SkinManager").GetComponent<SkinManager>();
        }
        catched = false;
        rigid = GetComponent<Rigidbody>();
        if (photonView.isMine)
        {
            uiMine.SetActive(true);
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0, 1, -2f);
            Camera.main.transform.localRotation = Quaternion.Euler(15f, 0, 0);
            //Cursor.visible = false;
            //Cursor.lockState = CursorLockMode.Locked;
            gameObject.layer = 2;
            nickName.gameObject.SetActive(false);
            lifeText.gameObject.SetActive(false);
            skin = PlayerPrefs.GetInt("Skin");
            GameObject mySkin = Resources.Load<GameObject>(skin.ToString());
            if (mySkin != null)
            {
                GameObject newSkin = Instantiate(mySkin, transform);
                newSkin.name = "Skin";
                newSkin.transform.localPosition = new Vector3(0f, -0.85f, 0f);
                anim = newSkin.GetComponent<Animator>();
                manager.AddPlayer(photonView.owner.UserId, skin);
            }
            ultiSkin.sprite = UIskin[skin];
        }
        else
        {
            ultiSkin.transform.parent.gameObject.SetActive(false);
            nickName.text = photonView.owner.NickName;
            lifeText.text = life.ToString();
        }
        originalPos = transform.position;
    }
    void Update()
    {
        if(photonView.isMine)
        {
            if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
            {
                anim.SetBool("Move", false);
            }
            else
            {
                anim.SetBool("Move", true);
            }
            if (isGrounded == false)
            {
                anim.SetBool("Unground", true);
            }
            else
            {
                anim.SetBool("Unground", false);
            }
        }       
        currentPower = Mathf.Lerp(ultiPower, currentPower, 20 * Time.deltaTime);
        UltiBar.fillAmount = (currentPower / 200);
        if (ultiPower < 100)
        {
            UltiBar.color = Color.red;
        }
        if (ultiPower >= 100)
        {
            UltiBar.color = Color.yellow;
        }
        if (ultiPower >= 200)
        {
            UltiBar.color = Color.blue;
        }
        if (photonView.owner.IsMasterClient && startGame == false)
        {
            gameStartTime -= Time.deltaTime;
            if (gameStartTime <= 0)
            {
                manager.SetSkins();
                startGame = true;
            }
        }
        //timerE += Time.deltaTime;
        if (photonView.isMine)
        {
            switch (state)
            {
                case PlayerState.NORMAL:
                    anim.SetBool("Damage", false);
                    anim.SetBool("Hit", false);
                    EndR = 40;
                    timerQ += Time.deltaTime;
                    rigid.isKinematic = false;
                    rotX += Input.GetAxis("Mouse X") * rotSpeed;
                    transform.rotation = Quaternion.Euler(0, rotX, 0);
                    if (isGrounded)
                    {
                        moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                        moveDir *= speed;
                        moveDir = transform.TransformDirection(moveDir);
                        if (Input.GetButtonDown("Jump"))
                        {
                            rigid.AddForce(Vector3.up * jumpSpeed);
                        }
                        rigid.velocity = new Vector3(moveDir.x, rigid.velocity.y, moveDir.z);
                    }
                    if (timerQ > 0.5f)
                    {
                        if (Input.GetKeyDown(KeyCode.Q))
                        {
                            if (Physics.Raycast(transform.position, transform.forward, out hitInfo, lineDistance))
                            {
                                Debug.DrawLine(transform.position, hitInfo.point, Color.green);
                                if (hitInfo.collider.tag == "Player")
                                {
                                    anim.SetBool("Hit", true);
                                    hitInfo.collider.GetComponent<PhotonView>().RPC("Hit", PhotonTargets.Others, transform.forward);
                                    ultiPower = ultiPower + 25;
                                    timerQ = 0;
                                }
                            }
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, lineDistance))
                        {
                            Debug.DrawLine(transform.position, hitInfo.point, Color.green);
                            if (hitInfo.collider.tag == "Player")
                            {
                                if (ultiPower >= 100)
                                {
                                    anim.SetBool("Final", true);
                                    //CargaAtras + Carga para delante
                                    firstposE = transform.position;
                                    rigid.AddForce(Vector3.up * jumpSpeed);
                                    Invoke("PrepareE", 0.1f);
                                    ultiPower -= 100;
                                    state = PlayerState.ATTACK_E;
                                }
                            }
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        if (caching == false)
                        {
                            if (Physics.Raycast(transform.position, transform.forward, out hitInfo, lineDistance))
                            {
                                Debug.DrawLine(transform.position, hitInfo.point, Color.green);
                                if (hitInfo.collider.tag == "Player")
                                {
                                    if (ultiPower >= 200)
                                    {
                                        speed = speed - 4;
                                        anim.SetBool("Coger", true);
                                        //Levantar contrincante hasta enemigo llene barra.
                                        Oponent = hitInfo.collider.gameObject;
                                        Oponent.transform.SetParent(transform);
                                        Oponent.GetComponent<PhotonView>().RPC("CatchedID", PhotonTargets.All, photonView.ownerId);
                                        //CatchedID
                                        ultiPower -= 200;
                                        caching = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            speed = 7;
                            anim.SetBool("Coger", false);
                            anim.SetBool("Lanzar", true);
                            Oponent.GetComponent<PhotonView>().RPC("ResetCatched", PhotonTargets.All);
                            Oponent.GetComponent<PhotonView>().RPC("Hit", PhotonTargets.All, transform.forward);
                            Oponent.transform.SetParent(null);
                            state = PlayerState.NORMAL;
                            caching = false;
                            Oponent = null;
                        }
                        anim.SetBool("Lanzar", false);
                    }
                    if (Oponent != null)
                    {
                        Oponent.transform.position = playerCach.transform.position;
                        //Oponent.GetComponent<PhotonView>().RPC("UltiCatched", PhotonTargets.All, Oponent.transform.position);
                    }
                    break;
                case PlayerState.CACHED:
                    anim.SetBool("Unground", true);
                    rigid.isKinematic = true;
                    //Activar Canvas pulsador deshabilitar
                    Pulser.SetActive(true);
                    EndR -= Time.deltaTime;
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        EndR += 5;

                        if (EndR >= 100)
                        {
                            print("Libre");
                            transform.parent.GetComponent<PhotonView>().RPC("RemoveCatched", PhotonTargets.All);
                            GetComponent<PhotonView>().RPC("ResetCatched", PhotonTargets.All);
                            Oponent = null;
                            anim.SetBool("Unground", false);
                        }
                    }
                    currentR = Mathf.Lerp(currentR, EndR, 20 * Time.deltaTime);
                    pulserBar.fillAmount = (currentR / 100);
                    break;
                case PlayerState.ATTACK_E:
                    if (isGrounded)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, firstposE, 40 * Time.deltaTime);
                        if (transform.position == firstposE)
                        {
                            anim.SetBool("Final", false);
                            anim.SetBool("Hit", true);
                            hitInfo.collider.GetComponent<PhotonView>().RPC("Hit", PhotonTargets.Others, transform.forward);
                            state = PlayerState.NORMAL;
                        }
                    }
                    break;
            }
            //UI otros jugadores
        }
        else
        {
            lifeText.transform.parent.transform.rotation = Camera.main.transform.rotation;
            lifeText.text = life.ToString();

            if (transform.parent != null)
            {
                transform.position = Vector3.Lerp(transform.position, currentPos, 5 * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, currentRot, 5 * Time.deltaTime);
            }
        }
        if (reciveSkin)
        {
            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
            for (int h = 0; h < allPlayers.Length; h++)
            {
                if (allPlayers[h].GetComponent<PhotonView>().owner.IsMasterClient)
                {
                    allPlayers[h].GetComponent<PlayerControl>().SendSkin(photonView.owner.UserId, skin);
                }
            }
            reciveSkin = false;
        }
    }
    public void DelayUI()
    {
        Invoke("SetUI", 1);
    }
    public void SetUI()
    {
        for (int i = 0; i < uiPlayer.Count; i++)
        {
            uiPlayer[i].SetActive(false);
        }

        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        print(allPlayers.Length);
        int currentPlayer = 0;
        for (int h = 0; h < allPlayers.Length; h++)
        {
            if (allPlayers[h].GetComponent<PhotonView>().isMine == false)
            {
                uiPlayer[currentPlayer].SetActive(true);
                uiPlayer[currentPlayer].GetComponent<Image>().sprite = OtherPlayers[allPlayers[h].GetComponent<PlayerControl>().skin];
                currentPlayer++;
            }
        }
    }
    public void SendSkin(string _id, int _skin)
    {
        print(photonView.owner.IsMasterClient);
        if (photonView.owner.IsMasterClient)
        {
            manager.AddPlayer(_id, _skin);
        }
    }
    private void FixedUpdate()
    {
        if (!photonView.isMine && transform.parent == null)
        {
            transform.position = Vector3.Lerp(transform.position, currentPos, 5 * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, currentRot, 5 * Time.deltaTime);
        }
    }
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //Yo en local
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(skin);
            stream.SendNext(rigid.isKinematic);
            stream.SendNext(GetComponent<CapsuleCollider>().enabled);
            stream.SendNext(GetComponent<SphereCollider>().enabled);
            stream.SendNext((int)state);
            stream.SendNext(anim.GetBool("Move"));
            stream.SendNext(anim.GetBool("Coger"));
            stream.SendNext(anim.GetBool("Lanzar"));
            stream.SendNext(anim.GetBool("Unground"));
            stream.SendNext(anim.GetBool("Final"));
            stream.SendNext(anim.GetBool("Hit"));
            stream.SendNext(anim.GetBool("Damage"));
        }
        else
        {
            //Yo online           
            currentPos = (Vector3)stream.ReceiveNext();
            currentRot = (Quaternion)stream.ReceiveNext();
            int currentSkin = (int)stream.ReceiveNext();
            if (currentSkin != skin)
            {
                skin = currentSkin;
                GameObject mySkin = Resources.Load<GameObject>(skin.ToString());
                if (transform.Find("Skin") != null) Destroy(transform.Find("Skin").gameObject);
                if (mySkin != null)
                {
                    GameObject newSkin = Instantiate(mySkin, transform);
                    newSkin.name = "Skin";
                    newSkin.transform.localPosition = new Vector3(0f, -0.85f, 0f);
                    anim = newSkin.GetComponent<Animator>();
                }
                //ultiSkin.sprite = UIskin[skin];
            }
            bool kinematic = (bool)stream.ReceiveNext();
            if (rigid != null) rigid.isKinematic = kinematic;
            GetComponent<CapsuleCollider>().enabled = (bool)stream.ReceiveNext();
            GetComponent<SphereCollider>().enabled = (bool)stream.ReceiveNext();
            state = (PlayerState)((int)stream.ReceiveNext());
            if (!firstTake)
            {
                reciveSkin = true;
                firstTake = true;
            }
            anim.SetBool("Move", (bool)stream.ReceiveNext());
            anim.SetBool("Coger", (bool)stream.ReceiveNext());
            anim.SetBool("Lanzar", (bool)stream.ReceiveNext());
            anim.SetBool("Unground", (bool)stream.ReceiveNext());
            anim.SetBool("Final", (bool)stream.ReceiveNext());
            anim.SetBool("Hit", (bool)stream.ReceiveNext());
            anim.SetBool("Damage", (bool)stream.ReceiveNext());
        }
    }
    //Tirar para atras si te dan con la Q
    [PunRPC]
    public void Hit(Vector3 _dir)
    {
        if (photonView.isMine)
        {
            GameObject tempDamage = Instantiate(damage);
            Destroy(tempDamage, 2);
            anim.SetBool("Damage", true);
            rigid.AddForce(Vector3.up * jumpSpeed);
            dir = _dir;
            Invoke("GoBack", 0.1f);
        }
    }
    //Comprovar Si la skin esta pillada y colocar la variante
    [PunRPC]
    public void DuplicatedSkin(int Other)
    {
        if (PlayerPrefs.GetInt("Skin") == 1 && Other == 1)
        {
            PlayerPrefs.SetInt("Skin", 3);
        }
        else if (PlayerPrefs.GetInt("Skin") == 2 && Other == 2)
        {
            PlayerPrefs.SetInt("Skin", 4);
        }
    }
    //Entrar en estado pillado por la ulti
    [PunRPC]
    public void CatchedID(int _id)
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < allPlayers.Length; i++)
        {
            if (allPlayers[i].GetComponent<PhotonView>().ownerId == _id)
            {
                transform.SetParent(allPlayers[i].transform);
                break;
            }
        }
        GetComponent<CapsuleCollider>().enabled = false;
        GetComponent<SphereCollider>().enabled = false;
        state = PlayerState.CACHED;
    }
    //Salir estado de la ulti
    [PunRPC]
    public void ResetCatched()
    {
        Pulser.SetActive(false);
        transform.SetParent(null);
        GetComponent<CapsuleCollider>().enabled = true;
        GetComponent<SphereCollider>().enabled = true;
        rigid.isKinematic = false;
        state = PlayerState.NORMAL;
    }
    [PunRPC]
    public void RemoveCatched()
    {
        speed = 7;
        caching = false;
        GameObject findPlayer = transform.GetComponentInChildren<PlayerControl>().gameObject;
        if (findPlayer != null) findPlayer.transform.SetParent(null);
        Oponent = null;
        state = PlayerState.NORMAL;
    }
    [PunRPC]
    public void UltiCatched(Vector3 _pos)
    {
        transform.position = _pos;
        //state = PlayerState.CACHED;
    }
    //Hacer fuerza hacia atras
    private void GoBack()
    {
        rigid.AddForce(dir * gobackforce);
    }
    //Entrar en el estado de la E
    private void PrepareE()
    {
        rigid.AddForce(-transform.forward * 400);
        state = PlayerState.ATTACK_E;
    }
    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }
    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Lose")
        {
            if(photonView.isMine)
            {
                //invoke lose;
                GetComponent<PhotonView>().RPC("Lose", PhotonTargets.All);
            }
        }
    }
    private void Respawn()
    {       
        if(photonView.isMine)
        {
            transform.position = new Vector3(0f, 10f, 0f); //originalPos;
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0, 1, -2f);
            Camera.main.transform.localRotation = Quaternion.Euler(15f, 0, 0);
        }
    }
    [PunRPC]
    public void Lose()
    {
        if (photonView.isMine)
        {
            Camera.main.transform.SetParent(null);
        GameObject tempSplash = Instantiate(splash, splashpoint.transform);
        Invoke("Respawn", 4f);
        }
    }
}
