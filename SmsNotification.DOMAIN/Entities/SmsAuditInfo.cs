namespace SmsNotification.DOMAIN.Entities
{
    public record SmsAuditInfo
    {
        /// <summary>
        /// Entity Client
        /// </summary>
        public string? ClientIP { get; set; }

        /// <summary>
        /// Entity Flag
        /// </summary>
        public string? Flag { get; set; }

        /// <summary>
        /// Entity CountryMobile code
        /// </summary>
        public string? CountryMobileCode { get; set; }

        /// <summary>
        /// Entity username
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Enitty Page Name
        /// </summary>
        public string? PageName { get; set; } 
    }
}
