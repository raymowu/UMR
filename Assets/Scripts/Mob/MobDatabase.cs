using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Mob Database", menuName = "Mobs/Database")]
public class MobDatabase : ScriptableObject
{
    [SerializeField] private Mob[] mobsDatabase = new Mob[0];

    public Mob[] GetAllMobs() => mobsDatabase;

    public Mob GetMobById(int id)
    {
        foreach (var mob in mobsDatabase)
        {
            if (mob.Id == id)
            {
                return mob;
            }
        }
        return null;
    }

    // validate character id selected
    public bool IsValidMobId(int id)
    {
        return mobsDatabase.Any(x => x.Id == id);
    }
}
