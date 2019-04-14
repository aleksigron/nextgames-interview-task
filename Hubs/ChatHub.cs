using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;

namespace NextGamesInterviewTask.Hubs
{
    public class ChatHub : Hub
    {
        private const string ContextUsernameKey = "username";
        private const string ChatRoomName = "global";

        private Services.IChatStorage storage;

        public ChatHub(Services.IChatStorage chatStorage)
        {
            storage = chatStorage;
        }

		/// <summary>
		/// Client will execute Join after it has connected to the hub to set
		/// their username in the chat room.
		/// </summary>
		/// <param name="username">The username to use with the client</param>
		/// <returns>Task</returns>
        public async Task Join(string username)
        {
            // Don't allow the client to change the username once they've joined
            if (Context.Items.ContainsKey(ContextUsernameKey) == false)
            {
				// Save the username to the connection context
                Context.Items.Add(ContextUsernameKey, username);

                // Saving the membership to storage succeeds
                if (await storage.AddMember(ChatRoomName, username))
                {
                    // Initiate sending a notification to other clients that a user joined
                    Task othersTask = Clients.Others.SendAsync("UserJoined", new string[] { username });

                    // Get all members from storage and send the list to the joined client
                    var members = await storage.GetMembers(ChatRoomName);
                    await Clients.Caller.SendAsync("UserJoined", members);
                    
                    // Wait for the notification to other clients to finish
                    await othersTask;
                }
            }
        }

		/// <summary>
		/// OnDisconnectedAsync will be called when a connection is closed.
		/// </summary>
		/// <param name="exception">Exception containing information about why
		/// the connection closed, or null if it was closed intentionally.</param>
		/// <returns>Task</returns>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // The client had set their username with the Join command
            if (Context.Items.TryGetValue(ContextUsernameKey, out object usernameObj))
            {
                if (usernameObj is string)
                {
                    string username = usernameObj as string;

                    // Initiate the remove member from storage task
                    var removeTask = storage.RemoveMember(ChatRoomName, username);

                    // Send notification of user leaving to all connected clients
                    await Clients.All.SendAsync("UserLeft", username);

					// Wait for remove task to finish
					await removeTask;
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

		/// <summary>
		/// Client will execute SendMessage to send a message to the chat room.
		/// </summary>
		/// <param name="message">The message text to send</param>
		/// <returns>Task</returns>
        public async Task SendMessage(string message)
        {
            // The client has set their username with the Join command
            if (Context.Items.TryGetValue(ContextUsernameKey, out object usernameObj))
            {
                if (usernameObj is string)
                {
                    string username = usernameObj as string;

                    // Message successfully saved to storage
                    if (await storage.AddMessage(ChatRoomName, username, message))
                    {
                        // Broadcast the message to all clients
                        await Clients.All.SendAsync("MessageReceived", username, message);
                    }
                }
            }
        }
    }
}
