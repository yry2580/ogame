using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feeling
{
    class User
    {
        string mConfigFile = NativeConst.CfgDirectory + "UserData.dat";
        UserData MyUserData = null;

        public string Account => MyUserData.Account;
        public string Password => MyUserData.Password;
        public int Universe => MyUserData.Universe;
        public bool AutoLogout => MyUserData.AutoLogout;
        public bool MorningIdle => MyUserData.MorningIdle;

        public User()
        {
            // 读取配置
            ReadUserData();
        }

        public UserData ReadUserData()
        {
            if (!File.Exists(mConfigFile))
            {
                MyUserData = new UserData();
                SaveUserData();
            }
            else
            {
                try
                {
                    var text = File.ReadAllText(mConfigFile);
                    var bytes = Convert.FromBase64String(text);
                    text = Encoding.UTF8.GetString(bytes);
                    MyUserData = JsonConvert.DeserializeObject<UserData>(text);
                }
                catch (Exception ex) 
                {
                    NativeLog.Error($"ReadUserData catch {ex.Message}");
                    MyUserData = MyUserData??(new UserData());
                }
            }

            return MyUserData;
        }

        protected void SaveUserData()
        {
            if (MyUserData == null) return;

            try
            {
                string text = JsonConvert.SerializeObject(MyUserData, Formatting.Indented);
                var bytes = Encoding.UTF8.GetBytes(text);
                text = Convert.ToBase64String(bytes);
                File.WriteAllText(mConfigFile, text);

            }
            catch(Exception ex)
            {
                NativeLog.Error($"SaveUserData catch {ex.Message}");
            }
        }

        public void SetUserData(string account, string psw, int universe)
        {
            MyUserData.Add(account, psw, universe);

            SaveUserData();
        }

        public List<string> GetAccountLists()
        {
            return MyUserData.GetAccountLists();
        }

        public string GetPassword(string account)
        {
            return MyUserData.GetPassword(account);
        }

        public void SetAutoLogout(bool autoLogout = false)
        {
            MyUserData.AutoLogout = autoLogout;
            SaveUserData();
        }

        public void SetMorningIdle(bool isIdle = false)
        {
            MyUserData.MorningIdle = isIdle;
            SaveUserData();
        }
    }
}
