using UnityEngine;
using UnityEngine.Events;

namespace nostra.booboogames.Tanuki
{
    
    public class BeatManager : MonoBehaviour
    {
        [SerializeField] float _bpm;
        public AudioSource _audiosource;
        [SerializeField] Intervals[] _intervals;


        private void Update()
        {
            foreach (var interval in _intervals) 
            {
                float sampleTime = (_audiosource.timeSamples / (_audiosource.clip.frequency * interval.GetIntervalLenght(_bpm)));
                interval.CheckForNewInterval(sampleTime);
            }
        }
    }


    [System.Serializable]
    public class Intervals 
    {
        [SerializeField] float _steps;
        [SerializeField] UnityEvent _trigger;
        int _lastinterval;

       public float GetIntervalLenght(float bpm)
        {
            return 60f / (bpm * _steps);
        }

       public  void CheckForNewInterval(float interval)
        {
            if (Mathf.FloorToInt(interval) != _lastinterval) 
            {
                _lastinterval = Mathf.FloorToInt(interval);
                _trigger.Invoke();
            }
            
        }

    }


}