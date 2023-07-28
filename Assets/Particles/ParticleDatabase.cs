using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Particles Database", menuName = "Particles/Database")]
public class ParticleDatabase : ScriptableObject
{
    public const int GLOW_PARTICLES = 1;
    public const int STRENGTH_PARTICLES = 2;
    public const int STUN_PARTICLES = 3;

    [SerializeField] private Particle[] particles = new Particle[0];

    public Particle[] GetAllParticles() => particles;

    public GameObject GetParticleById(int id)
    {
        foreach (var particle in particles)
        {
            if (particle.Id == id)
            {
                return particle.ParticlePrefab.gameObject;
            }
        }
        return null;
    }

    public bool IsValidParticle(int id)
    {
        return particles.Any(x => x.Id == id);
    }
}
