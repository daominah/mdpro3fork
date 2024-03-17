using DG.Tweening;
using MDPro3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace YgomSystem.Timeline
{
	public class EventPlayableBehaviour : PlayableBehaviour
	{
		public class EventInfo
		{
			public string label;

			public double time;

			public bool isDone;
		}

		public List<EventInfo> eventList;

		public double startTime;

		private bool processed;

		//DIY
        public string label;
        PlayableDirector director;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
            PlayContent();
        }

		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
            if(playable.GetPlayState() == PlayState.Playing)
                PlayContent();
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
		{
        }

		private void CheckEventInfos(Playable playable)
		{
		}

		bool played = false;
		private void PlayContent()
		{
			if(played)
				return;
			played = true;
            if (label == "StartCard")
            {
                if (Program.I().currentServant != Program.I().ocgcore)
                    return;
                var t = TimeLineManager.dummyCard.transform;
                var position = t.position;
                var angels = t.eulerAngles;
                angels = new Vector3(-angels.x, angels.y + 180, -angels.z);
                Program.I().ocgcore.summonCard.StrongSummonLand(position, angels);
                CameraManager.BlackOut(0f, 0.3f);
            }
            else if (label == "StrongSummon")
            {
                if (Program.I().currentServant != Program.I().ocgcore)
                    return;
                TimeLineManager.skippable = false;
                if (MonsterCutin.HasCutin(Program.I().ocgcore.summonCard.GetData().Id))
                    MonsterCutin.Play(Program.I().ocgcore.summonCard.GetData().Id, (int)Program.I().ocgcore.summonCard.p.controller);
            }
        }

    }
}
