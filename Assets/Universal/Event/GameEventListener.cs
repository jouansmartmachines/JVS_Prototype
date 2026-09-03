using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [SerializeField]
    private GameEvent _event;

    [SerializeField]
    private UnityEvent _onEventRaised;

    public void OnEventRaised()
    {
        // 1. TRACE : Est-ce que l'événement a des abonnés dans le UnityEvent ?
        int persistentEventCount = _onEventRaised.GetPersistentEventCount();
        //Debug.Log($"<color=cyan>[Listener Debug]</color> {gameObject.name} exécute OnEventRaised. Nombre de fonctions configurées dans l'inspecteur : {persistentEventCount}");

        for (int i = 0; i < persistentEventCount; i++)
        {
            string targetMethod = _onEventRaised.GetPersistentMethodName(i);
            Object targetObject = _onEventRaised.GetPersistentTarget(i);
            //Debug.Log($"   └── <color=orange>[UnityEvent Target]</color> Prévu d'appeler : <b>{targetObject?.name ?? "Null"}.{targetMethod}()</b>    Object name:{gameObject.name}    Event name:{_event?.name ?? "Null"}");
        }

        try
        {
            _onEventRaised.Invoke();
            //Debug.Log("<color=green>[Listener Debug]</color> Invoke() exécuté avec succès par Unity.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>[Listener Error]</color> Crash pendant l'Invoke sur {gameObject.name} : {e.Message}\n{e.StackTrace}");
        }
    }

    private void OnEnable()
    {
        if (_event != null) _event.RegisterListener(this);
    }

    private void OnDisable()
    {
        if (_event != null) _event.UnregisterListener(this);
    }
}