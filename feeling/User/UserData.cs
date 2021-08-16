using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feeling
{
    class UserData
    {
        public string Account = "";
        public string Password = "";
        public int Universe = 21;
        public bool AutoLogout = false;
        public Dictionary<string, string> AccountDict = new Dictionary<string, string>();

        public void Add(string account, string psw, int universe)
        {
            if (string.IsNullOrWhiteSpace(account)) return;
            if (string.IsNullOrWhiteSpace(psw)) return;

            Account = account;
            Password = psw;
            Universe = universe;

            // 记录
            AccountDict[account] = psw;
        }

        public List<string> GetAccountLists()
        {
            return AccountDict.Keys.ToList();
        }

        public string GetPassword(string account)
        {
            AccountDict.TryGetValue(account, out string psw);
            return psw??string.Empty;
        }
    }
}
