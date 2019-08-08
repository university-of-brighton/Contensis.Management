namespace UniversityOfBrighton.Contensis.Management
{
    /// <summary>
    /// Helper class for the credentials needed to use
    /// the Contensis Management API
    /// </summary>
    public class CmsCredentials
    {
        public string ClientId { get; set; }
        public string RootUrl { get; set; }
        public string SharedSecret { get; set; }
        public string ProjectName { get; set; }
    }
}
