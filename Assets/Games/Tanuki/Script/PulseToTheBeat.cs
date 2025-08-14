using System.Collections;
using UnityEngine;

namespace nostra.booboogames.Tanuki
{
    
    public class PulseToTheBeat : MonoBehaviour
    {
        [SerializeField] bool _useTestBeat;
        [SerializeField] float _pulseSize = 1.15f;
        [SerializeField] float _returnspeed = 5f;
        Vector3 _startSize;

        private void Start()
        {
            _startSize = transform.localScale;

            if (_useTestBeat)
                StartCoroutine(TestBeat());

        }
        private void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _startSize, Time.deltaTime * _returnspeed);
        }

        public void pulse()
        {
            transform.localScale = _startSize * _pulseSize;
        }

        IEnumerator TestBeat() 
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                pulse();
            }
        }

    }
    
}