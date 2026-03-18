using Xunit;
using Library.Domain.Models;
using System;

namespace Library.Tests;

public class LoanTests
{
    [Fact]
    public void Loan_IsActive_WhenNotReturned()
    {
        // Arrange
        var loan = new Loan { ReturnedDate = null };
        
        // Act & Assert
        Assert.True(loan.IsActive);
    }

    [Fact]
    public void Loan_IsNotActive_WhenReturned()
    {
        // Arrange
        var loan = new Loan { ReturnedDate = DateTime.Today };
        
        // Act & Assert
        Assert.False(loan.IsActive);
    }

    [Fact]
    public void Loan_IsOverdue_WhenDueDatePassedAndNotReturned()
    {
        // Arrange
        var loan = new Loan 
        { 
            DueDate = DateTime.Today.AddDays(-1),
            ReturnedDate = null 
        };
        
        // Act & Assert
        Assert.True(loan.IsOverdue);
    }

    [Fact]
    public void Loan_IsNotOverdue_WhenDueDateNotPassed()
    {
        // Arrange
        var loan = new Loan 
        { 
            DueDate = DateTime.Today.AddDays(1),
            ReturnedDate = null 
        };
        
        // Act & Assert
        Assert.False(loan.IsOverdue);
    }

    [Fact]
    public void Loan_IsNotOverdue_WhenReturned()
    {
        // Arrange
        var loan = new Loan 
        { 
            DueDate = DateTime.Today.AddDays(-1),
            ReturnedDate = DateTime.Today 
        };
        
        // Act & Assert
        Assert.False(loan.IsOverdue);
    }
}
