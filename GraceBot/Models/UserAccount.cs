using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GraceBot.Models
{
    public class UserAccount
    {
        public UserAccount()
        {
            Id = Guid.NewGuid().ToString();
        }       

        [Key]
        public string Id { get; private set; }
        public string Name { get; set; }
        public UserRole Role { get; set; }

        public virtual List<ChannelAccountModel> ChannelAccountModels { get; set; }
    }

    public enum UserRole
    {
        Administrator = 1,
        Developer = 2,
        Ranger = 3,
        User = 4,

        Blocked = 99
    }
}