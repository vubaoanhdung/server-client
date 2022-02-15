.PHONY: clean

all: chatroom.dll server.exe client

chatroom.dll: chatroom.cs
	mcs -t:library chatroom.cs

server.exe: server.cs chatroom.dll
	mcs -r:chatroom.dll server.cs

client:
	javac client.java

clean:
	rm -f *.exe *.dll *.class