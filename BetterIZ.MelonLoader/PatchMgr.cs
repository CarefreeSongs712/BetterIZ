using HarmonyLib;
using Il2Cpp;
using Il2CppTMPro;
using MelonLoader;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BetterIZ;

[RegisterTypeInIl2Cpp]
public class PatchMgr : MonoBehaviour
{
    public void LoadData(InGameUI_IZ __instance)
    {
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
        if(data is null) return;
        var igui = IZBottomMenu.Instance;
        if (igui is not null)
        {
            igui.ChangeString(data.CustomName);
        }

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
[HarmonyPatch(typeof(GameAPP))]
public static class GameAppPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    public static void PostStart()
    {
        GameObject obj = new("Modifier");
        Object.DontDestroyOnLoad(obj);
        obj.AddComponent<PatchMgr>();
    }
}


[HarmonyPatch(typeof(UIMgr), "EnterMainMenu")]
public static class UIMgrPatch
{
    public static void Postfix()
    {
        return;
        GameObject obj1 = new("ModifierInfo");
        var text1 = obj1.AddComponent<TextMeshProUGUI>();
        // ReSharper disable once Unity.UnknownResource
        text1.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
        text1.color = new Color(1, 1, 0, 1);
        text1.text = "BetterIZ 作者: 听雨夜荷\n" +
                     "快捷键列表:\n" +
                     "i = 启用更好的IZ存档";
        obj1.transform.SetParent(GameObject.Find("Leaves").transform);
        obj1.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        obj1.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 50);
        obj1.transform.localPosition = new Vector3(-345.5f, 55f, 0);
    }
}

[HarmonyPatch(typeof(EveManager), nameof(EveManager.SaveCustomIZ))]
public static class EveManagerPatchA
{
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void Postfix(EveManager __instance)
    {
        if (!Core.BetterIZDataEnabled) return;
        var zombiesList = new List<ZombieData>();
        foreach (var zb in Board.Instance.zombieArray)
        {
            if (zb != null)
            {
                zombiesList.Add(new ZombieData()
                {
                    Type = (int)zb.theZombieType,
                    Row = zb.theZombieRow,
                    PositionX = zb.transform.position.x,
                    IsMindControlled = zb.isMindControlled
                });
            }
        }

        var gridItemsList = new List<GridItemData>();
        foreach (var git in GameAPP.board.GetComponent<Board>().griditemArray)
        {
            if (git != null)
            {
                if (git.theItemType == GridItemType.IceBlock)
                {
                    git.TryGetComponent<FreezedPlant>(out var freezedPlant);
                    if (freezedPlant is not null)
                    {
                        gridItemsList.Add(new GridItemData()
                        {
                            Type = (int)git.theItemType,
                            Column = git.theItemColumn,
                            Row = git.theItemRow,
                            PlantType = (int)freezedPlant.thePlantType
                        });
                        continue;
                    }
                }

                gridItemsList.Add(new GridItemData()
                {
                    Type = (int)git.theItemType,
                    Column = git.theItemColumn,
                    Row = git.theItemRow
                });
            }
        }

        CustomIZData combinedData = new CustomIZData();
        combinedData.CustomName = "BetterIZ关卡";
        combinedData.Zombies = zombiesList;
        combinedData.GridItems = gridItemsList;

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

[HarmonyPatch(typeof(InGameUI_IZ), nameof(InGameUI_IZ.Awake))]
public class InGameUI_IZPatchA
{
    [HarmonyPostfix]
    public static void Postfix(InGameUI_IZ __instance)
    {
        if (!Core.BetterIZDataEnabled) return;
        var pm = new PatchMgr();
        pm.LoadData(__instance);
    }
}

public class CustomIZData
{
    public String? CustomName { get; set; }
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