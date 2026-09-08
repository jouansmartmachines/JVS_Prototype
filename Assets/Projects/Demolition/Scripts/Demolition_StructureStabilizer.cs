using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Demolition
{
    public class Demolition_StructureStabilizer : MonoBehaviour
    {
        private List<Rigidbody> targetRbs = new List<Rigidbody>();
        private Dictionary<Rigidbody, RigidbodyConstraints> originalConstraints = new Dictionary<Rigidbody, RigidbodyConstraints>();
        private Dictionary<Rigidbody, float> originalLinearDamping = new Dictionary<Rigidbody, float>();
        private Dictionary<Rigidbody, float> originalAngularDamping = new Dictionary<Rigidbody, float>();

        public void Initialize(List<Rigidbody> rbs, float stabilizationDuration)
        {
            targetRbs = rbs ?? new List<Rigidbody>();

            foreach (var rb in targetRbs)
            {
                if (rb == null) continue;

                // Sauvegarde l'état initial individuel de chaque Rigidbody
                originalConstraints[rb] = rb.constraints;
                originalLinearDamping[rb] = rb.linearDamping;
                originalAngularDamping[rb] = rb.angularDamping;

                // Active la physique avec verrouillage d'axes pour un atterrissage en douceur
                rb.isKinematic = false;
                rb.useGravity = true;
                
                // Bloque les rotations parasites hors-plan et l'axe Z
                rb.constraints = RigidbodyConstraints.FreezeRotationX | 
                                 RigidbodyConstraints.FreezeRotationZ | 
                                 RigidbodyConstraints.FreezePositionZ;

                // Amortissement fort pendant la pose initiale
                rb.linearDamping = 6f;
                rb.angularDamping = 6f;
            }

            StartCoroutine(StabilizationRoutine(stabilizationDuration));
        }

        private IEnumerator StabilizationRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);

            foreach (var rb in targetRbs)
            {
                if (rb != null)
                {
                    // Restaure les dampings propres à chaque bloc
                    if (originalLinearDamping.TryGetValue(rb, out var lDamping))
                        rb.linearDamping = lDamping;
                    if (originalAngularDamping.TryGetValue(rb, out var aDamping))
                        rb.angularDamping = aDamping;

                    // Restaure les contraintes d'origine tout en gardant le plan 2D verrouillé en Z
                    if (originalConstraints.TryGetValue(rb, out var constraints))
                    {
                        rb.constraints = constraints | RigidbodyConstraints.FreezePositionZ;
                    }
                    else
                    {
                        rb.constraints = RigidbodyConstraints.FreezePositionZ;
                    }
                }
            }

            // Stabilisation terminée avec succès
            Destroy(this);
        }
    }
}
