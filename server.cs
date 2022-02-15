using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;

namespace chatroom
{
    class Worker
    {
        ChatRoom chatRoom;
        TcpClient client; // current client
        Hashtable clients; // List of all clients
        int clientID; // id of the current client

        /*
            Constructor
            This will create a thread to handle client requests (each thread per client)
        */
        public Worker(TcpClient client, ChatRoom chatRoom, Hashtable clients)
        {
            this.client = client;
            this.chatRoom = chatRoom;
            this.clients = clients;
            this.clientID = clients.Count;
            Thread thread = new Thread(new ThreadStart(HandleRequest));
            thread.Start();
        }

        public void broadcastMessage(string chatRoomName, string message)
        {
            ArrayList clientIDs = this.chatRoom.GetIds(chatRoomName);
            foreach (var clientID in clientIDs)
            {
                // broadcast message to other clients
                if ((int)clientID != this.clientID)
                {
                    TcpClient c = ((Worker)this.clients[clientID]).client;
                    StreamWriter w = new StreamWriter(c.GetStream());
                    w.WriteLine(message);
                    w.Flush();
                }
            }
        }

        /*
            Thread routine
        */
        public void HandleRequest()
        {
            bool loop = true;
            StreamReader reader = new StreamReader(this.client.GetStream());
            StreamWriter writer = new StreamWriter(this.client.GetStream());

            while (loop)
            {
                // Read in the command from user
                string command = reader.ReadLine();

                // Create a Chat Room
                if (command == "!CREATE_CHAT_ROOM")
                {
                    string roomName = reader.ReadLine();
                    bool success = this.chatRoom.CreateRoom(roomName);
                    if (success)
                    {
                        writer.WriteLine("Chat Room " + "<" + roomName + ">" + " created successfully");
                        writer.Flush();
                    }
                    else
                    {
                        writer.WriteLine("Failed to create the chat room");
                        writer.Flush();

                    }
                }

                // List all existing Chat Rooms
                else if (command == "!LIST_CHAT_ROOMS")
                {
                    string chatRoomsStringRepresentation = this.chatRoom.ListRooms();
                    if (chatRoomsStringRepresentation == "")
                    {
                        writer.WriteLine("There is no existing chat room! Please create one");
                        writer.Flush();
                    }
                    else
                    {
                        writer.WriteLine(chatRoomsStringRepresentation);
                        writer.Flush();
                    }

                }

                else if (command == "!JOIN_CHAT_ROOM")
                {
                    // Give client all available chat rooms to choose
                    string chatRooms = this.chatRoom.ListRooms();
                    writer.WriteLine(chatRooms);
                    writer.Flush();

                    // get the name of the chat room the client want to join
                    string name = reader.ReadLine();
                    bool success = this.chatRoom.Join(name, this.clientID);
                    if (!success)
                    {
                        writer.WriteLine("!FAIL");
                        writer.Flush();
                        continue;
                    }
                    else
                    {
                        writer.WriteLine("!SUCCESS");
                        writer.Flush();

                        string oldMessages = this.chatRoom.GetMessages(name);
                        writer.WriteLine(oldMessages);
                        writer.Flush();

                        // while user inside a room

                        while (true)
                        {
                            // read user's input/messages
                            string message = reader.ReadLine();

                            // check if client wants to quit
                            if (message == "!quit")
                            {
                                this.chatRoom.LeaveRoom(name, this.clientID);
                                writer.WriteLine("!quit");
                                break;
                            }

                            else
                            {
                                this.chatRoom.SendMessage(name, message); // send message
                                this.broadcastMessage(name, message); // broadcast message to other clients
                            }
                        }
                        continue;
                    }
                }

            }
        }
    }

    class Server
    {
        static void Main(string[] args)
        {
            ChatRoom chatRoom = new ChatRoom();
            Int32 port = 8080;
            Hashtable clients = new Hashtable();

            TcpListener server = new TcpListener(port);
            server.Start();
            Console.WriteLine("Server is up and running!");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                if (clients.Count < 15) // 15 is the maximum number of clients that can be created
                {
                    Worker w = new Worker(client, chatRoom, clients);
                    clients.Add(clients.Count, w);
                    Console.WriteLine("Client Connected!");

                }
                else
                {
                    StreamWriter writer = new StreamWriter(client.GetStream());
                    writer.WriteLine("Number of Connected Client exceeds the Maximum - 15");
                    writer.Flush();
                    writer.Close();
                    client.Close();
                    Console.WriteLine("Fail to make a connection to a client! Exceeds Maximum Number of Clients");
                }

            }
        }
    }
}