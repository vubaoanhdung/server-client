using System;
using System.Collections;

namespace chatroom
{
    public class ChatRoom
    {
        private Hashtable rooms = new Hashtable();

        /*
            Create a new Chat Room
            Params:
                - name: name of the chat room
        */
        public bool CreateRoom(string name)
        {
            try
            {
                Room newRoom = new Room(name, "", new ArrayList());
                this.rooms.Add(name, newRoom);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        /*
            Return the string representation for all chat rooms
        */
        public string ListRooms()
        {
            string result = "";
            ICollection keys = this.rooms.Keys;
            foreach (var k in keys)
            {
                result = k.ToString() + "#SEP#" + result;
            }
            return result;
        }

        /*
            Send a message to a specific chat room
            Params:
            - name: name of the chat room
            - message: message to send
        */
        public void SendMessage(string name, string message)
        {
            string oldMessages = GetMessages(name);
            string result;
            // if there is no existing message in the chat room
            if (oldMessages == "")
            {
                result = message;
            }
            else
            {
                result = oldMessages + "#SEP#" + message;
            }
            var r = (Room)this.rooms[name];
            r.Messages = result;
            this.rooms[name] = r;
        }

        /*
            Get a string representation of all messages in a specific chat room
            Params:
            - name: name of the chat room
        */
        public string GetMessages(string name)
        {
            return ((Room)this.rooms[name]).Messages;

        }

        /*
            Join a specific chat room
            Params:
            - name: name of the chat room
            - clientID: the id of the client who wants to join
        */
        public bool Join(string name, int clientID)
        {
            if (rooms.ContainsKey(name))
            {
                ((Room)rooms[name]).IDs.Add(clientID);
                return true;
            }
            else
            {
                return false;
            }
        }

        /*
            Leave a specific chat room
            Params:
            - name: name of the chat room
            - clientID: the id of the client who wants to leave
        */
        public void LeaveRoom(string name, int clientID)
        {
            ((Room)rooms[name]).IDs.Remove(clientID);
        }

        public ArrayList GetIds(string name)
        {
            return ((Room)rooms[name]).IDs;
        }

    }

    /*
        Room Structure
    */
    public struct Room
    {

        public string Messages { get; set; }
        public ArrayList IDs { get; set; }
        public string Name { get; set; }

        public Room(string name, string initialMessages, ArrayList initialIds)
        {
            this.Name = name; // Name of the chat room
            this.Messages = initialMessages; // messages 
            this.IDs = initialIds; // List of IDs of Clients
        }
    }
}