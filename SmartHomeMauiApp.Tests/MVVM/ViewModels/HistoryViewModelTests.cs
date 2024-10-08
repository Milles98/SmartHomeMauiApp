using Moq;
using Shared.Library.Models;
using SmartHomeMauiApp.Database;
using SmartHomeMauiApp.MVVM.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace SmartHomeMauiApp.Tests.MVVM.ViewModels
{
    public class HistoryViewModelTests
    {
        private readonly Mock<ISmarthomeContext> _dbContextMock;
        private readonly HistoryViewModel _viewModel;

        public HistoryViewModelTests()
        {
            _dbContextMock = new Mock<ISmarthomeContext>();

            _viewModel = new HistoryViewModel(_dbContextMock.Object);
        }

        [Fact]
        public async Task LoadSettingsAsync_ShouldHandleException()
        {
            // Arrange
            _dbContextMock.Setup(db => db.GetAllDeviceSettingsAsync()).ThrowsAsync(new System.Exception("Database error"));

            // Act
            await _viewModel.LoadSettingsAsync();

            // Assert
            Assert.Equal("Failed to load settings.", _viewModel.ResponseMessage);
        }
    }
}
