using UnityEngine;
using System.Collections;
using System;

public class IEnumeratorTest : MonoBehaviour {
    public enum Result
    {
        Play,
        Pause,
        Success,
        Fail,
        Cancel,
    }
    // Use this for initialization
    void Start() {
        IEnumerator ienumerator = Processing(100, TestUpdate, TestEnd);
        
        Coroutine coroutine =  StartCoroutine(Processing(100, TestUpdate, TestEnd));
        
    }

    // Update is called once per frame
    void Update() {

    }
    
    Result TestUpdate()
    {
        return Result.Play;
    }

    void TestEnd(Result result)
    {

    }

    IEnumerator Processing(float endTime, Func<Result> update, Action<Result> end)
    {
        while(endTime > 0)
        {   
            endTime -= Time.deltaTime;
            yield return null;
        }
    }
}
