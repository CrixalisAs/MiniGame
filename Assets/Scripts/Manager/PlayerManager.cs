﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using Assets.Scripts.Tools;
using Common;
using UnityEngine;

public class PlayerManager : BaseManager
{
    public PlayerManager(GameFacade facade) : base(facade) { }

    //UIPanel使用
    public UserData UserData;

    private int _idCount = 0;
    private int IdCount { get { return  _idCount++; } }

    private List<RoleData> _roleDataList;
    public List<RoleData> RoleDataList { get
        {
            return _roleDataList ?? (_roleDataList = facade.GetRoleDataList());
        } }

    private GameObject _currentCamGameObject;
    private GameObject currentCamGameObject { get { return _currentCamGameObject; } set
        {
            _currentCamGameObject = value;
            facade.CamFollowTarget(_currentCamGameObject.transform);
        } }

    private List<Player> playerList=new List<Player>();
    private Player _localPlayer;
    private Player LocalPlayer { get { return _localPlayer ?? (_localPlayer = GetLocalPlayer()); } }


    //场景中每一个英雄对象都对应一个独一无二Id
    private Dictionary<int, GameObject> roleGameObjects =new Dictionary<int, GameObject>();
    

    private MoveRequest moveRequest;
    private UseSkillRequest useSkillRequest;
    private UseItemRequest useItemRequest;

    private SkillManager skillManager;
    private ItemManager itemManager;

    //private ShootRequest shootRequest;
    //private AttackRequest attackRequest;

    public override void OnInit()
    {
        base.OnInit();
        skillManager = GameObject.FindGameObjectWithTag("SkillManager").GetComponent<SkillManager>();
        itemManager = GameObject.FindGameObjectWithTag("ItemManager").GetComponent<ItemManager>();
    }

    public override void Update()
    {
        base.Update();
    }
    
    public void EnterPlaying()
    {
        //TODO 进入游戏场景后
        SetPlayerSpawnPosition();
        CreateSyncRequest();
        CreateBattleManager();
    }

    public void InitPlayerData(UserData ud,List<UserData> userDatas)
    {
        foreach (var userData in userDatas)
        {
            Player player = new Player(userData, userData.SeatIndex);
            if (ud.Id == userData.Id)
            {
                player.IsLocal = true;
            }
            playerList.Add(player);
        }
    }

    private void SetPlayerSpawnPosition()
    {
        Transform spawnPositions = GameObject.Find("RolePositions").transform;
        foreach (var player in playerList)
        {
            player.SpawnPosition = spawnPositions.Find("Position" + player.SeatIndex).transform.position;
        }
    }

    public void SpawnRoles(CampType campType)
    {
        foreach (Player player in playerList)
        {
            if (player.CampType != campType) continue;
            HeroData rd = GetHeroDataBySeatIndex(player.SeatIndex);
            GameObject go = null;
            go = Object.Instantiate(rd.RolePrefab, player.SpawnPosition, Quaternion.identity);
            switch (campType)
            {
                case CampType.Fish:
                    go.tag = "Fish";
                    break;
                case CampType.Monkey:
                    go.tag = "Monkey";
                    break;
            }
            int instanceId = IdCount;
            roleGameObjects.Add(instanceId, go);
            player.RoleInstanceIdList.Add(instanceId);
            player.currentRoleInstanceId = instanceId;
            
            if (player.IsLocal)
            {
                go.AddComponent<PlayerMove>().SetPlayerMng(this).IsLocal = true;
                currentCamGameObject = go;
                if(facade.GetCurrentPanel().GetType().Name=="GamePanel")
                    ((GamePanel)facade.GetCurrentPanel()).SetPlayer(go);
            }
            go.AddComponent<PlayerSkill>().SetPlayerMng(this);
            go.AddComponent<PlayerItem>().SetPlayerMng(this);
        }
    }

    #region Get_Function
    
    private HeroData GetHeroDataBySeatIndex(int seatIndex)
    {
        foreach (var roleData in RoleDataList)
        {
            if(roleData.RoleType==RoleType.Hero)
                if (((HeroData)roleData).seatIndex.Contains(seatIndex))
                    return (HeroData)roleData;
        }
        return null;
    }
    public Transform GetCurrentCamTarget()
    {
        return currentCamGameObject.transform;
    }

    private Player GetLocalPlayer()
    {
        foreach (var player in playerList)
        {
            if (player.IsLocal)
                return player;
        }
        return null;
    }

    private int GetCurrentGoId()
    {
        return LocalPlayer.currentRoleInstanceId;
    }
    #endregion

    public void UpdateResult(int totalCount, int winCount)
    {
        //userData.TotalCount = totalCount;
        //userData.WinCount = winCount;
    }
    
    private void CreateSyncRequest()
    {
        GameObject playerSyncRequest = new GameObject("PlayerSyncRequest");
        moveRequest = playerSyncRequest.AddComponent<MoveRequest>();
        moveRequest.PlayerManager = this;
        useSkillRequest = playerSyncRequest.AddComponent<UseSkillRequest>();
        useSkillRequest.PlayerManager = this;
        useItemRequest = playerSyncRequest.AddComponent<UseItemRequest>();
        useItemRequest.PlayerManager = this;
        //shootRequest = playerSyncRequest.AddComponent<ShootRequest>();
        //shootRequest.PlayerMng = this;
        //attackRequest = playerSyncRequest.AddComponent<AttackRequest>();
        //attackRequest.PlayerManager = this;
    }

    private void CreateBattleManager()
    {
        GameObject battleManager=new GameObject("BattleManager");
    }
    public void Move()
    {
        if (LocalPlayer == null|| LocalPlayer.RoleInstanceIdList.Count == 0)
            return;
        StringBuilder sb = new StringBuilder();
        int count = 0;
        foreach (var id in LocalPlayer.RoleInstanceIdList)
        {
            GameObject go = roleGameObjects[id];
            if (go.GetComponent<PlayerInfo>().CurrentState != PlayerInfo.State.Move)continue;
            count++;
            sb.Append(id + "|" + UnityTools.PackVector3(go.transform.position) + "|" +
                      UnityTools.PackVector3(go.transform.eulerAngles)+":");
        }
        if (count == 0) return;
        sb.Remove(sb.Length - 1, 1);
        moveRequest.SendRequest(sb.ToString());
    }

    public void MoveSync(int goId, Vector3 pos, Vector3 rot)
    {
        GameObject go = roleGameObjects[goId];
        PlayerInfo playerInfo = go.GetComponent<PlayerInfo>();
        if (playerInfo != null)
            playerInfo.CurrentState= PlayerInfo.State.Move;
        roleGameObjects[goId].transform.position = pos;
        roleGameObjects[goId].transform.eulerAngles = rot;
    }

    
    public void UseSkill(string skillName,string axis=null)
    {
        useSkillRequest.SendRequest(GetCurrentGoId(),skillName,axis);
    }

    public void UseSkillSync(int instanceId,string skillName,string axis=null)
    {
        GameObject go = roleGameObjects[instanceId];
        Skill skill = skillManager.GetInstanceOfSkillWithString(skillName, go);
        if (skill == null)
        {
            Debug.Log("技能不存在");
            return;
        }
        go.GetComponent<PlayerSkill>().StartUseSkill(skill,axis);
    }

    public void UseItem(string itemName, string point = null)
    {
        useItemRequest.SendRequest(LocalPlayer.UserData.Id,GetCurrentGoId(), itemName, point);
    }

    public void UseItemSync(int id,int instanceId, string itemName, string point = null)
    {
        GameObject go = roleGameObjects[instanceId];
        bool isLocal = id == LocalPlayer.UserData.Id;
        Item item = itemManager.GetInstanceOfItemWithString(itemName, go);
        if (item == null)
        {
            Debug.Log("道具不存在");
            return;
        }
        go.GetComponent<PlayerItem>().StartUseItem(isLocal, item, point);
    }
}
