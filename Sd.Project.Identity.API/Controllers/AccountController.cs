using Azure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sd.Project.Identity.API.Models;
using Sd.Project.Identity.API.Models.Request;
using Sd.Project.Identity.API.Models.Response;
using Sd.Project.Identity.API.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace Sd.Project.Identity.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager, AppSettings appSettings)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._signInManager = signInManager;

        }

        [HttpPost]
        public async Task<IActionResult> Post(IdentityPostRequest request)
        {
            var user = new IdentityUser
            {
                Email = request.Email,
                UserName = request.UserName,
                EmailConfirmed = false,
            };

            var result = await _userManager.CreateAsync(user, request.Password);


            if (!result.Succeeded)
            {
                throw new Exception(string.Join("-", result.Errors.Select(x => x.Code + x.Description)));
            }

            await _userManager.AddClaimsAsync(user, request.Claims.Select(x => new Claim(x.Key, x.Value)));
            await _userManager.AddToRolesAsync(user, request.Roles);

            return Ok();
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> Get([FromRoute] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            var response = new IdentityGetResponse
            {
                UserName = user.UserName,
                Email = user.Email,
                NormalizedUserName = user.NormalizedUserName,
                Roles = userRoles.ToList(),
                Claims = userClaims.ToDictionary(x => x.Type, x => x.Value),
                Id = user.Id
            };

            return Ok(response);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {

            return await CheckUser(request.UserName, request.Password);
        }


        [HttpPost("Roles")]
        public async Task<IActionResult> Roles(UserRoleRequest request)
        {
            var userRole = new IdentityRole
            {
                Name = request.Role,
                NormalizedName = request.Role

            };

            var response = await _roleManager.CreateAsync(userRole);

            if (!response.Succeeded)
            {
                throw new Exception("This role doesnt exist");
            }

            return Ok();
        }

        [HttpGet("Roles")]
        public async Task<IActionResult> Roles()
        {

            var response = await _roleManager.Roles.ToListAsync();

            return Ok(response);
        }
        private async Task<IActionResult> CheckUser(string userName, string password)
        {
            var roles = new List<string>();
            var claims = new List<Claim>();

            var result = await _signInManager.PasswordSignInAsync(userName,
                         password, false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(userName);
                roles = (await _userManager.GetRolesAsync(user)).ToList();
                claims = (await _userManager.GetClaimsAsync(user)).ToList();
            }
            else
            {
                throw new Exception("Kullanıcı Adı veya Sifresi Hatalı");
            }


            claims.Add(new Claim("UserCode", userName));

            var token = GetToken(claims, roles);

            TokenGenerateGetResponse response = new TokenGenerateGetResponse();

            response = new TokenGenerateGetResponse()
            {
                Token = token
            };

            return Ok(response);
        }

        private string GetToken(IList<Claim> claims, List<string> roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("burayagizlikeygirilecek");

            ClaimsIdentity getClaims()
            {
                return new ClaimsIdentity(
                    getClaims()
                    );

                Claim[] getClaims()
                {
                    foreach (var item in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, item));
                    }

                    return claims.ToArray();
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = getClaims(),
                Expires = DateTime.UtcNow.AddDays(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

    }
}
