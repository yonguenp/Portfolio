using UnityEngine;

public class BlockObject : PropController
{
    public bool _isChaser = false;
    public bool IsChaser
    {
        get{return _isChaser;}
        set{_isChaser = value;}
    }

    [SerializeField]
    ParticleSystem particle = null;

    private void Awake()
    {
        particle.Stop();
    }
    private void Start() {
        Managers.Object.AddBlockObject(this);
    }

    //isShow == show target type
    public override void ShowRenderer(bool isShow)      
    {
        
        if(IsChaser == isShow)
            particle.Play();
    }
}
