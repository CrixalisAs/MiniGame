﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillItem : MonoBehaviour
{

    public float coldTime = 2f;
    private float timer = 0;
    private Image filledImage;
    private bool isStartTime = false;
	// Use this for initialization
	void Start ()
	{
	    filledImage = transform.Find("FilledImage").GetComponent<Image>();
	    transform.GetComponent<Button>().onClick.AddListener(OnClick);
	}
	
	// Update is called once per frame
	void Update () {
	    if (isStartTime)
	    {
	        timer += Time.deltaTime;
	        filledImage.fillAmount = (coldTime - timer) / coldTime;
	        if (timer >= coldTime)
	        {
	            filledImage.fillAmount = 0;
	            timer = 0;
	            isStartTime = false;
	        }
	    }
	}

    private void OnClick()
    {
        isStartTime = true;
    }
}
