using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transparent : Item
{
    //timeSinceItemStart     道具从发动以来经过的时间
    //owner                   道具发动者
    //name                    道具名(不是类名，是游戏中想要展示的名字)
    //parameters              可调道具参数的字典
    //resources               资源预置体

    private Material m;
    private Material material;
    private Renderer renderer;
    private PlayerInfo pi;
    private VisualTest test;
    private bool isLocal = true;
    //这个方法会在道具发动时调用
    protected override bool ItemStart()
    {
        material = Resources.Load<Material>("Materials/Transparent");
        renderer = owner.GetComponent<Renderer>();
        pi = owner.GetComponent<PlayerInfo>();
        pi.IsTransparent = true;
        m = renderer.material;
        if (pi.CampType != GameFacade.Instance.GetLocalPlayer().CampType)
        {
            pi.HideHealthBar();
            renderer.enabled = false;
        }
        else
        {
            renderer.material = material;
        }
        if (pi.VisualTest != null)
        {
            if (pi.VisualTest.InVisual())
                GameFacade.Instance.PlaySound("Transparent");
        }
        else
        {
            GameFacade.Instance.PlaySound("Transparent");
        }
        return true;
    }

    //这个方法会在道具进行过程中不断调用，当返回false表示道具已经完成所有动作
    protected override bool ItemAction()
    {
        if (timeSinceItemStart < parameters.TryGet("During"))
        {
            if (pi != null)
            {
                if (!pi.anim.GetCurrentAnimatorStateInfo(0).IsName("Grounded")||pi.IsTransparent==false)
                {
                    return true;
                }
                if (pi.CampType != GameFacade.Instance.GetLocalPlayer().CampType)
                {
                    if (pi.IsInTrueVision)
                    {
                        pi.ShowHealthBar();
                        renderer.enabled = true;
                        renderer.material = material;
                    }
                    else
                    {
                        pi.HideHealthBar();
                        renderer.enabled = false;
                    }
                }
            }
            return false;
        }
        return true;
    }

    //这个方法会在道具结束时调用
    protected override void ItemEnd()
    {
        if (owner != null)
        {
            pi.IsTransparent = false;
            pi.ShowHealthBar();
            renderer.enabled = true;
            renderer.material = m;
        }
    }

    //您可以通过该方法提供一个道具的详细描述，您可以通过在文字中嵌入属性字典中的值来避免反复修改代码。
    public override string GetDescription()
    {
        return "这个技能使用后就能进行加速";
    }
}