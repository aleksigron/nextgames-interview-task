# nextgames-interview-task
Technical test done for Next Games interview process.

This project implements an online chat room where users can send messages in
real time.

## Features
- Messages are visible to all users online
- Users currently online are visible
- All messages are persisted in a Azure Storage account
- Web app is built with
    - ASP.NET Core as the server
    - SignalR as the realtime communication protocol
    - React.js as the frontend
- CSS&JS minification is working
- .NET Core 2 dependency injection is used
- Deployed to Azure App Service at https://nextgamesinterviewtaskdev.azurewebsites.net/

## Implementation highlights

### SignalR hub communication design
Clients can join the chat room and set their username after connecting to the
hub using Join command. The username is saved to the connection context within
the hub. Clients can send messages to the chat room using their previously set
username with the SendMessage command. Leaving the chat room is done by closing
the hub connection.

When users join or leave the chat room, their username is broadcast to other
connected clients with the UserJoined and UserLeft commands, respectively. When
a user joins a room, all current room member usernames and their own username
is sent with the same UserJoined command. That's why UserJoined uses an array
as the parameter to enable both of these use cases.

### Chat persistent storage design
The chat room stores messages sent and the current joined members in Azure
Storage table service. The SignalR hub ChatHub class depends on a simple
interface IChatStorage, that is concretely implemented by ChatStorageAzureTable.
The interface contains 4 methods: AddMessage, AddMember, RemoveMember,
GetMembers. The dependency is managed by built-in .Net Core 2 Dependecy
Injection.