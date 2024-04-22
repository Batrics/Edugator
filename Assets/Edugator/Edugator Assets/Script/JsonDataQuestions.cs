using System;

[Serializable]
public class MainDataJsonQuestions
{
    public bool success;
    public DataQuestions[] data;
}

[Serializable]
public struct DataQuestions
{
    public string game;
    public string card;
    public int id;
    public string uuid;
    public int game_id;
    public int card_id;
    public string question;
    public string option1;
    public string option2;
    public string option3;
    public string answer;
    public int score;
    public string created_at;
    public string created_by;
    public object modified_at;
    public string modified_by;
    public object deleted_at;
    public string deleted_by;
    public object restored_at;
    public string restored_by;
    public int is_deleted;
    public int is_restored;
    public int status;
}
