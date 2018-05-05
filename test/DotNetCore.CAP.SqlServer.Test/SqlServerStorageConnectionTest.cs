﻿using System;
using System.Threading.Tasks;
using Dapper;
using DotNetCore.CAP.Infrastructure;
using DotNetCore.CAP.Models;
using Xunit;

namespace DotNetCore.CAP.SqlServer.Test
{
    [Collection("sqlserver")]
    public class SqlServerStorageConnectionTest : DatabaseTestHost
    {
        private SqlServerStorageConnection _storage;

        public SqlServerStorageConnectionTest()
        {
            var options = GetService<SqlServerOptions>();
            var capOptions = GetService<CapOptions>();
            _storage = new SqlServerStorageConnection(options, capOptions);
        }

        [Fact]
        public async Task GetPublishedMessageAsync_Test()
        {
            var sql = "INSERT INTO [Cap].[Published]([Name],[Content],[Retries],[Added],[ExpiresAt],[StatusName]) OUTPUT INSERTED.Id VALUES(@Name,@Content,@Retries,@Added,@ExpiresAt,@StatusName);";
            var publishMessage = new CapPublishedMessage
            {
                Name = "SqlServerStorageConnectionTest",
                Content = "",
                StatusName = StatusName.Scheduled
            };
            var insertedId = default(int);
            using (var connection = ConnectionUtil.CreateConnection())
            {
                insertedId = connection.QueryFirst<int>(sql, publishMessage);
            }
            var message = await _storage.GetPublishedMessageAsync(insertedId);
            Assert.NotNull(message);
            Assert.Equal("SqlServerStorageConnectionTest", message.Name);
            Assert.Equal(StatusName.Scheduled, message.StatusName);
        }
         
        [Fact]
        public async Task StoreReceivedMessageAsync_Test()
        {
            var receivedMessage = new CapReceivedMessage
            {
                Name = "SqlServerStorageConnectionTest",
                Content = "",
                Group = "mygroup",
                StatusName = StatusName.Scheduled
            };

            Exception exception = null;
            try
            {
                await _storage.StoreReceivedMessageAsync(receivedMessage);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            Assert.Null(exception);
        }

        [Fact]
        public async Task GetReceivedMessageAsync_Test()
        {
            var sql = $@"
        INSERT INTO [Cap].[Received]([Name],[Group],[Content],[Retries],[Added],[ExpiresAt],[StatusName]) OUTPUT INSERTED.Id
        VALUES(@Name,@Group,@Content,@Retries,@Added,@ExpiresAt,@StatusName);";
            var receivedMessage = new CapReceivedMessage
            {
                Name = "SqlServerStorageConnectionTest",
                Content = "",
                Group = "mygroup",
                StatusName = StatusName.Scheduled
            };
            var insertedId = default(int);
            using (var connection = ConnectionUtil.CreateConnection())
            {
                insertedId = connection.QueryFirst<int>(sql, receivedMessage);
            }

            var message = await _storage.GetReceivedMessageAsync(insertedId);

            Assert.NotNull(message);
            Assert.Equal(StatusName.Scheduled, message.StatusName);
            Assert.Equal("SqlServerStorageConnectionTest", message.Name);
            Assert.Equal("mygroup", message.Group);
        }
    }
}