using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickAnimationPlayerGroup
    {

        #region PROTECTED ATTRIBUTES

        protected HashSet<QuickAnimationPlayer> _animationPlayers = new HashSet<QuickAnimationPlayer>();

        #endregion

        #region GET AND SET

        public virtual void Clear()
        {
            _animationPlayers.Clear();
        }

        public virtual void AddPlayer(QuickAnimationPlayer player)
        {
            _animationPlayers.Add(player);
        }

        public virtual void RemovePlayer(QuickAnimationPlayer player)
        {
            _animationPlayers.Remove(player);
        }

        public virtual List<QuickAnimationPlayer> GetAnimationPlayers()
        {
            return new List<QuickAnimationPlayer>(_animationPlayers);
        }

        public virtual void Record()
        {
            foreach (QuickAnimationPlayer player in _animationPlayers)
            {
                player.Record();
            }
        }

        public virtual void StopRecording()
        {
            foreach (QuickAnimationPlayer player in _animationPlayers)
            {
                player.StopRecording();
            }
        }

        public virtual void Play(QuickAnimation clip, float timeStart = 0, float timeEnd = Mathf.Infinity)
        {
            foreach (QuickAnimationPlayer player in _animationPlayers)
            {
                player.Play(clip, timeStart, timeEnd);
            }
        }

        public virtual void Play(float timeStart = 0, float timeEnd = Mathf.Infinity)
        {
            foreach (QuickAnimationPlayer player in _animationPlayers)
            {
                player.Play(timeStart, timeEnd);
            }
        }

        public virtual void Playback(float timeStart = 0, float timeEnd = Mathf.Infinity)
        {
            foreach (QuickAnimationPlayer player in _animationPlayers)
            {
                player.Playback(timeStart, timeEnd);
            }
        }

        public virtual bool IsPlaying()
        {
            foreach (QuickAnimationPlayer player in _animationPlayers)
            {
                if (player.IsPlaying()) return true;
            }

            return false;
        }

        public virtual bool IsRecording()
        {
            foreach (QuickAnimationPlayer player in _animationPlayers)
            {
                if (player.IsRecording()) return true;
            }

            return false;
        }

        #endregion

    }
}


