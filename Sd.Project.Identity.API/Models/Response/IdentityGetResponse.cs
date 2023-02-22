namespace Sd.Project.Identity.API.Models.Response
{
    public class IdentityGetResponse
    {
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string LocationCode { get; set; }
        public string UserCode { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public Dictionary<string, string> Claims { get; set; } = new Dictionary<string, string>();
        public string Id { get; set; }
    }
}
