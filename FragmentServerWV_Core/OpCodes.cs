namespace FragmentServerWV
{
    public static class OpCodes
    {
        public const ushort OPCODE_PING = 0x02;

        public const ushort OPCODE_DATA = 0x30;
        public const ushort OPCODE_KEY_EXCHANGE_REQUEST = 0x34;
        public const ushort OPCODE_KEY_EXCHANGE_RESPONSE = 0x35;
        public const ushort OPCODE_KEY_EXCHANGE_ACKNOWLEDGMENT = 0x36;


        public const ushort MAX_AS_NAME_LEN = 0x14;


        //lobby defines...

//0x30_0x7862


/*
struct packet0x30_0x7862
{
	uint16_t lobbyEventType
	uint16_t unkStrLen
	...
}


struct packet0x30_0x7862
//when Event Type == 0x01
//User enters.
{
	uint16_t lobbyEventType = 0x01;
	uint16_t unkStrLen = 12; //0x0c
	uint8_t  dataType;
	char unkStr[unkStrLen]; //I'm not entirely sure this is even a string...
	uint16_t dataLen;
	char data[dataLen]; //it includes null terminator...
}

struct packet0x30_0x7862
//when EventType == 0x02
//Tell?
{
	uint16_t lobbyEventType = 0x02;
	uint16_t unkStrLen = 12; //0x0c
	char unkStr[unkStrLen]; //still not sure it's a string...
	uint16_t dataLen;
	char data[dataLen];	//it includes null terminator, this is what to tell the other user.
}
	
//lobbyEventType = 0x01:
//	User Enters.
dataType 4 = userName
dataType 5 = userGreeting


//lobbyEventType = 0x02:
//	Tell?



*/

        public const ushort LOBBY_USER_ENTER = 0x01;
        public const ushort LOBBY_USER_TELL = 0x02;

        //There's several packets that come in when a user enters,
        public const ushort LOBBY_USER_ENTER_NAME = 0x4;
        public const ushort LOBBY_USER_ENTER_GREETING = 0x5;


        //MISC_DEFINES
        public const ushort CLIENTTYPE_GAME = 0x7430;
        public const ushort CLIENTTYPE_AREASERVER = 0x7431;
        public const ushort CLIENTTYPE_WEBCLIENT = 0x7432;

        public const ushort AREASERVER_STATUS_OPEN = 0x00;
        public const ushort AREASERVER_STATUS_BUSY = 0x02;

//Packet 0x30 subOpcode Defines
//The area server likes to ping in a DATA packet...
        public const ushort OPCODE_DATA_PING = 0x02;

//Nice to know.
        public const ushort OPCODE_DATA_SERVERKEY_CHANGE = 0x31;

//Not sure that's actually what this does.
        public const ushort OPCODE_DATA_PING2 = 0x40;
        public const ushort OPCODE_DATA_PONG2 = 0x41;


        public const ushort OPCODE_DATA_LOGON_REPEAT = 0x7000;
        public const ushort OPCODE_DATA_LOGON_RESPONSE = 0x7001;

//check and see if there's new posts on the BBS?
        public const ushort OPCODE_DATA_BBS_GET_UPDATES = 0x786A;


        public const ushort OPCODE_DATA_LOBBY_ENTERROOM = 0x7006;
        public const ushort OPCODE_DATA_LOBBY_ENTERROOM_OK = 0x7007;

        public const ushort OPCODE_DATA_LOBBY_CHATROOM_GETLIST = 0x7406;
        public const ushort OPCODE_DATA_LOBBY_CHATROOM_CATEGORY = 0x7407;

        public const ushort OPCODE_DATA_LOBBY_CHATROOM_LISTERROR = 0x7408;

//not seen?
        public const ushort OPCODE_DATA_LOBBY_CHATROOM_ENTRY_CATEGORY = 0x7409;
        public const ushort OPCODE_DATA_LOBBY_CHATROOM_CHATROOM = 0x740a;
        public const ushort OPCODE_DATA_LOBBY_CHATROOM_ENTRY_CHATROOM = 0x740b;


        public const ushort OPCODE_DATA_LOBBY_CHATROOM_CREATE = 0x7415;
        public const ushort OPCODE_DATA_LOBBY_CHATROOM_CREATE_OK = 0x7416;
        public const ushort OPCODE_DATA_LOBBY_CHATROOM_CREATE_ERROR = 0x7417;

//Why?
        public const ushort OPCODE_DATA_LOGON_AS2 = 0x7019;

//Doesn't work
        public const ushort OPCODE_DATA_LOGON_AS2_RESPONSE = 0x701d;

        public const ushort OPCODE_DATA_DISKID = 0x7423;

        /*
struct diskiddata
{
    char discID[65]; // might be variable, but so far only 64 byte (+1B null terminator) were encountered
    char static[9]; // might be variable, but so far only 8 byte (+1B null terminator) were encountered, value seems to be a static "dot_hack" string
};
*/


        public const ushort OPCODE_DATA_DISKID_OK = 0x7424;
        public const ushort OPCODE_DATA_DISKID_BAD = 0x7425;

        public const ushort OPCODE_DATA_SAVEID = 0x7426;
        public const ushort OPCODE_DATA_SAVEID_OK = 0x7427;

// public const ushort OPCODE_DATA_SAVEGAME_BAD = 0x7428;
//replies back with 0x7429, with no argument.


        public const ushort OPCODE_DATA_LOBBY_EXITROOM = 0x7444;
        public const ushort OPCODE_DATA_LOBBY_EXITROOM_OK = 0x7445;

        public const ushort OPCODE_DATA_REGISTER_CHAR = 0x742B;

        /*
struct registerChar
{
    uint8_t saveSlot; // 0-2
    char saveID[21] //includes null terminator
    char name[]; //variable length. includes null terminator.
    uint8_t class; // 0 = Twin Blade, 1 = Blademaster, 2 = Heavy Blade, 3 = Heavy Axe, 4 = Long Arm, 5 = Wavemaster
    uint16_t level;
    char greeting[]; //var len, null term.

    uint32_t model; // this code follows ncdysons formula as seen in client.cpp
    uint8_t unk1; // 0x01?
    uint16_t hp;
    uint16_t sp;
    uint32_t gp;
    uint16_t offlineGodCounter;
    uint16_t onlineGodCounter;
    uint16_t unk2; // maybe some kind of story completion bit?

    uint8_t unk4[44];
}
*/


        public const ushort OPCODE_DATA_REGISTER_CHAROK = 0x742C;

        public const ushort OPCODE_DATA_UNREGISTER_CHAR = 0x7432;
        public const ushort OPCODE_DATA_UNREGISTER_CHAROK = 0x7433;

        public const ushort OPCODE_DATA_RETURN_DESKTOP = 0x744a;
        public const ushort OPCODE_DATA_RETURN_DESKTOP_OK = 0x744b;


//main lobby...
        public const ushort OPCODE_DATA_LOBBY_GETMENU = 0x7500;
        public const ushort OPCODE_DATA_LOBBY_CATEGORYLIST = 0x7501; // uint16_t numberOfCategories
        public const ushort OPCODE_DATA_LOBBY_GETMENU_FAIL = 0x7502; //Failed to get list
        public const ushort OPCODE_DATA_LOBBY_ENTRY_CATEGORY = 0x7503; //uint16_t categoryNum, char* categoryName
        public const ushort OPCODE_DATA_LOBBY_LOBBYLIST = 0x7504; //uint16_t numberOfLobbies

        public const ushort
            OPCODE_DATA_LOBBY_ENTRY_LOBBY = 0x7505; //uint16_t lobbyNum, char* lobbyName, uint32_t numUsers (?)


//LOBBY_EVENT?
        public const ushort OPCODE_DATA_LOBBY_EVENT = 0x7862;


        public const ushort OPCODE_DATA_LOBBY_GETSERVERS = 0x7841;
        public const ushort OPCODE_DATA_LOBBY_GETSERVERS_OK = 0x7842;

//ANOTHER Tree
        public const ushort OPCODE_DATA_LOBBY_GETSERVERS_GETLIST = 0x7506;
        public const ushort OPCODE_DATA_LOBBY_GETSERVERS_CATEGORYLIST = 0x7507; //arg is # items?
        public const ushort OPCODE_DATA_LOBBY_GETSERVERS_FAIL = 0x7508; //FAILED
        public const ushort OPCODE_DATA_LOBBY_GETSERVERS_ENTRY_CATEGORY = 0x7509; //The DIRS
        public const ushort OPCODE_DATA_LOBBY_GETSERVERS_SERVERLIST = 0x750A; //arg is # items?
        public const ushort OPCODE_DATA_LOBBY_GETSERVERS_ENTRY_SERVER = 0x750B; //yay...


        public const ushort OPCODE_DATA_LOBBY_GETSERVERS_EXIT = 0x7844;
        public const ushort OPCODE_DATA_LOBBY_GETSERVERS_EXIT_OK = 0x7845;

        public const ushort OPCODE_DATA_NEWS_GETMENU = 0x784E;
        public const ushort OPCODE_DATA_NEWS_CATEGORYLIST = 0x784F; //arg is #of items in category list
        public const ushort OPCODE_DATA_NEWS_GETMENU_FAILED = 0x7850; //Failed
        public const ushort OPCODE_DATA_NEWS_ENTRY_CATEGORY = 0x7851; //Category list Entry
        public const ushort OPCODE_DATA_NEWS_ARTICLELIST = 0x7852; //Article list, Arg is # entries

        public const ushort OPCODE_DATA_NEWS_ENTRY_ARTICLE = 0x7853; //Article List Entry
//7853 - ok/no data
//7852 - ok/wants more data?
//7851 - ok/no data?
//7850 - failed
//784f - ok


        public const ushort OPCODE_DATA_NEWS_GETPOST = 0x7854;

        public const ushort OPCODE_DATA_NEWS_SENDPOST = 0x7855;
//7856
//7857
//7855


        public const ushort OPCODE_DATA_MAIL_GET = 0x7803;
        public const ushort OPCODE_DATA_MAIL_GETOK = 0x7804;
        public const ushort OPCODE_DATA_MAIL_GET_NEWMAIL_HEADER = 0x788a;
        public const ushort OPCODE_DATA_MAIL_GET_MAIL_BODY = 0x7806;
        public const ushort OPCODE_DATA_MAIL_GET_MAIL_BODY_RESPONSE = 0x7807;

//BBS	POSTING	STUFF
        public const ushort OPCODE_DATA_BBS_GETMENU = 0x7848;
        public const ushort OPCODE_DATA_BBS_CATEGORYLIST = 0x7849;
        public const ushort OPCODE_DATA_BBS_GETMENU_FAILED = 0x784a;
        public const ushort OPCODE_DATA_BBS_ENTRY_CATEGORY = 0x784b;
        public const ushort OPCODE_DATA_BBS_THREADLIST = 0x784c;

        public const ushort OPCODE_DATA_BBS_ENTRY_THREAD = 0x784d;
        //7849 threadCat
        //784a error
        //784b catEnrty
        //784c threadList
        //784d threadEnrty			

        public const ushort OPCODE_DATA_BBS_THREAD_GETMENU = 0x7818;
        public const ushort OPCODE_DATA_BBS_THREAD_LIST = 0x7819;
        public const ushort OPCODE_DATA_BBS_ENTRY_POST_META = 0x781a;
        public const ushort OPCODE_DATA_BBS_THREAD_ENTRY_POST = 0x781b;
        public const ushort OPCODE_DATA_BBS_THREAD_GET_CONTENT = 0x781c;
//7819
//781a
//781b


        public const ushort OPCODE_RANKING_VIEW_ALL = 0x7832;
        public const ushort OPCODE_RANKING_VIEW_PLAYER = 0x7838;
        //These happen upon entering ALTIMIT DESKTOP
        public const ushort OPCODE_DATA_MAILCHECK = 0x7867;
        public const ushort OPCODE_DATA_MAILCHECK_OK = 0x7868;

        public const ushort OPCODE_DATA_MAILCHECK_FAIL = 0x7869;

//
        public const ushort OPCODE_DATA_NEWCHECK = 0x786D;

        public const ushort OPCODE_DATA_NEWCHECK_OK = 0x786E;
//

        public const ushort OPCODE_DATA_COM = 0x7876;
        public const ushort OPCODE_DATA_COM_OK = 0x7877;

        public const ushort OPCODE_DATA_SELECT_CHAR = 0x789f;


        /*
struct selectchar
{
    char discID[65]; // most likely variable size, but we only encounter 64byte (+1B null terminator) really
    char systemSaveID[21]; // most likely variable size, but we only encounter 20byte (+1B null terminator) really
    uint8_t unk1; // same as unk1 in OPCODE_DATA_REGISTER_CHAR
    char characterSaveID[21]; // most likely variable size, but we only encounter 20 byte (+1B null terminator) really
};
OPCODE_DATA_SELECT_CHAR is a variable size packet with 3 null terminated strings appended to each others end.

ex. "1234ABCD\0DEADBEEF\0C01DB15D\0"

They seem to be hex values represented in ascii, like this "0e041409..." and so on.

The first string seems to be the disc id and usually is 0x40 (0x41 with terminator) bytes long, representing a 32byte hex value.
Due to the way we patched the disc id (DNAS) out of the iso, every user has the same value here for now, namely all zero bytes.

The second one seems to be the console / savedata id and probably corresponds to the system savedata file people create when they first launch fragment.

The third and final one is the character id and is created when people create a new character and reported to the server via the OPCODE_DATA_REGISTER_CHAR packet.
*/

        public const ushort OPCODE_DATA_SELECT_CHAROK = 0x78A0;

// probably something else but this works in our favor
        public const ushort OPCODE_DATA_SELECT_CHARDENIED = 0x78a1;


        public const ushort OPCODE_DATA_SELECT2_CHAR = 0x78a2;

/*
OPCODE_DATA_SELECT_CHAR2 seems to be a 1:1 clone of the normal OPCODE_DATA_SELECT_CHAR packet.
*/
        public const ushort OPCODE_DATA_SELECT2_CHAROK = 0x78a3;


        public const ushort OPCODE_DATA_LOGON = 0x78AB;

//Area server doesn't like 0x7001
        public const ushort OPCODE_DATA_LOGON_RESPONSE_AS = 0x78AD;


        public const ushort OPCODE_DATA_MAIL_SEND = 0x7800;

/*
	DATA_MAIL_SEND PACKET DESC
	struct mailPacket
	{
		uint32_t unk1 = 0xFFFFFFFF
		uint32_t date;
		char * recipient;
		uint32_t unk2;
		uint16_t unk3;
		char * sender;
		char unk4;
		char subject[0x80];
		char text[0x47e];
		
		
		
		
		
		
	}
*/
        public const ushort OPCODE_DATA_BBS_POST = 0x7812;
/*
	DATA_BBS_POST	PACKET	DESC
	struct bbsPostPacket
	{
		uint32_t unk1 0x00000000
		char userName[0x4c];
		uint16_t unk2;
		uint16_t dSize; //data size...
		char title[0x32];		//message title
		char body[0x25a]; //message body. 602 chars. 


*/


        public const ushort OPCODE_DATA_MAIL_SEND_OK = 0x7801;


        public const ushort OPCODE_DATA_LOBBY_FAVORITES_AS_INQUIRY = 0x7858;

//sends the DISKID of the lobby server to get the status of... I think.


///////////////
//AREA	SERVER	DEFINES:
///////////////
        public const ushort OPCODE_DATA_AS_DISKID = 0x785B;
        public const ushort OPCODE_DATA_AS_DISKID_OK = 0x785C;
        public const ushort OPCODE_DATA_AS_DISKID_FAIL = 0x785d;

        public const ushort OPCODE_DATA_AS_IPPORT = 0x7013;
        public const ushort OPCODE_DATA_AS_IPPORT_OK = 0x7014;

        public const ushort OPCODE_DATA_AS_PUBLISH = 0x78AE;
        public const ushort OPCODE_DATA_AS_PUBLISH_OK = 0x78AF;


        public const ushort OPCODE_DATA_AS_PUBLISH_DETAILS1 = 0x7011;

        public const ushort OPCODE_DATA_AS_PUBLISH_DETAILS1_OK = 0x7012;
//initial server details...
/*
	struct asPublishDetails1:
	{
		char diskID[65];
		char * serverName; //this is variable length, but no longer than 21 I believe, including null terminator.
		uint16_t serverLevel;
		uint16_t serverType;	//serverType
		uint16_t sUnk;	//I'm not sure what that's for yet.
		uint8_t sStatus;		//serverStatus.
		uint8_t serverID[8];
		//We don't really need to worry about the server type or status. the game know's what's up.
	}						
*/

        public const ushort OPCODE_DATA_AS_PUBLISH_DETAILS2 = 0x7016;

        public const ushort OPCODE_DATA_AS_PUBLISH_DETAILS2_OK = 0x7017;
//I'm still not sure what's up with this dude.


        public const ushort OPCODE_DATA_AS_PUBLISH_DETAILS3 = 0x7881;
        public const ushort OPCODE_DATA_AS_PUBLISH_DETAILS3_OK = 0x7882;

        public const ushort OPCODE_DATA_AS_PUBLISH_DETAILS4 = 0x7887;
        public const ushort OPCODE_DATA_AS_PUBLISH_DETAILS4_OK = 0x7888;

        public const ushort OPCODE_DATA_AS_UPDATE_USERNUM = 0x741D; //uint32_t numUsers

        public const ushort OPCODE_DATA_AS_PUBLISH_DETAILS5_OK = 0x741e;
//update user num?


        public const ushort OPCODE_DATA_AS_PUBLISH_DETAILS6 = 0x78a7;
        public const ushort OPCODE_DATA_AS_PUBLISH_DETAILS6_OK = 0x78a8;

        public const ushort OPCODE_DATA_AS_UPDATE_STATUS = 0x780C;

        public const ushort OPCODE_DATA_AS_PUBLISH_DETAILS7_OK = 0x780d;
/*
	struct asUpdatStatus:
	{
		uint16_t unk1;		//NO idea what this is about...
		char diskID[65];
		char * serverName; //this is variable length, but no longer than 21 I believe, including null terminator.
		uint16_t serverLevel;
		uint16_t serverType;	//serverType
		uint8_t sStatus;		//serverStatus.
		uint8_t serverID[8];
		//We don't really need to worry about the server type or status. the game know's what's up.
	}						
*/

//:3
        public const ushort OPCODE_DATA_AS_NAMEID = 0x5778;
        public const ushort OPCODE_DATA_AS_DISKID2 = 0x78a7; //again?


        /*
7011 diskid,name,unk,unk,id#
7016 uink
7881 diskid,id#,unk
7887 diskid,unk,name,id,unk
741d null
780c diskid,name,unk,unk,id#
78a7 diskid

    
        
*/


        public const ushort LOBBY_TYPE_GUILD = 0x7418;
        public const ushort LOBBY_TYPE_MAIN = 0x7403;
        public const ushort OPCODE_CLIENT_LEAVING_LOBBY = 0x700a;
        public const ushort ARGUMENT_INVITE_TO_GUILD = 0x7606;
        public const ushort OPCODE_INVITE_TO_GUILD = 0x7603;
        public const ushort OPCODE_ACCEPT_GUILD_INVITE = 0x7607;
        public const ushort OPCODE_PRIVATE_BROADCAST = 0x788c;
        public const ushort OPCODE_GUILD_VIEW = 0x772c;

    }
}