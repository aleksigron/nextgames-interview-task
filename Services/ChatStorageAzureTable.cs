using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace NextGamesInterviewTask.Services
{
    public class ChatStorageAzureTable : IChatStorage
    {
        private class MessageEntity : TableEntity
        {
            // PartitionKey: Room name
            // RowKey: Inverted message time + random identifier
            public string Sender { get; set; }
            public string Text { get; set; }

            public static string CreateRowKey(DateTime time)
            {
                var epoch = new DateTime(2100, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                long seconds = (long)(epoch - time).TotalSeconds;

                var sb = new StringBuilder(16);

                sb.AppendFormat("{0:x8}", seconds);

                using (RNGCryptoServiceProvider rg = new RNGCryptoServiceProvider())
                {
                    byte[] bytes = new byte[4];
                    rg.GetBytes(bytes);

                    foreach (byte b in bytes)
                        sb.AppendFormat("{0:x2}", b);
                }

                return sb.ToString();
            }
        }

        private class MemberEntity : TableEntity
        {
            // PartitionKey: Room name
            // RowKey: username
        }

        private const string ConnectionStringKey = "StorageConnectionString";
        private const string MemberTableName = "Member";
        private const string MessageTableName = "Message";

        private CloudTableClient tableClient = null;

        public ChatStorageAzureTable(IConfiguration configuration)
        {
            string connectionString = configuration[ConnectionStringKey];
            var account = CloudStorageAccount.Parse(connectionString);
            tableClient = account.CreateCloudTableClient();
        }

        public async Task<bool> AddMessage(string room, string username, string message)
        {
            var messageEntity = new MessageEntity
            {
                PartitionKey = room,
                RowKey = MessageEntity.CreateRowKey(DateTime.UtcNow),
                Sender = username,
                Text = message
            };

            var messageTable = tableClient.GetTableReference(MessageTableName);
            var insertOp = TableOperation.Insert(messageEntity);

            try
            {
                await messageTable.ExecuteAsync(insertOp);

                return true;
            }
            catch (StorageException)
            {
                return false;
            }
        }

        public async Task<bool> AddMember(string room, string username)
        {
            var memberEntity = new MemberEntity
            {
                PartitionKey = room,
                RowKey = username
            };

            var memberTable = tableClient.GetTableReference(MemberTableName);
            var insertOp = TableOperation.Insert(memberEntity);

            try
            {
                await memberTable.ExecuteAsync(insertOp);

                return true;
            }
            catch (StorageException)
            {
                return false;
            }
        }

        public async Task<bool> RemoveMember(string room, string username)
        {
            var memberEntity = new MemberEntity
            {
                PartitionKey = room,
                RowKey = username,
                ETag = "*"
            };

            var memberTable = tableClient.GetTableReference(MemberTableName);
            var deleteOp = TableOperation.Delete(memberEntity);

            try
            {
                await memberTable.ExecuteAsync(deleteOp);

                return true;
            }
            catch (StorageException)
            {
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetMembers(string room)
        {
            string filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, room);
            var query = new TableQuery<MemberEntity>().Where(filter);

            var memberTable = tableClient.GetTableReference(MemberTableName);

            var members = new List<string>();
            TableContinuationToken continuation = null;

            do
            {
                var segment = await memberTable.ExecuteQuerySegmentedAsync(query, continuation);
                continuation = segment.ContinuationToken;
                members.AddRange(segment.Select(entity => entity.RowKey));

            } while (continuation != null);

            return members;
        }
    }
}
