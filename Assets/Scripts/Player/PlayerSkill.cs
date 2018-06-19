﻿using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Tools;
using UnityEngine;

public class PlayerSkill : MonoBehaviour
{

    private CharactorSkills charactorSkills;
    private PlayerManager playerManager;
    private Rigidbody rb;
    public float rotateSpeed = 3f;
    private Vector3 direction=Vector3.zero;
    private PlayerInfo playerInfo;

    private Skill currentSkill = null;
	// Use this for initialization
	void Start ()
	{
	    charactorSkills = GetComponent<CharactorSkills>();
	    rb = transform.GetComponent<Rigidbody>();
	    playerInfo = GetComponent<PlayerInfo>();
	}
	
	// Update is called once per frame
	void Update () {
	    if (playerInfo.CurrentState==PlayerInfo.State.UsingSkill)
	    {
	        if (IsTurnDone())
	        {
	            UseSkill();
	            playerInfo.CurrentState = PlayerInfo.State.Idle;
	        }
            if(direction!=Vector3.zero)
                TurnToUseSkill();
	    }
	    else
	    {
	        currentSkill = null;
	        direction = Vector3.zero;
	    }
	}

    private void UseSkill()
    {
        charactorSkills.Skills[currentSkill.GetType().Name].direction = direction;
        charactorSkills.UseSkill(currentSkill.GetType().Name);
    }
    public void StartUseSkill(Skill skill,string axis=null)
    {
        playerInfo.CurrentState = PlayerInfo.State.UsingSkill;
        if (axis != null)
        {
            Vector2 v2 = UnityTools.ParseVector2(axis);
            direction=new Vector3(v2.x,0,v2.y);
        }
        else
        {
            direction=Vector3.zero;
        }
        currentSkill = skill;
    }
    public PlayerSkill SetPlayerMng(PlayerManager playerMng)
    {
        this.playerManager = playerMng;
        return this;
    }

    private bool IsTurnDone()
    {
        if (direction == Vector3.zero)
            return true;
        if (Mathf.Abs(transform.rotation.eulerAngles.y - Quaternion.LookRotation(direction, transform.up).eulerAngles.y) <= 20)
        {
            return true;
        }
        return false;
    }
    private void TurnToUseSkill()
    {
        transform.rotation = Quaternion.Lerp(rb.rotation, Quaternion.LookRotation(direction, transform.up), Time.fixedDeltaTime * rotateSpeed);
    }
}