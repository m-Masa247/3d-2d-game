using UnityEngine;

public enum EquipmentType
{
    Sword,  // 剣
    Shield, // 盾  
    Staff   // 杖
}

[System.Serializable]
public class EquipmentData
{
    public EquipmentType type;
    public string name;
    public Color color;
    public Sprite icon; // アイコンスプライト追加
    
    public EquipmentData(EquipmentType equipType, string equipName, Color equipColor, Sprite equipIcon = null)
    {
        type = equipType;
        name = equipName;
        color = equipColor;
        icon = equipIcon;
    }
}

public static class EquipmentManager
{
    private static bool iconsLoaded = false;
    private static Sprite swordIcon, shieldIcon, staffIcon;
    
    private static void LoadIcons()
    {
        if (iconsLoaded) return;
        
        // Resources.Loadでアイコンを読み込み（失敗しても動作が停止しないように）
        swordIcon = Resources.Load<Sprite>("EquipmentIcons/sword");
        shieldIcon = Resources.Load<Sprite>("EquipmentIcons/shield"); 
        staffIcon = Resources.Load<Sprite>("EquipmentIcons/Wand");
        
        if (swordIcon == null) Debug.LogWarning("[装備システム] sword.pngが見つかりません");
        if (shieldIcon == null) Debug.LogWarning("[装備システム] shield.pngが見つかりません");
        if (staffIcon == null) Debug.LogWarning("[装備システム] Wand.pngが見つかりません");
        
        iconsLoaded = true;
        Debug.Log("[装備システム] アイコン読み込み完了");
    }
    
    public static EquipmentData[] GetAllEquipments()
    {
        LoadIcons();
        return new EquipmentData[]
        {
            new EquipmentData(EquipmentType.Sword, "剣", Color.red, swordIcon),
            new EquipmentData(EquipmentType.Shield, "盾", Color.blue, shieldIcon),
            new EquipmentData(EquipmentType.Staff, "杖", Color.green, staffIcon)
        };
    }
    
    public static EquipmentData GetRandomEquipment()
    {
        EquipmentData[] equipments = GetAllEquipments();
        int randomIndex = Random.Range(0, equipments.Length);
        return equipments[randomIndex];
    }
    
    public static EquipmentData GetEquipmentByType(EquipmentType type)
    {
        EquipmentData[] equipments = GetAllEquipments();
        foreach (var equipment in equipments)
        {
            if (equipment.type == type)
                return equipment;
        }
        return equipments[0]; // フォールバック
    }
    
    public static Sprite GetEquipmentIcon(EquipmentType type)
    {
        return GetEquipmentByType(type).icon;
    }
}