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
    public static bool LoadDataEnabled = true;

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

        if (Input.GetKeyDown(KeyCode.C))
        {
            var flag = true;
            for (var i = Board.Instance.plantArray.Count - 1; i >= 0; i--) Board.Instance.plantArray[i]?.Die();
            Board.Instance.plantArray.Clear();
            for (var j = Board.Instance.zombieArray.Count; j >= 0; j--)
                try
                {
                    Board.Instance.zombieArray[j]?.TakeDamage(DmgType.MaxDamage, 2147483647);
                    Board.Instance.zombieArray[j]?.BodyTakeDamage(2147483647);
                    Board.Instance.zombieArray[j]?.Die();
                }
                catch
                {
                    flag = false;
                }
            if(flag)
                Board.Instance.zombieArray.Clear();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            foreach (var a in InGameUI_IZ.Instance.cardOnBank)
            {
                if (a is not null)
                {
                    MelonLogger.Msg($"zt:{a.theZombieType}");
                }
            }
            MelonLogger.Msg("-=-=-=-=-=-=");
            foreach (var a in InGameUI_IZ.Instance.Cards)
            {
                if (a is not null)
                {
                    MelonLogger.Msg($"zt2:{a.theZombieType}");
                }
            }
            MelonLogger.Msg("-=-=-=-=-=-=");
            foreach (var a in InGameUI_IZ.Instance.cardOnBank
                     )
            {
                if (a is not null)
                {
                    MelonLogger.Msg($"zt2:{a.theZombieType}");
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            MelonLogger.Msg("=============================================================");
            if (IZBottomMenu.Instance.zombieLibary == null)
            {
                Debug.LogError("Zombie library reference not set!");
                return;
            }
        
            // 2. 获取僵尸库的第一个子对象（卡片容器）
            Transform libraryContainer = IZBottomMenu.Instance.zombieLibary.transform.GetChild(0).GetChild(0); // 原来的疑似获取到的不是Page1而是Pages
            if (libraryContainer == null)
            {
                Debug.LogError("Zombie library container not found!");
                return;
            }
        
            // 3. 获取所有卡片对象
            List<GameObject> cardObjects = GetAllCardObjects(libraryContainer);
        
            // 4. 遍历所有卡片
            foreach (GameObject pageObject in cardObjects)
            {
                MelonLogger.Msg($"Page {pageObject.name}");
                for (int i = 0; i < pageObject.transform.GetChildCount(); i++)
                {
                    MelonLogger.Msg($"----");
                    var cardObject = pageObject.transform.GetChild(i);
                    var cardComponent = cardObject.gameObject.GetComponent<IZECard>();
                    MelonLogger.Msg($"cardObject {cardComponent}");
                    if (cardComponent != null && cardComponent.isActiveAndEnabled)
                    {
                        // 6. 将卡片添加到银行
                        bool added = InGameUI_IZ.Instance.AddCardToBank(cardComponent);
                        MelonLogger.Msg($"{cardComponent.ToString()}:{added}");
                    }
                }
            }
        }
        
        if (false && Input.GetKeyDown(KeyCode.A))
        {
            MelonLogger.Msg("=============================================================");
            if (IZBottomMenu.Instance.zombieLibary == null)
            {
                Debug.LogError("Zombie library reference not set!");
                return;
            }
        
            // 2. 获取僵尸库的第一个子对象（卡片容器）
            Transform libraryContainer = IZBottomMenu.Instance.zombieLibary.transform.GetChild(0);
            if (libraryContainer == null)
            {
                Debug.LogError("Zombie library container not found!");
                return;
            }
        
            // 3. 获取所有卡片对象
            List<GameObject> cardObjects = GetAllCardObjects(libraryContainer);
        
            // 4. 遍历所有卡片
            foreach (GameObject pageObject in cardObjects)
            {
                MelonLogger.Msg($"Page {pageObject.name}");
                for (int i = 0; i < pageObject.transform.GetChildCount(); i++)
                {
                    var cardObject = pageObject.transform.GetChild(i);
                    var cardComponent = cardObject.gameObject.GetComponent<IZECard>();
                    MelonLogger.Msg($"cardObject {cardComponent}");
                    if (cardComponent != null)
                    {
                        // 6. 将卡片添加到银行
                        bool added = InGameUI_IZ.Instance.AddCardToBank(cardComponent);
                        MelonLogger.Msg($"{cardComponent.ToString()}:{added}");
                    }
                }
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