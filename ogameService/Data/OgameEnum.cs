using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OgameService
{
    public enum ErrEnum
    {
        Unkown = -1,
        Ok,
        Failed
    }

    public enum CmdEnum
    {
        None,
        Auth,
        Hello,
        Data,
        Login,
        Pirate,
        Logout,
        Expedition,
        GetCode,
        AuthCode,
        Imperium,
        Npc,
        Screenshot,
        Fs,
        AutoPirateOpen,
        AutoExpeditionOpen,
        PirateCfg,
        GoCross,
        BackUniverse,
        ExpeditionCfg,
        AutoLogoutOpen,
        AutoImperiumOpen,
        AutoPirateOpen1,
        AutoExpeditionOpen1,
        QuickAutoCheck,
        QuickAutoUncheck,
        QuickAutoStart,
        PirateSpeed,
    }

    public enum StatusEnum
    {
        None = 0,
        System = 1,
        Galaxy,
        Expedition,
        Pirate,
    }
}
