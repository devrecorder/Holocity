﻿using CityResources;
using Infrastructure.Residents;
using Infrastructure.Tick;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.Grid.Entities.Buildings
{
    public class House : Residential, ITickable
    {
        public float ElectricityDrain = 1;
        public float WaterDrain = 0.3f;
        public float[] ResourceMissingTimes = { 0f, 0f };

        private float timeoutTime = 3f;
        WorldGridTile gridtile;

        //private WorldGridTaskManager.WorldGridTask setResTask;

        public House()
        {
            Name = "House";
            category = BuildingCategory.Residential;
            PrefabName = "Large Old House Fixed";
            Cost = 100;
        }

        public override void OnWorldGridTileCreated(WorldGridTile tile)
        {
            base.OnWorldGridTileCreated(tile);

            gridtile = tile;
        }

        public override void Tick(float time)
        {
            base.Tick(time);

            if (VacantSlots == ResidentCapacity) return;

            bool startElecState = HasPower;
            bool startWaterState = HasWaterSupply;

            //Request the drain amount of each resource. If the request and recieved amount doesnt match we handle that.
            #region Electricity
            float request_electricity = ElectricityDrain * time;
            float recieved_electricity = 0;

            if (CurrentResources.ContainsKey(typeof(Electricity)))
            {
                List<ResourceData> electricity = CurrentResources[typeof(Electricity)];
                foreach (ResourceData rData in electricity)
                {
                    recieved_electricity = rData.resource.Recieve(request_electricity);
                    if (request_electricity == recieved_electricity) break;
                } 
            }

            if (recieved_electricity != request_electricity)
            {
                //Track how long we have not had the power we requested. If its above the acceptable value we dont consider this house to have power.
                ResourceMissingTimes[0] += time;
                if (ResourceMissingTimes[0] > timeoutTime) HasPower = false;
            }
            else
            {
                //We got the power we requested.
                ResourceMissingTimes[0] = 0;
                HasPower = true;
            }

            if(startElecState != HasPower) Game.CurrentSession.TaskManager.Tasks.Enqueue(electricityWarningTask);
            #endregion
            #region Water
            float request_water = WaterDrain * time;
            float recieved_water = 0;

            if (CurrentResources.ContainsKey(typeof(Water)))
            {
                List<ResourceData> water = CurrentResources[typeof(Water)];
                foreach (ResourceData rData in water)
                {
                    recieved_water = rData.resource.Recieve(request_water);
                    if (request_water == recieved_water) break;
                } 
            }

            if (recieved_water != request_water)
            {
                //Track how long we have not had the water we requested. If its above the acceptable value we dont consider this house to have water.
                ResourceMissingTimes[1] += time;
                if (ResourceMissingTimes[1] > timeoutTime) HasWaterSupply = false;
            }
            else
            {
                //We got the power we requested.
                ResourceMissingTimes[1] = 0;
                HasWaterSupply = true;
            }

            if (startWaterState != HasWaterSupply) Game.CurrentSession.TaskManager.Tasks.Enqueue(waterWarningTask);
            #endregion
        }
    }
}
