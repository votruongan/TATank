using System.Collections;
using ConnectorSpace;

public class RoomInfo : BaseInfoWrapper {    
    public int id;
    public eRoomType roomType;
    public eHardLevel hardLevel;
    public byte timeMode;
    public byte playerCount;
    public byte placescount;
    public bool isPasswd;
    public int mapid;
    public bool isplaying;
    public string roomname;
    public eGameType gametype;
    public int levelLimits;
}