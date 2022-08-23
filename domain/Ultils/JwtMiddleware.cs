using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace domain.Ultils
{
    public class JwtMiddleware
    {
        private readonly Microsoft.AspNetCore.Http.RequestDelegate _next;
   


        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/user/login"))
            {
                var token = context.Request.Headers["Token"].FirstOrDefault()?.Split(" ").Last();

                if (token != null)
                    validateToken(context, token);
            }
         

            await _next(context);


        }

        private void validateToken(HttpContext context,string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Startup.StaticConfig.GetSection("AppSettings:Secret").Value.ToString()));
                //var key = Encoding.ASCII.GetBytes(Startup.StaticConfig.`GetSection("AppSettings:Secret").Value).ToString();

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(hmac.Key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                context.Items["username"] = jwtToken.Claims.First(x => x.Type == "username").Value;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
