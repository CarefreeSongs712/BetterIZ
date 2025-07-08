using HarmonyLib;
using Il2Cpp;
using Il2CppTMPro;
using MelonLoader;
using Newtonsoft.Json;
using UnityEngine;

namespace BetterIZ;

public class PatchMgr
{
    
}

[HarmonyPatch(typeof(UIMgr), "EnterMainMenu")]
public static class UIMgrPatch
{
    public static void Postfix()
    {
        GameObject obj1 = new("ModifierInfo");
        var text1 = obj1.AddComponent<TextMeshProUGUI>();
        // ReSharper disable once Unity.UnknownResource
        text1.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
        text1.color = new Color(1, 1, 0, 1);
        text1.text = "修改器作者为b站@Infinite75\n若存在任何付费/要求三连+关注/私信发链接的情况\n说明你被盗版骗了，请注意隐私和财产安全！！！\n此信息仅在游戏主菜单和修改窗口显示";
        obj1.transform.SetParent(GameObject.Find("Leaves").transform);
        obj1.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        obj1.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 50);
        obj1.transform.localPosition = new Vector3(-345.5f, -96.1f, 0);

        GameObject obj2 = new("UpgradeInfo");
        var text2 = obj2.AddComponent<TextMeshProUGUI>();
        // ReSharper disable once Unity.UnknownResource
        text2.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
        text2.color = new Color(0, 1, 0, 1);
        text2.text = "原作者@Infinite75已停更，这是@听雨夜荷的一个fork。\n" +
                     "项目地址: https://github.com/CarefreeSongs712/PVZRHTools\n" +
                     "\n" +
                     "修改器2.7-3.26.4更新日志:\n" +
                     "1. 修复了一大堆bug。详见github\n"+
                     "2. 新增功能诸神进化无限刷新\n"+
                     "3. 新增betterizdata保存冰块里的植物";
        obj2.transform.SetParent(GameObject.Find("Leaves").transform);
        obj2.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        obj2.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 50);
        obj2.transform.localPosition = new Vector3(-345.5f, 55f, 0);
    }
}


[HarmonyPatch(typeof(EveManager), nameof(EveManager.SaveCustomIZ))]
public static class EveManagerPatch
{
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void Postfix(EveManager __instance)
    {
        if(!Core.BetterIZDataEnabled)return;
        var zombiesList = new List<object>();
        foreach (var zb in Board.Instance.zombieArray)
        {
            if (zb != null)
            {
                zombiesList.Add(new
                {
                    Type = (int)zb.theZombieType,
                    Row = zb.theZombieRow,
                    PositionX = zb.transform.position.x,
                    IsMindControlled = zb.isMindControlled
                });
           }
        }

        var gridItemsList = new List<object>();
        foreach (var git in GameAPP.board.GetComponent<Board>().griditemArray)
        {
            if (git != null)
            {
                if (git.theItemType == GridItemType.IceBlock)
                {
                    git.TryGetComponent<FreezedPlant>(out var freezedPlant);
                    if (freezedPlant is not null)
                    {
                        gridItemsList.Add(new
                        {
                            Type = (int)git.theItemType,
                            Column = git.theItemColumn,
                            Row = git.theItemRow,
                            PlantType = (int)freezedPlant.thePlantType
                        });
                        continue;
                    }
                }
                gridItemsList.Add(new
                {
                    Type = (int)git.theItemType,
                    Column = git.theItemColumn,
                    Row = git.theItemRow
                });
            }
        }

        var combinedData = new
        {
            Zombies = zombiesList,
            GridItems = gridItemsList
        };

        string basePath = Application.persistentDataPath;
        string fileName = "CustomIZ.extra.json";
        var path = Path.Combine(basePath, fileName);

        string json = JsonConvert.SerializeObject(combinedData, Formatting.Indented);

        string? directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            if (directory != null) Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, json);
        MelonLogger.Msg("CustomIZ.extra.json 已保存！");
    }
}

[HarmonyPatch(typeof(InGameUI_IZ),nameof(InGameUI_IZ.Awake))]
// ReSharper disable once InconsistentNaming
public static class InGameUI_IZPatch
{
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void Postfix(InGameUI_IZ __instance)
    { 
        if(Core.BetterIZDataEnabled)return;
        
        string basePath = Application.persistentDataPath;
        string fileName = "CustomIZ.extra.json";
        var filePath = Path.Combine(basePath, fileName);

        if (!File.Exists(filePath))
        {
            MelonLogger.Msg("CustomIZ.extra.json文件不存在");
            return;
        }

        string json = File.ReadAllText(filePath);
        var data = JsonConvert.DeserializeObject<CustomIZData>(json);

        foreach (var zombieData in data?.Zombies ?? new List<ZombieData>())
        {
            if (zombieData.IsMindControlled)
            {
                CreateZombie.Instance.SetZombieWithMindControl(
                    zombieData.Row,
                    (ZombieType)zombieData.Type,
                    zombieData.PositionX
                );
            }
            else
            {
                CreateZombie.Instance.SetZombie(
                    zombieData.Row,
                    (ZombieType)zombieData.Type,
                    zombieData.PositionX
                );
            }
        }
        foreach (var gridItemData in data?.GridItems ?? new List<GridItemData>())
        {
            if (gridItemData.Type == (int)GridItemType.IceBlock)
            {
                var gridItem = GridItem.SetGridItem(
                    gridItemData.Column,
                    gridItemData.Row,
                    (GridItemType)gridItemData.Type
                );

                gridItem.TryGetComponent<FreezedPlant>(out var component);
                if (component is not null)
                {
                    component.InitFreezedPlant((PlantType)gridItemData.PlantType);
                }
#if  false
                    MelonLogger.Msg(gridItemData.PlantType);
                    var plant = CreatePlant.Instance.SetPlant(gridItemData.Column, gridItemData.Row, (PlantType)gridItemData.PlantType,
                        null, default, true, false,
                        null);
                    plant.TryGetComponent<Plant>(out var component);
                    if (component is not null)
                    {
                        MelonLogger.Msg(component.thePlantType);
                    }
#endif
                continue;
            }
            GridItem.SetGridItem(
                gridItemData.Column,
                gridItemData.Row,
                (GridItemType)gridItemData.Type
            );
        }

        MelonLogger.Msg("成功加载自定义IZ数据");
    }
}

// ReSharper disable once InconsistentNaming
public class CustomIZData
{
    public List<ZombieData>? Zombies { get; set; }
    public List<GridItemData>? GridItems { get; set; }
}

public class ZombieData
{
    public int Type { get; set; }
    public int Row { get; set; }
    public float PositionX { get; set; }
    public bool IsMindControlled { get; set; }
}

public class GridItemData
{
    public int Type { get; set; }
    public int Column { get; set; }
    public int Row { get; set; }
    public int PlantType { get; set; }
}