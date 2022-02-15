import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.net.Socket;
import java.util.Scanner;

public class client {
    private Socket socket;
    private BufferedReader bufferedReader;
    private BufferedWriter bufferedWriter;
    private Thread listeningThread;

    public client(Socket socket) {
        try {
            this.socket = socket;
            this.bufferedReader = new BufferedReader(new InputStreamReader(socket.getInputStream()));
            this.bufferedWriter = new BufferedWriter(new OutputStreamWriter(socket.getOutputStream()));
            this.listeningThread = null;
        }catch (Exception e) {
            cleanup();
        }
    }

    /*
    Helper method to split a string using a separator into a list of strings
    and print them out
     */
    private void printHelper(String data, String separator) {
        String[] strings = data.split(separator);
        for(String s : strings) {
            System.out.println(s);
        }
    }

    /*
    Clean up method helps to clean up client
     */
    private void cleanup() {
        try {
            if (this.bufferedReader != null) {
                this.bufferedReader.close();
            }

            if (this.bufferedWriter != null) {
                this.bufferedWriter.close();
            }

            if (this.socket != null) {
                this.socket.close();
            }

        }catch (Exception e) {
            e.printStackTrace();
        };
    }

    /*
    Handle client-side requests
     */
    public void sendCommands() {
        try {
            Scanner scanner = new Scanner(System.in);
            while(socket.isConnected()) {
                System.out.println("OPTIONS");
                System.out.println("1 - Create a chat room");
                System.out.println("2 - List All chat rooms");
                System.out.println("3 - Join a chat room");
                System.out.print(">>> ");

                String command = scanner.nextLine();
                while (command.compareTo("") == 0) {
                    System.out.print(">>> ");
                    command = scanner.nextLine();
                }

                switch(command) {
                    case "1": // Create chat room
                        System.out.print("Please Enter Chat-Room's Name: ");
                        String chatRoomName = scanner.nextLine();
                        System.out.println("Chat Room Name: " + chatRoomName);
                        bufferedWriter.write("!CREATE_CHAT_ROOM");
                        bufferedWriter.newLine();
                        bufferedWriter.write(chatRoomName);
                        bufferedWriter.newLine();
                        bufferedWriter.flush();

                        // Print out the result
                        String result = bufferedReader.readLine();
                        System.out.println();
                        System.out.println(result);
                        System.out.println();
                        continue;

                    case "2": // list chat rooms
                        bufferedWriter.write("!LIST_CHAT_ROOMS");
                        bufferedWriter.newLine();
                        bufferedWriter.flush();

                        // print out the string representation of chat rooms
                        System.out.println();
                        System.out.println("---Chat Rooms---");
                        String chatRoomsStringRepresentation = bufferedReader.readLine();
                        printHelper(chatRoomsStringRepresentation, "#SEP#");
                        System.out.println();
                        continue;

                    case "3": // join chat room
                        bufferedWriter.write("!JOIN_CHAT_ROOM");
                        bufferedWriter.newLine();
                        bufferedWriter.flush();

                        String existingChatRooms = bufferedReader.readLine();
                        System.out.println();
                        System.out.println("---Existing Chat Rooms---");
                        printHelper(existingChatRooms, "#SEP#");
                        System.out.println();

                        System.out.print("Enter Chat Room's Name You Want To Join: ");
                        String name = scanner.nextLine();
                        bufferedWriter.write(name);
                        bufferedWriter.newLine();
                        bufferedWriter.flush();

                        String success = bufferedReader.readLine();
                        if (success.compareTo("!FAIL") == 0) { // fail case
                            System.out.println("There is no chat room with name: " + name);
                            System.out.println();
                            continue;
                        }

                        else if (success.compareTo("!SUCCESS") == 0){ //success case
                            System.out.println();
                            System.out.println("Join chat room "+"<"+name+">"+ " successfully");
                            System.out.println("\t\t*****NOTE*****");
                            System.out.println("\tType your message and hit ENTER to send");
                            System.out.println("\tType \"!quit\" to leave the current chat room");
                            System.out.println();

                            String previousMessages = bufferedReader.readLine();
                            if (previousMessages.compareTo("") != 0) {
                                System.out.println("---Previous Messages---");
                                printHelper(previousMessages, "#SEP#");
                                System.out.println("---End Of Previous Messages---");
                            }

                            createListeningThread();
                            this.listeningThread.start();

                            while(true) {
                                // Read in message and send
                                String message = scanner.nextLine();
                                bufferedWriter.write(message);
                                bufferedWriter.newLine();
                                bufferedWriter.flush();

                                // break out of the while loop if user wants to quit
                                if (message.compareTo("!quit") == 0) {
                                    if (this.listeningThread != null) {
                                        this.listeningThread.interrupt();
                                        this.listeningThread = null;
                                    }
                                    break;
                                }
                            }
                            System.out.println();
                            continue;
                        }

                    default:
                        System.out.println();
                        System.out.println("Unsupported option! Please try again");
                        System.out.println();
                }
            }
        }catch (Exception e){
            cleanup();
        }
    }

    /*
    Creating a thread for listening for messages if the client join a chat room
     */
    public void createListeningThread() {
        this.listeningThread = new Thread(() -> {
            String message;

            while(this.listeningThread != null) {
                try {
                    message = bufferedReader.readLine();
                    if (message.compareTo("!quit") != 0) {
                        System.out.println(message);
                    } else {
                        break;
                    }
                } catch (Exception e) {
                    cleanup();
                }
            }
        });

    }

    public static void main(String[] args) {
        try {
            Socket socket = new Socket("mono", 8080);
            client client = new client(socket);
            client.sendCommands();

        }catch (Exception e){
            e.printStackTrace();
        }
    }
}
