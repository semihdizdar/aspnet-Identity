namespace Sd.Project.Identity.API.Models
{
    public class IdentityPostRequest
    {
        public IdentityPostRequest()
        {
            this.Claims = new Dictionary<string, string>();
            this.Roles = new List<string>();
        }

        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public Dictionary<string, string> Claims { get;set; }
        public List<string> Roles { get; set; }
    }
}
