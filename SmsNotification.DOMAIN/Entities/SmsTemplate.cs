namespace SmsNotification.DOMAIN.Entities
{
    public class SmsTemplate
    {
        /// <summary>
        /// Entity template id
        /// </summary>
        public int SmsTemplateId { get; set; }

        /// <summary>
        /// Entity template name
        /// </summary>
        public string TemplateName { get; set; } = string.Empty;

        /// <summary>
        /// Entity dlt id
        /// </summary>
        public string TemplateDltId { get; set; } = string.Empty;

        /// <summary>
        /// Entity Header
        /// </summary>
        public string Header { get; set; } = string.Empty;

        /// <summary>
        /// Entity communication
        /// </summary>
        public string CommunicationType { get; set; } = string.Empty;

        /// <summary>
        /// Entity sms content
        /// </summary>
        public string SmsContent { get; set; } = string.Empty;

        /// <summary>
        /// Entity consent dlt
        /// </summary>
        public string? ConsentDltId { get; set; }

        /// <summary>
        /// Entity status
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }
}