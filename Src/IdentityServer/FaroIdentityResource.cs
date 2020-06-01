using IdentityServer4.Models;
using System.Collections.Generic;
using IdentityServer4;

namespace IdentityServer
{
    public class FaroIdentityResource : IdentityResource
    {
        public FaroIdentityResource() //: base(name: "Faro", claimTypes: new List<string>() { "FaroClaim" })
        {
            Name = "FaroResource"; // TODO usar constant!
            DisplayName = "Faro Custom Identity Resource";
            Description = "My fisrt custom identity resource for testing";
            Emphasize = true; // can be false? not important
            UserClaims = new[] { "faroClaim" };
        }
    }
}
