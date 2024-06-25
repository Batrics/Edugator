using System;
using System.Collections.Generic;

[Serializable]
public class JsonGameId {
    public int gameId;
}
[Serializable]
public class Wrapper<T>
{
    public List<T> items;
}
