using System;

[Serializable]
public class Users
{
    public bool success;
    public DataUser[] data;
}
[Serializable]
public class DataUser
{
    public int id;
    public string username;
    public string password;
    public string email;
    public int user_id;
}