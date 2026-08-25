using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.Authentication.ClientServer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Atlas.Conta.BackOffice.WebApi.JWT {
    [ApiController]
    [Route("api/[controller]")]
    // F13-D3: `InvalidModelStateResponseFactory` e global pe TOATE controllerele
    // `[ApiController]`, deci și aici un corp malformat iese `400 EroriDto` —
    // declarat, ca openapi-ul să nu mintă (review F13, defect 3).
    [ProducesResponseType(typeof(Atlas.Conta.BackOffice.Module.Api.EroriDto), StatusCodes.Status400BadRequest)]
    // This is a JWT authentication service sample.
    public class AuthenticationController : ControllerBase {
        readonly IAuthenticationTokenProvider tokenProvider;
        public AuthenticationController(IAuthenticationTokenProvider tokenProvider) {
            this.tokenProvider = tokenProvider;
        }
        [HttpPost("Authenticate")]
        [SwaggerOperation("Checks if the user with the specified logon parameters exists in the database. If it does, authenticates this user.", "Refer to the following help topic for more information on authentication methods in the XAF Security System: <a href='https://docs.devexpress.com/eXpressAppFramework/119064/data-security-and-safety/security-system/authentication'>Authentication</a>.")]
        public IActionResult Authenticate(
            [FromBody]
            [SwaggerRequestBody(@"For example: <br /> { ""userName"": ""Admin"", ""password"": """" }")]
            AuthenticationStandardLogonParameters logonParameters
        ) {
            try {
                return Ok(tokenProvider.Authenticate(logonParameters));
            } catch (AuthenticationException ex) {
                return Unauthorized(ex.GetJson());
            }
        }
    }
}
