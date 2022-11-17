using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Registration_Login.Data;
using Registration_Login.Identity;
using Registration_Login.Models.ViewModels;
using Registration_Login.Service.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vonage;
using Vonage.Request;
using Vonage.Verify;

namespace Registration_Login.Controllers
{
    [Route("api/Registration")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRegister _userServie;
        public VonageCredentials _vonageCredentials  { get; }
        public RegisterController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,IRegister userServie, IOptions<VonageCredentials> vonageCredentials)
        {
            _context = context;
            _userManager = userManager;
           _userServie= userServie;
            _vonageCredentials = vonageCredentials.Value;
          
        }
        [HttpGet("UserList")]
        public   IActionResult GetUserList()
        {
            var userList =  _context.ApplicationUsers.Select(x =>new {
                x.Name,
                x.Email,
                x.UserName,
                x.StreetAddress,
                x.State,
                x.City,
                x.PostalCode,
                x.PhoneNumber
               
            }).ToList();

            return Ok(userList);
           
        }

        [HttpPost("RegisterUser")]
        public async Task<IActionResult> Register([FromBody] RegisterVM register)
        {
            if (!ModelState.IsValid)
            {
                return Ok("Fill Valid Data");
            }
            else
            {
                var existingUser = await _userManager.FindByEmailAsync(register.Email);
                if (existingUser == null)
                {
                    ApplicationUser user = new ApplicationUser()
                    {

                        Name = register.Name,
                        StreetAddress = register.StreetAddress,
                        State = register.State,
                        City = register.City,
                        PostalCode = register.PostalCode,
                        UserName = register.UserName,
                        Email = register.Email,
                            PhoneNumber=register.PhoneNumber
                    };
                /*    //Otp
                    var credentials = Credentials.FromApiKeyAndSecret(
                      _vonageCredentials.APIKey,
                      _vonageCredentials.APISecret
                        );
                    var VonageClient = new VonageClient(credentials);
                    var request = new VerifyRequest()
                    {
                        Brand = "Vonage ",
                        Number = user.PhoneNumber,
                        NextEventWait = 180,
                        PinExpiry=180,
                        CodeLength=4
                    };

                    var Response = VonageClient.VerifyClient.VerifyRequest(request);

                    if (Response.RequestId.Length > 0)
                    {
                        return RedirectToAction("VerifyPhone", "Register");
                    }
                    if(Response.Status=="0")
                    {
                        return Ok("User Verified Successfully");
                    }
                    else
                    {
                        return BadRequest("User Enter wrong OTP");
                    }*/

                    var result = await _userManager.CreateAsync(user, register.Password);
                   if  (!result.Succeeded)
                    
                        return BadRequest("Error ,User creation failed! Please check user details and try again.");




                    //Send Message
                    var credentials = Credentials.FromApiKeyAndSecret(
                      _vonageCredentials.APIKey,
                      _vonageCredentials.APISecret
                        );
                    var VonageClient = new VonageClient(credentials);




                    var response = VonageClient.SmsClient.SendAnSms(new Vonage.Messaging.SendSmsRequest()
                    {
                        To = user.PhoneNumber,
                        From = _vonageCredentials.PhoneNumber,
                        Text = "You have Register Successfully ",
                        Title = "Vonage Sms",
                        Body = "Thank you for register with use ."

                    });

                    return Ok(result);
                   /* _context.SaveChanges();
                    return Ok(user);*/
                }
                return Ok(existingUser);
            }
           
            return BadRequest(" ");
                
           

        }

        [HttpPost("LoginUser")]
        public async Task<IActionResult> login(LoginVM loginVM)
        {
            var user = await _userServie.Authenticate(loginVM);
            if (user == null)
                return BadRequest(new { message = "Wrong Email or Password" });
            return Ok(user);
        }
        /*[HttpPost("VerifyPhone")]

        public async Task<IActionResult> VerifyPhone(VerifyPhon verifyPhon)
        {
            var credentials = Credentials.FromApiKeyAndSecret(
                                     _vonageCredentials.APIKey,
                                     _vonageCredentials.APISecret
                                       );
            var VonageClient = new VonageClient(credentials);
            var req = new VerifyCheckRequest()
            {
                RequestId=verifyPhon.RequestId,
                Code = verifyPhon.Code
            };

            var Res = VonageClient.VerifyClient.VerifyCheck(req);
            return Ok(Res);
        }*/



    }
}
