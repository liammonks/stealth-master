using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Gadgets;

public class NetworkPlayer : NetworkBehaviour
{
    
    private NetworkUnit networkUnit;
    private Player playerUnit;
    private Unit activeUnit;

    private Coroutine enableCrawlCoroutine;
    private List<BaseGadget> gadgets = new List<BaseGadget>();

    [SyncVar(hook = nameof(LoadGadgetData))]
    private int gadgetData;

    private void Awake() {
        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetSceneByName("Network"));
        networkUnit = GetComponentInChildren<NetworkUnit>();
        if(NetworkManager.networkType == NetworkType.Server || NetworkManager.networkType == NetworkType.Host)
        {
            NetworkManager.onNetworkUpdate += OnNetworkUpdate;
        }
    }
    
    private void Start() {
        if(hasAuthority)
        {
            transform.name = "MyNetworkPlayer" + netIdentity.netId;
            playerUnit = UnitHelper.Player;
            playerUnit.networkPlayer = this;
            networkUnit.gameObject.SetActive(false);
            CmdLoadGadgetData(PlayerPrefs.GetInt("PlayerGadgets", 1));
        }
        activeUnit = hasAuthority ? playerUnit : networkUnit;
    }
    
    private void FixedUpdate() {
        // Debug Gadgets
        for (int i = 0; i < gadgets.Count; ++i)
        {
            Vector2 pos = UnityEngine.Camera.main.WorldToScreenPoint(activeUnit.transform.position);
            pos += Vector2.up * i * 20;
            Color textColor = activeUnit.GetEquippedGadget().GetType() == gadgets[i].GetType() ? Color.green : Color.white;
            Log.Text("GadgetData" + i + netId, gadgets[i].name, pos, textColor);
        }
    }
    
    private void OnDestroy() {
        NetworkManager.onNetworkUpdate -= OnNetworkUpdate;
    }

    [TargetRpc]
    public void TargetLoadLevel(NetworkConnection conn, string level)
    {
        Debug.Log("Loading Level " + level);
        LevelManager.Instance.LoadLevel(level);
    }
    
    private void OnNetworkUpdate()
    {
        RpcUpdateState(networkUnit.transform.position, networkUnit.data.rb.velocity, networkUnit.GetState());
    }

    [ClientRpc]
    private void RpcUpdateState(Vector2 position, Vector2 velocity, UnitState state)
    {
        Unit targetUnit = hasAuthority ? playerUnit : networkUnit;
        if (targetUnit == null) { return; }

        if(Vector2.Distance(targetUnit.transform.position, position) > velocity.magnitude * 0.1f)
        {
            Debug.DrawRay(targetUnit.transform.position, Vector3.Cross(Vector3.forward, targetUnit.transform.position - (Vector3)position) * 0.2f, Color.green, 0.1f);
            Debug.DrawLine(targetUnit.transform.position, position, Color.red, 0.1f);
            targetUnit.transform.position = position;
            targetUnit.data.rb.velocity = velocity;
        }
        if(targetUnit.GetState() != state)
        {
            //targetUnit.SetState(state);
        }
    }
    
    [Command]
    private void CmdLoadGadgetData(int data)
    {
        gadgetData = data;
        LoadGadgetData(0, data);
    }

    private void LoadGadgetData(int oldData, int newData)
    {
        gadgets.Clear();
        for (int i = 0; i < GlobalData.Gadgets.Count; ++i)
        {
            int bin = Mathf.RoundToInt(Mathf.Pow(2, i));
            if ((newData & bin) == bin)
            {
                gadgets.Add(GlobalData.Gadgets[i]);
            }
        }
    }

    [Command]
    public void CmdOnMovement(int value)
    {
        networkUnit.data.input.movement = value;
        RpcOnMovement(value);
    }
    [ClientRpc]
    public void RpcOnMovement(int value)
    {
        if (hasAuthority) { return; }
        networkUnit.data.input.movement = value;
    }
    
    [Command]
    public void CmdOnRun(bool value)
    {
        networkUnit.data.input.running = value;
        RpcOnRun(value);
    }
    [ClientRpc]
    public void RpcOnRun(bool value)
    {
        if (hasAuthority) { return; }
        networkUnit.data.input.running = value;
    }
    
    [Command]
    public void CmdOnJump()
    {
        networkUnit.data.input.jumpRequestTime = Time.unscaledTime;
        RpcOnJump();
    }
    [ClientRpc]
    public void RpcOnJump()
    {
        if (hasAuthority) { return; }
        networkUnit.data.input.jumpRequestTime = Time.unscaledTime;
    }
    
    
    [Command]
    public void CmdOnCrawl(bool value)
    {
        RpcOnCrawl(value);
        if (value)
        {
            // Set crawling
            if (Time.unscaledTime - networkUnit.data.input.crawlRequestTime > 0.6f)
            {
                networkUnit.data.input.crawling = true;
                networkUnit.data.input.crawlRequestTime = Time.unscaledTime;
            }
            else
            {
                // We tried crawling too quickly after the last crawl input
                enableCrawlCoroutine = StartCoroutine(EnableCrawlDelay(Time.unscaledTime - networkUnit.data.input.crawlRequestTime));
            }
        }
        else
        {
            networkUnit.data.input.crawling = false;
            if (enableCrawlCoroutine != null)
            {
                StopCoroutine(enableCrawlCoroutine);
                enableCrawlCoroutine = null;
            }
        }
    }
    [ClientRpc]
    public void RpcOnCrawl(bool value)
    {
        if (hasAuthority) { return; }
        if (value)
        {
            // Set crawling
            if (Time.unscaledTime - networkUnit.data.input.crawlRequestTime > 0.6f)
            {
                networkUnit.data.input.crawling = true;
                networkUnit.data.input.crawlRequestTime = Time.unscaledTime;
            }
            else
            {
                // We tried crawling too quickly after the last crawl input
                enableCrawlCoroutine = StartCoroutine(EnableCrawlDelay(Time.unscaledTime - networkUnit.data.input.crawlRequestTime));
            }
        }
        else
        {
            networkUnit.data.input.crawling = false;
            if (enableCrawlCoroutine != null)
            {
                StopCoroutine(enableCrawlCoroutine);
                enableCrawlCoroutine = null;
            }
        }
    }

    private IEnumerator EnableCrawlDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        networkUnit.data.input.crawling = true;
        networkUnit.data.input.crawlRequestTime = Time.unscaledTime;
    }
    
    [Command]
    public void CmdOnMelee()
    {
        networkUnit.data.input.meleeRequestTime = Time.unscaledTime;
        RpcOnMelee();
    }
    [ClientRpc]
    public void RpcOnMelee()
    {
        if (hasAuthority) { return; }
        networkUnit.data.input.meleeRequestTime = Time.unscaledTime;
    }
    
    [Command]
    public void CmdOnMouseMove(Vector2 value)
    {
        networkUnit.SetAimOffset(value);
        RpcOnMouseMove(value);
    }
    [ClientRpc]
    public void RpcOnMouseMove(Vector2 value)
    {
        if (hasAuthority) { return; }
        networkUnit.SetAimOffset(value);
    }
    
    [Command]
    public void CmdOnInteract()
    {
        networkUnit.Interact();
        RpcOnInteract();
    }
    [ClientRpc]
    public void RpcOnInteract()
    {
        if (hasAuthority) { return; }
        networkUnit.Interact();
    }

    [Command]
    public void CmdOnGadgetPrimary(bool value)
    {
        networkUnit.GadgetPrimary(value);
        RpcOnGadgetPrimary(value);
    }
    [ClientRpc]
    public void RpcOnGadgetPrimary(bool value)
    {
        if (hasAuthority) { return; }
        networkUnit.GadgetPrimary(value);
    }

    [Command]
    public void CmdOnGadgetSecondary(bool value)
    {
        networkUnit.GadgetSecondary(value);
        RpcOnGadgetSecondary(value);
    }
    [ClientRpc]
    public void RpcOnGadgetSecondary(bool value)
    {
        if (hasAuthority) { return; }
        networkUnit.GadgetSecondary(value);
    }
    
    [Command]
    public void CmdEquipGadget(int index)
    {
        if(index == -1) 
        {
            networkUnit.EquipGadget(GlobalData.DefaultGadget);
        }
        else
        {
            networkUnit.EquipGadget(gadgets[index]);
        }
        RpcEquipGadget(index);
    }
    [ClientRpc]
    public void RpcEquipGadget(int index)
    {
        if (hasAuthority) { return; }
        if (index == -1)
        {
            networkUnit.EquipGadget(GlobalData.DefaultGadget);
        }
        else
        {
            networkUnit.EquipGadget(gadgets[index]);
        }
    }
}
