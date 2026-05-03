
namespace SmsNotification.APPLICATION.Utilities
{
    /// <summary>
    /// Class for replace OTP variable
    /// </summary>
    public static class SmsTemplateRenderer
    {
       public static string Render(
            string template,
            Dictionary<string, string> values)
        {
            foreach (var kv in values)
            {
                template = template.Replace(
                    "{#" + kv.Key + "#}", kv.Value);
            }
            return template;
        }
    }
}
