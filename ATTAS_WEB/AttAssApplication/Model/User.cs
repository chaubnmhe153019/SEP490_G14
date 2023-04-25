using MessagePack;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Xml;
using KeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;

namespace AttAssApplication.Model
{
    public class User
    {
        [Key]
        [Required]
        [BindProperty]
        public string UserName { get; set; }
        [DataType(DataType.Password)]
        [BindProperty]
        public string Password { get; set; }
        public bool IsValid()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load("Data.xml");

            XmlNodeList users = xmlDoc.SelectNodes("//user");
            foreach (XmlNode user in users)
            {
                XmlNode usernameNode = user.SelectSingleNode("username");
                XmlNode passwordNode = user.SelectSingleNode("password");

                if (usernameNode.InnerText == UserName && passwordNode.InnerText == Password)
                {
                    return true;
                }
            }
            return false;

        }
    }
}
