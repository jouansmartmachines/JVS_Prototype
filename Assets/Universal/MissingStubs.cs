// Fichier de stubs final - Nettoyage des doublons Demolition

using UnityEngine;

namespace CovidKiller { }
namespace Olou { }
namespace DeadWar { }

// --- GENERAL VARIABLES STUBS ---
public class CrudiCrush_GeneralVariables 
{
    public static CrudiCrush_GeneralVariables Instance => null;
    public static float GetTime() => 0f;
}

namespace RGSMS 
{
    namespace Scene 
    {
        public class SceneManager { }
    }
}

namespace RailShooter 
{
    public class RailShooter_GameManager 
    {
        public static RailShooter_GameManager Instance => null;
    }
}

// --- UNITY RAW INPUT STUBS ---
namespace UnityRawInput 
{ 
    public class RawKeyInput 
    {
        public static void Start(object arg = null) { }
        public static void Stop() { }
        public static bool IsKeyDown(RawKey key) => false;
        public static bool IsPressed(RawKey key) => false;
        public static bool IsDown(RawKey key) => false;
        public static bool IsUp(RawKey key) => false;
    }

    public enum RawKey 
    {
        None, A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
        Space, Return, Escape, Tab, BackSpace, LeftShift, RightShift, LeftCtrl, RightCtrl
    }
} 

// --- OSC MANAGER STUB ---
public class OSC_Manager : MonoBehaviour 
{
    private static OSC_Manager _instance;
    public static OSC_Manager Instance 
    {
        get 
        {
            if (_instance == null) 
            {
                GameObject obj = new GameObject("OSC_Manager_Stub");
                _instance = obj.AddComponent<OSC_Manager>();
            }
            return _instance;
        }
    }

    public object receiveP;

    public void onOSCAccueilTous(object arg = null) { }
    public void onOSCAccueilAppli(object arg = null) { }
    public void messageOutQuit(object arg = null) { }
    public void OnResetAllScoreBoard(object arg = null) { }

    public void ShowSoftKeyboard() { }
    public void GameEnCours() { }
    public void DeactivateAllOscMessages() { }
    public void ReactivateAllOscMessages() { }
    public void StartChoix() { }
    public void SendAccueilTous() { }
}

// --- DOTWEEN STUBS & EXTENSIONS ---
namespace DG.Tweening.Core 
{
    public class TweenerCore<T1, T2, T3> 
    {
        public object WaitForCompletion() => null;
        public TweenerCore<T1, T2, T3> SetTarget(object target) => this;
    }

    namespace Easing 
    {
        public static class EaseManager 
        {
            public static float Evaluate(object ease, float time, float duration, float overshootOrAmplitude, float period) => 0f;
        }
    }
}

namespace DG.Tweening.Plugins.Options 
{
    public struct VectorOptions { }
    public struct FloatOptions { }
}

namespace DG.Tweening.Plugins 
{
    public struct VectorOptions { }
}

namespace DG.Tweening 
{
    public static class DOTweenExtensions 
    {
        public static DG.Tweening.Core.TweenerCore<UnityEngine.Vector3, UnityEngine.Vector3, DG.Tweening.Plugins.Options.VectorOptions> DOMoveX(this UnityEngine.Transform target, float endValue, float duration, bool snapping = false) => null;
        public static DG.Tweening.Core.TweenerCore<UnityEngine.Vector3, UnityEngine.Vector3, DG.Tweening.Plugins.Options.VectorOptions> DOMoveY(this UnityEngine.Transform target, float endValue, float duration, bool snapping = false) => null;
        public static DG.Tweening.Core.TweenerCore<UnityEngine.Vector3, UnityEngine.Vector3, DG.Tweening.Plugins.Options.VectorOptions> DOMoveZ(this UnityEngine.Transform target, float endValue, float duration, bool snapping = false) => null;
        public static DG.Tweening.Core.TweenerCore<UnityEngine.Vector3, UnityEngine.Vector3, DG.Tweening.Plugins.Options.VectorOptions> DOMove(this UnityEngine.Transform target, UnityEngine.Vector3 endValue, float duration, bool snapping = false) => null;
        public static DG.Tweening.Core.TweenerCore<UnityEngine.Vector3, UnityEngine.Vector3, DG.Tweening.Plugins.Options.VectorOptions> DOScale(this UnityEngine.Transform target, UnityEngine.Vector3 endValue, float duration) => null;
        public static DG.Tweening.Core.TweenerCore<UnityEngine.Vector3, UnityEngine.Vector3, DG.Tweening.Plugins.Options.VectorOptions> DOScale(this UnityEngine.Transform target, float endValue, float duration) => null;
        public static object WaitForCompletion(this object tween) => null;
        public static object SetTarget(this object tween, object target) => tween;
    }

    public static class DOTween 
    {
        public static void Init(bool? recycleAllByDefault = null, bool? useSafeMode = null, LogBehaviour? logBehaviour = null) { }
        
        public static DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> To(System.Func<float> getter, System.Action<float> setter, float endValue, float duration) => null;
        public static DG.Tweening.Core.TweenerCore<UnityEngine.Vector3, UnityEngine.Vector3, DG.Tweening.Plugins.Options.VectorOptions> To(System.Func<UnityEngine.Vector3> getter, System.Action<UnityEngine.Vector3> setter, UnityEngine.Vector3 endValue, float duration) => null;
    }

    public enum LogBehaviour { Default, Verbose, ErrorsOnly }
}