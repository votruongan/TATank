namespace ConnectorSpace
{
    using System;

    internal class ConfigMgr
    {
        public static string AccountFormat = "test{0}";
        public static int AccountStart = 0x2710;
        public static int ActionInterval = 0x3e8;
        public static int Count = 100;
        public static int CountPerSecond = 50;
        public static string ServerIp="127.0.0.1";
        public static string HostUri = "http://127.0.0.1/";
        public static string RegisterUrl="http://127.0.0.1/register/Default.aspx";
        public static string CreateLoginUrl="http://127.0.0.1/request/CreateLogin.aspx";
        public static string LoginSelectList="http://127.0.0.1/request/LoginSelectList.ashx";
        public static string LoginUrl="http://127.0.0.1/request/Login.ashx";
        public static string Md5Key="QY-16-WAN-0668-2555555-7ROAD-dandantang-love777";
        public static int RoomType = 0;
        public static string RSAKey= "<RSAKeyValue><Modulus>zRSdzFcnZjOCxDMkWUbuRgiOZIQlk7frZMhElQ0a7VqZI9VgU3+lwo0ghZLU3Gg63kOY2UyJ5vFpQdwJUQydsF337ZAUJz4rwGRt/MNL70wm71nGfmdPv4ING+DyJ3ZxFawwE1zSMjMOqQtY4IV8his/HlgXuUfIHVDK87nMNLc=</Modulus><Exponent>AQAB</Exponent><P>7lzjJCmL0/unituEcjoJsZhUDYajgiiIwWwuh0NlCZElmfa5M6l8H+Ahd9yo7ruT6Hrwr4DAdrIKP6LDmFhBdw==</P><Q>3EFKHt4FcDiZXRBLqYZaNSmM1KDrrU97N3CtEDCYS4GimmFOGJhmuK3yGfp/nYLcL2BTKyOZLSQO+/nAjRp2wQ==</Q><DP>SFdkkGsThuiPdrMcxVYb7wxeJiTApxYKOznL/T1VAsxMbyfUGXvMshfh0HDlzF6diycUuQ8IWn26YonRdwECDQ==</DP><DQ>xR9x1NpkB6HAMHhLHzftODMtpYc4Jm5CGsYvPZQgWUN2YbDAkmajWJnlWbbFzBS4N3aAONWtW6cv+ff2itKqgQ==</DQ><InverseQ>oyJzP0Sn+NgdNRRc7/cUKkbbbYaNxkDLDvKLDYMKV6+gcDce85t/FGfaTwkuYQNFqkrRBtDYjtfGsPRTGS6Mow==</InverseQ><D>wM33JNtzUSRwdmDWdZC4BuOYa2vJoD0zc0bNI4x0ml2oyAWdUCMcBfKEds/6i1T6s2e91d2dcJ/aI27o22gO/sfNg3tsr7uYMiUuhSjniqBDB/zyUVig29E4qdfuY1GHxTE8zurroY8mgGEB0aLj+gE0yX9T7sDFkY0QYRqJnwE=</D></RSAKeyValue>";
        
        public static int ServerPort=9200;
        public static int TimeType = 3;
        public static int Version = 2612558;

        public static void UpdateHostAddress(string hostIp){
            ServerIp = hostIp;
            // GameController.LogToScreen(ServerIp);
            HostUri = "http://"+ServerIp+"/";
            // GameController.LogToScreen(HostUri);
            RegisterUrl = HostUri + "register/Default.aspx";
            CreateLoginUrl = HostUri + "request/CreateLogin.aspx";
            LoginSelectList = HostUri + "request/LoginSelectList.ashx";
            LoginUrl = HostUri + "request/Login.ashx";
        }
    }
}

