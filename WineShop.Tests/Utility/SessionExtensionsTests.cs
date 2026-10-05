using Microsoft.AspNetCore.Http;
using Xunit;
using WineShop.Models;
using WineShop.Utility;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace WineShop.Tests.Utility
{
    public class FakeSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();
        public bool IsAvailable => true;
        public string Id => "fake-session-id";
        public IEnumerable<string> Keys => _store.Keys;
        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
    }

    public class SessionExtensionsTests
    {
        [Fact]
        public void Set_ShouldSerializeAndStoreValue()
        {
            // Arrange
            var session = new FakeSession();
            var testObj = new { Name = "Test", Value = 123 };

            // Act
            session.Set("TestKey", testObj);

            // Assert
            var storedJson = session.GetString("TestKey");
            Assert.NotNull(storedJson);
            Assert.Contains("Test", storedJson);
        }

        [Fact]
        public void Get_ShouldReturnDefault_WhenKeyDoesNotExist()
        {
            // Arrange
            var session = new FakeSession();

            // Act
            var result = session.Get<Product>("NonExistentKey");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Get_ShouldReturnDeserializedObject_WhenKeyExists()
        {
            // Arrange
            var session = new FakeSession();
            var testObj = new Product { Id = 1, Name = "Wino" };
            session.SetString("TestKey", JsonSerializer.Serialize(testObj));

            // Act
            var result = session.Get<Product>("TestKey");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Wino", result.Name);
        }
    }
}