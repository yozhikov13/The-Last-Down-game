using Amazon.DynamoDBv2.DataModel;

[DynamoDBTable("Items")]
public class WeaponParser
{
    [DynamoDBHashKey]
    public string Name { get; set; }
    [DynamoDBProperty]
    public float Accuracy { get; set; }
    [DynamoDBProperty]
    public float Damage { get; set; }
    [DynamoDBProperty]
    public float Range { get; set; }
    [DynamoDBProperty]
    public float Rate { get; set; }
    [DynamoDBProperty]
    public float Weight { get; set; }
    [DynamoDBProperty]
    public float Recoil { get; set; }
    [DynamoDBProperty]
    public float Control { get; set; }
    [DynamoDBProperty]
    public int Magazine { get; set; }
    [DynamoDBProperty]
    public float ReloadTime { get; set; }
    [DynamoDBProperty]
    public string Type { get; set; }
    [DynamoDBProperty]
    public string Slot { get; set; }
    [DynamoDBProperty]
    public int MinLevel { get; set; }
    [DynamoDBProperty]
    public int Price { get; set; }
    [DynamoDBProperty]
    public int PremiumPrice { get; set; }
}
