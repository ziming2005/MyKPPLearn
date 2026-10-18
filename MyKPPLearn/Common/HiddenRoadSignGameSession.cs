using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.SessionState;

namespace MyKPPLearn.Common
{
    /// <summary>
    /// Server-side bookkeeping for games that are currently being played. This lives in the ASP.NET
    /// Session only: nothing is written to the database when a game starts or while it is played
    /// (only a COMPLETED game is ever persisted). A game "token" is issued each time the game page
    /// is loaded, remembers when that game started, and makes the final submission one-shot:
    /// submitting the same token again returns the attempt that was already saved instead of
    /// creating a duplicate.
    /// </summary>
    public static class HiddenRoadSignGameSession
    {
        private const string SessionKey = "HiddenRoadSign.Games";
        private const int MaxTrackedGames = 10;

        private sealed class Entry
        {
            public int PracticeId;
            public DateTime StartedAtUtc;
            public int AttemptId;   // 0 until the completed game has been saved
        }

        /// <summary>Begins a game for the Set and returns its token.</summary>
        public static string Start(HttpSessionState session, int practiceId)
        {
            Dictionary<string, Entry> games = GetGames(session);

            if (games.Count >= MaxTrackedGames)
            {
                foreach (string stale in games.OrderBy(g => g.Value.StartedAtUtc).Take(games.Count - MaxTrackedGames + 1).Select(g => g.Key).ToList())
                {
                    games.Remove(stale);
                }
            }

            string token = Guid.NewGuid().ToString("N");
            games[token] = new Entry { PracticeId = practiceId, StartedAtUtc = DateTime.UtcNow };
            session[SessionKey] = games;
            return token;
        }

        /// <summary>
        /// Looks up a running game. Returns false when the token is unknown or was issued for a
        /// different Set. savedAttemptId is non-zero if this game was already saved.
        /// </summary>
        public static bool TryGet(HttpSessionState session, string token, int practiceId, out DateTime startedAtUtc, out int savedAttemptId)
        {
            startedAtUtc = DateTime.UtcNow;
            savedAttemptId = 0;

            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            Entry entry;

            if (!GetGames(session).TryGetValue(token, out entry) || entry.PracticeId != practiceId)
            {
                return false;
            }

            startedAtUtc = entry.StartedAtUtc;
            savedAttemptId = entry.AttemptId;
            return true;
        }

        public static void MarkSaved(HttpSessionState session, string token, int attemptId)
        {
            Dictionary<string, Entry> games = GetGames(session);
            Entry entry;

            if (token != null && games.TryGetValue(token, out entry))
            {
                entry.AttemptId = attemptId;
                session[SessionKey] = games;
            }
        }

        private static Dictionary<string, Entry> GetGames(HttpSessionState session)
        {
            Dictionary<string, Entry> games = session[SessionKey] as Dictionary<string, Entry>;
            return games ?? new Dictionary<string, Entry>();
        }
    }
}
