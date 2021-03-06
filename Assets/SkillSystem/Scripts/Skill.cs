﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Skill
{
    public SkillManager sm;
    public PlayerManager PlayerManager { get { return sm.PlayerManager; } }

    public delegate int SkillDelegate();

    public Vector3 Direction=Vector3.zero;
    public string resourcesName_;
    private GameObject resources_ = null;
    public GameObject resources
    {
        get
        {
            if (resources_ == null)
                resources_ = Resources.Load<GameObject>("SkillPerferbs/" + resourcesName_);
            return Object.Instantiate(resources_);
        }
    }
    private GameObject _prefab;
    public GameObject Prefab
    {
        get
        {
            if (_prefab == null)
            {
                _prefab = Resources.Load<GameObject>("SkillPerferbs/" + resourcesName_);
            }
            return _prefab;
        }
    }
    private List<Sprite> _sprites = null;
    public List<Sprite> Sprites
    {
        get
        {
            if (_sprites == null)
            {
                List<Sprite> sprites = new List<Sprite>();
                GameObject res = Object.Instantiate(Prefab);
                SpriteRenderer[] sr = res.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer spriteRenderer in sr)
                {
                    sprites.Add(spriteRenderer.sprite);
                }
                _sprites = sprites;
                Object.Destroy(res);
            }
            return _sprites;
        }
    }
    public string name
    {
        get { return name_; }
    }
    protected string name_ = "null";


    public GameObject owner = null;


    public Dictionary<string, float> parameters = new Dictionary<string, float>();

    public float timeSinceSkillStart = 0;


    public IEnumerator Execute(SkillDelegate didStart = null, SkillDelegate didAction = null, SkillDelegate didEnd = null)
    {
        timeSinceSkillStart = 0;
        if (!SkillStart()) yield break;
        if (didStart != null) didStart();
        sm.StartASkill(this);
        while (!SkillAction())
        {
            if (didAction != null) didAction();
            timeSinceSkillStart += Time.deltaTime;
            yield return 0;
        }
        SkillEnd();
        if (didEnd != null) didEnd();
    }

    protected abstract bool SkillStart();
    protected abstract bool SkillAction();
    protected abstract void SkillEnd();

    public abstract string GetDescription();

    public void SetName(string Name)
    {
        name_ = Name;
    }
}


public class SkillInfo : Skill
{
    override protected bool SkillStart() { return true; }
    override protected bool SkillAction() { return false; }
    override protected void SkillEnd() { }
    override public string GetDescription() { return ""; }
}
