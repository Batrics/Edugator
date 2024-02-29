
[System.Serializable]
public class MainDataJson
{
    public bool success;
    public Data data;
}

[System.Serializable]
public class Card 
{
    public string id;
    public string name;
}

[System.Serializable]
public class Data
{
    public string author;
    public int id;
    public string uuid;
    public int user_id;
    public string name;
    public string description;
    public string token;
    public Card[] cards;
    public string dueDate;
    public string created_at;
    public string created_by;
    public string modified_at;
    public string modified_by;
    public object deleted_at;
    public string deleted_by;
    public object restored_at;
    public string restored_by;
    public int is_deleted;
    public int is_restored;
    public int status;
}