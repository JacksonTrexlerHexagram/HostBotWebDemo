using System.Runtime.CompilerServices;

namespace Hexagram.Saga.Networking {
    public class Routes {
        
        public const string USER_LOGIN = "/users/login";

        public static string USER_REFRESH_TOKEN(string id) {
            return "/users/" + id + "/refresh_token";
        }

        public const string USERS = "/users";

        public static string USER(string id) {
            return "/users/" + id;
        }

        public static string USER_PROPERTIES(string id) {
            return "/users/" + id + "/properties";
        }

        public static string USER_PROPERTY(string id, string propertyName, string query) {
            string propertyFragment = "/users/" + id + "/properties/" + propertyName;

            if (query != "") {
                propertyFragment += "/" + query;
            }

            return propertyFragment;
        }

        public const string BOTS = "/bots";

        public static string BOT(string botId) {
            return "/bots/" + botId;
        }

        public static string BOT_PROPERTIES(string botId) {
            return "/bots/" + botId + "/properties";
        }

        public static string BOT_PROPERTY(string botId, string propertyName, string query) {
            string propertyFragment = "/bots/" + botId + "/properties/" + propertyName;

            if (query != "") {
                propertyFragment += "/" + query;
            }

            return propertyFragment;
        }
        
        public const string GLOBAL_PROPERTIES = "/globals/properties";

        public static string GLOBAL_PROPERTY(string propertyName) {
            return "/globals/properties/" + propertyName;
        }

        public const string WS_AUTHENTICATE = "authentication";
        public const string WS_PROPERTY_CHANGE = "/properties";
        public const string WS_MESSAGE = "/messages";
        public const string WS_MESSAGE_SEND = "/users/message";
        public static string WS_PROPERTY_BOT_SEND(string operation = null) {
            if (operation == null) {
                return "/bots/properties";
            }
            return "/bots/properties/" + operation;
        }

        public static string WS_PROPERTY_USER_SEND(string operation = null) {
            if (operation == null) {
                return "/users/properties";
            }
            return "/users/properties/" + operation;
        }
        
        public static string WS_PROPERTY_GLOBAL_SEND(string operation = null) {
            if (operation == null) {
                return "/globals/properties";
            }
            return "/globals/properties/" + operation;
        }
        
        public const string WS_USER_JOIN = "/users/join";
        public const string WS_USER_LEAVE = "/users/leave";
        public const string WS_BOT_JOIN = "/bots/join";
        public const string WS_BOT_LEAVE = "/bots/leave";
        public const string WS_GLOBALS_JOIN = "/globals/join";
        public const string WS_GLOBALS_LEAVE = "/globals/leave";
    }
}