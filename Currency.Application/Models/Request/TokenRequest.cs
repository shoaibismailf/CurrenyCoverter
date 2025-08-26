using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Models.Request
{
    public class TokenRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
    }
}
