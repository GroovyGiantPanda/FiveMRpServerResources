using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Access
{
    enum PriorityType
    {
        None,
        Reconnect,
        Priority
    }

    // TODO: Remove grace period for AFK people
    class QueueManager : BaseScript
    {
        PlayerList playerList = new PlayerList();
        List<Tuple<Player, string, string, CallbackDelegate, ExpandoObject, PriorityType>> queue = new List<Tuple<Player, string, string, CallbackDelegate, ExpandoObject, PriorityType>>();
        Dictionary<string, int> gracePeriod = new Dictionary<string, int>();
        int gracePeriodDuration = 3 * 60 * 1000;
        int maxPlayerSlots = 32;
        int nonReservedSlots = 24;
        int dots = 1; // "Please wait..." <- dots

        public QueueManager()
        {
            EventHandlers["playerConnecting"] += new Action<Player, string, CallbackDelegate, ExpandoObject>(HandlePlayerConnecting);
            EventHandlers["playerDropped"] += new Action<Player, string>(HandlePlayerDropped);
            PeriodicRefresh();
        }

        private async Task PeriodicRefresh()
        {
            while(true)
            {
                if (queue.Count() > 0 && playerList.Count() < maxPlayerSlots)
                {
                    if((queue[0].Item6 == PriorityType.None && playerList.Count() < nonReservedSlots) || queue[0].Item6 != PriorityType.None)
                    {
                        Debug.WriteLine($"[ACCESS | {playerList.Count()}/32] Letting '{queue.ElementAt(0).Item1.Name}' leave queue and load into the server...");
                        ((CallbackDelegate)queue.ElementAt(0).Item5.ToList()[1].Value)();
                        if(gracePeriod.ContainsKey(queue.ElementAt(0).Item2)) gracePeriod.Remove(queue.ElementAt(0).Item2);
                        queue.RemoveAt(0);
                    }
                }

                int numPriorityQueuers = queue.Where(i => i.Item6 > PriorityType.None).Count();
                queue.Select((queueItem, position) => new { position, queueItem }).ToList().ForEach(q =>
                {
                    ((CallbackDelegate)q.queueItem.Item5.ToList()[2].Value)($"You are in queue position {q.position+1} out of {queue.Count}{(numPriorityQueuers > 0 ? $" (out of which {numPriorityQueuers} priority)" : "")}. Please wait{new String('.', dots + 1)}");
                });
                dots = (dots + 1) % 3;

                // Remove any players from queue that have now loaded in and entered the player list
                List<string> steamIdsOnServer = playerList.Select(p => p.Identifiers["steam"]).ToList();
                queue.RemoveAll(q => steamIdsOnServer.Contains(q.Item2));

                int currentTime = Function.Call<int>(Hash.GET_GAME_TIMER);

                gracePeriod = gracePeriod.ToList().Where(p =>
                {
                    if(currentTime > p.Value)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }).ToDictionary(p => p.Key, p => p.Value);

                await BaseScript.Delay(800);
            }
        }

        private void HandlePlayerDropped([FromSource] Player source, string reason)
        {
            var dropped = queue.Select((queueItem, position) => new { position, queueItem }).Where(q => q.queueItem.Item2 == source.Identifiers["steam"]);
            if (dropped.Count() > 0)
            {
                dropped.ToList().ForEach(d => queue.RemoveAll(q => d.queueItem.Item2 == q.Item2));
                Debug.WriteLine($"[ACCESS | {playerList.Count()}/32] Removed player {source.Name} ({source.Handle}) from queue for dropping");
            }
            else
            {
                Debug.WriteLine($"[ACCESS | {playerList.Count()}/32] Player {source.Name} ({source.Handle}) left ({reason}). He has a {gracePeriodDuration/(float)60000:0.0} minute grace period to reconnect while skipping queue.");
                gracePeriod[source.Identifiers["steam"]] = Function.Call<int>(Hash.GET_GAME_TIMER) + gracePeriodDuration;
            }
        }

        private void HandlePlayerConnecting([FromSource] Player source, string name, CallbackDelegate setReason, ExpandoObject deferrals)
        {
            if (!IsSteamIdAllowed(source.Identifiers["steam"]))
            {
                Debug.WriteLine($"[ACCESS | {playerList.Count()}/32] {source.Name} tried to connect as server ID {source.Handle} ({String.Join(", ", source.Identifiers.ToList())}) but does not have his steam whitelisted");
                setReason.Invoke("Steam not on whitelist");
            }
            // Priority order will be
            // - Players with reconnect grace period
            // - Priority queue/slot players
            // - The rest
            if (playerList.Count() < maxPlayerSlots)
            {
                if (playerList.Count() >= nonReservedSlots)
                {
                    if (gracePeriod.ContainsKey(source.Identifiers["steam"]))
                    {
                        Debug.WriteLine($"[ACCESS | {playerList.Count()}/32] {source.Name} connecting as server ID {source.Handle} ({String.Join(", ", source.Identifiers.ToList())}) within his grace period and got a slot ({(gracePeriod[source.Identifiers["steam"]] - Function.Call<int>(Hash.GET_GAME_TIMER)) / (float)60000:0.0} minutes remaining)");
                        gracePeriod.Remove(source.Identifiers["steam"]);
                        return;
                    }
                    else if (HasPriority(source.Identifiers["steam"]) && playerList.Where(p => HasPriority(p.Identifiers["steam"])).Count() < (maxPlayerSlots - nonReservedSlots)) // If player has priority and there are not enough priority people on the server already
                    {
                        if (playerList.Count() + gracePeriod.Count >= maxPlayerSlots) // If we have recent disconnects we want to wait out their reconnect grace period
                        {
                            Debug.WriteLine($"[ACCESS | {playerList.Count()}/32] {source.Name} connecting as server ID {source.Handle} ({String.Join(", ", source.Identifiers.ToList())}) with priority but there are currently reconnect grace periods that are not yet expired; put in priority queue");
                            // Find last queue position of priority+potential grace period queuers
                            var priorityQueuers = queue.Where(q => q.Item6 > PriorityType.None).Select((queueItem, index) => new { queueItem, index });
                            int insertAt = priorityQueuers.Count() > 0 ? priorityQueuers.Max(q => q.index) : 0;
                            queue.Insert(insertAt, Tuple.Create(source, source.Identifiers["steam"], name, setReason, deferrals, PriorityType.Priority));
                        }
                        else
                        {
                            Debug.WriteLine($"[ACCESS | {playerList.Count()}/32] {source.Name} connecting as server ID {source.Handle} ({String.Join(", ", source.Identifiers.ToList())}) with priority and was let into a priority slot");
                            return;
                        }
                    }
                    else // Otherwise just put the person into normal queue
                    {
                        Debug.WriteLine($"[ACCESS | {playerList.Count()}/32] {source.Name} added to queue");
                        ((CallbackDelegate)deferrals.ToList()[0].Value)();
                        queue.Add(Tuple.Create(source, source.Identifiers["steam"], name, setReason, deferrals, PriorityType.None));
                    }
                }
                else
                {
                    Debug.WriteLine($"[ACCESS | {playerList.Count()}/32] Enough slots to go around! {source.Name} loading into server.");
                    return;
                }
            }
            else
            {
                if (gracePeriod.ContainsKey(source.Identifiers["steam"]))
                {
                    Debug.WriteLine($"[{playerList.Count()}/32] {source.Name} connecting as server ID {source.Handle} ({String.Join(", ", source.Identifiers.ToList())}) within his grace period, but his slot has been taken somehow. Put at the front of the queue. ({(gracePeriod[source.Identifiers["steam"]] - Function.Call<int>(Hash.GET_GAME_TIMER)) / (float)60000:0.0} minutes remaining on grace period)");
                    // Find last queue position of grace period queuers
                    var priorityQueuers = queue.Where(q => q.Item6 == PriorityType.Reconnect).Select((queueItem, index) => new { queueItem, index });
                    int insertAt = priorityQueuers.Count() > 0 ? priorityQueuers.Max(q => q.index) : 0;
                    queue.Insert(insertAt, Tuple.Create(source, source.Identifiers["steam"], name, setReason, deferrals, PriorityType.Reconnect));
                }
                else if (HasPriority(source.Identifiers["steam"]) && playerList.Where(p => HasPriority(p.Identifiers["steam"])).Count() < (playerList.Count() - nonReservedSlots)) // If player has priority and there are not enough priority people on the server already
                {
                    Debug.WriteLine($"[{playerList.Count()}/32] {source.Name} connecting as server ID {source.Handle} ({String.Join(", ", source.Identifiers.ToList())}) with priority and priority slots open but the server is somehow full (should generally not be happening)");
                    var priorityQueuers = queue.Where(q => q.Item6 > PriorityType.None).Select((queueItem, index) => new { queueItem, index });
                    int insertAt = priorityQueuers.Count() > 0 ? priorityQueuers.Max(q => q.index) : 0;
                    queue.Insert(insertAt, Tuple.Create(source, source.Identifiers["steam"], name, setReason, deferrals, PriorityType.Priority));
                }
                else
                {
                    Debug.WriteLine($"[ACCESS | {playerList.Count()}/32] {source.Name} added to normal queue {(HasPriority(source.Identifiers["steam"]) ? "(enough priority players on the server already)" : "")}");
                    queue.Add(Tuple.Create(source, source.Identifiers["steam"], name, setReason, deferrals, PriorityType.None));
                }
                ((CallbackDelegate)deferrals.ToList()[0].Value)();
                ((CallbackDelegate)deferrals.ToList()[2].Value)("The server is full, entering the queue.");
            }
        }

        private bool HasPriority(string id)
        {
            return true;
        }

        bool IsSteamIdAllowed(string id)
        {
            if(id == "110000101dd04f2")
                return true;
            return false;
        }
    }
}
