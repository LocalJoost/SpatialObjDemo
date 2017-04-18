﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using HoloToolkit.Unity;

namespace HoloToolkit.Sharing
{
    /// <summary>
    /// Keeps track of the users in the current session.
    /// </summary>
    public class SessionUsersTracker : IDisposable
    {
        /// <summary>
        /// UserJoined event notifies when a user joins the current session.
        /// </summary>
        public event Action<User> UserJoined;

        /// <summary>
        /// UserLeft event notifies when a user leaves the current session.
        /// </summary>
        public event Action<User> UserLeft;

        /// <summary>
        /// Local cached pointer to the sessions tracker..
        /// </summary>
        private readonly ServerSessionsTracker serverSessionsTracker;
        
        /// <summary>
        /// List of users that are in the current session.
        /// </summary>
        public List<User> CurrentUsers { get; private set; }

        public SessionUsersTracker(ServerSessionsTracker sessionsTracker)
        {
            CurrentUsers = new List<User>();

            this.serverSessionsTracker = sessionsTracker;
            this.serverSessionsTracker.CurrentUserJoined += OnCurrentUserJoinedSession;
            this.serverSessionsTracker.CurrentUserLeft += OnCurrentUserLeftSession;

            this.serverSessionsTracker.UserJoined += OnUserJoinedSession;
            this.serverSessionsTracker.UserLeft += OnUserLeftSession;
		}

		/// <summary>
		/// Finds and returns an object representing a user who has the supplied id number. Returns null if the user is not found.
		/// </summary>
		/// <param name="userId">The numerical id of the session User to find</param>
		/// <returns>The User with the specified id or null (if not found)</returns>
		public User GetUserById(int userId)
		{
			for (int u = 0; u < CurrentUsers.Count; u++)
			{
				var user = CurrentUsers[u];
				if (user.GetID() == userId)
					return user;
			}
			return null;
		}

		#region IDisposable

		public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.serverSessionsTracker.CurrentUserJoined -= OnCurrentUserJoinedSession;
                this.serverSessionsTracker.CurrentUserLeft -= OnCurrentUserLeftSession;

                this.serverSessionsTracker.UserJoined -= OnUserJoinedSession;
                this.serverSessionsTracker.UserLeft -= OnUserLeftSession;
            }
        }

        #endregion

        private void OnCurrentUserJoinedSession(Session joinedSession)
        {
            //Debug.LogFormat("Joining session {0}.", joinedSession.GetName());

            // If joining a new session, any user in the previous session (if any) have left
            ClearCurrentSession();

            // Send a join event for every user currently in the session we joined
            for (int i = 0; i < joinedSession.GetUserCount(); i++)
            {
                User user = joinedSession.GetUser(i);
                CurrentUsers.Add(user);
                UserJoined.RaiseEvent(user);;
            }
        }

        private void OnCurrentUserLeftSession(Session leftSession)
        {
            //Debug.Log("Left current session.");

            // If we leave a session, notify that every user has left the current session of this app
            ClearCurrentSession();
        }

        private void OnUserJoinedSession(Session session, User user)
        {
            if (!session.IsJoined())
            {
                return;
            }

            if (!CurrentUsers.Contains(user))
            {
				//Debug.LogFormat("User {0} joined current session.", user.GetName());
				CurrentUsers.RemoveAll(x => x.GetID() == user.GetID()); // in case there was an old user with the same ID
				CurrentUsers.Add(user);
                UserJoined.RaiseEvent(user);
            }
        }

        private void OnUserLeftSession(Session session, User user)
        {
            if (!session.IsJoined())
            {
                return;
            }

            if (CurrentUsers.RemoveAll(x => x.GetID() == user.GetID()) > 0)
            {
                //Debug.LogFormat("User {0} left current session.", user.GetName());
                UserLeft.RaiseEvent(user);
            }
		}

		/// <summary>
		/// Clears the current session, removing any users being tracked.
		/// This should be called whenever the current session changes, to reset this class
		/// and handle a new curren session.
		/// </summary>
		private void ClearCurrentSession()
        {
            for (int i = 0; i < CurrentUsers.Count; i++)
            {
                UserLeft.RaiseEvent(CurrentUsers[i]);
            }

            CurrentUsers.Clear();
        }
    }
}