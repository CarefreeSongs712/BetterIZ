using System.Text;
using BetterIZ;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly:MelonInfo(typeof(Core),"BetterIZ","1.0.0","tingyuyehe")]
[assembly:MelonGame("LanPiaoPiao","PlantsVsZombiesRH") ]
[assembly:MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]

namespace BetterIZ;

public class Core : MelonMod
{

    public static bool BetterIZDataEnabled = true;

    public override void OnInitializeMelon()
    {
        Console.OutputEncoding = Encoding.UTF8;
        MelonLogger.Msg(System.ConsoleColor.Green,"插件已加载");
        base.OnInitializeMelon();
    }
    
    public override void OnUpdate()
    {
        base.OnUpdate();
        GameAPP.developerMode = true;
        if (Input.GetKeyDown(KeyCode.I))
        {
            BetterIZDataEnabled = !BetterIZDataEnabled;
            if (BetterIZDataEnabled)
            {
                MelonLogger.Msg(System.ConsoleColor.Green, $"BetterIZData功能已开启");
            }
            else
            {
                MelonLogger.Msg(System.ConsoleColor.Red, $"BetterIZData功能已关闭");
            }
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            var pm = new PatchMgr();
            if (InGameUI_IZ.Instance is not null)
            {
                pm.LoadData(InGameUI_IZ.Instance);
            }
            else
            {
                MelonLogger.Msg(System.ConsoleColor.Red, $"获取不到InGameUI_IZ！请先进入游戏");
            }
        }

    }
    
    private List<GameObject> GetAllCardObjects(Transform container)
    {
        List<GameObject> cardObjects = new List<GameObject>();
        
        // 遍历所有子对象
        for (int i = 0; i < container.childCount; i++)
        {
            Transform child = container.GetChild(i);
            cardObjects.Add(child.gameObject);
        }
        
        return cardObjects;
    }
}