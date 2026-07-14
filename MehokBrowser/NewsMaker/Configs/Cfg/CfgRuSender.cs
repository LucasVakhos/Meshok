using GH.Components;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
namespace NewsMaker
{
    public class CfgRuSender : CfgCore
    {
        [DataMember]
        [Display(Name = "User Id", Description = "User Id ??? ???????????")]
        public int ID { get; set; } = 3875;
        [DataMember]
        [Display(Name = "API Key", Description = "API key ??? ???????????")]
        public string ApiKey { get; set; } = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZFVzZXIiOjIyMDc1LCJpZEV4dGVybmFsTWFpbEFwaUtleSI6Mzg3NSwiaWF0IjoxNzczMTk5NjU5fQ.9oYzM7cw0vK6Sr3zcL1wLzX0m0inNcl4qii4z14iMDA";
        [DataMember]
        [Display(Name = "Back Email", Description = "???????? ?????")]
        public string BackEmail { get; set; } = "info@bridgenote.com";
        [DataMember]
        [Display(Name = "Send Limit In 1 Second", Description = "??????????? ???????? ?? 1 ???????")]
        public int SendLimit { get; set; } = 10;
        protected override void CreateSomething()
        {
            //????????
        }
    }
}
