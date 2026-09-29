namespace DigitalLibraryManagementSystem.Models;

public enum BorrowStatus
{
    Borrowed = 1,
    Returned = 2,
    Overdue = 3
}

public enum ReservationStatus
{
    Pending = 1,
    Available = 2,
    Completed = 3,
    Cancelled = 4,
    Expired = 5
}

public enum FineStatus
{
    Unpaid = 1,
    Paid = 2
}
