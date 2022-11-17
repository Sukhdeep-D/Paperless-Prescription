using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Registration_Login.Data;
using Registration_Login.Identity;
using Registration_Login.Models.ViewModels;
using Registration_Login.Service.IService;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Registration_Login.Service
{
    public class Register : IRegister
    {
        private readonly ApplicationDbContext _context;
        private readonly ApplicationSignInManager _applicationSignInManager;
        private readonly AppSettings _appSettings;
        private readonly ApplicationUserManager _applicationUserManager;
        public Register(IOptions<AppSettings> appSettings, ApplicationUserManager applicationUserManager, ApplicationDbContext context, UserManager<ApplicationUser> userManager, ApplicationSignInManager applicationSignInManager)
        {
            _context = context;
            _applicationSignInManager = applicationSignInManager;
            _applicationUserManager = applicationUserManager;
            _appSettings = appSettings.Value;
        }

        public async Task<ApplicationUser> Authenticate(LoginVM loginVM)
        {
            var user = await _applicationSignInManager.PasswordSignInAsync(loginVM.UserName, loginVM.Password, false, false);
            if (user.Succeeded)
            {
                var appicationUser = await _applicationUserManager.FindByNameAsync(loginVM.UserName);
                appicationUser.PasswordHash = "";

               

                //JWT Token Genrated

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor()
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.Name, appicationUser.Id),
                  
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                appicationUser.Token = tokenHandler.WriteToken(token);
                return appicationUser;

            }
            else
            {
                return null;
            }
        }
    }
}
