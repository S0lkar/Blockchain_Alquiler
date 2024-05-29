// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

contract VirtualLand is ERC721, Ownable {
    struct Land {
        uint256 x;
        uint256 y;
        uint256 width;
        uint256 height;
    }

    uint256 public nextTokenId;
    uint256 public gridWidth;
    uint256 public gridHeight;
    mapping(uint256 => Land) public lands;

    constructor(uint256 _gridWidth, uint256 _gridHeight) 
        ERC721("VirtualLand", "VLAND") 
        Ownable(msg.sender) 
    {
        gridWidth = _gridWidth;
        gridHeight = _gridHeight;
    }

    function buyLand(uint256 x, uint256 y, uint256 width, uint256 height) public payable {
        require(x + width <= gridWidth, "Land exceeds grid width");
        require(y + height <= gridHeight, "Land exceeds grid height");
        require(!isOverlapping(x, y, width, height), "Land overlaps with an existing one");

        uint256 tokenId = nextTokenId;
        nextTokenId++;

        lands[tokenId] = Land(x, y, width, height);
        _safeMint(msg.sender, tokenId);
    }

    function isOverlapping(uint256 x, uint256 y, uint256 width, uint256 height) internal view returns (bool) {
        for (uint256 i = 0; i < nextTokenId; i++) {
            Land storage land = lands[i];
            if (x < land.x + land.width && x + width > land.x &&
                y < land.y + land.height && y + height > land.y) {
                return true;
            }
        }
        return false;
    }

    function getLand(uint256 tokenId) public view returns (uint256, uint256, uint256, uint256) {
        Land storage land = lands[tokenId];
        return (land.x, land.y, land.width, land.height);
    }
}
