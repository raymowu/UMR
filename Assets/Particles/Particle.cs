using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Particle", menuName = "Particles/Particle")]
public class Particle : ScriptableObject
{
    [SerializeField] private int id = -1;
    [SerializeField] private string displayName = "New Particle Display Name";
    [SerializeField] private NetworkObject particlePrefab;

    // Get private stuff publicly
    public int Id => id;
    public string DisplayName => displayName;
    public NetworkObject ParticlePrefab => particlePrefab;
}
