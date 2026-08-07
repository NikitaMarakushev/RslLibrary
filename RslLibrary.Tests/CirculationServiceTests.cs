using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using RslLibrary.Services;
using RslLibrary.Models;
using RslLibrary.Repositories;
using RslLibrary.Constants;

namespace RslLibrary.Tests;

public class CirculationServiceTests
{
    [Fact]
    public async Task IssueBookAsync_ShouldReturnFailure_WhenUserExceedsMaxAllowedBooks()
    {
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockAuditService = new Mock<IAuditService>();
        var mockLogger = new Mock<ILogger<CirculationService>>();

        var userId = 101;
        var inventoryNumber = "RBG-004912";
   
        var mockCopy = new BookCopy
        {
            Id = 1,
            InventoryNumber = inventoryNumber,
            StatusId = CopyStatuses.Available
        };

        mockUnitOfWork.Setup(u => u.Copies.GetByInventoryNumberAsync(inventoryNumber))
            .ReturnsAsync(mockCopy);

        mockUnitOfWork.Setup(u => u.Loans.GetActiveLoansCountByUserAsync(userId))
            .ReturnsAsync(5);

        var circulationService = new CirculationService(
            mockUnitOfWork.Object,
            mockAuditService.Object,
            mockLogger.Object);

        var result = await circulationService.IssueBookAsync(userId, inventoryNumber);

        Assert.False(result.IsSuccess);
        Assert.Equal("Превышен лимит книг. Читатель уже удерживает 5 экз.", result.Message);

        mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task IssueBookAsync_ShouldReturnSuccess_WhenBookIsAvailable()
    {
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockAuditService = new Mock<IAuditService>();
        var mockLogger = new Mock<ILogger<CirculationService>>();

        var userId = 101;
        var inventoryNumber = "RBG-004912";

        var mockCopy = new BookCopy
        {
            Id = 1,
            InventoryNumber = inventoryNumber,
            StatusId = CopyStatuses.Available
        };

        mockUnitOfWork.Setup(u => u.Copies.GetByInventoryNumberAsync(inventoryNumber))
            .ReturnsAsync(mockCopy);

        mockUnitOfWork.Setup(u => u.Loans.GetActiveLoansCountByUserAsync(userId))
            .ReturnsAsync(2);

        mockUnitOfWork.Setup(u => u.Loans.HasUserOverdueLoansAsync(userId))
            .ReturnsAsync(false);

        var circulationService = new CirculationService(
            mockUnitOfWork.Object,
            mockAuditService.Object,
            mockLogger.Object);

        var result = await circulationService.IssueBookAsync(userId, inventoryNumber);

        Assert.True(result.IsSuccess);

        Assert.Equal(CopyStatuses.Loaned, mockCopy.StatusId);

        mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }
}