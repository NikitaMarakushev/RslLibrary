namespace RslLibrary.Constants;

public static class CopyStatuses
{
    public const short Available = 1;
    public const short Loaned = 2;
    public const short Reserved = 3;
    public const short Lost = 4;
    public const short Repair = 5;
}

public static class LoanStatuses
{
    public const short Active = 1;
    public const short Returned = 2;
    public const short Overdue = 3;
}

public static class ReservationStatuses
{
    public const short Waiting = 1;
    public const short Ready = 2;
    public const short Completed = 3;
    public const short Cancelled = 4;
}

public static class RoleNames
{
    public const string Administrator = "Administrator";
    public const string Librarian = "Librarian";
    public const string Reader = "Reader";
}