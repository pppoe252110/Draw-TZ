using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private Transform _rig;
    [SerializeField] private Vector3 _pos = new Vector3(0, 0.09f, 0);
    [SerializeField] private Animator _animator;

    private static int speedHash = Animator.StringToHash("Speed");
    private static int diedHash = Animator.StringToHash("Died");
    private static int gameEndedHash = Animator.StringToHash("GameEnded");

    private void Start()
    {
        _animator.Update(Random.value);
    }

    public void SetSpeed(float speed)
    {
        _animator.SetFloat(speedHash, speed);
    }
    
    public void SetGameEnded()
    {
        _animator.SetBool(gameEndedHash, true);
        _animator.Update(Random.Range(0, 18));
    }

    private void LateUpdate()
    {
        _rig.transform.localPosition = _pos; 
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out TriggerActionBase triggerAction))
        {
            triggerAction.Proceed(this);
        }
    }
}
