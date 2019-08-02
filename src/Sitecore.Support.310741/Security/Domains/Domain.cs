using Sitecore.Configuration;
using Sitecore.Security.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;

namespace Sitecore.Support.Security.Domains
{
    public class Domain: Sitecore.Security.Domains.Domain
    {
        public override IEnumerable<User> GetUsersByName(int pageIndex, int pageSize, string search, out int total)
        {
            List<User> userListByEmail = GetUserListByEmail(search);
            if (userListByEmail.Any())
            {
                total = userListByEmail.Count;
                return userListByEmail;
            }
            MembershipUserCollection usersByName = Membership.FindUsersByName(AccountPrefix + search, pageIndex,
                pageSize, out total);
            var list = new List<User>();
            foreach (MembershipUser membershipUser in usersByName)
                list.Add(User.FromName(membershipUser.UserName, false));
            return list;
        }

        public override int GetUsersByNameCount(string search)
        {
            int totalRecords;
            List<User> userListByEmail = GetUserListByEmail(search);
            if (userListByEmail.Any())
                return userListByEmail.Count;
            Membership.FindUsersByName(GetSearchString(search), 0, 1, out totalRecords);
            return totalRecords;
        }

        public List<User> GetUserListByEmail(string search)
        {
            string searchWithNoWildcard = StringUtil.RemovePostfix(Settings.Authentication.VirtualMembershipWildcard,
                StringUtil.RemovePrefix(Settings.Authentication.VirtualMembershipWildcard, search));
            var list = new List<User>();
            if (searchWithNoWildcard.Contains("@"))
            {
                MembershipUserCollection usersByEmail = Membership.FindUsersByEmail(searchWithNoWildcard);
                foreach (MembershipUser membershipUser in usersByEmail)
                {
                    User sitecoreUser = User.FromName(membershipUser.UserName, false);
                    if (sitecoreUser.Domain.Name + "\\" == AccountPrefix)
                        list.Add(sitecoreUser);
                }
            }
            return list;
        }
    }
}