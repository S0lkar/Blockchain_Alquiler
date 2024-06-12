using UnityEngine;
using UnityEngine.Events;

public class Destructor : MonoBehaviour
{
    static public UnityEvent m_MyEvent;
    void Start()
    {
        m_MyEvent ??= new UnityEvent();
        m_MyEvent.AddListener(Death);
    }

    void Death()
    {
        Destroy(this.gameObject);
    }
}
