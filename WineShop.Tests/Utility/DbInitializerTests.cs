using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using WineShop.Data;
using WineShop.Utility;
using System;
using System.Threading.Tasks;

namespace WineShop.Tests.Utility
{
    public class DbInitializerTests
    {
        private Mock<RoleManager<IdentityRole>> GetMockRoleManager()
        {
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object, null, null, null, null);
        }

        [Fact]
        public async Task InitializeAsync_ShouldMigrateAndCreateRoles_WhenSuccess()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var dbContext = new ApplicationDbContext(options);

            var roleManagerMock = GetMockRoleManager();
            roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(ApplicationDbContext))).Returns(dbContext);
            serviceProviderMock.Setup(x => x.GetService(typeof(RoleManager<IdentityRole>))).Returns(roleManagerMock.Object);

            await DbInitializer.InitializeAsync(serviceProviderMock.Object);

            roleManagerMock.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == WC.AdminRole)), Times.Once);
            roleManagerMock.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == WC.CustomerRole)), Times.Once);
        }

        [Fact]
        public async Task InitializeAsync_ShouldCatchExceptionAndDelay_WhenErrorOccurs()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var dbContext = new ApplicationDbContext(options);

            var roleManagerMock = GetMockRoleManager();

            roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ThrowsAsync(new Exception("Forced error"));

            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(ApplicationDbContext))).Returns(dbContext);
            serviceProviderMock.Setup(x => x.GetService(typeof(RoleManager<IdentityRole>))).Returns(roleManagerMock.Object);

            await DbInitializer.InitializeAsync(serviceProviderMock.Object);

            roleManagerMock.Verify(x => x.RoleExistsAsync(It.IsAny<string>()), Times.Exactly(10));
        }
    }
}