using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DamageHealTesst : MonoBehaviour
{
    [SerializeField] private AdvancedRagdollController controller;
    [SerializeField] float damageAmount = 10f;
    [SerializeField] float healAmount = 10f;

    public void ApplyDamage()
    {
        controller.ApplyDamage(damageAmount);
    }

    public void ApplyHeal()
    {
        controller.ApplyHealth(healAmount);
    }
}

[CustomEditor(typeof(DamageHealTesst))]
public class DamageHealTesstEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draw the default inspector

        DamageHealTesst damageHeal = (DamageHealTesst)target;

        // Add a button to the inspector
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Heal Player"))
            {
                damageHeal.ApplyHeal();
            }

            if (GUILayout.Button("Damage Player"))
            {
                damageHeal.ApplyDamage();
            }
        }
    }
}
