//using CitizenFX.Core;
//using FamilyRP.Roleplay.Enums.Character;
//using FamilyRP.Roleplay.Enums.Session;
//using FamilyRP.Roleplay.SharedClasses;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FamilyRP.Roleplay.Server.Classes.Environment
//{
//    static class Admin
//    {
//        static public void Init()
//        {
//            Server.GetInstance().RegisterEventHandler("Admin.SetPermission", new Action<Player, int, string, bool>(SetPermission));
//            Log.Debug("Admin class initialized");
//        }

//        static private void SetPermission([FromSource] Player source, int targetId, string permission, bool targetState)
//        {
//            try
//            {
//                Log.Debug("SetPermission: Trigger Received");
//                if (!(      SessionManager.SessionList[source.Handle].Privilege.HasFlag(Privilege.DEV)
//                        || SessionManager.SessionList[source.Handle].Privilege.HasFlag(Privilege.ADMIN)))
//                {
//                    Log.Debug("SetPermission: Player has insufficient permissions to change others' permissions.");
//                    return;
//                }
//                if(!SessionManager.SessionList.ContainsKey(targetId.ToString()))
//                {
//                    Log.Debug("SetPermission: Could not find target ID.");
//                    return;
//                }
//                Session TargetSession = SessionManager.SessionList[targetId.ToString()];
//                if (String.Equals("Dev", permission, StringComparison.InvariantCultureIgnoreCase))
//                {
//                    TargetSession.SetPrivilege(Privilege.DEV, targetState);
//                }
//                else if (String.Equals("Admin", permission, StringComparison.InvariantCultureIgnoreCase))
//                {
//                    TargetSession.SetPrivilege(Privilege.DEV, targetState);
//                }
//                else if (String.Equals("EMS", permission, StringComparison.InvariantCultureIgnoreCase))
//                {
//                    TargetSession.Character.SetDuty(Duty.EMS, targetState);
//                }
//                else if (String.Equals("Police", permission, StringComparison.InvariantCultureIgnoreCase))
//                {
//                    TargetSession.Character.SetDuty(Duty.Police, targetState);
//                }
//                else if (String.Equals("FireDept", permission, StringComparison.InvariantCultureIgnoreCase))
//                {
//                    TargetSession.Character.SetDuty(Duty.FireDept, targetState);
//                }
//            }
//            catch(Exception ex)
//            {
//                Log.Error($"Admin SetPermission: {ex}");
//            }
//        }
//    }
//}
