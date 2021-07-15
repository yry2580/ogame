﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OgameService
{
    public class OgameData
    {
        public CmdEnum Cmd = CmdEnum.None;
        public ErrEnum Error = ErrEnum.Ok;
        public StatusEnum Status = StatusEnum.None;
        public string Id = "";
        public string Content = "";
        public string PirateAutoMsg = "";
        public string SessionKey = "";

        public static byte[] ToBytes(OgameData data)
        {
            return Encoding.UTF8.GetBytes(ToJson(data));
        }

        public static string ToJson(OgameData data)
        {
            string content = "";

            if (null == data) return content;

            try
            {
                content = JsonConvert.SerializeObject(data);
            }
            catch (Exception ex)
            {
                LogUtil.Error($"ToJson catch {ex.Message}");
            }

            return content;
        }

        public static OgameData ParseData(string content)
        {
            OgameData result = null;
            try
            {
                if (string.IsNullOrWhiteSpace(content)) return result;
                result = JsonConvert.DeserializeObject<OgameData>(content);
            }
            catch (Exception ex)
            {
                LogUtil.Error($"ParseData catch {ex.Message}");
            }

            return result;
        }
    }
}