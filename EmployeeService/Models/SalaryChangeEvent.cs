namespace EmployeeService.Models
{
    /// <summary>
    /// Represents a salary change CDC event from Debezium Server.
    /// Debezium wraps the record in an envelope with before/after states.
    /// The ExtractNewRecordState transform is NOT used in Debezium Server
    /// so we map the full envelope here.
    /// </summary>
    public class SalaryChangeEvent
    {
        public EmployeeRecord? Before { get; set; }
        public EmployeeRecord? After { get; set; }
        public string Op { get; set; } = string.Empty; // c=create, u=update, d=delete, r=read(snapshot)
        public long TsMs { get; set; }
    }

    public class EmployeeRecord
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public decimal Salary { get; set; }
    }
}