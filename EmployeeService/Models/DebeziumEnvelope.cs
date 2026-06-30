namespace EmployeeService.Models
{
	/// <summary>
	/// Outer wrapper — Debezium always sends {schema:{...}, payload:{...}}
	/// </summary>
	public class DebeziumEnvelope
	{
		public SalaryChangeEvent? Payload { get; set; }
	}

	/// <summary>
	/// Represents a salary change CDC event from Debezium Server.
	/// </summary>
	public class SalaryChangeEvent
	{
		public EmployeeRecord? Before { get; set; }
		public EmployeeRecord? After { get; set; }
		public string Op { get; set; } = string.Empty;
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
		public string Salary { get; set; } = string.Empty;
	}
}