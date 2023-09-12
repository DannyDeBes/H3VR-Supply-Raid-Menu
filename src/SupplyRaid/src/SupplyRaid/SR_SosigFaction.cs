﻿using FistVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SupplyRaid
{
    [CreateAssetMenu(fileName = "Faction", menuName = "Supply Raid/Create Sosig Faction", order = 1)]
    public class SR_SosigFaction : ScriptableObject
    {
        [Tooltip("(REQUIRED) Name of this Sosig Faction")]
        public string title = "Faction Name";
        [Tooltip("Short explanation of this faction"), Multiline(6)]
        public string description = "A short description of this sosig faction";
        [Tooltip("(REQUIRED) The menu category this faction will go in, Recommend mod creator names etc")]
        public string category = "Mod";
        [Tooltip("Preview image of the faction when selected")]
        public Sprite? thumbnail;

        [Tooltip("This factions defenders team ID, 0 = Player, 1,2,3... = Enemy")]
        public TeamEnum teamID = TeamEnum.Ally0;

        //List / Table / Group / Faction
        public FactionLevel[]? levels;
        [Tooltip("Endless looping levels after all levels are complete")]
        public FactionLevel[]? endless;
    }

    [System.Serializable]
    public class FactionLevel
    {
        public string name = "Level";

        [Header("Defenders")]

        [Tooltip("The total sosig count for this entire level")]
        public int enemiesTotal = 5;    //Per Player

        [Space]
        [Header("Guards")]
        //GUARDS
        [Tooltip("The minimum size for Gaurds Groups")]
        public int guardCount = 0;
        [Tooltip("The sosig ID pool for stationary gaurds")]
        public SosigEnemyID[]? guardPool;

        [Space]
        [Header("Sniper")]
        //SNIPERS
        [Tooltip("The minimum size for Gaurds Groups")]
        public int sniperCount = 0;
        [Tooltip("The sosig ID pool for stationary snipers")]
        public SosigEnemyID[]? sniperPool;

        [Space]
        [Header("Patrol")]
        //PATROL
        [Tooltip("The minimum size for Patrol Groups")]
        public int minPatrolSize = 2;
        [Tooltip("The sosig ID pool for patrols")]
        public SosigEnemyID[]? patrolPool;

        [Space, Space]
        [Header("Squad")]
        //A Squad is a group of sosigs that spawn at a random supply point then move towards the capture supply point.
        //They can be assigned a team to help or hinder the player

        [Tooltip("The team the world patrol sosigs are on, 0 = Player, 1,2,3 = Enemies")]
        public TeamSquadEnum squadTeamRandomized = TeamSquadEnum.Team1;
        [Tooltip("The amount of squads that wil be spawned")]
        public int squadCount = 0;
        [Tooltip("The minimum sosigs in the squad")]
        public int squadSizeMin = 0;
        [Tooltip("The maximum sosigs in the squad")]
        public int squadSizeMax = 0;
        [Tooltip("Main Supply Point - Squads will move towards the next player capture supply point \n Random Supply Point - Squads will move to random supply points across the map")]
        public SquadBehaviour squadBehaviour = SquadBehaviour.CaptureSupplyPoint;
        [Tooltip("The sosig ID pool for world patrols")]
        public SosigEnemyID[]? squadPool;

        public SosigEnemyID GetEnemyFromPool()
        {
            SosigEnemyID id = SosigEnemyID.None;
            if(patrolPool != null)
                id = patrolPool[Random.Range(0, patrolPool.Length)];

            return id;
        }
        public SosigEnemyID GetGuardFromPool()
        {
            SosigEnemyID id = SosigEnemyID.None;
            if (guardPool != null)
                id = guardPool[Random.Range(0, guardPool.Length)];

            return id;
        }

        public SosigEnemyID GetSniperFromPool()
        {
            SosigEnemyID id = SosigEnemyID.None;
            if (sniperPool != null)
                id = sniperPool[Random.Range(0, sniperPool.Length)];
            return id;
        }
        public SosigEnemyID GetSquadFromPool()
        {
            SosigEnemyID id = SosigEnemyID.None;
            if (squadPool != null)
                id = squadPool[Random.Range(0, squadPool.Length)];

            return id;
        }


    }

    public enum SquadBehaviour
    {
        CaptureSupplyPoint,
        RandomSupplyPoint,
    }

    public enum TeamEnum
    {
        Ally0 = 0,
        Team1 = 1,
        Team2 = 2,
        Team3 = 3,
    }

    public enum TeamSquadEnum
    {
        Ally0 = 0,
        Team1 = 1,
        Team2 = 2,
        Team3 = 3,
        RandomTeam,         //0-4
        RandomEnemyTeam,    //Only enemy teams 1-2-3 etc

    }
}