﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Infrastructure.Grid.Entities.Buildings
{
    [TileEntityMeta("Buildings/2x2House", MultiTileSize.x2)]
    public class MultiTileHouse : House
    {
        public MultiTileHouse()
        {
            PrefabName = "2x2House";
            Cost = 250;
            AllowRotation = false;
            CanBeMoved = false;
            Name = "Large House";
        }

        public override void OnEntityProduced(GridSystem grid)
        {
            base.OnEntityProduced(grid);

            //Occupy the adjacent tiles depending on our size
            //Get the layout from the enum.
            OccupyTiles(TileEntityMetaAttribute.GetTileLayout(MultiTileSize.x2));
        }

        public override void OnWorldGridTileCreated(WorldGridTile tile)
        {
            base.OnWorldGridTileCreated(tile);

            tile.Model.transform.localPosition = new Vector3(-0.075f, 0, 0.075f);
        }

        protected override GridTile[] GetAdjacentGridTiles()
        {
            //Modified to return correct tiles that surround a 2x2 building.
            GridSystem sys = ParentTile.ParentGridSystem;
            Vector2Int position = ParentTile.Position;
            GridTile[] adjacentTiles = {
                sys.GetTile(position + new Vector2Int(1, 0)),
                sys.GetTile(position + new Vector2Int(0, -1)),
                sys.GetTile(position + new Vector2Int(-1, 0)),
                sys.GetTile(position + new Vector2Int(-1, -1)),
                sys.GetTile(position + new Vector2Int(-2, 0)),
                sys.GetTile(position + new Vector2Int(-2, 1)),
                sys.GetTile(position + new Vector2Int(-1, 2)),
                sys.GetTile(position + new Vector2Int(0, 2)),
                sys.GetTile(position + new Vector2Int(1, 1))
            };
            return adjacentTiles;
        }

        public override void OnDestroy()
        {
            UnOccupyTiles(TileEntityMetaAttribute.GetTileLayout(MultiTileSize.x2));

            base.OnDestroy();
        }
    }
}
