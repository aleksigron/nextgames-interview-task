using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextGamesInterviewTask.Services
{
    public interface IChatStorage
    {
		/// <summary>
		/// Add a message to storage
		/// </summary>
		/// <param name="room">The chat room in which the message was sent</param>
		/// <param name="username">The user that sent the message</param>
		/// <param name="message">The message text</param>
		/// <returns>Whether adding the message to storage succeeded</returns>
		Task<bool> AddMessage(string room, string username, string message);

		/// <summary>
		/// Add a chat room member to storage
		/// </summary>
		/// <param name="room">The chat room to which to add the member</param>
		/// <param name="username">The user to add</param>
		/// <returns>Whether adding the member to storage succeeded</returns>
		Task<bool> AddMember(string room, string username);

		/// <summary>
		/// Remove a chat room member from storage
		/// </summary>
		/// <param name="room">The chat room from which to remove the member</param>
		/// <param name="username">The user to remove</param>
		/// <returns>Whether removing the member from storage succeeded</returns>
		Task<bool> RemoveMember(string room, string username);

		/// <summary>
		/// Get all members in a chat room
		/// </summary>
		/// <param name="room">The chat room from which to get members</param>
		/// <returns>A list of the chat room members</returns>
        Task<IEnumerable<string>> GetMembers(string room);
    }
}
