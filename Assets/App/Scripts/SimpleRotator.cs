using UnityEngine;

public class SimpleRotator : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        LeanTween.rotateAround(gameObject, Vector3.up, 360, 10f).setLoopClamp();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
