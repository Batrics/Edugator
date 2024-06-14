using System;

[Serializable]
public class AllGamesJson
{
    public bool success;
    public DataAllGames[] data;
}
[Serializable]
public class CardAllGames
{
    public string id;
    public string name;
}
[Serializable]
public class DataAllGames
{
    public string author;
    public int id;
    public string uuid;
    public int user_id;
    public string name;
    public string description;
    public string token;
    public CardAllGames[] cards;
    public string dueDate;
    public string created_at;
    public string created_by;
    public string modified_at;
    public string modified_by;
    public string deleted_at;
    public string deleted_by;
    public string restored_at;
    public string restored_by;
    public int is_deleted;
    public int is_restored;
    public int status;
}
