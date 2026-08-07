using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using RslLibrary.Services;
using RslLibrary.Models;
using RslLibrary.Repositories;
using RslLibrary.Constants;

namespace RslLibrary.Tests;

public class ReservationServiceTests
{
    [Fact]
    public async Task CreateReservationAsync_ShouldReturnTrue_WhenCopyIsAvailable()
    {
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockAuditService = new Mock<IAuditService>();
        var mockLogger = new Mock<ILogger<ReservationService>>();

        var userId = 101;
        var copyId = 5;

        var mockCopy = new BookCopy
        {
            Id = copyId,
            BookId = 1,
            StatusId = CopyStatuses.Available
        };

        mockUnitOfWork.Setup(u => u.Copies.GetByIdAsync(copyId))
            .ReturnsAsync(mockCopy);

        var reservationService = new ReservationService(
            mockUnitOfWork.Object,
            mockAuditService.Object,
            mockLogger.Object);

        var result = await reservationService.CreateReservationAsync(userId, copyId);

        Assert.True(result);
        Assert.Equal(CopyStatuses.Reserved, mockCopy.StatusId);
        mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldReturnFalse_WhenCopyIsNotAvailable()
    {
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockAuditService = new Mock<IAuditService>();
        var mockLogger = new Mock<ILogger<ReservationService>>();

        var userId = 101;
        var copyId = 5;

        var mockCopy = new BookCopy
        {
            Id = copyId,
            StatusId = CopyStatuses.Loaned
        };

        mockUnitOfWork.Setup(u => u.Copies.GetByIdAsync(copyId))
            .ReturnsAsync(mockCopy);

        var reservationService = new ReservationService(
            mockUnitOfWork.Object,
            mockAuditService.Object,
            mockLogger.Object);

        var result = await reservationService.CreateReservationAsync(userId, copyId);

        Assert.False(result);
        mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task CancelReservationAsync_ShouldReturnTrue_WhenReservationExists()
    {
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockAuditService = new Mock<IAuditService>();
        var mockLogger = new Mock<ILogger<ReservationService>>();

        var reservationId = 10;
        var userId = 101;
        var copyId = 5;

        var mockReservation = new Reservation
        {
            Id = reservationId,
            UserId = userId,
            CopyId = copyId,
            StatusId = ReservationStatuses.Waiting
        };

        var mockCopy = new BookCopy
        {
            Id = copyId,
            StatusId = CopyStatuses.Reserved
        };

        mockUnitOfWork.Setup(u => u.Reservations.GetByIdAsync(reservationId))
            .ReturnsAsync(mockReservation);

        mockUnitOfWork.Setup(u => u.Copies.GetByIdAsync(copyId))
            .ReturnsAsync(mockCopy);

        var reservationService = new ReservationService(
            mockUnitOfWork.Object,
            mockAuditService.Object,
            mockLogger.Object);

        var result = await reservationService.CancelReservationAsync(reservationId, userId);

        Assert.True(result);
        Assert.Equal(ReservationStatuses.Cancelled, mockReservation.StatusId);
        Assert.Equal(CopyStatuses.Available, mockCopy.StatusId);
        mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }
}