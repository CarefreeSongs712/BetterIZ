using System.Text;
using BetterIZ;
using MelonLoader;
using UnityEngine;

[assembly:MelonInfo(typeof(Core),"BetterIZ","1.0.0","tingyuyehe")]
[assembly:MelonGame("LanPiaoPiao","PlantsVsZombiesRH") ]
[assembly:MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]

namespace BetterIZ;

public class Core : MelonMod
{
    
    // ReSharper disable once InconsistentNaming
    public static bool BetterIZDataEnabled = false;

    public override void OnInitializeMelon()
    {
        Console.OutputEncoding = Encoding.UTF8;
        MelonLogger.Msg(System.ConsoleColor.Green,"[BetterIZ] 插件已加载");
        base.OnInitializeMelon();
    }
    
    public override void OnUpdate()
    {
        base.OnUpdate();

        if (Input.GetKeyDown(KeyCode.I) && Input.GetKeyDown(KeyCode.Alpha1))
        {
            BetterIZDataEnabled = !BetterIZDataEnabled;
            MelonLogger.Msg(System.ConsoleColor.Yellow, $"[BetterIZ] BetterIZDataEnabled功能已{(BetterIZDataEnabled ? "开启" : "关闭")}");
        }
    }
}