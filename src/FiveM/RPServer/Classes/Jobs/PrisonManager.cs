using FamilyRP.Roleplay.Helpers;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Server.Classes.Jobs
{
    static class PrisonManager
    {
        static Dictionary<Session, int> jailTimes = new Dictionary<Session, int>();

        static public void Start()
        {
            RegularPrisonTimeUpdate();
        }

        static internal void RegularPrisonTimeUpdate()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        SessionManager.SessionList.ToList().ForEach(async s =>
                        {
                            if (s.Value.Character.PrisonTime > 0)
                            {
                                s.Value.Character.PrisonTime--;
                                using (var conn = DBManager.GetConnection())
                                {
                                    using (var transaction = conn.BeginTransaction())
                                    {
                                        using (var command = conn.GetCommand())
                                        {
                                            try
                                            {
                                                command.Transaction = transaction;
                                                command.CommandText = @"update Characters set PrisonTime = @ActualPrisonTime where CharID = @CharID;";
                                                command.Parameters.AddWithValue("@CharID", s.Value.Character.CharID);
                                                command.Parameters.AddWithValue("@ActualPrisonTime", s.Value.Character.PrisonTime);
                                                await command.ExecuteNonQueryAsync();
                                                transaction.Commit();
                                            }
                                            catch (Exception ex)
                                            {
                                                Log.Error($"PrisonManager Decrement Time Error: {ex.Message}");
                                                transaction.Rollback();
                                            }
                                        }
                                    }
                                }

                                s.Value.Player.TriggerEvent("Arrest.UpdatePrisonTimer", s.Value.Character.PrisonTime);
                            }
                        });
                        Log.Verbose($"Prison Update Ping");
                        await Task.Delay(TimeSpan.FromMinutes(1.0d));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"PrisonManager RegularPrisonTimeUpdate Error: {ex.Message}");
                }
            });
        }
    }
}
