namespace ConnectorSpace{
    public enum eRoomType
    {
        Match = 0,
        Freedom = 1,
        Exploration = 2,
        Boss = 3,
        Treasure = 4,
    }
    
    public enum eGameType
    {
        Free = 0,               //自由站
        Guild = 1,              //工会战
        Training = 2,           //训练
        ALL = 4,                //不分类型
        Exploration = 5,
        Boss = 6,
        Treasure = 7,
    }

    public enum eHardLevel
    {
        Simple = 0,
        Normal = 1,
        Hard = 2,
        Terror = 3,
    }

    public enum eGameState
    {
        Inited,
        Prepared,
        Loading,
        GameStartMovie,
        GameStart,
        Playing,
        GameOverMovie,
        GameOver,
        Stopped,
        SessionPrepared,
        ALLSessionStopped
    }

    public enum eLevelLimits
    {
        Other = 0,
        ZeroToTen = 1,
        ElevenToTwenty = 2,
        TwentyOneToThirty = 3,

    }

}