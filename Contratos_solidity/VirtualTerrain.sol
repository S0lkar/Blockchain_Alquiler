// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

contract VirtualTerrain {
    struct Terrain {
        address owner;
        uint256 price;
        bool forSale;
    }

    // Mapping of coordinates to Terrain
    mapping(uint256 => mapping(uint256 => Terrain)) public terrainMap;
    uint256 public constant GRID_SIZE = 100;

    // Event for when a terrain is bought
    event TerrainBought(address indexed buyer, uint256 x, uint256 y, uint256 price);

    // Function to set a piece of terrain for sale
    function setForSale(uint256 x, uint256 y, uint256 price) public {
        require(x < GRID_SIZE && y < GRID_SIZE, "Invalid coordinates");
        require(msg.sender == terrainMap[x][y].owner, "Not the owner");
        terrainMap[x][y].price = price;
        terrainMap[x][y].forSale = true;
    }


    // Function to buy a piece of terrain
    function buyTerrain(uint256 x, uint256 y) public payable {
        require(x < GRID_SIZE && y < GRID_SIZE, "Invalid coordinates");
        Terrain storage terrain = terrainMap[x][y];
        require(terrain.forSale, "Terrain not for sale");
        require(msg.value >= terrain.price, "Insufficient funds");

        address previousOwner = terrain.owner;
        uint256 price = terrain.price;

        terrain.owner = msg.sender;
        terrain.forSale = false;
        terrain.price = 0;

        // Transfer the funds to the previous owner
        payable(previousOwner).transfer(price);

        emit TerrainBought(msg.sender, x, y, price);
    }

    // Function to initialize terrain ownership
    function initializeTerrain(uint256 x, uint256 y) public {
        require(x < GRID_SIZE && y < GRID_SIZE, "Invalid coordinates");
        require(terrainMap[x][y].owner == address(0), "Terrain already initialized");

        terrainMap[x][y].owner = msg.sender;
    }


    // Function to get the details of a piece of terrain
    function getTerrainDetails(uint256 x, uint256 y) public view returns (address, uint256, bool) {
        require(x < GRID_SIZE && y < GRID_SIZE, "Invalid coordinates");
        Terrain storage terrain = terrainMap[x][y];
        return (terrain.owner, terrain.price, terrain.forSale);
    }
}
