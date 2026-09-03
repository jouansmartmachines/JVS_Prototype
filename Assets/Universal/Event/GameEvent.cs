using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent", menuName = "ScriptableObjects/Event/GameEvent")]
public class GameEvent : ScriptableObject
{
    private List<GameEventListener> _listeners = new List<GameEventListener>();

    public void Raise()
    {
        var caller = new System.Diagnostics.StackTrace().GetFrame(1)?.GetMethod();
        string callerName = caller != null ? $"{caller.DeclaringType?.Name}.{caller.Name}()" : "Inconnu";
        
        //Debug.Log($"<color=cyan><b>[EVENT RAISE]</b></color> L'événement <b>{name}</b> a été déclenché par <b>{callerName}</b>. Nombre d'écouteurs : {_listeners.Count}");

        for (int i = _listeners.Count - 1; i >= 0; i--)
        {
            if (i < _listeners.Count && _listeners[i] != null)
            {
                // ON COUVRE CHAQUE ÉCOUTEUR INDÉPENDAMMENT
                try 
                {
                    //Debug.Log($"   └── <color=silver>[Listener]</color> Notification envoyée à l'objet : <b>{_listeners[i].gameObject.name}</b>");
                    _listeners[i].OnEventRaised();
                }
                catch (System.Exception e)
                {
                    string listenerObjName = _listeners[i].gameObject != null ? _listeners[i].gameObject.name : "Inconnu";
                    
                    Debug.LogError($"<color=red><b>[CRASH LISTENER FIX]</b></color> Événement : <b>{name}</b>\n" +
                                   $"Déclenché par : <b>{callerName}</b>\n" +
                                   $"Écouteur sur GameObject : <color=orange><b>{listenerObjName}</b></color>\n" +
                                   $"Message d'erreur : {e.Message}\n" +
                                   $"Détail : {e.InnerException?.Message ?? e.StackTrace}");
                    // Si un objet plante (ex: problème de Canvas ou de référence), on l'affiche mais on n'arrête pas la boucle !
                    //Debug.LogError($"<color=red>[CRASH ÉCOUTEUR]</color> Erreur provoquée par l'écouteur sur {_listeners[i].gameObject.name} : {e.Message}\n{e.StackTrace}");
                }
            }
        }
    }
    public void RegisterListener(GameEventListener listener)
    {
        if (!_listeners.Contains(listener))
            _listeners.Add(listener);
    }

    public void UnregisterListener(GameEventListener listener)
    {
        if (_listeners.Contains(listener))
            _listeners.Remove(listener);
    }
}