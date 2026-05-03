namespace SanuApi.Application.DTOs.Customer
{
    public class AddCustomerAbsenceRequestDto
    {
        public int IdClass { get; set; }
        public int IdCustomer { get; set; }
        public DateTime DateAbsence { get; set; }
        /// <summary>Estado: 'presente', 'ausente', 'ausente_justificado'. Por defecto: 'ausente'.</summary>
        public string? Status { get; set; }
    }
}
